using System.Collections.Generic;
using System.Linq;
using Entidades;
using Entidades.Contratos;

namespace Logica
{
    public class ClienteServicio
    {
        private readonly object candado = new object();
        private readonly IClienteRepositorio repositorio;
        private readonly List<Cliente> clientes;

        public ClienteServicio(IClienteRepositorio repositorio)
        {
            this.repositorio = repositorio;
            clientes = repositorio.Cargar();
        }

        public void RegistrarSiEsNuevo(long chatId, string nombre)
        {
            lock (candado)
            {
                if (clientes.Any(c => c.ChatId == chatId))
                    return;

                // Se guarda antes de agregarlo a la caché: pedidos.chat_id tiene
                // llave foránea contra clientes, así que el cliente debe existir.
                Cliente cliente = new Cliente(chatId, nombre);
                repositorio.Insertar(cliente);
                clientes.Add(cliente);
            }
        }
    }
}
