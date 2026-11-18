using System;

namespace Entidades
{
    /// <summary>
    /// Pedido de un cliente. Es el experto en sus propios datos (GRASP Experto en Información):
    /// sabe cuándo debe avisarse, cuánto falta y si cumple un filtro de estado.
    /// </summary>
    public class Pedido
    {
        public Pedido(int id, long chatId, string nombreCliente, string opcion, string nombrePlato,
                      int minutos, DateTime horaPedido, DateTime horaListo, bool notificado, bool cancelado = false)
        {
            Id = id;
            ChatId = chatId;
            NombreCliente = nombreCliente;
            Opcion = opcion;
            NombrePlato = nombrePlato;
            Minutos = minutos;
            HoraPedido = horaPedido;
            HoraListo = horaListo;
            Notificado = notificado;
            Cancelado = cancelado;
        }

        /// <summary>Crea un pedido nuevo a partir de un plato del menú (GRASP Creador).</summary>
        public static Pedido Crear(int id, long chatId, string nombreCliente, Plato plato, DateTime ahora)
        {
            return new Pedido(id, chatId, nombreCliente, plato.Opcion, plato.Nombre,
                              plato.Minutos, ahora, ahora.AddMinutes(plato.Minutos), false);
        }

        public int Id { get; }
        public long ChatId { get; }
        public string NombreCliente { get; }
        public string Opcion { get; }
        public string NombrePlato { get; }
        public int Minutos { get; }
        public DateTime HoraPedido { get; }
        public DateTime HoraListo { get; private set; }
        public bool Notificado { get; private set; }
        public bool Cancelado { get; private set; }

        public bool EstaPendiente
        {
            get { return !Notificado && !Cancelado; }
        }

        public bool DebeAvisarse(DateTime ahora)
        {
            return EstaPendiente && ahora >= HoraListo;
        }

        public TimeSpan TiempoRestante(DateTime ahora)
        {
            TimeSpan resta = HoraListo - ahora;
            return resta < TimeSpan.Zero ? TimeSpan.Zero : resta;
        }

        public int MinutosRestantes(DateTime ahora)
        {
            return (int)Math.Ceiling(TiempoRestante(ahora).TotalMinutes);
        }

        public bool Cumple(FiltroEstado filtro)
        {
            return filtro == FiltroEstado.Todos
                || (filtro == FiltroEstado.EnPreparacion && EstaPendiente)
                || (filtro == FiltroEstado.Listos && Notificado)
                || (filtro == FiltroEstado.Cancelados && Cancelado);
        }

        public void MarcarComoListo(DateTime ahora)
        {
            HoraListo = ahora;
        }

        public void MarcarComoNotificado()
        {
            if (Cancelado)
                return;     // un pedido cancelado nunca pasa a "listo"

            Notificado = true;
        }

        /// <summary>Marca el pedido como cancelado. Si ya se avisó al cliente, no se puede cancelar.</summary>
        public void MarcarComoCancelado()
        {
            if (Notificado)
                return;

            Cancelado = true;
        }
    }
}
