using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entidades;
using Logica;
using Timer = System.Windows.Forms.Timer;

namespace BotTelegram
{
    /// <summary>Historial y estadísticas. Pide los datos ya filtrados al servicio y los muestra.</summary>
    public class FormHistorial : Form
    {
        private readonly HistorialServicio historial;
        private readonly Timer tmr;

        private TextBox txtBuscar;
        private ComboBox cboEstado;
        private Button btnActualizar;
        private DataGridView dgv;
        private Label lblTotal, lblEnPreparacion, lblListos, lblCancelados, lblPlatoTop, lblClientes;

        public FormHistorial(HistorialServicio historial)
        {
            this.historial = historial;

            ConstruirInterfaz();

            btnActualizar.Click += (s, e) => Cargar();
            txtBuscar.TextChanged += (s, e) => Cargar();
            cboEstado.SelectedIndexChanged += (s, e) => Cargar();

            tmr = new Timer { Interval = 5000 };
            tmr.Tick += (s, e) => Cargar();
            tmr.Start();
            FormClosing += (s, e) => tmr.Stop();

            Cargar();
        }

        private void ConstruirInterfaz()
        {
            Text = "Historial y estadísticas de pedidos";
            Width = 980;
            Height = 640;
            MinimumSize = new Size(800, 520);
            StartPosition = FormStartPosition.CenterParent;
            Tema.EstilizarVentana(this);

            var encabezado = Tema.Encabezado("Historial de pedidos", "Consulta y estadísticas");

            // ---- Filtros ----
            var panelTop = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Tema.Crema };
            txtBuscar = new TextBox { Left = 90, Top = 18, Width = 230 };
            cboEstado = new ComboBox { Left = 410, Top = 18, Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
            Tema.EstilizarCampo(txtBuscar);
            Tema.EstilizarCampo(cboEstado);
            // El orden de las opciones coincide con el orden de FiltroEstado.
            cboEstado.Items.AddRange(new object[] { Textos.Todos, Textos.EnPreparacion, Textos.ListoAvisado, Textos.Cancelado });
            cboEstado.SelectedIndex = 0;
            btnActualizar = new Button { Text = "Actualizar", Left = 600, Top = 14, Width = 120 };
            Tema.BotonPrimario(btnActualizar);
            panelTop.Controls.Add(Tema.Etiqueta("Cliente", 24, 22));
            panelTop.Controls.Add(Tema.Etiqueta("Estado", 340, 22));
            panelTop.Controls.AddRange(new Control[] { txtBuscar, cboEstado, btnActualizar });

            // ---- Tarjetas de estadísticas (abajo) ----
            var tarjetas = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 104,
                Padding = new Padding(24, 6, 14, 18),
                BackColor = Tema.Crema,
                ColumnCount = 6,
                RowCount = 1
            };
            float[] anchos = { 15f, 18f, 14f, 15f, 22f, 16f };
            foreach (float a in anchos)
                tarjetas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, a));
            tarjetas.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            tarjetas.Controls.Add(CrearTarjeta("Total de pedidos", out lblTotal, 20f), 0, 0);
            tarjetas.Controls.Add(CrearTarjeta("En preparación", out lblEnPreparacion, 20f), 1, 0);
            tarjetas.Controls.Add(CrearTarjeta("Listos", out lblListos, 20f), 2, 0);
            tarjetas.Controls.Add(CrearTarjeta("Cancelados", out lblCancelados, 20f), 3, 0);
            tarjetas.Controls.Add(CrearTarjeta("Plato más pedido", out lblPlatoTop, 13f), 4, 0);
            tarjetas.Controls.Add(CrearTarjeta("Clientes distintos", out lblClientes, 20f), 5, 0);

            // ---- Tabla ----
            var panelTabla = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 4, 24, 8), BackColor = Tema.Crema };
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            Tema.EstilizarGrid(dgv);
            dgv.Columns.Add("id", "N°");
            dgv.Columns.Add("cliente", "Cliente");
            dgv.Columns.Add("plato", "Plato");
            dgv.Columns.Add("minutos", "Minutos");
            dgv.Columns.Add("fecha", "Fecha y hora");
            dgv.Columns.Add("estado", "Estado");
            dgv.Columns[0].FillWeight = 30;
            dgv.Columns[3].FillWeight = 50;
            dgv.Columns[4].FillWeight = 110;
            Tema.CentrarColumna(dgv.Columns[0]);
            Tema.CentrarColumna(dgv.Columns[3]);
            panelTabla.Controls.Add(dgv);

            // Se agregan de adentro hacia afuera.
            Controls.Add(panelTabla);
            Controls.Add(tarjetas);
            Controls.Add(panelTop);
            Controls.Add(encabezado);
        }

        /// <summary>Tarjeta con una franja de color acento, un valor grande y su descripción.</summary>
        private static Panel CrearTarjeta(string titulo, out Label valor, float tamanoValor)
        {
            var tarjeta = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 10, 0),
                BackColor = Tema.Superficie
            };
            var franja = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = Tema.Acento };
            valor = new Label
            {
                Left = 18,
                Top = 10,
                Width = 200,
                Height = 36,
                AutoEllipsis = true,
                Text = "-",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", tamanoValor, FontStyle.Bold),
                ForeColor = Tema.Oscuro,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            var descripcion = new Label
            {
                Left = 18,
                Top = 48,
                Width = 200,
                Height = 20,
                AutoEllipsis = true,
                Text = titulo,
                Font = Tema.Pequena,
                ForeColor = Tema.Apagado,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            tarjeta.Controls.Add(valor);
            tarjeta.Controls.Add(descripcion);
            tarjeta.Controls.Add(franja);
            return tarjeta;
        }

        private void Cargar()
        {
            var pedidos = historial.Consultar(txtBuscar.Text, (FiltroEstado)cboEstado.SelectedIndex);

            dgv.Rows.Clear();
            foreach (var p in pedidos)
            {
                int fila = dgv.Rows.Add(p.Id, p.NombreCliente, p.NombrePlato, p.Minutos,
                    p.HoraPedido.ToString("dd/MM/yyyy HH:mm:ss"),
                    Textos.DeEstado(p));

                Tema.EstiloFila(dgv.Rows[fila], 5, p.Cancelado, p.Notificado);
            }

            MostrarResumen(historial.ObtenerResumen());
        }

        private void MostrarResumen(ResumenHistorial resumen)
        {
            lblTotal.Text = resumen.Total.ToString();
            lblEnPreparacion.Text = resumen.EnPreparacion.ToString();
            lblListos.Text = resumen.Listos.ToString();
            lblCancelados.Text = resumen.Cancelados.ToString();
            lblClientes.Text = resumen.ClientesDistintos.ToString();
            lblPlatoTop.Text = resumen.PlatoMasPedido == null
                ? "-"
                : resumen.PlatoMasPedido + " (" + resumen.CantidadPlatoMasPedido + ")";
        }
    }
}
