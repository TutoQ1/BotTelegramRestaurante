using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Entidades;
using Entidades.Contratos;

namespace Datos
{
    /// <summary>
    /// Lee y escribe pedidos.txt. Formato por línea:
    /// Id|ChatId|Cliente|Opcion|Plato|Minutos|HoraPedido|HoraListo|Notificado(1/0)|Cancelado(1/0)
    /// Las horas se guardan como 2026-11-08 12:00:00.
    /// Los archivos anteriores también se leen: horas en ticks y/o 9 campos (se toman como no cancelados).
    /// Alternativa sin base de datos: la aplicación usa PedidoRepositorioPostgres.
    /// Se conserva para pruebas y para comparar comportamientos.
    /// </summary>
    public class PedidoRepositorio : IPedidoRepositorio
    {
        private const int CantidadCampos = 9;   // mínimo de campos; el décimo (Cancelado) es opcional
        private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";
        private readonly string ruta;

        public PedidoRepositorio()
            : this(FormatoArchivo.RutaEnCarpetaDeLaAplicacion("pedidos.txt"))
        {
        }

        public PedidoRepositorio(string ruta)
        {
            this.ruta = ruta;
        }

        public List<Pedido> Cargar()
        {
            List<Pedido> pedidos = new List<Pedido>();
            if (!File.Exists(ruta))
                return pedidos;

            foreach (string linea in File.ReadAllLines(ruta))
            {
                Pedido pedido;
                if (TryLeer(linea, out pedido))
                    pedidos.Add(pedido);
            }
            return pedidos;
        }

        public void Guardar(IEnumerable<Pedido> pedidos)
        {
            File.WriteAllLines(ruta, pedidos.Select(Serializar));
        }

        public Pedido Insertar(Pedido pedido)
        {
            List<Pedido> pedidos = Cargar();
            int id = pedidos.Count == 0 ? 1 : pedidos.Max(p => p.Id) + 1;

            Pedido guardado = new Pedido(id, pedido.ChatId, pedido.NombreCliente, pedido.Opcion,
                                         pedido.NombrePlato, pedido.Minutos, pedido.HoraPedido,
                                         pedido.HoraListo, pedido.Notificado, pedido.Cancelado);
            pedidos.Add(guardado);
            Guardar(pedidos);
            return guardado;
        }

        public void Actualizar(Pedido pedido)
        {
            List<Pedido> pedidos = Cargar();
            int indice = pedidos.FindIndex(p => p.Id == pedido.Id);
            if (indice < 0)
                throw new InvalidOperationException("No existe el pedido #" + pedido.Id + " en pedidos.txt.");

            pedidos[indice] = pedido;
            Guardar(pedidos);
        }

        private static string Serializar(Pedido p)
        {
            return string.Join(FormatoArchivo.Separador.ToString(), new[]
            {
                p.Id.ToString(),
                p.ChatId.ToString(),
                FormatoArchivo.Limpiar(p.NombreCliente),
                p.Opcion,
                FormatoArchivo.Limpiar(p.NombrePlato),
                p.Minutos.ToString(),
                p.HoraPedido.ToString(FormatoFecha, CultureInfo.InvariantCulture),
                p.HoraListo.ToString(FormatoFecha, CultureInfo.InvariantCulture),
                p.Notificado ? "1" : "0",
                p.Cancelado ? "1" : "0"
            });
        }

        // Las líneas dañadas se omiten para que un registro malo no impida abrir el resto.
        private static bool TryLeer(string linea, out Pedido pedido)
        {
            pedido = null;
            string[] campos = linea.Split(FormatoArchivo.Separador);
            if (campos.Length < CantidadCampos)
                return false;

            int id, minutos;
            long chatId;
            DateTime horaPedido, horaListo;
            if (!int.TryParse(campos[0], out id)
                || !long.TryParse(campos[1], out chatId)
                || !int.TryParse(campos[5], out minutos)
                || !TryLeerFecha(campos[6], out horaPedido)
                || !TryLeerFecha(campos[7], out horaListo))
                return false;

            pedido = new Pedido(id, chatId, campos[2], campos[3], campos[4], minutos,
                                horaPedido, horaListo, campos[8] == "1",
                                campos.Length > CantidadCampos && campos[9] == "1");
            return true;
        }

        // Acepta el formato actual (2026-11-08 12:00:00) y el anterior en ticks, para no perder pedidos viejos.
        private static bool TryLeerFecha(string texto, out DateTime fecha)
        {
            if (DateTime.TryParseExact(texto, FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                return true;

            long ticks;
            if (long.TryParse(texto, out ticks) && ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks)
            {
                fecha = new DateTime(ticks);
                return true;
            }

            fecha = default(DateTime);
            return false;
        }
    }
}
