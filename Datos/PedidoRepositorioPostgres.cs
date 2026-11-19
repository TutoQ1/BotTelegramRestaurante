using System;
using System.Collections.Generic;
using Entidades;
using Entidades.Contratos;
using Npgsql;

namespace Datos
{
    /// <summary>
    /// Pedidos en PostgreSQL. El id lo asigna la base (identity) y las horas se
    /// guardan tal cual llegan, como hora local sin zona horaria.
    /// </summary>
    public class PedidoRepositorioPostgres : IPedidoRepositorio
    {
        private const string Seleccion = @"
select p.id,
       p.chat_id,
       p.cliente_nombre,
       p.opcion,
       p.plato_nombre,
       p.minutos,
       p.hora_pedido,
       p.hora_listo,
       (p.notificado_en is not null) as notificado,
       (p.cancelado_en is not null)  as cancelado
from pedidos p";

        public List<Pedido> Cargar()
        {
            List<Pedido> pedidos = new List<Pedido>();

            using (NpgsqlConnection conexion = ConexionPostgres.Abrir())
            using (NpgsqlCommand comando = new NpgsqlCommand(Seleccion + " order by p.id", conexion))
            using (NpgsqlDataReader lector = comando.ExecuteReader())
            {
                while (lector.Read())
                    pedidos.Add(Leer(lector));
            }

            return pedidos;
        }

        public Pedido Insertar(Pedido pedido)
        {
            const string sql = @"
insert into pedidos (chat_id, cliente_nombre, opcion, plato_nombre, minutos,
                     hora_pedido, hora_listo, notificado_en, cancelado_en)
values (@chat_id, @cliente_nombre, @opcion, @plato_nombre, @minutos,
        @hora_pedido, @hora_listo, @notificado_en, @cancelado_en)
returning id;";

            using (NpgsqlConnection conexion = ConexionPostgres.Abrir())
            using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("chat_id", pedido.ChatId);
                comando.Parameters.AddWithValue("cliente_nombre", pedido.NombreCliente);
                comando.Parameters.AddWithValue("opcion", pedido.Opcion);
                comando.Parameters.AddWithValue("plato_nombre", pedido.NombrePlato);
                comando.Parameters.AddWithValue("minutos", pedido.Minutos);
                SqlPostgres.AgregarFecha(comando.Parameters, "hora_pedido", pedido.HoraPedido);
                SqlPostgres.AgregarFecha(comando.Parameters, "hora_listo", pedido.HoraListo);
                SqlPostgres.AgregarFecha(comando.Parameters, "notificado_en",
                    pedido.Notificado ? (DateTime?)DateTime.Now : null);
                SqlPostgres.AgregarFecha(comando.Parameters, "cancelado_en",
                    pedido.Cancelado ? (DateTime?)DateTime.Now : null);

                int id = Convert.ToInt32(comando.ExecuteScalar());

                return new Pedido(id, pedido.ChatId, pedido.NombreCliente, pedido.Opcion, pedido.NombrePlato,
                                  pedido.Minutos, pedido.HoraPedido, pedido.HoraListo,
                                  pedido.Notificado, pedido.Cancelado);
            }
        }

        public void Actualizar(Pedido pedido)
        {
            const string sql = @"
update pedidos
   set hora_listo    = @hora_listo,
       notificado_en = case when @notificado then coalesce(notificado_en, @ahora) end,
       cancelado_en  = case when @cancelado  then coalesce(cancelado_en,  @ahora) end
 where id = @id;";

            using (NpgsqlConnection conexion = ConexionPostgres.Abrir())
            using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("id", pedido.Id);
                SqlPostgres.AgregarFecha(comando.Parameters, "hora_listo", pedido.HoraListo);
                SqlPostgres.AgregarFecha(comando.Parameters, "ahora", DateTime.Now);
                comando.Parameters.AddWithValue("notificado", pedido.Notificado);
                comando.Parameters.AddWithValue("cancelado", pedido.Cancelado);

                if (comando.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException(
                        "No existe el pedido #" + pedido.Id + " en la base de datos.");
            }
        }

        private static Pedido Leer(NpgsqlDataReader lector)
        {
            return new Pedido(
                Convert.ToInt32(lector.GetInt64(0)),
                lector.GetInt64(1),
                lector.GetString(2),
                lector.GetString(3),
                lector.GetString(4),
                lector.GetInt32(5),
                lector.GetDateTime(6),
                lector.GetDateTime(7),
                lector.GetBoolean(8),
                lector.GetBoolean(9));
        }
    }
}
