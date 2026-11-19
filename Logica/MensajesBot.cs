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
            return "👋 ¡Hola " + nombreCliente + "! Bienvenido a Restaurante Supremo.\n\n" + Menu(menu);
        }

        public static string Menu(MenuRestaurante menu)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("🍽 MENÚ\nSelecciona el plato que deseas:");
            foreach (Plato plato in menu.Platos)
                sb.AppendLine("🔹 " + plato.Opcion + ") " + plato.Nombre + " - " + plato.Minutos + " min");
            sb.AppendLine();
            sb.AppendLine("\nPulsa A, B, C o D para agregar platos.\nPuedes elegir varios y confirmarlos todos juntos.");
            return sb.ToString();
        }

        public static string NoEntendido(MenuRestaurante menu)
        {
            return "No entendí esa opción 😅\nUsa los botones del menú para continuar.\n\n" + Menu(menu);
        }

        public static string ResumenPrevio(IReadOnlyCollection<Plato> platos)
        {
            StringBuilder sb = new StringBuilder("🛒 RESUMEN DE TU PEDIDO\n\n");
            int numero = 1;
            foreach (Plato plato in platos)
            {
                sb.AppendLine(numero + ". 🍽 " + plato.Nombre + " - " + plato.Minutos + " min");
                numero++;
            }
            sb.AppendLine();
            sb.AppendLine("Total de platos: " + platos.Count);
            sb.AppendLine("\nPuedes elegir más platos (A, B, C o D) antes de confirmar.");
            sb.AppendLine("Pulsa ✅ Confirmar pedido cuando termines.");
            sb.Append("Aún no se ha registrado ningún pedido.");
            return sb.ToString();
        }

        public static string OpcionesCancelacion()
        {
            return "❌ CANCELAR PEDIDOS\n\n¿Deseas cancelar un solo pedido o todos tus pedidos pendientes?";
        }

        public static string PedidosParaCancelar(IReadOnlyCollection<Pedido> pendientes)
        {
            if (pendientes.Count == 0) return "📭 No tienes pedidos pendientes para cancelar.";
            StringBuilder sb = new StringBuilder("Escribe el número del pedido que deseas cancelar, por ejemplo: #12\n\n");
            foreach (Pedido pedido in pendientes)
                sb.AppendLine("#" + pedido.Id + " - " + pedido.NombrePlato);
            return sb.ToString();
        }

        public static string PedidosRegistrados(IReadOnlyCollection<Pedido> pedidos)
        {
            if (pedidos.Count == 0) return "No tienes platos seleccionados. 🍽 Pulsa Menú para elegir.";
            StringBuilder sb = new StringBuilder("✅ ¡Tus pedidos fueron registrados!\n\n");
            foreach (Pedido pedido in pedidos)
                sb.AppendLine("Pedido #" + pedido.Id + " - " + pedido.NombrePlato + " (" + pedido.Minutos + " min)");
            sb.AppendLine("\nTe avisaré cuando cada pedido esté listo.");
            return sb.ToString();
        }

        public static string Estado(IReadOnlyCollection<Pedido> pendientes, DateTime ahora)
        {
            if (pendientes.Count == 0)
                return "📭 No tienes pedidos pendientes.\nPulsa 🍽 Menú para realizar un pedido.";

            StringBuilder sb = new StringBuilder("📦 Tus pedidos en preparación:\n");
            foreach (Pedido pedido in pendientes)
                sb.AppendLine("#" + pedido.Id + " " + pedido.NombrePlato +
                              " - faltan aprox. " + pedido.MinutosRestantes(ahora) + " min");
            return sb.ToString();
        }

        public static string CancelacionDelCliente(int cantidad)
        {
            return cantidad > 0
                ? "Se cancelaron " + cantidad + " pedido(s) pendiente(s)."
                : "No tienes pedidos pendientes que se puedan cancelar.";
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
