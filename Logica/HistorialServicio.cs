using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;

namespace Logica
{
    public class HistorialServicio
    {
        private readonly PedidoServicio pedidos;

        public HistorialServicio(PedidoServicio pedidos)
        {
            this.pedidos = pedidos;
        }

        /// <summary>Pedidos filtrados por nombre de cliente y estado, del más reciente al más antiguo.</summary>
        public List<Pedido> Consultar(string cliente, FiltroEstado estado)
        {
            string filtro = (cliente ?? string.Empty).Trim();

            return pedidos.ObtenerTodos()
                .Where(p => filtro.Length == 0
                         || p.NombreCliente.IndexOf(filtro, StringComparison.CurrentCultureIgnoreCase) >= 0)
                .Where(p => p.Cumple(estado))
                .OrderByDescending(p => p.HoraPedido)
                .ToList();
        }

        public ResumenHistorial ObtenerResumen()
        {
            return new ResumenHistorial(pedidos.ObtenerTodos());
        }
    }
}
