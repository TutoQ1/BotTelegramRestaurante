using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entidades;
using Logica;
using Timer = System.Windows.Forms.Timer;

namespace BotTelegram
{
    /// <summary>
    /// Historial EATBOT: consulta los pedidos existentes y presenta filtros y
    /// estadísticas de los resultados visibles. No altera la base de datos.
    /// </summary>
    public class FormHistorial : Form
    {
        private readonly HistorialServicio historial;
        private readonly Timer tmr;

        private TextBox txtBuscar;
        private ComboBox cboEstado;
        private CheckBox chkFechas;
        private DateTimePicker dtDesde, dtHasta;
        private Button btnActualizar;
        private DataGridView dgv;
        private Label lblCantidad;
        private Label[] valoresResumen;

        public FormHistorial(HistorialServicio historial)
        {
            this.historial = historial;
            ConstruirInterfaz();

            btnActualizar.Click += (s, e) => Cargar();
            txtBuscar.TextChanged += (s, e) => Cargar();
            cboEstado.SelectedIndexChanged += (s, e) => Cargar();
            chkFechas.CheckedChanged += (s, e) =>
            {
                dtDesde.Enabled = dtHasta.Enabled = chkFechas.Checked;
                Cargar();
            };
            dtDesde.ValueChanged += (s, e) => Cargar();
            dtHasta.ValueChanged += (s, e) => Cargar();

            tmr = new Timer { Interval = 5000 };
            tmr.Tick += (s, e) => Cargar();
            tmr.Start();
            FormClosing += (s, e) => tmr.Stop();
            Cargar();
        }

        private void ConstruirInterfaz()
        {
            Text = "EATBOT - Historial y estadísticas de pedidos";
            Icon = Principal.ObtenerIcono();
            Width = 1190;
            Height = 695;
            MinimumSize = new Size(1070, 575);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);
            BackColor = Principal.Fondo;

            var cabecera = new Panel { Dock = DockStyle.Top, Height = 86, BackColor = Color.White };
            var logo = new PictureBox
            {
                Image = new Icon(Icon, new Size(64, 64)).ToBitmap(),
                Location = new Point(23, 14),
                Size = new Size(57, 57),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            cabecera.Controls.Add(logo);
            cabecera.Controls.Add(new Label
            {
                Text = "EATBOT  -  Historial y estadísticas",
                Location = new Point(94, 13),
                Size = new Size(720, 38),
                Font = new Font("Segoe UI", 19F, FontStyle.Bold),
                ForeColor = Principal.AzulOscuro
            });
            cabecera.Controls.Add(new Label
            {
                Text = "Consulta los pedidos y analiza los resultados del restaurante",
                Location = new Point(97, 52),
                Size = new Size(620, 24),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Principal.GrisTexto
            });
            lblCantidad = new Label
            {
                Text = "0 pedidos",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Principal.Azul,
                BackColor = Principal.AzulClaro,
                Size = new Size(124, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(Width - 160, 25)
            };
            cabecera.Controls.Add(lblCantidad);
            cabecera.Resize += (s, e) => lblCantidad.Left = cabecera.ClientSize.Width - lblCantidad.Width - 25;
            Principal.Redondear(lblCantidad, 9);

            var barra = new Panel { Dock = DockStyle.Top, Height = 86, Padding = new Padding(23, 18, 23, 15), BackColor = Principal.Fondo };
            var filtros = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 10, RowCount = 1 };
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 138));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 43));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 121));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 121));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            filtros.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            filtros.Controls.Add(Etiqueta("Cliente:"), 0, 0);
            txtBuscar = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F), Margin = new Padding(0, 12, 14, 10), BorderStyle = BorderStyle.FixedSingle };
            filtros.Controls.Add(txtBuscar, 1, 0);
            Principal.PonerPlaceholder(txtBuscar, "Buscar cliente...");
            filtros.Controls.Add(Etiqueta("Estado:"), 2, 0);
            cboEstado = new ComboBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 10, 8, 10) };
            cboEstado.Items.AddRange(new object[] { Textos.Todos, Textos.EnPreparacion, Textos.ListoAvisado, Textos.Cancelado });
            cboEstado.SelectedIndex = 0;
            filtros.Controls.Add(cboEstado, 3, 0);
            chkFechas = new CheckBox { Text = "Filtrar fechas", Checked = false, Dock = DockStyle.Fill, ForeColor = Principal.GrisTexto };
            filtros.Controls.Add(chkFechas, 4, 0);
            filtros.Controls.Add(Etiqueta("Desde:"), 5, 0);
            dtDesde = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Enabled = false, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Margin = new Padding(0, 10, 8, 10) };
            filtros.Controls.Add(dtDesde, 6, 0);
            filtros.Controls.Add(Etiqueta("Hasta:"), 7, 0);
            dtHasta = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Enabled = false, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Margin = new Padding(0, 10, 9, 10) };
            filtros.Controls.Add(dtHasta, 8, 0);
            btnActualizar = Principal.CrearBoton("↻  Actualizar", true);
            btnActualizar.Dock = DockStyle.Fill;
            btnActualizar.Margin = new Padding(0, 3, 0, 3);
            filtros.Controls.Add(btnActualizar, 9, 0);
            barra.Controls.Add(filtros);

            var resumenContenedor = new Panel { Dock = DockStyle.Bottom, Height = 125, Padding = new Padding(23, 10, 23, 20), BackColor = Principal.Fondo };
            var resumen = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 7, BackColor = Color.White, CellBorderStyle = TableLayoutPanelCellBorderStyle.Single };
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 13));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 21));
            resumen.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            resumen.Controls.Add(Bloque("ESTADÍSTICAS DEL PERÍODO", "Filtros aplicados", Principal.AzulOscuro, true, false), 0, 0);
            valoresResumen = new Label[6];
            string[] titulos = { "Total pedidos", "En preparación", "Listos", "Cancelados", "Clientes distintos", "Plato más pedido" };
            Color[] colores = { Principal.Azul, Color.FromArgb(177, 111, 36), Principal.Verde, Principal.Rojo, Principal.Azul, Principal.AzulOscuro };
            for (int i = 0; i < 6; i++)
            {
                Panel bloque = Bloque(titulos[i], "—", colores[i], false, i == 5);
                valoresResumen[i] = bloque.Controls.OfType<Label>().First(l => (string)l.Tag == "valor");
                resumen.Controls.Add(bloque, i + 1, 0);
            }
            resumenContenedor.Controls.Add(resumen);

            var zonaTabla = new Panel { Dock = DockStyle.Fill, BackColor = Principal.Fondo, Padding = new Padding(23, 0, 23, 0) };
            var bordeTabla = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            dgv = new DataGridView { Dock = DockStyle.Fill };
            Principal.ConfigurarTabla(dgv);
            dgv.Columns.Add("id", "N°");
            dgv.Columns.Add("cliente", "Cliente");
            dgv.Columns.Add("plato", "Plato");
            dgv.Columns.Add("minutos", "Minutos");
            dgv.Columns.Add("fecha", "Fecha y hora");
            dgv.Columns.Add("estado", "Estado");
            dgv.Columns[0].FillWeight = 28;
            dgv.Columns[1].FillWeight = 85;
            dgv.Columns[2].FillWeight = 99;
            dgv.Columns[3].FillWeight = 48;
            dgv.Columns[4].FillWeight = 112;
            dgv.Columns[5].FillWeight = 112;
            dgv.CellPainting += Principal.PintarEstadoGrid;
            bordeTabla.Controls.Add(dgv);
            zonaTabla.Controls.Add(bordeTabla);

            Controls.Add(zonaTabla);
            Controls.Add(resumenContenedor);
            Controls.Add(barra);
            Controls.Add(cabecera);
        }

        private static Label Etiqueta(string texto)
        {
            return new Label { Text = texto, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(39, 51, 68), TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) };
        }

        private static Panel Bloque(string titulo, string valor, Color color, bool encabezado, bool textoPequeno)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12, 13, 5, 4) };
            var nombre = new Label
            {
                Text = titulo,
                Dock = DockStyle.Top,
                Height = 23,
                ForeColor = encabezado ? Principal.AzulOscuro : Principal.GrisTexto,
                Font = new Font("Segoe UI", encabezado ? 9F : 8.3F, encabezado ? FontStyle.Bold : FontStyle.Regular),
                TextAlign = ContentAlignment.BottomLeft
            };
            var dato = new Label
            {
                Text = valor,
                Tag = "valor",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", encabezado ? 10F : textoPequeno ? 10F : 17F, FontStyle.Bold),
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            panel.Controls.Add(dato);
            panel.Controls.Add(nombre);
            return panel;
        }

        private void Cargar()
        {
            if (dgv == null) return;
            List<Pedido> pedidos = historial.Consultar(txtBuscar.Text, (FiltroEstado)cboEstado.SelectedIndex);
            if (chkFechas.Checked)
            {
                DateTime desde = dtDesde.Value.Date;
                DateTime hasta = dtHasta.Value.Date;
                pedidos = pedidos.Where(p => p.HoraPedido.Date >= desde && p.HoraPedido.Date <= hasta).ToList();
            }

            int idSeleccionado = -1;
            if (dgv.CurrentRow != null && dgv.CurrentRow.Cells[0].Value != null)
                idSeleccionado = Convert.ToInt32(dgv.CurrentRow.Cells[0].Value);
            dgv.Rows.Clear();
            int seleccion = -1;
            foreach (Pedido p in pedidos)
            {
                int fila = dgv.Rows.Add(p.Id, p.NombreCliente, p.NombrePlato, p.Minutos,
                    p.HoraPedido.ToString("dd/MM/yyyy HH:mm:ss"), Textos.DeEstado(p));
                if (p.Id == idSeleccionado) seleccion = fila;
            }
            if (dgv.Rows.Count == 0) dgv.ClearSelection();
            else if (seleccion >= 0) dgv.CurrentCell = dgv.Rows[seleccion].Cells[0];
            else dgv.CurrentCell = dgv.Rows[0].Cells[0];

            lblCantidad.Text = pedidos.Count + (pedidos.Count == 1 ? " pedido" : " pedidos");
            MostrarResumen(new ResumenHistorial(pedidos));
        }

        private void MostrarResumen(ResumenHistorial resumen)
        {
            valoresResumen[0].Text = resumen.Total.ToString();
            valoresResumen[1].Text = resumen.EnPreparacion.ToString();
            valoresResumen[2].Text = resumen.Listos.ToString();
            valoresResumen[3].Text = resumen.Cancelados.ToString();
            valoresResumen[4].Text = resumen.ClientesDistintos.ToString();
            valoresResumen[5].Text = resumen.PlatoMasPedido == null ? "—" : resumen.PlatoMasPedido + " (" + resumen.CantidadPlatoMasPedido + ")";
        }
    }
}
