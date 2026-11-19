using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Entidades;
using Entidades.Contratos;

namespace Logica
{
    /// <summary>
    /// Revisa los pedidos cuyo tiempo se cumplió, los encola y envía la cola de
    /// avisos. La cola está en la base de datos, así que un fallo de Telegram
    /// (o cerrar la aplicación) no pierde el aviso: se reintenta después.
    /// </summary>
    public class NotificadorPedidos
    {
        private const int IntervaloRevisionMs = 2000;
        private const int EsperaTrasErrorMs = 5000;
        private const int LoteMaximo = 20;
        private const int IntentosMaximos = 6;

        private readonly ICanalMensajeria canal;
        private readonly PedidoServicio pedidos;
        private readonly INotificacionRepositorio notificaciones;
        private readonly Bitacora bitacora;

        public NotificadorPedidos(ICanalMensajeria canal, PedidoServicio pedidos,
                                  INotificacionRepositorio notificaciones, Bitacora bitacora)
        {
            this.canal = canal;
            this.pedidos = pedidos;
            this.notificaciones = notificaciones;
            this.bitacora = bitacora;
        }

        public async Task EjecutarAsync(CancellationToken cancelacion)
        {
            while (!cancelacion.IsCancellationRequested)
            {
                try
                {
                    EncolarAvisosVencidos();
                    await EnviarPendientesAsync(cancelacion);

                    await Task.Delay(IntervaloRevisionMs, cancelacion);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    bitacora.Registrar("Error en revisión: " + ex.Message);
                    if (!await Espera.PorAsync(EsperaTrasErrorMs, cancelacion))
                        break;
                }
            }
        }

        /// <summary>Pasa a la cola los pedidos que ya cumplieron su tiempo.</summary>
        private void EncolarAvisosVencidos()
        {
            foreach (Pedido pedido in pedidos.ObtenerPorAvisar(DateTime.Now))
            {
                bool nuevo = notificaciones.Encolar(pedido.Id, TipoNotificacion.PedidoListo,
                                                    pedido.ChatId, MensajesBot.PedidoListo(pedido));
                if (nuevo)
                    bitacora.Registrar("Pedido #" + pedido.Id + " en cola para avisar a " + pedido.NombreCliente);
            }
        }

        private async Task EnviarPendientesAsync(CancellationToken cancelacion)
        {
            List<NotificacionPendiente> pendientes = notificaciones.ObtenerPendientes(DateTime.Now, LoteMaximo);
            foreach (NotificacionPendiente aviso in pendientes)
            {
                if (cancelacion.IsCancellationRequested)
                    return;

                Pedido pedido = pedidos.ObtenerPorId((int)aviso.PedidoId);

                // Si el pedido se canceló antes de que saliera el aviso, no se envía.
                if (pedido == null || (aviso.Tipo == TipoNotificacion.PedidoListo && pedido.Cancelado))
                {
                    notificaciones.MarcarOmitida(aviso.Id, "El pedido ya no está activo.");
                    continue;
                }

                try
                {
                    await canal.EnviarAsync(aviso.ChatId, aviso.Texto, cancelacion);
                    notificaciones.MarcarEnviada(aviso.Id);

                    if (aviso.Tipo == TipoNotificacion.PedidoListo)
                        pedidos.MarcarNotificado(pedido);

                    bitacora.Registrar("<<< Aviso enviado: pedido #" + aviso.PedidoId +
                                       " (" + aviso.Tipo + ") a " + pedido.NombreCliente);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    notificaciones.MarcarFallo(aviso.Id, ex.Message, IntentosMaximos);
                    bitacora.Registrar("No se pudo enviar el aviso del pedido #" + aviso.PedidoId +
                                       " (intento " + (aviso.Intentos + 1) + "): " + ex.Message);
                }
            }
        }
    }
}
