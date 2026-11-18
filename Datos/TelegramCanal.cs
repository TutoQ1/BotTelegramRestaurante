using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Entidades;
using Entidades.Contratos;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Datos
{
    /// <summary>
    /// Único lugar que conoce Telegram.Bot. Traduce entre los tipos de Telegram
    /// y las entidades del sistema.
    /// </summary>
    public class TelegramCanal : ICanalMensajeria
    {
        private const int SegundosDeEspera = 30;
        private TelegramBotClient cliente;
        private int desplazamiento;

        public async Task<InformacionBot> ConectarAsync(string token, CancellationToken cancelacion)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            TelegramBotClient nuevoCliente = new TelegramBotClient(token);
            User bot = await nuevoCliente.GetMeAsync(cancelacion);

            cliente = nuevoCliente;
            return new InformacionBot(bot.FirstName, bot.Username);
        }

        public async Task<IReadOnlyList<MensajeEntrante>> RecibirAsync(CancellationToken cancelacion)
        {
            ExigirConexion();

            Update[] actualizaciones = await cliente.GetUpdatesAsync(
                offset: desplazamiento,
                timeout: SegundosDeEspera,
                allowedUpdates: new[] { UpdateType.Message },
                cancellationToken: cancelacion);

            List<MensajeEntrante> mensajes = new List<MensajeEntrante>();
            foreach (Update actualizacion in actualizaciones)
            {
                desplazamiento = actualizacion.Id + 1;
                if (actualizacion.Message != null && actualizacion.Message.Text != null)
                    mensajes.Add(Convertir(actualizacion.Message));
            }
            return mensajes;
        }

        public async Task EnviarAsync(long chatId, string texto, CancellationToken cancelacion)
        {
            ExigirConexion();
            await cliente.SendTextMessageAsync(chatId, texto, cancellationToken: cancelacion);
        }

        private static MensajeEntrante Convertir(Message mensaje)
        {
            string nombre = mensaje.From != null ? mensaje.From.FirstName : null;
            return new MensajeEntrante(mensaje.Chat.Id, nombre, mensaje.Text);
        }

        private void ExigirConexion()
        {
            if (cliente == null)
                throw new System.InvalidOperationException("El canal de Telegram no está conectado.");
        }
    }
}
