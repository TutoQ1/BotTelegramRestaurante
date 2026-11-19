using System;
using Entidades;
namespace Logica.Comandos
{
    public class ComandoCancelarPorId : IComandoBot
    {
        private readonly PedidoServicio pedidos;
        public ComandoCancelarPorId(PedidoServicio pedidos) { this.pedidos = pedidos; }
        public bool Atiende(string texto) { return texto.StartsWith("#", StringComparison.Ordinal) && texto.Length > 1; }
        public string Ejecutar(MensajeEntrante mensaje)
        {
            int id;
            if (!int.TryParse(mensaje.Texto.Substring(1), out id)) return "Escribe el número así: #12";
            Pedido pedido = pedidos.CancelarPendienteDeCliente(mensaje.ChatId, id);
            return pedido == null ? "No se encontró ese pedido pendiente. Revisa el número e inténtalo de nuevo." : "✅ Se canceló tu pedido #" + pedido.Id + " (" + pedido.NombrePlato + ").";
        }
    }
}
