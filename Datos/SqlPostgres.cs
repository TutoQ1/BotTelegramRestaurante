using System;
using Npgsql;
using NpgsqlTypes;

namespace Datos
{
    /// <summary>Detalles de PostgreSQL que no deben repetirse en cada repositorio.</summary>
    internal static class SqlPostgres
    {
        /// <summary>
        /// La aplicación trabaja con la hora local del restaurante y las columnas
        /// son 'timestamp' (sin zona). Npgsql exige DateTimeKind.Unspecified para
        /// escribirlas: con Kind=Local lanza una excepción.
        /// </summary>
        public static DateTime SinZona(DateTime valor)
        {
            return DateTime.SpecifyKind(valor, DateTimeKind.Unspecified);
        }

        /// <summary>Agrega un parámetro de fecha que admite null.</summary>
        public static void AgregarFecha(NpgsqlParameterCollection parametros, string nombre, DateTime? valor)
        {
            NpgsqlParameter parametro = parametros.Add(nombre, NpgsqlDbType.Timestamp);
            parametro.Value = valor.HasValue ? (object)SinZona(valor.Value) : DBNull.Value;
        }
    }
}
