using Entidades;
namespace Logica.Comandos
{
    public class ComandoCancelarSeleccion : IComandoBot
    {
        private readonly PedidoServicio pedidos;
        public ComandoCancelarSeleccion(PedidoServicio pedidos) { this.pedidos = pedidos; }
        public bool Atiende(string texto) { return texto == "↩️ Cancelar selección"; }
        public string Ejecutar(MensajeEntrante mensaje)
        {
            pedidos.LimpiarSeleccion(mensaje.ChatId);
            return "Selección cancelada. No se registró ningún pedido. 🍽 Puedes volver al menú.";
        }
    }
}
