using System;
using System.Configuration;
using Npgsql;

namespace Datos
{
    /// <summary>
    /// Punto único de conexión a PostgreSQL. La cadena se lee de App.config
    /// (sección connectionStrings, nombre "Restaurante").
    /// </summary>
    internal static class ConexionPostgres
    {
        public const string NombreCadena = "Restaurante";

        public static NpgsqlConnection Abrir()
        {
            ConnectionStringSettings configuracion = ConfigurationManager.ConnectionStrings[NombreCadena];
            if (configuracion == null || string.IsNullOrWhiteSpace(configuracion.ConnectionString))
                throw new InvalidOperationException(
                    "Falta la cadena de conexión '" + NombreCadena + "' en el App.config de la aplicación.");

            NpgsqlConnection conexion = new NpgsqlConnection(configuracion.ConnectionString);
            try
            {
                conexion.Open();
                return conexion;
            }
            catch
            {
                conexion.Dispose();
                throw;
            }
        }
    }
}
