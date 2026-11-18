using System;
using Entidades;

namespace Logica.Comandos
{
    public class ComandoEstado : ComandoConPrefijo
    {
        private readonly PedidoServicio pedidos;

        public ComandoEstado(PedidoServicio pedidos) : base("/estado")
        {
            this.pedidos = pedidos;
        }

        public override string Ejecutar(MensajeEntrante mensaje)
        {
            return MensajesBot.Estado(pedidos.ObtenerPendientesDe(mensaje.ChatId), DateTime.Now);
        }
    }
}
