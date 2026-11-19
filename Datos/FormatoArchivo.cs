using System;
using System.IO;

namespace Datos
{
    /// <summary>Reglas comunes de los archivos planos (campos separados por barra vertical).</summary>
    internal static class FormatoArchivo
    {
        public const char Separador = '|';

        public static string RutaEnCarpetaDeLaAplicacion(string nombreArchivo)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nombreArchivo);
        }

        public static string Limpiar(string texto)
        {
            return (texto ?? string.Empty)
                .Replace(Separador, ' ')
                .Replace('\r', ' ')
                .Replace('\n', ' ');
        }
    }
}
