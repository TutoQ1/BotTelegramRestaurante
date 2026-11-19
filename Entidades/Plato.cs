namespace Entidades
{
    public class Plato
    {
        public Plato(string opcion, string nombre, int minutos)
        {
            Opcion = opcion;
            Nombre = nombre;
            Minutos = minutos;
        }

        public string Opcion { get; }
        public string Nombre { get; }
        public int Minutos { get; }
    }
}
