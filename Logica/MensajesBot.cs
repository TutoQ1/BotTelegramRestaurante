using System;
using System.Collections.Generic;
using System.Text;
using Entidades;

namespace Logica
{
    /// <summary>
    /// Todos los textos que el bot le envía al cliente, en un solo lugar (GRASP Fabricación Pura):
    /// si cambia la redacción, solo se toca esta clase.
    /// </summary>
    public static class MensajesBot
    {
        public static string Bienvenida(string nombreCliente, MenuRestaurante menu)
        {
            return "¡Hola " + nombreCliente + "! Bienvenido al restaurante.\n\n" + Menu(menu);
        }

        public static string Menu(MenuRestaurante menu)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("🍽 MENÚ - responde con la letra de tu opción:");
            foreach (Plato plato in menu.Platos)
                sb.AppendLine(plato.Opcion + ") " + plato.Nombre + " - " + plato.Minutos + " min");
            sb.AppendLine();
            sb.AppendLine("Comandos: /menu  /estado  /cancelar");
            return sb.ToString();
        }

        public static string NoEntendido(MenuRestaurante menu)
        {
            return "No entendí tu mensaje 😅\n\n" + Menu(menu);
        }

        public static string PedidoRegistrado(Pedido pedido)
        {
            return "✅ Pedido #" + pedido.Id + " registrado\n" +
                   "Plato: " + pedido.NombrePlato + "\n" +
                   "Tiempo estimado: " + pedido.Minutos + " minutos\n" +
                   "Hora estimada: " + pedido.HoraListo.ToString("HH:mm") + "\n\n" +
                   "Te avisaré cuando esté listo.";
        }

        public static string Estado(IReadOnlyCollection<Pedido> pendientes, DateTime ahora)
        {
            if (pendientes.Count == 0)
                return "No tienes pedidos pendientes. Escribe /menu para pedir.";

            StringBuilder sb = new StringBuilder("Tus pedidos en preparación:\n");
            foreach (Pedido pedido in pendientes)
                sb.AppendLine("#" + pedido.Id + " " + pedido.NombrePlato +
                              " - faltan aprox. " + pedido.MinutosRestantes(ahora) + " min");
            return sb.ToString();
        }

        public static string CancelacionDelCliente(int cantidad)
        {
            return cantidad > 0
                ? "Se cancelaron " + cantidad + " pedido(s) pendiente(s)."
                : "No tienes pedidos pendientes.";
        }

        public static string PedidoListo(Pedido pedido)
        {
            return "🔔 ¡" + pedido.NombreCliente + ", tu pedido #" + pedido.Id + " (" + pedido.NombrePlato + ") está listo!\n" +
                   "Puedes pasar a recogerlo. ¡Buen provecho!";
        }

        public static string PedidoCanceladoPorElRestaurante(Pedido pedido)
        {
            return "❌ Lo sentimos, tu pedido #" + pedido.Id + " (" + pedido.NombrePlato + ") fue cancelado por el restaurante.";
        }
    }
}
