using System.Collections.Generic;
using System.IO;
using System.Linq;
using Entidades;
using Entidades.Contratos;

namespace Datos
{
    /// <summary>
    /// Lee y escribe chats.txt. Formato por línea: ChatId|Nombre
    /// Alternativa sin base de datos: la aplicación usa ClienteRepositorioPostgres.
    /// </summary>
    public class ClienteRepositorio : IClienteRepositorio
    {
        private const int CantidadCampos = 2;
        private readonly string ruta;

        public ClienteRepositorio()
            : this(FormatoArchivo.RutaEnCarpetaDeLaAplicacion("chats.txt"))
        {
        }

        public ClienteRepositorio(string ruta)
        {
            this.ruta = ruta;
        }

        public List<Cliente> Cargar()
        {
            List<Cliente> clientes = new List<Cliente>();
            if (!File.Exists(ruta))
                return clientes;

            foreach (string linea in File.ReadAllLines(ruta))
            {
                string[] campos = linea.Split(FormatoArchivo.Separador);
                long chatId;
                if (campos.Length >= CantidadCampos && long.TryParse(campos[0], out chatId))
                    clientes.Add(new Cliente(chatId, campos[1]));
            }
            return clientes;
        }

        public void Guardar(IEnumerable<Cliente> clientes)
        {
            File.WriteAllLines(ruta, clientes.Select(c =>
                c.ChatId + FormatoArchivo.Separador.ToString() + FormatoArchivo.Limpiar(c.Nombre)));
        }

        public void Insertar(Cliente cliente)
        {
            List<Cliente> clientes = Cargar();
            int indice = clientes.FindIndex(c => c.ChatId == cliente.ChatId);
            if (indice >= 0)
                clientes[indice] = cliente;
            else
                clientes.Add(cliente);

            Guardar(clientes);
        }
    }
}
