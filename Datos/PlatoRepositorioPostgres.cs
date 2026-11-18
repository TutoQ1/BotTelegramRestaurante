using System;
using System.Collections.Generic;
using Entidades;
using Entidades.Contratos;
using Npgsql;

namespace Datos
{
    /// <summary>Menú del restaurante (tabla platos).</summary>
    public class PlatoRepositorioPostgres : IPlatoRepositorio
    {
        public List<Plato> Cargar()
        {
            List<Plato> platos = new List<Plato>();

            using (NpgsqlConnection conexion = ConexionPostgres.Abrir())
            using (NpgsqlCommand comando = new NpgsqlCommand(
                "select opcion, nombre, minutos from platos where activo order by orden, opcion", conexion))
            using (NpgsqlDataReader lector = comando.ExecuteReader())
            {
                while (lector.Read())
                    platos.Add(new Plato(lector.GetString(0), lector.GetString(1), lector.GetInt32(2)));
            }

            return platos;
        }
    }
}
