using System;
using System.Windows.Forms;
using Logica;

namespace BotTelegram
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RestauranteControlador controlador;
            try
            {
                controlador = RestauranteFabrica.Crear();
            }
            catch (RestauranteException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo iniciar la aplicación");
                return;
            }

            Application.Run(new Principal(controlador));
        }
    }
}
