using System.Collections.Generic;
using System.Linq;
using Entidades;

namespace Logica
{
    /// <summary>Estadísticas de todos los pedidos. Calcula sus valores a partir de los datos que recibe.</summary>
    public class ResumenHistorial
    {
        public ResumenHistorial(IReadOnlyCollection<Pedido> pedidos)
        {
            Total = pedidos.Count;
            EnPreparacion = pedidos.Count(p => p.EstaPendiente);
            Listos = pedidos.Count(p => p.Notificado);
            Cancelados = pedidos.Count(p => p.Cancelado);

            // Clientes y plato más pedido se calculan solo con los pedidos que no se cancelaron.
            List<Pedido> validos = pedidos.Where(p => !p.Cancelado).ToList();
            ClientesDistintos = validos.Select(p => p.NombreCliente).Distinct().Count();

            IGrouping<string, Pedido> masPedido = validos
                .GroupBy(p => p.NombrePlato)
                .OrderByDescending(grupo => grupo.Count())
                .FirstOrDefault();

            PlatoMasPedido = masPedido == null ? null : masPedido.Key;
            CantidadPlatoMasPedido = masPedido == null ? 0 : masPedido.Count();
        }

        public int Total { get; }
        public int EnPreparacion { get; }
        public int Listos { get; }
        public int Cancelados { get; }
        public int ClientesDistintos { get; }

        /// <summary>Null cuando todavía no hay pedidos.</summary>
        public string PlatoMasPedido { get; }
        public int CantidadPlatoMasPedido { get; }
    }
}
