namespace Entidades
{
    /// <summary>
    /// Tipos de aviso que el bot guarda en la cola de notificaciones.
    /// Deben coincidir con el check de la tabla notificaciones.
    /// </summary>
    public static class TipoNotificacion
    {
        public const string PedidoListo = "pedido_listo";
        public const string PedidoCancelado = "pedido_cancelado";
    }
}
