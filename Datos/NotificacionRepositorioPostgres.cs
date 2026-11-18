using System;
using System.Collections.Generic;
using Entidades;
using Entidades.Contratos;
using Npgsql;

namespace Datos
{
    /// <summary>
    /// Bandeja de salida de avisos al cliente (tabla notificaciones).
    /// Encolar es idempotente gracias al índice único (pedido_id, tipo).
    /// </summary>
    public class NotificacionRepositorioPostgres : INotificacionRepositorio
    {
        public bool Encolar(long pedidoId, string tipo, long chatId, string texto)
        {
            const string sql = @"
insert into notificaciones (pedido_id, tipo, chat_id, texto, proximo_intento_en, creado_en)
values (@pedido_id, @tipo, @chat_id, @texto, @ahora, @ahora)
on conflict (pedido_id, tipo) do nothing;";

            using (NpgsqlConnection conexion = ConexionPostgres.Abrir())
            using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("pedido_id", pedidoId);
                comando.Parameters.AddWithValue("tipo", tipo);
                comando.Parameters.AddWithValue("chat_id", chatId);
                comando.Parameters.AddWithValue("texto", texto);
                SqlPostgres.AgregarFecha(comando.Parameters, "ahora", DateTime.Now);
                return comando.ExecuteNonQuery() > 0;
            }
        }

        public List<NotificacionPendiente> ObtenerPendientes(DateTime ahora, int limite)
        {
            const string sql = @"
select id, pedido_id, tipo, chat_id, texto, intentos
from notificaciones
where estado = 'pendiente'
  and proximo_intento_en <= @ahora
order by proximo_intento_en, id
limit @limite;";

            List<NotificacionPendiente> pendientes = new List<NotificacionPendiente>();

            using (NpgsqlConnection conexion = ConexionPostgres.Abrir())
            using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
            {
                SqlPostgres.AgregarFecha(comando.Parameters, "ahora", ahora);
                comando.Parameters.AddWithValue("limite", limite);

                using (NpgsqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        pendientes.Add(new NotificacionPendiente(
                            lector.GetInt64(0),
                            lector.GetInt64(1),
                            lector.GetString(2),
                            lector.GetInt64(3),
                            lector.GetString(4),
                            lector.GetInt32(5)));
                    }
                }
            }

            return pendientes;
        }

        public void MarcarEnviada(long id)
        {
            const string sql = @"
update notificaciones
   set estado = 'enviada',
       enviado_en = @ahora,
       ultimo_error = null
 where id = @id;";

            using (NpgsqlConnection conexion = ConexionPostgres.Abrir())
            using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("id", id);
                SqlPostgres.AgregarFecha(comando.Parameters, "ahora", DateTime.Now);
                comando.ExecuteNonQuery();
            }
        }

        public void MarcarFallo(long id, string error, int intentosMaximos)
        {
            const string sql = @"
update notificaciones
   set intentos   = intentos + 1,
       ultimo_error = left(@error, 500),
       estado = case when intentos + 1 >= @maximo then 'fallida' else 'pendiente' end,
       -- espera creciente: 30 s, 1 min, 2 min, 4 min... con tope de 10 min
       proximo_intento_en = @ahora
           + make_interval(secs => (least(power(2, intentos + 1) * 15, 600))::int)
 where id = @id;";

            using (NpgsqlConnection conexion = ConexionPostgres.Abrir())
            using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("id", id);
                comando.Parameters.AddWithValue("error", error ?? string.Empty);
                comando.Parameters.AddWithValue("maximo", intentosMaximos);
                SqlPostgres.AgregarFecha(comando.Parameters, "ahora", DateTime.Now);
                comando.ExecuteNonQuery();
            }
        }

        public void MarcarOmitida(long id, string motivo)
        {
            const string sql = @"
update notificaciones
   set estado = 'omitida',
       ultimo_error = left(@motivo, 500)
 where id = @id;";

            using (NpgsqlConnection conexion = ConexionPostgres.Abrir())
            using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("id", id);
                comando.Parameters.AddWithValue("motivo", motivo ?? string.Empty);
                comando.ExecuteNonQuery();
            }
        }
    }
}
