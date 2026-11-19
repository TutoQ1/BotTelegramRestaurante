using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Entidades;
using Entidades.Contratos;
using Newtonsoft.Json.Linq;

namespace Datos
{
    /// <summary>
    /// Comunicación con la API HTTP de Telegram. Se evita depender del
    /// deserializador de Telegram.Bot para que la conexión no falle por
    /// cambios en las propiedades que Telegram agrega a sus respuestas.
    /// </summary>
    public class TelegramCanal : ICanalMensajeria
    {
        private const int SegundosDeEspera = 30;
        private static readonly HttpClient Http = CrearClienteHttp();
        private string token;
        private int desplazamiento;

        private static HttpClient CrearClienteHttp()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClient cliente = new HttpClient();
            cliente.Timeout = TimeSpan.FromSeconds(65);
            return cliente;
        }

        public async Task<InformacionBot> ConectarAsync(string token, CancellationToken cancelacion)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("El token está vacío.");

            string tokenLimpio = token.Trim();
            JObject respuesta = await SolicitarGetMeAsync(tokenLimpio, cancelacion);
            JObject resultado = respuesta["result"] as JObject;
            if (resultado == null)
                throw new InvalidOperationException("Telegram no devolvió los datos del bot.");

            this.token = tokenLimpio;
            desplazamiento = 0;
            return new InformacionBot(
                (string)resultado["first_name"] ?? "Bot",
                (string)resultado["username"] ?? string.Empty);
        }

        private static async Task<JObject> SolicitarGetMeAsync(string token, CancellationToken cancelacion)
        {
            string url = "https://api.telegram.org/bot" + token + "/getMe";
            using (HttpResponseMessage respuesta = await Http.GetAsync(url, cancelacion))
            {
                string contenido = await respuesta.Content.ReadAsStringAsync();
                return ValidarRespuesta(respuesta, contenido);
            }
        }

        public async Task<IReadOnlyList<MensajeEntrante>> RecibirAsync(CancellationToken cancelacion)
        {
            ExigirConexion();
            string url = "https://api.telegram.org/bot" + token + "/getUpdates" +
                "?offset=" + desplazamiento +
                "&timeout=" + SegundosDeEspera +
                "&allowed_updates=%5B%22message%22%5D";

            using (HttpResponseMessage respuesta = await Http.GetAsync(url, cancelacion))
            {
                string contenido = await respuesta.Content.ReadAsStringAsync();
                JObject json = ValidarRespuesta(respuesta, contenido);
                JArray actualizaciones = json["result"] as JArray;
                List<MensajeEntrante> mensajes = new List<MensajeEntrante>();
                if (actualizaciones == null)
                    return mensajes;

                foreach (JToken actualizacion in actualizaciones)
                {
                    int idActualizacion = (int?)actualizacion["update_id"] ?? 0;
                    desplazamiento = idActualizacion + 1;

                    JObject mensaje = actualizacion["message"] as JObject;
                    if (mensaje == null || mensaje["text"] == null)
                        continue;

                    JObject chat = mensaje["chat"] as JObject;
                    if (chat == null || chat["id"] == null)
                        continue;

                    JObject remitente = mensaje["from"] as JObject;
                    string nombre = remitente == null ? "Cliente" : (string)remitente["first_name"];
                    long chatId = (long)chat["id"];
                    mensajes.Add(new MensajeEntrante(chatId, nombre, (string)mensaje["text"]));
                }
                return mensajes;
            }
        }

        public async Task EnviarAsync(long chatId, string texto, CancellationToken cancelacion)
        {
            ExigirConexion();

            JArray filasTeclado = new JArray();
            if ((texto ?? string.Empty).StartsWith("¿Deseas cancelar un solo pedido", StringComparison.Ordinal))
            {
                filasTeclado.Add(new JArray("❌ Cancelar uno", "❌ Cancelar todos"));
                filasTeclado.Add(new JArray("↩️ Volver al menú"));
            }
            else if ((texto ?? string.Empty).StartsWith("Escribe el número del pedido", StringComparison.Ordinal))
            {
                string[] lineas = (texto ?? string.Empty).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string linea in lineas)
                {
                    if (linea.StartsWith("#", StringComparison.Ordinal))
                    {
                        string id = linea.Split(new[] { ' ' }, 2)[0];
                        filasTeclado.Add(new JArray(id));
                    }
                }
                filasTeclado.Add(new JArray("❌ Cancelar todos"));
                filasTeclado.Add(new JArray("↩️ Volver al menú"));
            }
            else
            {
                filasTeclado = new JArray
                {
                    new JArray("🍽 Menú", "📦 Seguimiento"),
                    new JArray("A", "B", "C", "D"),
                    new JArray("✅ Confirmar pedido", "↩️ Cancelar selección"),
                    new JArray("❌ Cancelar pedido")
                };
            }

            JObject teclado = new JObject
            {
                ["keyboard"] = filasTeclado,
                ["resize_keyboard"] = true,
                ["one_time_keyboard"] = false
            };

            Dictionary<string, string> datos = new Dictionary<string, string>
            {
                { "chat_id", chatId.ToString() },
                { "text", texto ?? string.Empty },
                { "reply_markup", teclado.ToString(Newtonsoft.Json.Formatting.None) }
            };

            string url = "https://api.telegram.org/bot" + token + "/sendMessage";
            using (FormUrlEncodedContent contenido = new FormUrlEncodedContent(datos))
            using (HttpResponseMessage respuesta = await Http.PostAsync(url, contenido, cancelacion))
            {
                string cuerpo = await respuesta.Content.ReadAsStringAsync();
                ValidarRespuesta(respuesta, cuerpo);
            }
        }

        private static JObject ValidarRespuesta(HttpResponseMessage respuesta, string contenido)
        {
            JObject json;
            try
            {
                json = JObject.Parse(contenido);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Telegram devolvió una respuesta que no se pudo leer. " + contenido, ex);
            }

            bool correcto = (bool?)json["ok"] ?? false;
            if (!respuesta.IsSuccessStatusCode || !correcto)
            {
                string descripcion = (string)json["description"] ?? respuesta.ReasonPhrase;
                throw new InvalidOperationException("Telegram: " + descripcion);
            }
            return json;
        }

        private void ExigirConexion()
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("El canal de Telegram no está conectado.");
        }
    }
}

