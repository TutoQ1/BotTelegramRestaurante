namespace Entidades
{
    public class InformacionBot
    {
        public InformacionBot(string nombre, string usuario)
        {
            Nombre = nombre;
            Usuario = usuario;
        }

        public string Nombre { get; }
        public string Usuario { get; }
    }
}
