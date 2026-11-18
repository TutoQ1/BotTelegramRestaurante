using Entidades;

namespace Logica.Comandos
{
    /// <summary>El cliente responde con la letra de un plato del menú.</summary>
    public class ComandoPedirPlato : IComandoBot
    {
        private readonly MenuRestaurante menu;
        private readonly PedidoServicio pedidos;
        private readonly Bitacora bitacora;

        public ComandoPedirPlato(MenuRestaurante menu, PedidoServicio pedidos, Bitacora bitacora)
        {
            this.menu = menu;
            this.pedidos = pedidos;
            this.bitacora = bitacora;
        }

        public bool Atiende(string texto)
        {
            return menu.Existe(texto);
        }

        public string Ejecutar(MensajeEntrante mensaje)
        {
            Plato plato = menu.Buscar(mensaje.Texto);
            Pedido pedido = pedidos.Registrar(mensaje.ChatId, mensaje.NombreCliente, plato);

            bitacora.Registrar(">>> NUEVO PEDIDO #" + pedido.Id + ": " + pedido.NombrePlato + " para " + mensaje.NombreCliente);
            return MensajesBot.PedidoRegistrado(pedido);
        }
    }
}
