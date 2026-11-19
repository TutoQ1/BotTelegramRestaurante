using Entidades;
namespace Logica.Comandos
{
    public class ComandoResumen : IComandoBot
    {
        private readonly PedidoServicio pedidos;
        public ComandoResumen(PedidoServicio pedidos) { this.pedidos = pedidos; }
        public bool Atiende(string texto) { return texto == "🛒 Resumen"; }
        public string Ejecutar(MensajeEntrante mensaje)
        {
            var platos = pedidos.ObtenerSelecciones(mensaje.ChatId);
            return platos.Count == 0 ? "📭 No tienes una selección pendiente. Pulsa 🍽 Menú para elegir." : MensajesBot.ResumenPrevio(platos);
        }
    }
}
