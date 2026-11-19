using System.Collections.Generic;

namespace Entidades.Contratos
{
    public interface IPedidoRepositorio
    {
        List<Pedido> Cargar();

        /// <summary>
        /// Inserta un pedido nuevo y devuelve una copia con el Id que asignó la
        /// base de datos (el pedido recibido llega con Id 0).
        /// </summary>
        Pedido Insertar(Pedido pedido);

        /// <summary>Guarda los cambios de estado: hora de listo, avisado o cancelado.</summary>
        void Actualizar(Pedido pedido);
    }
}
