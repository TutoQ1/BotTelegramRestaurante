using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;
using Entidades.Contratos;

namespace Logica
{
    /// <summary>Catálogo de platos. Agregar un plato no obliga a tocar ninguna otra clase (OCP).</summary>
    public class MenuRestaurante
    {
        private readonly List<Plato> platos;

        public MenuRestaurante(IEnumerable<Plato> platos)
        {
            this.platos = platos.ToList();
        }

        /// <summary>Menú que vive en la tabla platos de PostgreSQL.</summary>
        public static MenuRestaurante DesdeRepositorio(IPlatoRepositorio repositorio)
        {
            List<Plato> platos = repositorio.Cargar();
            if (platos == null || platos.Count == 0)
                throw new RestauranteException(
                    "El menú está vacío: la tabla 'platos' no tiene platos activos. Ejecuta db/02_menu.sql.");

            return new MenuRestaurante(platos);
        }

        /// <summary>Menú de ejemplo, para pruebas o para arrancar sin base de datos.</summary>
        public static MenuRestaurante PorDefecto()
        {
            return new MenuRestaurante(new[]
            {
                new Plato("A", "Hamburguesa", 1),
                new Plato("B", "Perro caliente", 6),
                new Plato("C", "Pizza", 7),
                new Plato("D", "Ensalada", 3)
            });
        }

        public IReadOnlyList<Plato> Platos
        {
            get { return platos; }
        }

        public bool Existe(string opcion)
        {
            return Buscar(opcion) != null;
        }

        public Plato Buscar(string opcion)
        {
            string buscada = (opcion ?? string.Empty).Trim();
            return platos.FirstOrDefault(p => string.Equals(p.Opcion, buscada, StringComparison.OrdinalIgnoreCase));
        }
    }
}
