-- ============================================================================
--  Deshacer TODO: borra las cuatro tablas del bot y sus índices.
--  Úsalo solo en desarrollo. No hay vuelta atrás.
-- ============================================================================

begin;

drop table if exists notificaciones cascade;
drop table if exists pedidos        cascade;
drop table if exists platos         cascade;
drop table if exists clientes       cascade;

-- El índice pg_trgm se va con la tabla pedidos; la extensión se deja instalada
-- porque puede usarla otra base del mismo servidor. Si quieres quitarla:
--   drop extension if exists pg_trgm;

commit;
