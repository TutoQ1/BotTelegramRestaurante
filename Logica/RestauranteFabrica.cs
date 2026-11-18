using System;
using Datos;
using Entidades.Contratos;
using Logica.Comandos;

namespace Logica
{
    /// <summary>
    /// Único lugar donde se arma el sistema (GRASP Creador). Para agregar un comando nuevo
    /// del bot basta con crear su clase y añadirla a la lista de comandos de abajo.
    /// Las implementaciones concretas de persistencia son las de PostgreSQL.
    /// </summary>
    public static class RestauranteFabrica
    {
        public static RestauranteControlador Crear()
        {
            try
            {
                return Armar();
            }
            catch (Exception ex)
            {
                throw new RestauranteException("Error al cargar datos: " + ex.Message, ex);
            }
        }

        private static RestauranteControlador Armar()
        {
            Bitacora bitacora = new Bitacora();

            MenuRestaurante menu = MenuRestaurante.DesdeRepositorio(new PlatoRepositorioPostgres());
            PedidoServicio pedidos = new PedidoServicio(new PedidoRepositorioPostgres());
            ClienteServicio clientes = new ClienteServicio(new ClienteRepositorioPostgres());
            INotificacionRepositorio notificaciones = new NotificacionRepositorioPostgres();
            ICanalMensajeria canal = new TelegramCanal();

            // El orden importa: ComandoNoEntendido va al final porque atiende cualquier texto.
            ProcesadorMensajes procesador = new ProcesadorMensajes(new IComandoBot[]
            {
                new ComandoPedirPlato(menu, pedidos, bitacora),
                new ComandoInicio(menu),
                new ComandoMenu(menu),
                new ComandoEstado(pedidos),
                new ComandoCancelar(pedidos),
                new ComandoNoEntendido(menu)
            });

            ReceptorMensajes receptor = new ReceptorMensajes(canal, procesador, clientes, bitacora);
            NotificadorPedidos notificador = new NotificadorPedidos(canal, pedidos, notificaciones, bitacora);
            BotServicio bot = new BotServicio(canal, receptor, notificador, bitacora);

            return new RestauranteControlador(bot, pedidos, new HistorialServicio(pedidos), notificaciones, bitacora);
        }
    }
}
