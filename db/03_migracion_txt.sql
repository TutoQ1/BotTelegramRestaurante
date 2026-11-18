-- ============================================================================
--  Migración de los archivos planos a PostgreSQL.
--
--  Copia chats.txt y pedidos.txt a los datos que ya existen en la base.
--  - No duplica nada: usa ON CONFLICT y respeta los ids originales.
--  - Los pedidos cancelados se conservan como cancelados.
--  - Si un pedido usa una opción de plato que ya no está en la tabla platos,
--    se crea ese plato como inactivo para no perder el historial.
--  - Acepta el formato de fecha actual (2026-11-08 12:00:00) y el antiguo en
--    ticks de .NET, igual que hacía PedidoRepositorio.
--
--  USO (obligatorio psql, por los comandos \copy):
--    1. Copia chats.txt y pedidos.txt a una carpeta.
--    2. Abre una terminal en esa carpeta.
--    3. psql "postgresql://usuario:clave@host:5432/restaurante?sslmode=require" \
--            -v ON_ERROR_STOP=1 -f 03_migracion_txt.sql
--
--  Si solo tienes uno de los dos archivos, comenta el \copy y el bloque
--  correspondiente del otro (están marcados).
-- ============================================================================

begin;

-- ---------------------------------------------------------------------------
-- 1. Leer los archivos (los lee el cliente psql, no el servidor)
-- ---------------------------------------------------------------------------
create temp table stg_chats   (linea text) on commit drop;
create temp table stg_pedidos (linea text) on commit drop;

\copy stg_chats   (linea) from 'chats.txt'   with (format text)
\copy stg_pedidos (linea) from 'pedidos.txt' with (format text)

-- Quitar BOM, retornos de carro y líneas vacías.
update stg_chats
   set linea = btrim(replace(replace(linea, E'\ufeff', ''), E'\r', ''));
update stg_pedidos
   set linea = btrim(replace(replace(linea, E'\ufeff', ''), E'\r', ''));

delete from stg_chats   where linea = '';
delete from stg_pedidos where linea = '';

-- ---------------------------------------------------------------------------
-- 2. Parsear pedidos a una tabla temporal tipada
--    Cada cast está protegido con una expresión regular dentro de un CASE,
--    así una línea dañada se descarta en vez de abortar la migración.
-- ---------------------------------------------------------------------------
create temp table mig_pedidos (
    id            bigint,
    chat_id       bigint,
    cliente       text,
    opcion        text,
    plato         text,
    minutos       integer,
    hora_pedido   timestamp,
    hora_listo    timestamp,
    notificado    boolean,
    cancelado     boolean
) on commit drop;

insert into mig_pedidos
select
    case when split_part(linea, '|', 1) ~ '^\d{1,18}$'
         then split_part(linea, '|', 1)::bigint end,
    case when split_part(linea, '|', 2) ~ '^-?\d{1,19}$'
         then split_part(linea, '|', 2)::bigint end,
    nullif(btrim(split_part(linea, '|', 3)), ''),
    nullif(btrim(split_part(linea, '|', 4)), ''),
    nullif(btrim(split_part(linea, '|', 5)), ''),
    case when split_part(linea, '|', 6) ~ '^\d{1,6}$'
         then split_part(linea, '|', 6)::integer end,
    case when split_part(linea, '|', 7) ~ '^\d{17,19}$'
              then timestamp '0001-01-01 00:00:00'
                   + ((split_part(linea, '|', 7)::bigint / 10000000) * interval '1 second')
         when split_part(linea, '|', 7) ~ '^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$'
              then split_part(linea, '|', 7)::timestamp
    end,
    case when split_part(linea, '|', 8) ~ '^\d{17,19}$'
              then timestamp '0001-01-01 00:00:00'
                   + ((split_part(linea, '|', 8)::bigint / 10000000) * interval '1 second')
         when split_part(linea, '|', 8) ~ '^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$'
              then split_part(linea, '|', 8)::timestamp
    end,
    split_part(linea, '|', 9) = '1',
    split_part(linea, '|', 10) = '1'
from stg_pedidos;

-- ---------------------------------------------------------------------------
-- 3. Clientes: primero los de chats.txt y después los que solo aparecen en
--    pedidos.txt (por si chats.txt se perdió).
-- ---------------------------------------------------------------------------
insert into clientes (chat_id, nombre, creado_en)
select chat_id, nombre, localtimestamp
from (
    select distinct on (chat_id) chat_id, nombre
    from (
        select case when split_part(linea, '|', 1) ~ '^-?\d{1,19}$'
                    then split_part(linea, '|', 1)::bigint end as chat_id,
               nullif(btrim(split_part(linea, '|', 2)), '')     as nombre
        from stg_chats
        where linea ~ '^-?\d+\|'
    ) c
    where chat_id is not null and nombre is not null
    order by chat_id
) d
on conflict (chat_id) do update
    set nombre = excluded.nombre,
        actualizado_en = localtimestamp;

insert into clientes (chat_id, nombre, creado_en)
select distinct on (chat_id) chat_id, cliente, localtimestamp
from mig_pedidos
where chat_id is not null and cliente is not null
order by chat_id, hora_pedido desc
on conflict (chat_id) do nothing;

-- ---------------------------------------------------------------------------
-- 4. Platos que aparecen en pedidos viejos y ya no están en el menú.
--    Se crean inactivos para respetar la llave foránea de pedidos.opcion.
-- ---------------------------------------------------------------------------
insert into platos (opcion, nombre, minutos, activo, orden)
select distinct on (opcion) opcion,
       coalesce(plato, 'Plato ' || opcion),
       greatest(coalesce(minutos, 1), 1),
       false,
       999
from mig_pedidos
where opcion is not null
order by opcion
on conflict do nothing;

-- ---------------------------------------------------------------------------
-- 5. Pedidos
-- ---------------------------------------------------------------------------
insert into pedidos (id, chat_id, cliente_nombre, opcion, plato_nombre, minutos,
                     hora_pedido, hora_listo, notificado_en, cancelado_en)
select m.id,
       m.chat_id,
       m.cliente,
       m.opcion,
       coalesce(m.plato, 'Plato ' || m.opcion),
       greatest(coalesce(m.minutos, 1), 1),
       m.hora_pedido,
       case when m.hora_listo < m.hora_pedido then m.hora_pedido else m.hora_listo end,
       case when m.notificado and not m.cancelado then m.hora_listo end,
       case when m.cancelado then m.hora_pedido end
from mig_pedidos m
where m.id      is not null and m.id > 0
  and m.chat_id is not null
  and m.cliente is not null
  and m.opcion  is not null
  and m.hora_pedido is not null
  and m.hora_listo  is not null
  and exists (select 1 from clientes c where c.chat_id = m.chat_id)
  and exists (select 1 from platos   p where p.opcion  = m.opcion)
on conflict (id) do nothing;

-- ---------------------------------------------------------------------------
-- 6. Dejar la secuencia del id alineada con el id máximo migrado
-- ---------------------------------------------------------------------------
select setval(
           pg_get_serial_sequence('pedidos', 'id'),
           coalesce((select max(id) from pedidos), 0) + 1,
           false
       ) as secuencia_pedidos;

-- ---------------------------------------------------------------------------
-- 7. Resumen de lo migrado
-- ---------------------------------------------------------------------------
select 'clientes' as tabla, count(*) as filas from clientes
union all
select 'platos',   count(*) from platos
union all
select 'pedidos',  count(*) from pedidos;

select count(*) as lineas_descartadas
from mig_pedidos
where id is null or chat_id is null or cliente is null
   or opcion is null or hora_pedido is null or hora_listo is null;

commit;
