using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Entidades.Contratos
{
    /// <summary>
    /// Canal por el que el bot habla con los clientes (hoy Telegram).
    /// La lógica de negocio depende de este contrato y no de Telegram.
    /// </summary>
    public interface ICanalMensajeria
    {
        Task<InformacionBot> ConectarAsync(string token, CancellationToken cancelacion);
        Task<IReadOnlyList<MensajeEntrante>> RecibirAsync(CancellationToken cancelacion);
        Task EnviarAsync(long chatId, string texto, CancellationToken cancelacion);
    }
}
