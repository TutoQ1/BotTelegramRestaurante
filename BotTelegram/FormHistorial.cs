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
        private Label lblResumen;

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
            Width = 900;
            Height = 550;
            StartPosition = FormStartPosition.CenterParent;

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50 };
            txtBuscar = new TextBox { Left = 75, Top = 13, Width = 200 };
            cboEstado = new ComboBox { Left = 345, Top = 13, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            // El orden de las opciones coincide con el orden de FiltroEstado.
            cboEstado.Items.AddRange(new object[] { Textos.Todos, Textos.EnPreparacion, Textos.ListoAvisado, Textos.Cancelado });
            cboEstado.SelectedIndex = 0;
            btnActualizar = new Button { Text = "Actualizar", Left = 515, Top = 11, Width = 100, Height = 28 };
            panelTop.Controls.Add(new Label { Text = "Cliente:", Left = 10, Top = 17, AutoSize = true });
            panelTop.Controls.Add(new Label { Text = "Estado:", Left = 295, Top = 17, AutoSize = true });
            panelTop.Controls.AddRange(new Control[] { txtBuscar, cboEstado, btnActualizar });

            lblResumen = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10, 8, 10, 0),
                BackColor = Color.WhiteSmoke
            };

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };
            dgv.Columns.Add("id", "N°");
            dgv.Columns.Add("cliente", "Cliente");
            dgv.Columns.Add("plato", "Plato");
            dgv.Columns.Add("minutos", "Minutos");
            dgv.Columns.Add("fecha", "Fecha y hora");
            dgv.Columns.Add("estado", "Estado");
            dgv.Columns[0].FillWeight = 30;
            dgv.Columns[3].FillWeight = 50;

            Controls.Add(dgv);
            Controls.Add(lblResumen);
            Controls.Add(panelTop);
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

                if (p.Cancelado)
                    dgv.Rows[fila].DefaultCellStyle.ForeColor = Color.Firebrick;
            }

            MostrarResumen(historial.ObtenerResumen());
        }

        private void MostrarResumen(ResumenHistorial resumen)
        {
            string masPedido = resumen.PlatoMasPedido == null
                ? "-"
                : resumen.PlatoMasPedido + " (" + resumen.CantidadPlatoMasPedido + ")";

            lblResumen.Text =
                "Total de pedidos: " + resumen.Total +
                "   |   En preparación: " + resumen.EnPreparacion +
                "   |   Listos: " + resumen.Listos +
                "   |   Cancelados: " + resumen.Cancelados + Environment.NewLine +
                "Plato más pedido: " + masPedido +
                "   |   Clientes distintos: " + resumen.ClientesDistintos;
        }
    }
}
