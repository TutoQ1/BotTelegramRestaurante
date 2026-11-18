using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;
using Logica.Comandos;

namespace Logica
{
    /// <summary>
    /// Elige el comando que atiende cada mensaje. No contiene if/else por comando:
    /// recorre la lista en orden y usa el primero que lo atienda (GRASP Polimorfismo).
    /// </summary>
    public class ProcesadorMensajes
    {
        private readonly List<IComandoBot> comandos;

        public ProcesadorMensajes(IEnumerable<IComandoBot> comandos)
        {
            this.comandos = comandos.ToList();
        }

        public string Procesar(MensajeEntrante mensaje)
        {
            IComandoBot comando = comandos.FirstOrDefault(c => c.Atiende(mensaje.Texto));
            if (comando == null)
                throw new InvalidOperationException("Ningún comando atiende el mensaje. Falta registrar ComandoNoEntendido.");

            return comando.Ejecutar(mensaje);
        }
    }
}
