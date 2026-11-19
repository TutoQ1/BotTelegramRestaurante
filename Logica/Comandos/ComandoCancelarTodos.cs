using Entidades;
namespace Logica.Comandos
{
    public class ComandoCancelarTodos : IComandoBot
    {
        private readonly PedidoServicio pedidos;
        public ComandoCancelarTodos(PedidoServicio pedidos) { this.pedidos = pedidos; }
        public bool Atiende(string texto) { return texto == "❌ Cancelar todos"; }
        public string Ejecutar(MensajeEntrante mensaje)
        {
            int cantidad = pedidos.CancelarPendientesDe(mensaje.ChatId);
            return cantidad > 0 ? "❌ Se cancelaron " + cantidad + " pedido(s) pendiente(s)." : "📭 No tienes pedidos pendientes que se puedan cancelar.";
        }
    }
}
