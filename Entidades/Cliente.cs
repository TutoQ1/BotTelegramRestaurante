namespace Entidades
{
    public class Cliente
    {
        public Cliente(long chatId, string nombre)
        {
            ChatId = chatId;
            Nombre = nombre;
        }

        public long ChatId { get; }
        public string Nombre { get; }
    }
}
