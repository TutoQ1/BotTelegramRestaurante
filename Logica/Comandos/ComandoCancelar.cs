using Entidades;

namespace Logica.Comandos
{
    public class ComandoCancelar : ComandoConPrefijo
    {
        private readonly PedidoServicio pedidos;

        public ComandoCancelar(PedidoServicio pedidos) : base("/cancelar")
        {
            this.pedidos = pedidos;
        }

        public override string Ejecutar(MensajeEntrante mensaje)
        {
            int cancelados = pedidos.CancelarPendientesDe(mensaje.ChatId);
            return MensajesBot.CancelacionDelCliente(cancelados);
        }
    }
}
