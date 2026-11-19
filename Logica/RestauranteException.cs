using System;

namespace Logica
{
    /// <summary>Error controlado: su mensaje es apto para mostrarse al usuario.</summary>
    public class RestauranteException : Exception
    {
        public RestauranteException(string mensaje) : base(mensaje)
        {
        }

        public RestauranteException(string mensaje, Exception interna) : base(mensaje, interna)
        {
        }
    }
}
