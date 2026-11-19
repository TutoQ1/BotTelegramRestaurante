using Entidades;
namespace Logica.Comandos
{
    public class ComandoCancelarUno : IComandoBot
    {
        private readonly PedidoServicio pedidos;
        public ComandoCancelarUno(PedidoServicio pedidos) { this.pedidos = pedidos; }
        public bool Atiende(string texto) { return texto == "❌ Cancelar uno"; }
        public string Ejecutar(MensajeEntrante mensaje) { return MensajesBot.PedidosParaCancelar(pedidos.ObtenerPendientesDe(mensaje.ChatId)); }
    }
}
