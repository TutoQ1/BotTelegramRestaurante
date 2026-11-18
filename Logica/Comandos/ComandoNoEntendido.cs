using Entidades;

namespace Logica.Comandos
{
    /// <summary>Respuesta por defecto. Debe registrarse de último: atiende cualquier texto.</summary>
    public class ComandoNoEntendido : IComandoBot
    {
        private readonly MenuRestaurante menu;

        public ComandoNoEntendido(MenuRestaurante menu)
        {
            this.menu = menu;
        }

        public bool Atiende(string texto)
        {
            return true;
        }

        public string Ejecutar(MensajeEntrante mensaje)
        {
            return MensajesBot.NoEntendido(menu);
        }
    }
}
