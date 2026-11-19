using Entidades;

namespace BotTelegram
{
    /// <summary>Textos de estado que muestran las tablas de ambas ventanas.</summary>
    internal static class Textos
    {
        public const string EnPreparacion = "En preparación";
        public const string ListoAvisado = "Listo (avisado)";
        public const string Cancelado = "Cancelado";
        public const string Todos = "Todos";

        /// <summary>Texto que se muestra en la tabla según el estado del pedido.</summary>
        public static string DeEstado(Pedido pedido)
        {
            if (pedido.Cancelado)
                return Cancelado;

            return pedido.Notificado ? ListoAvisado : EnPreparacion;
        }
    }
}
