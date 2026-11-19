using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace BotTelegram
{
    /// <summary>
    /// Estilo visual compartido por las ventanas. Paleta de 3 colores:
    /// Oscuro (azul carbón), Acento (naranja) y Crema (fondo).
    /// El resto son tonos mezclados de esos mismos tres colores.
    /// </summary>
    internal static class Tema
    {
        // ---- Paleta base (3 colores) ----
        public static readonly Color Oscuro = ColorTranslator.FromHtml("#2B2D42");
        public static readonly Color Acento = ColorTranslator.FromHtml("#20A477");
        public static readonly Color Crema = ColorTranslator.FromHtml("#F6F1EA");

        // ---- Tonos derivados ----
        public static readonly Color Superficie = Mezclar(Crema, Color.White, 0.65);
        public static readonly Color Apagado = Mezclar(Oscuro, Crema, 0.45);
        public static readonly Color Linea = Mezclar(Oscuro, Crema, 0.88);
        public static readonly Color TextoSobreOscuro = Mezclar(Crema, Oscuro, 0.35);
        public static readonly Color AcentoSobreOscuro = Mezclar(Acento, Crema, 0.25);

        // ---- Fuentes ----
        public static readonly Font Normal = new Font("Segoe UI", 10f);
        public static readonly Font Negrita = new Font("Segoe UI", 10f, FontStyle.Bold);
        public static readonly Font Tachada = new Font("Segoe UI", 10f, FontStyle.Strikeout);
        public static readonly Font Titulo = new Font("Segoe UI", 17f, FontStyle.Bold);
        public static readonly Font Pequena = new Font("Segoe UI", 9f);

        /// <summary>Mezcla dos colores: t = 0 devuelve a, t = 1 devuelve b.</summary>
        public static Color Mezclar(Color a, Color b, double t)
        {
            return Color.FromArgb(
                (int)Math.Round(a.R + (b.R - a.R) * t),
                (int)Math.Round(a.G + (b.G - a.G) * t),
                (int)Math.Round(a.B + (b.B - a.B) * t));
        }

        public static void EstilizarVentana(Form f)
        {
            f.BackColor = Crema;
            f.ForeColor = Oscuro;
            f.Font = Normal;
        }

        /// <summary>Barra superior oscura con título y subtítulo.</summary>
        public static Panel Encabezado(string titulo, string subtitulo)
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = Oscuro };
            panel.Controls.Add(new Label
            {
                Text = titulo,
                Left = 24,
                Top = 10,
                AutoSize = true,
                Font = Titulo,
                ForeColor = Crema,
                BackColor = Color.Transparent
            });
            panel.Controls.Add(new Label
            {
                Text = subtitulo,
                Left = 26,
                Top = 47,
                AutoSize = true,
                Font = Pequena,
                ForeColor = TextoSobreOscuro,
                BackColor = Color.Transparent
            });
            return panel;
        }

        public static void EstilizarCampo(Control campo)
        {
            campo.Font = Normal;
            campo.BackColor = Color.White;
            campo.ForeColor = Oscuro;
            var texto = campo as TextBox;
            if (texto != null) texto.BorderStyle = BorderStyle.FixedSingle;
            var combo = campo as ComboBox;
            if (combo != null) combo.FlatStyle = FlatStyle.Flat;
        }

        public static Label Etiqueta(string texto, int left, int top)
        {
            return new Label
            {
                Text = texto,
                Left = left,
                Top = top,
                AutoSize = true,
                Font = Negrita,
                ForeColor = Oscuro,
                BackColor = Color.Transparent
            };
        }

        /// <summary>Botón relleno de color acento (acción principal).</summary>
        public static void BotonPrimario(Button b)
        {
            Base(b);
            Action aplicar = delegate
            {
                if (b.Enabled)
                {
                    b.BackColor = Acento;
                    b.ForeColor = Color.White;
                }
                else
                {
                    b.BackColor = Linea;
                    b.ForeColor = Apagado;
                }
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Mezclar(Acento, Oscuro, 0.2);
            b.FlatAppearance.MouseDownBackColor = Mezclar(Acento, Oscuro, 0.35);
            b.EnabledChanged += delegate { aplicar(); };
            aplicar();
        }

        /// <summary>Botón con borde y fondo transparente en el color indicado (Oscuro o Acento).</summary>
        public static void BotonSecundario(Button b, Color color)
        {
            Base(b);
            Action aplicar = delegate
            {
                b.BackColor = Crema;
                b.ForeColor = b.Enabled ? color : Apagado;
                b.FlatAppearance.BorderColor = b.Enabled ? color : Linea;
            };
            b.FlatAppearance.BorderSize = 2;
            b.FlatAppearance.MouseOverBackColor = Mezclar(Crema, color, 0.12);
            b.FlatAppearance.MouseDownBackColor = Mezclar(Crema, color, 0.22);
            b.EnabledChanged += delegate { aplicar(); };
            aplicar();
        }

        private static void Base(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.UseVisualStyleBackColor = false;
            b.Cursor = Cursors.Hand;
            b.Font = Negrita;
            b.Height = 38;
        }

        /// <summary>Tabla plana: encabezado oscuro, filas alternadas y selección en color acento.</summary>
        public static void EstilizarGrid(DataGridView g)
        {
            g.BorderStyle = BorderStyle.None;
            g.BackgroundColor = Crema;
            g.EnableHeadersVisualStyles = false;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Linea;
            g.RowHeadersVisible = false;
            g.AllowUserToResizeRows = false;
            g.AllowUserToResizeColumns = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 42;
            g.RowTemplate.Height = 38;

            var encabezado = g.ColumnHeadersDefaultCellStyle;
            encabezado.BackColor = Oscuro;
            encabezado.ForeColor = Crema;
            encabezado.SelectionBackColor = Oscuro;
            encabezado.SelectionForeColor = Crema;
            encabezado.Font = Negrita;
            encabezado.Padding = new Padding(10, 0, 10, 0);
            encabezado.Alignment = DataGridViewContentAlignment.MiddleLeft;

            var celda = g.DefaultCellStyle;
            celda.BackColor = Superficie;
            celda.ForeColor = Oscuro;
            celda.SelectionBackColor = Acento;
            celda.SelectionForeColor = Color.White;
            celda.Font = Normal;
            celda.Padding = new Padding(10, 0, 10, 0);

            g.AlternatingRowsDefaultCellStyle.BackColor = Crema;
            g.AlternatingRowsDefaultCellStyle.SelectionBackColor = Acento;
            g.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

            // Evita el parpadeo al refrescar la tabla cada segundo.
            typeof(DataGridView)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(g, true, null);
        }

        /// <summary>Centra una columna (encabezado y celdas).</summary>
        public static void CentrarColumna(DataGridViewColumn col)
        {
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        /// <summary>
        /// Colores de una fila según el estado del pedido:
        /// en preparación = acento y negrita, listo = apagado, cancelado = apagado y tachado.
        /// </summary>
        public static void EstiloFila(DataGridViewRow fila, int columnaEstado, bool cancelado, bool listo)
        {
            bool enPreparacion = !cancelado && !listo;

            fila.DefaultCellStyle.ForeColor = enPreparacion ? Oscuro : Apagado;
            fila.DefaultCellStyle.Font = cancelado ? Tachada : Normal;

            var estado = fila.Cells[columnaEstado].Style;
            estado.ForeColor = enPreparacion ? Acento : Apagado;
            estado.Font = enPreparacion ? Negrita : (cancelado ? Tachada : Normal);
        }
    }
}
