using System;
using System.Collections.Generic;
using Entidades;
using Entidades.Contratos;
using Npgsql;

namespace Datos
{
    /// <summary>Clientes de Telegram (tabla clientes).</summary>
    public class ClienteRepositorioPostgres : IClienteRepositorio
    {
        public List<Cliente> Cargar()
        {
            List<Cliente> clientes = new List<Cliente>();

            using (NpgsqlConnection conexion = ConexionPostgres.Abrir())
            using (NpgsqlCommand comando = new NpgsqlCommand(
                "select chat_id, nombre from clientes order by nombre", conexion))
            using (NpgsqlDataReader lector = comando.ExecuteReader())
            {
                while (lector.Read())
                    clientes.Add(new Cliente(lector.GetInt64(0), lector.GetString(1)));
            }

            return clientes;
        }

        public void Insertar(Cliente cliente)
        {
            const string sql = @"
insert into clientes (chat_id, nombre, creado_en, actualizado_en)
values (@chat_id, @nombre, @ahora, @ahora)
on conflict (chat_id) do update
   set nombre = excluded.nombre,
       actualizado_en = @ahora;";

            using (NpgsqlConnection conexion = ConexionPostgres.Abrir())
            using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("chat_id", cliente.ChatId);
                comando.Parameters.AddWithValue("nombre", cliente.Nombre);
                SqlPostgres.AgregarFecha(comando.Parameters, "ahora", DateTime.Now);
                comando.ExecuteNonQuery();
            }
        }
    }
}
