using Entidades;
namespace Logica.Comandos
{
    public class ComandoConfirmar : IComandoBot
    {
        private readonly PedidoServicio pedidos;
        public ComandoConfirmar(PedidoServicio pedidos) { this.pedidos = pedidos; }
        public bool Atiende(string texto) { return texto == "✅ Confirmar pedido" || texto == "/confirmar"; }
        public string Ejecutar(MensajeEntrante mensaje)
        {
            return MensajesBot.PedidosRegistrados(pedidos.ConfirmarSeleccion(mensaje.ChatId, mensaje.NombreCliente));
        }
    }
}
