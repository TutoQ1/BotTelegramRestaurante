using Entidades;

namespace Logica.Comandos
{
    /// <summary>Prepara el plato elegido; todavía no registra el pedido.</summary>
    public class ComandoPedirPlato : IComandoBot
    {
        private readonly MenuRestaurante menu;
        private readonly PedidoServicio pedidos;

        public ComandoPedirPlato(MenuRestaurante menu, PedidoServicio pedidos, Bitacora bitacora)
        {
            this.menu = menu; this.pedidos = pedidos;
        }

        public bool Atiende(string texto) { return menu.Existe(texto); }

        public string Ejecutar(MensajeEntrante mensaje)
        {
            Plato plato = menu.Buscar(mensaje.Texto);
            pedidos.PrepararSeleccion(mensaje.ChatId, plato);
            return MensajesBot.ResumenPrevio(pedidos.ObtenerSelecciones(mensaje.ChatId));
        }
    }
}
