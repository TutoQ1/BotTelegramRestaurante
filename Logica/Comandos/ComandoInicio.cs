using Entidades;

namespace Logica.Comandos
{
    public class ComandoInicio : ComandoConPrefijo
    {
        private readonly MenuRestaurante menu;

        public ComandoInicio(MenuRestaurante menu) : base("/start")
        {
            this.menu = menu;
        }

        public override string Ejecutar(MensajeEntrante mensaje)
        {
            return MensajesBot.Bienvenida(mensaje.NombreCliente, menu);
        }
    }
}
