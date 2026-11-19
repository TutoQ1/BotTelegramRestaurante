using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Entidades;
using Entidades.Contratos;

namespace Logica
{
    /// <summary>Recibe los mensajes de los clientes y responde a cada uno.</summary>
    public class ReceptorMensajes
    {
        private const int EsperaTrasErrorMs = 2000;

        private readonly ICanalMensajeria canal;
        private readonly ProcesadorMensajes procesador;
        private readonly ClienteServicio clientes;
        private readonly Bitacora bitacora;

        public ReceptorMensajes(ICanalMensajeria canal, ProcesadorMensajes procesador,
                                ClienteServicio clientes, Bitacora bitacora)
        {
            this.canal = canal;
            this.procesador = procesador;
            this.clientes = clientes;
            this.bitacora = bitacora;
        }

        public async Task EjecutarAsync(CancellationToken cancelacion)
        {
            while (!cancelacion.IsCancellationRequested)
            {
                try
                {
                    IReadOnlyList<MensajeEntrante> mensajes = await canal.RecibirAsync(cancelacion);
                    foreach (MensajeEntrante mensaje in mensajes)
                        await AtenderAsync(mensaje, cancelacion);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    bitacora.Registrar("Error al recibir: " + ex.Message);
                    if (!await Espera.PorAsync(EsperaTrasErrorMs, cancelacion))
                        break;
                }
            }
        }

        private async Task AtenderAsync(MensajeEntrante mensaje, CancellationToken cancelacion)
        {
            try
            {
                clientes.RegistrarSiEsNuevo(mensaje.ChatId, mensaje.NombreCliente);
                bitacora.Registrar(mensaje.NombreCliente + " (" + mensaje.ChatId + "): " + mensaje.Texto);

                string respuesta = procesador.Procesar(mensaje);
                await canal.EnviarAsync(mensaje.ChatId, respuesta, cancelacion);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                bitacora.Registrar("Error al atender a " + mensaje.NombreCliente + ": " + ex.Message);
            }
        }
    }
}
