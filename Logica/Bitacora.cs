using System;

namespace Logica
{
    /// <summary>
    /// Registro de eventos del bot. Quien quiera mostrarlos (la GUI) se suscribe a Registrada.
    /// </summary>
    public class Bitacora
    {
        public event Action<string> Registrada;

        public void Registrar(string mensaje)
        {
            Action<string> manejador = Registrada;
            if (manejador != null)
                manejador(mensaje);
        }
    }
}
