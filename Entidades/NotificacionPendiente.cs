namespace Entidades
{
    /// <summary>
    /// Aviso ya guardado en la base que todavía no se pudo enviar al cliente.
    /// El texto se guarda ya armado para que un reintento no dependa del estado
    /// del pedido en ese momento.
    /// </summary>
    public class NotificacionPendiente
    {
        public NotificacionPendiente(long id, long pedidoId, string tipo, long chatId, string texto, int intentos)
        {
            Id = id;
            PedidoId = pedidoId;
            Tipo = tipo;
            ChatId = chatId;
            Texto = texto;
            Intentos = intentos;
        }

        public long Id { get; }
        public long PedidoId { get; }
        public string Tipo { get; }
        public long ChatId { get; }
        public string Texto { get; }
        public int Intentos { get; }
    }
}
