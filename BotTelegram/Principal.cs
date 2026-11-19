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
            Width = 1050;
            Height = 700;
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Tema.EstilizarVentana(this);

            // ---- Encabezado oscuro con el estado del bot ----
            var encabezado = Tema.Encabezado("Panel de pedidos", "Restaurante · Bot de Telegram");
            lblEstado = new Label
            {
                Text = "● Bot detenido",
                Dock = DockStyle.Right,
                Width = 200,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = Tema.Negrita,
                ForeColor = Tema.TextoSobreOscuro,
                BackColor = Color.Transparent
            };
            encabezado.Controls.Add(lblEstado);

            // ---- Barra del token ----
            var panelTop = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Tema.Crema };
            txtToken = new TextBox { Left = 100, Top = 18, Width = 400, UseSystemPasswordChar = true };
            Tema.EstilizarCampo(txtToken);
            btnIniciar = new Button { Text = "Iniciar bot", Left = 520, Top = 14, Width = 120, Enabled = true };
            btnDetener = new Button { Text = "Detener", Left = 650, Top = 14, Width = 110, Enabled = false };
            Tema.BotonPrimario(btnIniciar);
            Tema.BotonSecundario(btnDetener, Tema.Oscuro);
            panelTop.Controls.Add(Tema.Etiqueta("Token", 24, 22));
            panelTop.Controls.AddRange(new Control[] { txtToken, btnIniciar, btnDetener });

            // ---- Registro de actividad (abajo) ----
            var panelLog = new Panel { Dock = DockStyle.Bottom, Height = 190, Padding = new Padding(24, 0, 24, 20), BackColor = Tema.Crema };
            var fondoLog = new Panel { Dock = DockStyle.Fill, Padding = new Padding(14, 12, 8, 10), BackColor = Tema.Oscuro };
            txtLog = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                BackColor = Tema.Oscuro,
                ForeColor = Tema.Crema,
                Font = new Font("Consolas", 9.5f)
            };
            fondoLog.Controls.Add(txtLog);
            var tituloLog = new Label
            {
                Text = "Registro de actividad",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = Tema.Negrita,
                ForeColor = Tema.Oscuro
            };
            panelLog.Controls.Add(fondoLog);
            panelLog.Controls.Add(tituloLog);

            // ---- Botones de acción ----
            var panelBotones = new Panel { Dock = DockStyle.Bottom, Height = 62, Padding = new Padding(24, 12, 24, 12), BackColor = Tema.Crema };
            btnListo = new Button { Text = "Marcar como listo", Left = 24, Top = 12, Width = 180 };
            btnCancelar = new Button { Text = "Cancelar pedido", Left = 214, Top = 12, Width = 170 };
            btnHistorial = new Button { Text = "Historial", Width = 130, Dock = DockStyle.Right };
            Tema.BotonPrimario(btnListo);
            Tema.BotonSecundario(btnCancelar, Tema.Acento);
            Tema.BotonSecundario(btnHistorial, Tema.Oscuro);
            panelBotones.Controls.AddRange(new Control[] { btnListo, btnCancelar, btnHistorial });

            // ---- Tabla de pedidos ----
            var panelTabla = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 4, 24, 8), BackColor = Tema.Crema };
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            Tema.EstilizarGrid(dgv);
            dgv.Columns.Add("id", "N°");
            dgv.Columns.Add("cliente", "Cliente");
            dgv.Columns.Add("plato", "Plato");
            dgv.Columns.Add("hora", "Hora del pedido");
            dgv.Columns.Add("faltan", "Tiempo restante");
            dgv.Columns.Add("estado", "Estado");
            dgv.Columns[0].FillWeight = 30;
            Tema.CentrarColumna(dgv.Columns[0]);
            Tema.CentrarColumna(dgv.Columns[3]);
            Tema.CentrarColumna(dgv.Columns[4]);
            panelTabla.Controls.Add(dgv);

            // El último control agregado queda en el borde: se agregan de adentro hacia afuera.
            Controls.Add(panelTabla);
            Controls.Add(panelBotones);
            Controls.Add(panelLog);
            Controls.Add(panelTop);
            Controls.Add(encabezado);
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
            lblEstado.ForeColor = Tema.TextoSobreOscuro;
        }

        private void MostrarBotActivo()
        {
            lblEstado.Text = "● Bot activo";
            lblEstado.ForeColor = Tema.AcentoSobreOscuro;
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
            Tema.EstiloFila(fila, 5, pedido.Cancelado, pedido.Notificado);
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
