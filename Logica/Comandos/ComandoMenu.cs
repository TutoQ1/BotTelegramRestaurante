using Entidades;

namespace Logica.Comandos
{
    public class ComandoMenu : ComandoConPrefijo
    {
        private readonly MenuRestaurante menu;

        public ComandoMenu(MenuRestaurante menu) : base("/menu")
        {
            this.menu = menu;
        }

        public override string Ejecutar(MensajeEntrante mensaje)
        {
            return MensajesBot.Menu(menu);
        }
    }
}
