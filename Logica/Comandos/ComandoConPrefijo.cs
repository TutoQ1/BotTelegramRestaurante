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
            if (texto.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase)) return true;
            if (prefijo == "/menu") return texto == "🍽 Menú" || texto == "↩️ Volver al menú";
            if (prefijo == "/estado") return texto == "📦 Seguimiento";
            if (prefijo == "/cancelar") return texto == "❌ Cancelar pedido";
            return false;
        }

        public abstract string Ejecutar(MensajeEntrante mensaje);
    }
}
