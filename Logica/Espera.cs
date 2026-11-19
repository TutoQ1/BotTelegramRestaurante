using System;
using System.Threading;
using System.Threading.Tasks;

namespace Logica
{
    internal static class Espera
    {
        /// <summary>Espera el tiempo indicado. Devuelve false si se canceló antes de terminar.</summary>
        public static async Task<bool> PorAsync(int milisegundos, CancellationToken cancelacion)
        {
            try
            {
                await Task.Delay(milisegundos, cancelacion);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}
