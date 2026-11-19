using System.Collections.Generic;
using System.Threading.Tasks;
using Entidades;
using Entidades.Contratos;

namespace Logica
{
    /// <summary>
    /// Punto de entrada de la GUI (GRASP Controlador): recibe las acciones del panel y las reparte
    /// entre los servicios. Así los formularios no conocen cómo está construido el sistema.
    /// </summary>
    public class RestauranteControlador
    {
        private readonly BotServicio bot;
        private readonly PedidoServicio pedidos;
        private readonly INotificacionRepositorio notificaciones;

        public RestauranteControlador(BotServicio bot, PedidoServicio pedidos, HistorialServicio historial,
                                      INotificacionRepositorio notificaciones, Bitacora bitacora)
        {
            this.bot = bot;
            this.pedidos = pedidos;
            this.notificaciones = notificaciones;
            Historial = historial;
            Bitacora = bitacora;
        }

        public Bitacora Bitacora { get; }
        public HistorialServicio Historial { get; }

        public bool BotActivo
        {
            get { return bot.Activo; }
        }

        public Task<InformacionBot> IniciarBotAsync(string token)
        {
            return bot.IniciarAsync(token);
        }

        public void DetenerBot()
        {
            bot.Detener();
        }

        public List<Pedido> ObtenerPedidosPanel()
        {
            return pedidos.ObtenerParaPanel();
        }

        public void MarcarListo(int idPedido)
        {
            if (!bot.Activo)
                throw new RestauranteException("Primero inicia el bot para poder avisar al cliente.");
            if (!pedidos.MarcarListo(idPedido))
                throw new RestauranteException("Ese pedido ya fue avisado.");

            Bitacora.Registrar("Pedido #" + idPedido + " marcado como listo manualmente.");
        }

        public Task CancelarPedidoAsync(int idPedido)
        {
            Pedido cancelado = pedidos.CancelarPendiente(idPedido);
            if (cancelado == null)
                throw new RestauranteException("Solo se pueden cancelar pedidos pendientes.");

            Bitacora.Registrar("Pedido #" + idPedido + " cancelado.");

            // El aviso no se manda directo: entra a la cola, que reintenta sola
            // si Telegram falla. Si el bot está detenido no hay quien la procese,
            // así que solo se encola con el bot activo (igual que antes).
            if (bot.Activo)
                notificaciones.Encolar(cancelado.Id, TipoNotificacion.PedidoCancelado,
                                       cancelado.ChatId, MensajesBot.PedidoCanceladoPorElRestaurante(cancelado));

            return Task.CompletedTask;
        }
    }
}
