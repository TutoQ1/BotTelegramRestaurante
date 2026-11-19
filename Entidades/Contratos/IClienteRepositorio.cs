using System.Collections.Generic;

namespace Entidades.Contratos
{
    public interface IClienteRepositorio
    {
        List<Cliente> Cargar();

        /// <summary>Inserta el cliente o actualiza su nombre si el chat ya existe.</summary>
        void Insertar(Cliente cliente);
    }
}
