-- ============================================================================
--  Menú inicial del restaurante.
--  Idempotente: si vuelves a ejecutarlo, actualiza nombre, minutos y orden
--  de los platos existentes y no toca los que agregaste a mano con otra opción.
--
--  Nota: un pedido guarda una copia (opcion, plato_nombre, minutos), así que
--  cambiar los minutos aquí NO modifica los pedidos ya registrados.
-- ============================================================================

begin;

insert into platos (opcion, nombre, minutos, activo, orden) values
    ('A', 'Hamburguesa',   1, true, 1),
    ('B', 'Perro caliente', 6, true, 2),
    ('C', 'Pizza',          7, true, 3),
    ('D', 'Ensalada',       3, true, 4)
on conflict (opcion) do update
    set nombre  = excluded.nombre,
        minutos = excluded.minutos,
        activo  = excluded.activo,
        orden   = excluded.orden;

commit;

-- Verificación rápida
select opcion, nombre, minutos, activo, orden
from platos
order by orden, opcion;
