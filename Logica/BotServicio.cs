using System;
using System.Threading;
using System.Threading.Tasks;
using Entidades;
using Entidades.Contratos;

namespace Logica
{
    /// <summary>Ciclo de vida del bot: conectarlo, ponerlo a trabajar y detenerlo.</summary>
    public class BotServicio
    {
        private readonly ICanalMensajeria canal;
        private readonly ReceptorMensajes receptor;
        private readonly NotificadorPedidos notificador;
        private readonly Bitacora bitacora;
        private CancellationTokenSource cancelacion;

        public BotServicio(ICanalMensajeria canal, ReceptorMensajes receptor,
                           NotificadorPedidos notificador, Bitacora bitacora)
        {
            this.canal = canal;
            this.receptor = receptor;
            this.notificador = notificador;
            this.bitacora = bitacora;
        }

        public bool Activo
        {
            get { return cancelacion != null && !cancelacion.IsCancellationRequested; }
        }

        public async Task<InformacionBot> IniciarAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new RestauranteException("Escribe el token de tu bot.");
            if (Activo)
                throw new RestauranteException("El bot ya está en ejecución.");

            InformacionBot bot;
            try
            {
                bot = await canal.ConectarAsync(token.Trim(), CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw new RestauranteException("No se pudo conectar: " + ex.Message, ex);
            }

            cancelacion = new CancellationTokenSource();
            CancellationToken senal = cancelacion.Token;
            bitacora.Registrar("Bot conectado: " + bot.Nombre + " (@" + bot.Usuario + ")");

            // Ambos bucles atrapan sus propios errores, por eso no se espera su resultado.
            _ = Task.Run(() => receptor.EjecutarAsync(senal));
            _ = Task.Run(() => notificador.EjecutarAsync(senal));
            return bot;
        }

        public void Detener()
        {
            if (!Activo)
                return;

            cancelacion.Cancel();
            bitacora.Registrar("Bot detenido.");
        }

        /// <summary>Envía un mensaje sin lanzar excepciones: si falla, solo queda en la bitácora.</summary>
        public async Task EnviarSeguroAsync(long chatId, string texto)
        {
            try
            {
                await canal.EnviarAsync(chatId, texto, CancellationToken.None);
            }
            catch (Exception ex)
            {
                bitacora.Registrar("No se pudo enviar mensaje: " + ex.Message);
            }
        }
    }
}
