using Entidades;
namespace Logica.Comandos
{
    public class ComandoCancelar : ComandoConPrefijo
    {
        public ComandoCancelar(PedidoServicio pedidos) : base("/cancelar") { }
        public override string Ejecutar(MensajeEntrante mensaje) { return MensajesBot.OpcionesCancelacion(); }
    }
}
