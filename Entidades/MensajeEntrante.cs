namespace Entidades
{
    /// <summary>Mensaje de texto que un cliente le escribió al bot.</summary>
    public class MensajeEntrante
    {
        private const string NombreClientePorDefecto = "Cliente";

        public MensajeEntrante(long chatId, string nombreCliente, string texto)
        {
            ChatId = chatId;
            NombreCliente = string.IsNullOrWhiteSpace(nombreCliente) ? NombreClientePorDefecto : nombreCliente;
            Texto = (texto ?? string.Empty).Trim();
        }

        public long ChatId { get; }
        public string NombreCliente { get; }
        public string Texto { get; }
    }
}
