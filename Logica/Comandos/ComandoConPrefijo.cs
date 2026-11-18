using System;
using Entidades;

namespace Logica.Comandos
{
    /// <summary>Base de los comandos que empiezan por una palabra, como /menu o /estado.</summary>
    public abstract class ComandoConPrefijo : IComandoBot
    {
        private readonly string prefijo;

        protected ComandoConPrefijo(string prefijo)
        {
            this.prefijo = prefijo;
        }

        public bool Atiende(string texto)
        {
            return texto.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase);
        }

        public abstract string Ejecutar(MensajeEntrante mensaje);
    }
}
