-- ============================================================================
--  Consultas útiles para revisar la base a mano (DBeaver, pgAdmin, psql).
--  No las usa la aplicación; son para depurar y para mostrar el proyecto.
--
--  Las fechas del restaurante están en hora local, pero el servidor puede estar
--  en otra zona (por ejemplo UTC). Esta línea hace que localtimestamp coincida
--  con el restaurante en esta sesión. Ajusta la zona si no estás en Colombia.
-- ============================================================================

set time zone 'America/Bogota';

-- 1. Pedidos que el panel muestra ahora mismo (pendientes primero)
select id, chat_id, cliente_nombre, plato_nombre, minutos,
       hora_pedido, hora_listo, estado
from pedidos
where cancelado_en is null
order by notificado_en nulls first, hora_listo;

-- 2. Cuenta atrás de cada pedido pendiente
select id, cliente_nombre, plato_nombre,
       greatest(hora_listo - localtimestamp, interval '0') as falta
from pedidos
where notificado_en is null and cancelado_en is null
order by hora_listo;

-- 3. Bandeja de salida: qué está pendiente de enviar y qué falló
select id, pedido_id, tipo, chat_id, estado, intentos,
       proximo_intento_en, left(coalesce(ultimo_error, ''), 80) as error
from notificaciones
where estado <> 'enviada'
order by estado, proximo_intento_en;

-- 4. Estadísticas equivalentes al resumen de la ventana Historial
select count(*)                                                        as total,
       count(*) filter (where estado = 'en_preparacion')                as en_preparacion,
       count(*) filter (where estado = 'listo')                         as listos,
       count(*) filter (where estado = 'cancelado')                     as cancelados,
       count(distinct cliente_nombre) filter (where estado <> 'cancelado') as clientes
from pedidos;

-- 5. Plato más pedido (sin contar cancelados)
select plato_nombre, count(*) as veces
from pedidos
where cancelado_en is null
group by plato_nombre
order by veces desc, plato_nombre
limit 5;

-- 6. Pedidos por día (últimos 14 días)
select hora_pedido::date as dia, count(*) as pedidos
from pedidos
where hora_pedido >= localtimestamp - interval '14 days'
group by 1
order by 1 desc;

-- 7. Reencolar un aviso que quedó 'fallida' (poner el id de la notificación)
-- update notificaciones
--    set estado = 'pendiente', intentos = 0,
--        proximo_intento_en = localtimestamp, ultimo_error = null
--  where id = 123;

-- 8. Limpieza opcional: borrar pedidos de más de un año (y sus notificaciones)
-- delete from pedidos where hora_pedido < localtimestamp - interval '1 year';
