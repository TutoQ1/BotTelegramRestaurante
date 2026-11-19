using System.Collections.Generic;

namespace Entidades.Contratos
{
    /// <summary>Origen del menú. Hoy es la tabla platos de PostgreSQL.</summary>
    public interface IPlatoRepositorio
    {
        /// <summary>Platos activos, en el orden en que se muestran al cliente.</summary>
        List<Plato> Cargar();
    }
}
