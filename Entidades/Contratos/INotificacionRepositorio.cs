using System;
using System.Collections.Generic;

namespace Entidades.Contratos
{
    /// <summary>
    /// Bandeja de salida de avisos al cliente. Vive en la base de datos para que
    /// un fallo de red con Telegram no haga perder el aviso: se reintenta después.
    /// </summary>
    public interface INotificacionRepositorio
    {
        /// <summary>
        /// Encola un aviso. Es idempotente: si ya existe uno igual (mismo pedido y
        /// tipo) no hace nada y devuelve false.
        /// </summary>
        bool Encolar(long pedidoId, string tipo, long chatId, string texto);

        /// <summary>Avisos pendientes cuyo momento de reintento ya llegó.</summary>
        List<NotificacionPendiente> ObtenerPendientes(DateTime ahora, int limite);

        void MarcarEnviada(long id);

        /// <summary>Suma un intento fallido y programa el siguiente con espera creciente.</summary>
        void MarcarFallo(long id, string error, int intentosMaximos);

        /// <summary>Ya no tiene sentido enviarlo (por ejemplo, el pedido se canceló).</summary>
        void MarcarOmitida(long id, string motivo);
    }
}
