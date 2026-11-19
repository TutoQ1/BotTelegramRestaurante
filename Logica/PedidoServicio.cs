using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;
using Entidades.Contratos;

namespace Logica
{
    /// <summary>
    /// Caché en memoria de la tabla pedidos. Todas las operaciones están protegidas
    /// con un candado porque las usan el panel, la recepción de mensajes y el aviso
    /// de pedidos listos. Cada cambio se escribe en la base dentro del mismo candado,
    /// así la caché y PostgreSQL nunca se separan a propósito.
    /// </summary>
    public class PedidoServicio
    {
        private readonly object candado = new object();
        private readonly IPedidoRepositorio repositorio;
        private readonly List<Pedido> pedidos;

        public PedidoServicio(IPedidoRepositorio repositorio)
        {
            this.repositorio = repositorio;
            pedidos = repositorio.Cargar();
        }

        public Pedido Registrar(long chatId, string nombreCliente, Plato plato)
        {
            lock (candado)
            {
                Pedido nuevo = Pedido.Crear(0, chatId, nombreCliente, plato, DateTime.Now);
                Pedido guardado = repositorio.Insertar(nuevo);
                pedidos.Add(guardado);
                return guardado;
            }
        }

        public List<Pedido> ObtenerTodos()
        {
            lock (candado)
            {
                return pedidos.ToList();
            }
        }

        public Pedido ObtenerPorId(int id)
        {
            lock (candado)
            {
                return pedidos.FirstOrDefault(p => p.Id == id);
            }
        }

        /// <summary>
        /// Pendientes primero y, dentro de cada grupo, por hora de entrega.
        /// Los cancelados no se muestran en el panel: solo aparecen en el historial.
        /// </summary>
        public List<Pedido> ObtenerParaPanel()
        {
            return ObtenerTodos().Where(p => !p.Cancelado).OrderBy(p => p.Notificado).ThenBy(p => p.HoraListo).ToList();
        }

        public List<Pedido> ObtenerPendientesDe(long chatId)
        {
            lock (candado)
            {
                return pedidos.Where(p => p.ChatId == chatId && p.EstaPendiente).ToList();
            }
        }

        public List<Pedido> ObtenerPorAvisar(DateTime ahora)
        {
            lock (candado)
            {
                return pedidos.Where(p => p.DebeAvisarse(ahora)).ToList();
            }
        }

        public bool MarcarListo(int id)
        {
            lock (candado)
            {
                Pedido pedido = BuscarPendiente(id);
                if (pedido == null)
                    return false;

                pedido.MarcarComoListo(DateTime.Now);
                repositorio.Actualizar(pedido);
                return true;
            }
        }

        public void MarcarNotificado(Pedido pedido)
        {
            lock (candado)
            {
                pedido.MarcarComoNotificado();
                repositorio.Actualizar(pedido);
            }
        }

        /// <summary>
        /// Cancela un pedido pendiente. Devuelve null si no existe o ya fue avisado.
        /// El pedido no se borra: queda guardado como cancelado y se ve en el historial.
        /// </summary>
        public Pedido CancelarPendiente(int id)
        {
            lock (candado)
            {
                Pedido pedido = BuscarPendiente(id);
                if (pedido == null)
                    return null;

                pedido.MarcarComoCancelado();
                repositorio.Actualizar(pedido);
                return pedido;
            }
        }

        public int CancelarPendientesDe(long chatId)
        {
            lock (candado)
            {
                List<Pedido> pendientes = pedidos.Where(p => p.ChatId == chatId && p.EstaPendiente).ToList();
                foreach (Pedido pedido in pendientes)
                {
                    pedido.MarcarComoCancelado();
                    repositorio.Actualizar(pedido);
                }

                return pendientes.Count;
            }
        }

        private readonly Dictionary<long, List<Plato>> selecciones = new Dictionary<long, List<Plato>>();

        public void PrepararSeleccion(long chatId, Plato plato)
        {
            lock (candado)
            {
                List<Plato> lista;
                if (!selecciones.TryGetValue(chatId, out lista))
                {
                    lista = new List<Plato>();
                    selecciones[chatId] = lista;
                }
                lista.Add(plato);
            }
        }

        public List<Plato> ObtenerSelecciones(long chatId)
        {
            lock (candado)
            {
                List<Plato> lista;
                return selecciones.TryGetValue(chatId, out lista) ? lista.ToList() : new List<Plato>();
            }
        }

        public Plato ObtenerSeleccion(long chatId)
        {
            lock (candado)
            {
                List<Plato> lista;
                return selecciones.TryGetValue(chatId, out lista) && lista.Count > 0 ? lista[lista.Count - 1] : null;
            }
        }

        public void LimpiarSeleccion(long chatId)
        {
            lock (candado) { selecciones.Remove(chatId); }
        }

        // Como Registrar, pero para todos los platos elegidos. La base asigna el Id.
        public List<Pedido> ConfirmarSeleccion(long chatId, string nombreCliente)
        {
            lock (candado)
            {
                List<Plato> platos;
                if (!selecciones.TryGetValue(chatId, out platos) || platos.Count == 0)
                    return new List<Pedido>();

                List<Pedido> nuevos = new List<Pedido>();
                DateTime ahora = DateTime.Now;
                foreach (Plato plato in platos)
                {
                    Pedido guardado = repositorio.Insertar(Pedido.Crear(0, chatId, nombreCliente, plato, ahora));
                    pedidos.Add(guardado);
                    nuevos.Add(guardado);
                }
                selecciones.Remove(chatId);
                return nuevos;
            }
        }

        public Pedido CancelarPendienteDeCliente(long chatId, int id)
        {
            lock (candado)
            {
                Pedido pedido = pedidos.FirstOrDefault(p => p.Id == id && p.ChatId == chatId && p.EstaPendiente);
                if (pedido == null) return null;

                pedido.MarcarComoCancelado();
                repositorio.Actualizar(pedido);
                return pedido;
            }
        }

        private Pedido BuscarPendiente(int id)
        {
            return pedidos.FirstOrDefault(p => p.Id == id && p.EstaPendiente);
        }
    }
}
