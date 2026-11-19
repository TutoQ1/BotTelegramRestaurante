using System;
using System.Drawing;
using System.Windows.Forms;
using Entidades;
using Logica;
using Timer = System.Windows.Forms.Timer;

namespace BotTelegram
{
    /// <summary>
    /// Panel de pedidos. Solo captura datos, invoca al controlador y muestra resultados:
    /// no contiene reglas de negocio ni acceso a archivos ni a Telegram.
    /// </summary>
    public class Principal : Form
    {
        private readonly RestauranteControlador controlador;
        private readonly Timer tmr;

        private TextBox txtToken, txtLog;
        private Button btnIniciar, btnDetener, btnListo, btnCancelar, btnHistorial;
        private Label lblEstado;
        private DataGridView dgv;

        public Principal(RestauranteControlador controlador)
        {
            this.controlador = controlador;

            ConstruirInterfaz();
            ConectarEventos();

            tmr = new Timer { Interval = 1000 };
            tmr.Tick += (s, e) => RefrescarTabla();
            tmr.Start();

            RefrescarTabla();
        }

        private void ConstruirInterfaz()
        {
            Text = "Restaurante - Panel de pedidos (Bot de Telegram)";
            Width = 1000;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50 };
            txtToken = new TextBox { Left = 60, Top = 13, Width = 400, UseSystemPasswordChar = true };
            btnIniciar = new Button { Text = "Iniciar bot", Left = 475, Top = 11, Width = 100, Height = 28 };
            btnDetener = new Button { Text = "Detener", Left = 585, Top = 11, Width = 90, Height = 28, Enabled = false };
            lblEstado = new Label { Text = "● Bot detenido", Left = 695, Top = 17, AutoSize = true, ForeColor = Color.Firebrick };
            panelTop.Controls.Add(new Label { Text = "Token:", Left = 10, Top = 17, AutoSize = true });
            panelTop.Controls.AddRange(new Control[] { txtToken, btnIniciar, btnDetener, lblEstado });

            var panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 220 };
            txtLog = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.Black,
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 9)
            };
            var panelBotones = new Panel { Dock = DockStyle.Top, Height = 45 };
            btnListo = new Button { Text = "Marcar como listo", Left = 10, Top = 8, Width = 170, Height = 30 };
            btnCancelar = new Button { Text = "Cancelar pedido", Left = 190, Top = 8, Width = 160, Height = 30 };
            btnHistorial = new Button { Text = "Historial", Left = 360, Top = 8, Width = 120, Height = 30 };
            panelBotones.Controls.AddRange(new Control[] { btnListo, btnCancelar, btnHistorial });

            panelBottom.Controls.Add(txtLog);
            panelBottom.Controls.Add(panelBotones);

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };
            dgv.Columns.Add("id", "N°");
            dgv.Columns.Add("cliente", "Cliente");
            dgv.Columns.Add("plato", "Plato");
            dgv.Columns.Add("hora", "Hora del pedido");
            dgv.Columns.Add("faltan", "Tiempo restante");
            dgv.Columns.Add("estado", "Estado");
            dgv.Columns[0].FillWeight = 30;

            Controls.Add(dgv);
            Controls.Add(panelBottom);
            Controls.Add(panelTop);
        }

        private void ConectarEventos()
        {
            btnIniciar.Click += BtnIniciar_Click;
            btnDetener.Click += (s, e) => DetenerBot();
            btnListo.Click += BtnListo_Click;
            btnCancelar.Click += BtnCancelar_Click;
            btnHistorial.Click += (s, e) => new FormHistorial(controlador.Historial).Show();

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
                MessageBox.Show(ex.Message, "No se pudo iniciar el bot");
                btnIniciar.Enabled = true;
            }
        }

        private void DetenerBot()
        {
            controlador.DetenerBot();
            btnIniciar.Enabled = true;
            btnDetener.Enabled = false;
            txtToken.Enabled = true;
            lblEstado.Text = "● Bot detenido";
            lblEstado.ForeColor = Color.Firebrick;
        }

        private void MostrarBotActivo()
        {
            lblEstado.Text = "● Bot activo";
            lblEstado.ForeColor = Color.Green;
            btnDetener.Enabled = true;
            txtToken.Enabled = false;
        }

        private void BtnListo_Click(object sender, EventArgs e)
        {
            int id = PedidoSeleccionado();
            if (id < 0)
            {
                MessageBox.Show("Selecciona un pedido de la tabla.");
                return;
            }

            try
            {
                controlador.MarcarListo(id);
            }
            catch (RestauranteException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void BtnCancelar_Click(object sender, EventArgs e)
        {
            int id = PedidoSeleccionado();
            if (id < 0)
            {
                MessageBox.Show("Selecciona un pedido de la tabla.");
                return;
            }

            try
            {
                await controlador.CancelarPedidoAsync(id);
                RefrescarTabla();
            }
            catch (RestauranteException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int PedidoSeleccionado()
        {
            if (dgv.CurrentRow == null) return -1;
            return Convert.ToInt32(dgv.CurrentRow.Cells[0].Value);
        }

        private void RefrescarTabla()
        {
            var pedidos = controlador.ObtenerPedidosPanel();

            bool mismasFilas = dgv.Rows.Count == pedidos.Count;
            for (int i = 0; mismasFilas && i < pedidos.Count; i++)
            {
                if (Convert.ToInt32(dgv.Rows[i].Cells[0].Value) != pedidos[i].Id) mismasFilas = false;
            }

            if (mismasFilas)
            {
                for (int i = 0; i < pedidos.Count; i++) PonerFila(dgv.Rows[i], pedidos[i]);
                return;
            }

            int idSeleccionado = PedidoSeleccionado();
            dgv.Rows.Clear();
            foreach (var pedido in pedidos)
            {
                int i = dgv.Rows.Add();
                PonerFila(dgv.Rows[i], pedido);
                if (pedido.Id == idSeleccionado) dgv.CurrentCell = dgv.Rows[i].Cells[0];
            }
        }

        private static void PonerFila(DataGridViewRow fila, Pedido pedido)
        {
            fila.Cells[0].Value = pedido.Id;
            fila.Cells[1].Value = pedido.NombreCliente;
            fila.Cells[2].Value = pedido.NombrePlato;
            fila.Cells[3].Value = pedido.HoraPedido.ToString("HH:mm:ss");
            fila.Cells[4].Value = pedido.Notificado ? "-" : FormatearTiempo(pedido.TiempoRestante(DateTime.Now));
            fila.Cells[5].Value = pedido.Notificado ? Textos.ListoAvisado : Textos.EnPreparacion;
            fila.DefaultCellStyle.ForeColor = pedido.Notificado ? Color.Gray : Color.Black;
        }

        private static string FormatearTiempo(TimeSpan tiempo)
        {
            return ((int)tiempo.TotalMinutes).ToString("00") + ":" + tiempo.Seconds.ToString("00");
        }

        // Puede llamarse desde los hilos del bot, por eso se pasa al hilo de la ventana.
        private void Log(string texto)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(() => Log(texto))); } catch { }
                return;
            }
            txtLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + texto + Environment.NewLine);
        }
    }
}
