using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Entidades;
using Logica;
using Timer = System.Windows.Forms.Timer;

namespace BotTelegram
{
    /// <summary>
    /// EATBOT: presentación de los pedidos. Las reglas de negocio, los mensajes de
    /// Telegram y la persistencia permanecen en los servicios existentes.
    /// </summary>
    public class Principal : Form
    {
        internal static readonly Color Azul = Color.FromArgb(17, 74, 126);
        internal static readonly Color AzulOscuro = Color.FromArgb(12, 53, 94);
        internal static readonly Color AzulClaro = Color.FromArgb(226, 237, 249);
        internal static readonly Color Fondo = Color.FromArgb(246, 248, 251);
        internal static readonly Color GrisTexto = Color.FromArgb(91, 105, 125);
        internal static readonly Color Linea = Color.FromArgb(221, 227, 235);
        internal static readonly Color Verde = Color.FromArgb(31, 130, 77);
        internal static readonly Color Rojo = Color.FromArgb(183, 58, 58);

        private readonly RestauranteControlador controlador;
        private readonly Timer tmr;
        private int pulsos;
        private List<Pedido> pedidosPanel = new List<Pedido>();

        private TextBox txtToken, txtLog, txtBuscar;
        private Button btnIniciar, btnDetener, btnListo, btnCancelar, btnHistorial, btnLog, btnMostrarToken;
        private Label lblEstado, lblDetalleCliente, lblDetallePlato, lblDetalleHora, lblDetalleTiempo,
            lblDetalleEstado, lblSinPedido;
        private Label[] valoresResumen;
        private ComboBox cboEstado;
        private DataGridView dgv;
        private Panel panelRegistro;

        public Principal(RestauranteControlador controlador)
        {
            this.controlador = controlador;
            ConstruirInterfaz();
            ConectarEventos();

            tmr = new Timer { Interval = 1000 };
            tmr.Tick += (s, e) =>
            {
                RefrescarTabla();
                if (++pulsos % 5 == 0) RefrescarResumen();
            };
            tmr.Start();

            if (controlador.BotActivo) MostrarBotActivo();
            else MostrarBotInactivo();
            RefrescarTabla();
            RefrescarResumen();
        }

        private void ConstruirInterfaz()
        {
            Text = "EATBOT - Gestión de pedidos (Bot de Telegram)";
            Icon = ObtenerIcono();
            Width = 1390;
            Height = 810;
            MinimumSize = new Size(1120, 675);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);
            BackColor = Fondo;
            DoubleBuffered = true;

            // Encabezado y marca EATBOT.
            var encabezado = new Panel { Dock = DockStyle.Top, Height = 87, BackColor = Color.White };
            encabezado.Paint += (s, e) =>
            {
                using (var pen = new Pen(Linea))
                    e.Graphics.DrawLine(pen, 0, encabezado.Height - 1, encabezado.Width, encabezado.Height - 1);
            };
            var logo = new PictureBox
            {
                Image = new Icon(Icon, new Size(64, 64)).ToBitmap(),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(24, 14),
                Size = new Size(58, 58)
            };
            encabezado.Controls.Add(logo);
            encabezado.Controls.Add(new Label
            {
                Text = "EATBOT  -  Panel de pedidos",
                Font = new Font("Segoe UI", 19F, FontStyle.Bold),
                ForeColor = AzulOscuro,
                Location = new Point(96, 13),
                Size = new Size(475, 38)
            });
            encabezado.Controls.Add(new Label
            {
                Text = "Gestión de pedidos recibidos desde Telegram",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = GrisTexto,
                Location = new Point(99, 51),
                Size = new Size(438, 25)
            });
            lblEstado = new Label
            {
                Text = "●  Bot detenido",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Rojo,
                BackColor = Color.FromArgb(251, 236, 236),
                Size = new Size(166, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(Width - 210, 25)
            };
            encabezado.Controls.Add(lblEstado);
            encabezado.Resize += (s, e) => lblEstado.Left = encabezado.ClientSize.Width - lblEstado.Width - 25;
            Redondear(lblEstado, 9);

            // Controles superiores: token, acciones del bot, búsqueda y estado.
            var zonaFiltros = new Panel { Dock = DockStyle.Top, Height = 83, Padding = new Padding(24, 18, 24, 13), BackColor = Fondo };
            var filtros = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 9, RowCount = 1, Margin = new Padding(0) };
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 127));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 22));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 226));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 139));
            filtros.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            filtros.Controls.Add(CrearEtiqueta("Token:"), 0, 0);
            var tokenGrupo = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 12, 4), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            txtToken = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true, BorderStyle = BorderStyle.None, Margin = new Padding(0), Font = new Font("Segoe UI", 10F) };
            var tokenEspacio = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 8, 0, 0), BackColor = Color.White };
            tokenEspacio.Controls.Add(txtToken);
            btnMostrarToken = new Button { Text = "◉", Dock = DockStyle.Right, Width = 36, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = GrisTexto, Cursor = Cursors.Hand, TabStop = false };
            btnMostrarToken.FlatAppearance.BorderSize = 0;
            tokenGrupo.Controls.Add(tokenEspacio);
            tokenGrupo.Controls.Add(btnMostrarToken);
            filtros.Controls.Add(tokenGrupo, 1, 0);

            btnIniciar = CrearBoton("▶  Iniciar bot", true);
            btnDetener = CrearBoton("■  Detener", false);
            btnIniciar.Margin = new Padding(3, 2, 6, 2);
            btnDetener.Margin = new Padding(3, 2, 6, 2);
            btnIniciar.Dock = DockStyle.Fill;
            btnDetener.Dock = DockStyle.Fill;
            filtros.Controls.Add(btnIniciar, 2, 0);
            filtros.Controls.Add(btnDetener, 3, 0);
            filtros.Controls.Add(new Panel { Dock = DockStyle.Fill }, 4, 0);
            filtros.Controls.Add(CrearEtiqueta("Buscar:"), 5, 0);
            txtBuscar = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 9, 13, 8) };
            filtros.Controls.Add(txtBuscar, 6, 0);
            PonerPlaceholder(txtBuscar, "Buscar cliente o plato...");
            filtros.Controls.Add(CrearEtiqueta("Estado:"), 7, 0);
            cboEstado = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F), Margin = new Padding(0, 8, 0, 8), FlatStyle = FlatStyle.Flat };
            cboEstado.Items.AddRange(new object[] { Textos.Todos, Textos.EnPreparacion, Textos.ListoAvisado });
            cboEstado.SelectedIndex = 0;
            filtros.Controls.Add(cboEstado, 8, 0);
            zonaFiltros.Controls.Add(filtros);

            // Resumen del día basado en los pedidos de HOY, no en el historial completo.
            var zonaResumen = new Panel { Dock = DockStyle.Bottom, Height = 100, Padding = new Padding(24, 10, 24, 18), BackColor = Fondo };
            var resumen = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 1, BackColor = Color.White, CellBorderStyle = TableLayoutPanelCellBorderStyle.Single };
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            resumen.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            resumen.Controls.Add(CrearBloqueResumen("RESUMEN DEL DÍA", DateTime.Today.ToString("dd/MM/yyyy"), Azul, true), 0, 0);
            valoresResumen = new Label[5];
            string[] titulos = { "Total pedidos", "En preparación", "Listos", "Cancelados", "Plato más pedido" };
            Color[] colores = { Azul, Color.FromArgb(177, 111, 36), Verde, Rojo, AzulOscuro };
            for (int i = 0; i < 5; i++)
            {
                Panel bloque = CrearBloqueResumen(titulos[i], "-", colores[i], false);
                valoresResumen[i] = bloque.Controls.OfType<Label>().First(l => l.Tag as string == "valor");
                if (i == 4) valoresResumen[i].Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                resumen.Controls.Add(bloque, i + 1, 0);
            }
            zonaResumen.Controls.Add(resumen);

            // Botones y registro técnico plegable (los eventos de log no se pierden).
            var zonaAcciones = new Panel { Dock = DockStyle.Bottom, Height = 66, Padding = new Padding(24, 10, 24, 12), BackColor = Fondo };
            btnListo = CrearBoton("✓  Marcar como listo", true);
            btnCancelar = CrearBoton("✕  Cancelar pedido", false);
            btnHistorial = CrearBoton("▥  Ver historial", false);
            btnLog = CrearBoton("▤  Registro de actividad  ▾", false);
            btnListo.SetBounds(24, 10, 204, 43);
            btnCancelar.SetBounds(242, 10, 184, 43);
            btnHistorial.SetBounds(440, 10, 160, 43);
            btnLog.Size = new Size(240, 43);
            btnLog.Top = 10;
            btnLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            zonaAcciones.Controls.AddRange(new Control[] { btnListo, btnCancelar, btnHistorial, btnLog });
            zonaAcciones.Resize += (s, e) => btnLog.Left = zonaAcciones.ClientSize.Width - btnLog.Width - 24;

            panelRegistro = new Panel { Dock = DockStyle.Bottom, Height = 165, BackColor = Color.White, Visible = false, Padding = new Padding(24, 0, 24, 10) };
            var encabezadoLog = new Label { Text = "REGISTRO DE ACTIVIDAD DEL BOT", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = GrisTexto, Padding = new Padding(0, 7, 0, 0) };
            txtLog = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(24, 39, 57),
                ForeColor = Color.FromArgb(202, 226, 231),
                Font = new Font("Consolas", 9F)
            };
            panelRegistro.Controls.Add(txtLog);
            panelRegistro.Controls.Add(encabezadoLog);

            // Tabla principal y panel lateral de detalle del pedido seleccionado.
            var espacioPrincipal = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 0, 24, 0), BackColor = Fondo };
            var panelContenido = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            dgv = new DataGridView { Dock = DockStyle.Fill };
            ConfigurarTabla(dgv);
            dgv.Columns.Add("id", "N°");
            dgv.Columns.Add("cliente", "Cliente");
            dgv.Columns.Add("plato", "Plato");
            dgv.Columns.Add("hora", "Hora del pedido");
            dgv.Columns.Add("faltan", "Tiempo restante");
            dgv.Columns.Add("estado", "Estado");
            dgv.Columns[0].FillWeight = 28;
            dgv.Columns[1].FillWeight = 78;
            dgv.Columns[2].FillWeight = 93;
            dgv.Columns[3].FillWeight = 77;
            dgv.Columns[4].FillWeight = 82;
            dgv.Columns[5].FillWeight = 107;
            dgv.CellPainting += PintarEstadoGrid;
            dgv.SelectionChanged += (s, e) => ActualizarDetalle();

            var detalle = new Panel { Dock = DockStyle.Right, Width = 310, BackColor = Color.White, Padding = new Padding(14, 0, 0, 0) };
            var cabeceraDetalle = new Label
            {
                Text = "▤   Detalle del pedido",
                Dock = DockStyle.Top,
                Height = 49,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Azul,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0)
            };
            detalle.Controls.Add(cabeceraDetalle);
            lblDetalleCliente = AgregarDetalle(detalle, "CLIENTE", 63);
            lblDetallePlato = AgregarDetalle(detalle, "PLATO", 130);
            lblDetalleHora = AgregarDetalle(detalle, "HORA DEL PEDIDO", 197);
            lblDetalleTiempo = AgregarDetalle(detalle, "TIEMPO DE PREPARACIÓN", 264);
            lblDetalleEstado = AgregarDetalle(detalle, "ESTADO", 331);
            lblDetalleEstado.ForeColor = Azul;
            lblSinPedido = new Label { Text = "Selecciona un pedido para ver sus detalles.", TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Bottom, Height = 55, ForeColor = GrisTexto };
            detalle.Controls.Add(lblSinPedido);
            panelContenido.Controls.Add(dgv);
            panelContenido.Controls.Add(detalle);
            espacioPrincipal.Controls.Add(panelContenido);

            Controls.Add(espacioPrincipal);
            Controls.Add(panelRegistro);
            Controls.Add(zonaResumen);
            Controls.Add(zonaAcciones);
            Controls.Add(zonaFiltros);
            Controls.Add(encabezado);
        }

        private static Label CrearEtiqueta(string texto)
        {
            return new Label { Text = texto, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(39, 51, 68), TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) };
        }

        private static Label AgregarDetalle(Panel panel, string titulo, int y)
        {
            panel.Controls.Add(new Label
            {
                Text = titulo,
                ForeColor = GrisTexto,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Location = new Point(29, y),
                Size = new Size(252, 19),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            });
            var valor = new Label
            {
                Text = "—",
                ForeColor = Color.FromArgb(28, 45, 67),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(29, y + 24),
                Size = new Size(252, 26),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            panel.Controls.Add(valor);
            return valor;
        }

        private static Panel CrearBloqueResumen(string titulo, string valor, Color color, bool intro)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(13, 6, 5, 0) };
            var nombre = new Label
            {
                Text = titulo,
                Font = new Font("Segoe UI", intro ? 10F : 8.5F, intro ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = intro ? AzulOscuro : GrisTexto,
                Dock = DockStyle.Top,
                Height = 27,
                TextAlign = ContentAlignment.BottomLeft
            };
            var cantidad = new Label
            {
                Text = valor,
                Tag = "valor",
                Font = new Font("Segoe UI", intro ? 10F : 15F, FontStyle.Bold),
                ForeColor = color,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            panel.Controls.Add(cantidad);
            panel.Controls.Add(nombre);
            return panel;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr ventana, int mensaje, IntPtr parametro, string valor);

        internal static void PonerPlaceholder(TextBox campo, string texto)
        {
            campo.HandleCreated += (s, e) => SendMessage(campo.Handle, 0x1501, IntPtr.Zero, texto);
        }

        internal static Icon ObtenerIcono()
        {
            using (Stream stream = typeof(Principal).Assembly.GetManifestResourceStream("BotTelegram.EATBOT.ico"))
            {
                return stream == null ? SystemIcons.Application : new Icon(stream);
            }
        }

        internal static Button CrearBoton(string texto, bool principal)
        {
            var boton = new Button
            {
                Text = texto,
                Dock = DockStyle.None,
                Height = 39,
                Font = new Font("Segoe UI", 9F, principal ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = principal ? Color.White : AzulOscuro,
                BackColor = principal ? Azul : Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            boton.FlatAppearance.BorderSize = 1;
            boton.FlatAppearance.BorderColor = principal ? Azul : Color.FromArgb(190, 202, 217);
            boton.FlatAppearance.MouseOverBackColor = principal ? AzulOscuro : AzulClaro;
            boton.SizeChanged += (s, e) => Redondear(boton, 7);
            return boton;
        }

        internal static void Redondear(Control control, int radio)
        {
            if (control.Width < 4 || control.Height < 4) return;
            using (GraphicsPath path = RutaRedondeada(new Rectangle(0, 0, control.Width, control.Height), radio))
            {
                Region anterior = control.Region;
                control.Region = new Region(path);
                if (anterior != null) anterior.Dispose();
            }
        }

        internal static GraphicsPath RutaRedondeada(Rectangle rect, int radio)
        {
            int r = Math.Max(1, Math.Min(radio, Math.Min(rect.Width, rect.Height) / 2));
            int diametro = r * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.Left, rect.Top, diametro, diametro, 180, 90);
            path.AddArc(rect.Right - diametro, rect.Top, diametro, diametro, 270, 90);
            path.AddArc(rect.Right - diametro, rect.Bottom - diametro, diametro, diametro, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - diametro, diametro, diametro, 90, 90);
            path.CloseFigure();
            return path;
        }

        internal static void ConfigurarTabla(DataGridView tabla)
        {
            tabla.BackgroundColor = Color.White;
            tabla.BorderStyle = BorderStyle.None;
            tabla.GridColor = Linea;
            tabla.ReadOnly = true;
            tabla.AllowUserToAddRows = false;
            tabla.AllowUserToDeleteRows = false;
            tabla.AllowUserToResizeRows = false;
            tabla.MultiSelect = false;
            tabla.RowHeadersVisible = false;
            tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tabla.EnableHeadersVisualStyles = false;
            tabla.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            tabla.ColumnHeadersHeight = 44;
            tabla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            tabla.RowTemplate.Height = 36;
            tabla.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            tabla.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(234, 240, 247),
                ForeColor = Color.FromArgb(27, 42, 61),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(10, 0, 0, 0),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                SelectionBackColor = Color.FromArgb(234, 240, 247)
            };
            tabla.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(40, 54, 73),
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(10, 0, 0, 0),
                SelectionBackColor = Azul,
                SelectionForeColor = Color.White,
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };
            tabla.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 251, 253);
        }

        internal static void PintarEstadoGrid(object sender, DataGridViewCellPaintingEventArgs e)
        {
            var tabla = (DataGridView)sender;
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || tabla.Columns[e.ColumnIndex].Name != "estado") return;
            string estado = Convert.ToString(e.FormattedValue);
            if (string.IsNullOrEmpty(estado)) return;
            bool listo = estado == Textos.ListoAvisado;
            bool cancelado = estado == Textos.Cancelado;
            Color fondo = listo ? Color.FromArgb(221, 237, 252) : cancelado ? Color.FromArgb(253, 232, 231) : Color.FromArgb(254, 242, 220);
            Color texto = listo ? Azul : cancelado ? Rojo : Color.FromArgb(142, 91, 28);
            string etiqueta = listo ? "✓  " + estado : cancelado ? "✕  " + estado : "◷  " + estado;
            e.PaintBackground(e.CellBounds, true);
            Size medida = TextRenderer.MeasureText(etiqueta, e.CellStyle.Font);
            int ancho = Math.Min(e.CellBounds.Width - 14, medida.Width + 14);
            if (ancho > 0)
            {
                var rect = new Rectangle(e.CellBounds.X + 9, e.CellBounds.Y + (e.CellBounds.Height - 26) / 2, ancho, 26);
                using (GraphicsPath path = RutaRedondeada(rect, 9))
                using (Brush brocha = new SolidBrush(fondo))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brocha, path);
                }
                TextRenderer.DrawText(e.Graphics, etiqueta, e.CellStyle.Font,
                    new Rectangle(rect.X + 7, rect.Y, rect.Width - 11, rect.Height),
                    texto, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            }
            e.Handled = true;
        }

        private void ConectarEventos()
        {
            btnIniciar.Click += BtnIniciar_Click;
            btnDetener.Click += (s, e) => DetenerBot();
            btnListo.Click += BtnListo_Click;
            btnCancelar.Click += BtnCancelar_Click;
            btnHistorial.Click += (s, e) => new FormHistorial(controlador.Historial).Show(this);
            btnLog.Click += (s, e) =>
            {
                panelRegistro.Visible = !panelRegistro.Visible;
                btnLog.Text = panelRegistro.Visible ? "▤  Ocultar actividad  ▴" : "▤  Registro de actividad  ▾";
            };
            btnMostrarToken.Click += (s, e) =>
            {
                txtToken.UseSystemPasswordChar = !txtToken.UseSystemPasswordChar;
                btnMostrarToken.Text = txtToken.UseSystemPasswordChar ? "◉" : "◌";
            };
            txtBuscar.TextChanged += (s, e) => RefrescarTabla();
            cboEstado.SelectedIndexChanged += (s, e) => RefrescarTabla();

            controlador.Bitacora.Registrada += Log;
            FormClosing += (s, e) =>
            {
                tmr.Stop();
                controlador.Bitacora.Registrada -= Log;
                controlador.DetenerBot();
            };
        }

        private async void BtnIniciar_Click(object sender, EventArgs e)
        {
            btnIniciar.Enabled = false;
            try
            {
                await controlador.IniciarBotAsync(txtToken.Text);
                MostrarBotActivo();
            }
            catch (RestauranteException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo iniciar EATBOT", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                MostrarBotInactivo();
            }
        }

        private void DetenerBot()
        {
            controlador.DetenerBot();
            MostrarBotInactivo();
        }

        private void MostrarBotInactivo()
        {
            btnIniciar.Enabled = true;
            btnDetener.Enabled = false;
            txtToken.Enabled = true;
            lblEstado.Text = "●  Bot detenido";
            lblEstado.ForeColor = Rojo;
            lblEstado.BackColor = Color.FromArgb(251, 236, 236);
            ActualizarDetalle();
        }

        private void MostrarBotActivo()
        {
            lblEstado.Text = "●  Bot en ejecución";
            lblEstado.ForeColor = Verde;
            lblEstado.BackColor = Color.FromArgb(229, 246, 235);
            btnIniciar.Enabled = false;
            btnDetener.Enabled = true;
            txtToken.Enabled = false;
            ActualizarDetalle();
        }

        private void BtnListo_Click(object sender, EventArgs e)
        {
            int id = PedidoSeleccionado();
            if (id < 0) { MessageBox.Show("Selecciona un pedido de la tabla."); return; }
            try
            {
                controlador.MarcarListo(id);
                RefrescarTabla();
                RefrescarResumen();
            }
            catch (RestauranteException ex) { MessageBox.Show(ex.Message, "EATBOT", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        private async void BtnCancelar_Click(object sender, EventArgs e)
        {
            int id = PedidoSeleccionado();
            if (id < 0) { MessageBox.Show("Selecciona un pedido de la tabla."); return; }
            if (MessageBox.Show("¿Cancelar el pedido #" + id + "?", "Confirmar cancelación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                await controlador.CancelarPedidoAsync(id);
                RefrescarTabla();
                RefrescarResumen();
            }
            catch (RestauranteException ex) { MessageBox.Show(ex.Message, "EATBOT", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        private int PedidoSeleccionado()
        {
            if (dgv == null || dgv.CurrentRow == null || dgv.CurrentRow.Cells[0].Value == null) return -1;
            return Convert.ToInt32(dgv.CurrentRow.Cells[0].Value);
        }

        private void RefrescarTabla()
        {
            if (dgv == null || cboEstado == null || txtBuscar == null) return;
            pedidosPanel = controlador.ObtenerPedidosPanel();
            string buscar = txtBuscar.Text.Trim();
            int estado = cboEstado.SelectedIndex;
            List<Pedido> visibles = pedidosPanel
                .Where(p => buscar.Length == 0
                    || (p.NombreCliente ?? "").IndexOf(buscar, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || (p.NombrePlato ?? "").IndexOf(buscar, StringComparison.CurrentCultureIgnoreCase) >= 0)
                .Where(p => estado <= 0 || (estado == 1 && p.EstaPendiente) || (estado == 2 && p.Notificado))
                .ToList();

            bool mismasFilas = dgv.Rows.Count == visibles.Count;
            for (int i = 0; mismasFilas && i < visibles.Count; i++)
                if (Convert.ToInt32(dgv.Rows[i].Cells[0].Value) != visibles[i].Id) mismasFilas = false;

            if (mismasFilas)
            {
                for (int i = 0; i < visibles.Count; i++) PonerFila(dgv.Rows[i], visibles[i]);
            }
            else
            {
                int idSeleccionado = PedidoSeleccionado();
                dgv.Rows.Clear();
                int filaSeleccionada = -1;
                foreach (Pedido pedido in visibles)
                {
                    int i = dgv.Rows.Add();
                    PonerFila(dgv.Rows[i], pedido);
                    if (pedido.Id == idSeleccionado) filaSeleccionada = i;
                }
                if (visibles.Count == 0) dgv.ClearSelection();
                else if (filaSeleccionada >= 0) dgv.CurrentCell = dgv.Rows[filaSeleccionada].Cells[0];
                else dgv.CurrentCell = dgv.Rows[0].Cells[0];
            }
            ActualizarDetalle();
        }

        private static void PonerFila(DataGridViewRow fila, Pedido pedido)
        {
            fila.Cells[0].Value = pedido.Id;
            fila.Cells[1].Value = pedido.NombreCliente;
            fila.Cells[2].Value = pedido.NombrePlato;
            fila.Cells[3].Value = pedido.HoraPedido.ToString("HH:mm:ss");
            fila.Cells[4].Value = pedido.Notificado ? "—" : FormatearTiempo(pedido.TiempoRestante(DateTime.Now));
            fila.Cells[5].Value = pedido.Notificado ? Textos.ListoAvisado : Textos.EnPreparacion;
        }

        private static string FormatearTiempo(TimeSpan tiempo)
        {
            return ((int)tiempo.TotalMinutes).ToString("00") + ":" + tiempo.Seconds.ToString("00");
        }

        private void ActualizarDetalle()
        {
            if (lblDetalleCliente == null) return;
            Pedido pedido = pedidosPanel.FirstOrDefault(p => p.Id == PedidoSeleccionado());
            bool existe = pedido != null;
            lblDetalleCliente.Text = existe ? pedido.NombreCliente : "—";
            lblDetallePlato.Text = existe ? pedido.NombrePlato : "—";
            lblDetalleHora.Text = existe ? pedido.HoraPedido.ToString("HH:mm:ss") : "—";
            lblDetalleTiempo.Text = existe ? pedido.Minutos + " min · " + (pedido.Notificado ? "Finalizado" : FormatearTiempo(pedido.TiempoRestante(DateTime.Now)) + " restantes") : "—";
            lblDetalleEstado.Text = existe ? Textos.DeEstado(pedido) : "—";
            lblSinPedido.Visible = !existe;
            if (btnListo != null) btnListo.Enabled = existe && pedido.EstaPendiente && controlador.BotActivo;
            if (btnCancelar != null) btnCancelar.Enabled = existe && pedido.EstaPendiente;
        }

        private void RefrescarResumen()
        {
            if (valoresResumen == null) return;
            List<Pedido> hoy = controlador.Historial.Consultar("", FiltroEstado.Todos)
                .Where(p => p.HoraPedido.Date == DateTime.Today).ToList();
            var resumen = new ResumenHistorial(hoy);
            valoresResumen[0].Text = resumen.Total.ToString();
            valoresResumen[1].Text = resumen.EnPreparacion.ToString();
            valoresResumen[2].Text = resumen.Listos.ToString();
            valoresResumen[3].Text = resumen.Cancelados.ToString();
            valoresResumen[4].Text = resumen.PlatoMasPedido == null
                ? "—"
                : resumen.PlatoMasPedido + " (" + resumen.CantidadPlatoMasPedido + ")";
        }

        // La bitácora puede emitir desde un hilo de Telegram: se invoca en el hilo de UI.
        private void Log(string texto)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(() => Log(texto))); } catch (InvalidOperationException) { }
                return;
            }
            txtLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + texto + Environment.NewLine);
        }
    }
}
