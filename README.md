# BotTelegramRestaurante

Bot de Telegram para tomar pedidos de un restaurante, con panel de escritorio en
Windows Forms. El cliente pide desde el chat, el panel controla la cocina y el bot
avisa cuando el pedido está listo. Todo se guarda en **PostgreSQL**.

## Qué hace

- El cliente escribe `/menu` (o cualquier texto) y recibe la carta con las letras.
- Responde con la letra del plato y el pedido queda registrado con su hora estimada.
- Comandos del cliente: `/menu`, `/estado`, `/cancelar`.
- El panel muestra los pedidos pendientes y los listos, permite marcarlos como
  listos antes de tiempo o cancelarlos.
- Al cumplirse el tiempo, el bot envía el aviso de "pedido listo".
- Si Telegram falla, el aviso **no se pierde**: queda en una cola en la base y se
  reintenta con espera creciente hasta 6 veces.

## Requisitos

- Windows con .NET Framework 4.7.2 (viene incluido en Windows 10/11).
- Visual Studio 2022 o superior (o MSBuild) para compilar.
- PostgreSQL 12 o superior: local, OCI, Neon, Supabase, etc.
- Un token de bot de [@BotFather](https://t.me/BotFather).

## Puesta en marcha

1. **Base de datos.** Ejecuta `db/01_esquema.sql` y `db/02_menu.sql` en tu
   PostgreSQL. Instrucciones completas en [`db/README.md`](db/README.md).
   Si vienes de los `.txt`, el paso 4 de ese documento los importa.
2. **Cadena de conexión.** Edita `BotTelegram/App.config` y reemplaza `Host`,
   `Database`, `Username` y `Password` en la cadena `Restaurante`.
3. **Compilar.** Abre `BotTelegram.slnx` en Visual Studio y compila. Si Visual
   Studio marca que falta Npgsql, usa *Restaurar paquetes NuGet*.
4. **Ejecutar.** Abre `BotTelegram.exe`, pega el token y pulsa *Iniciar*.

Si la base no responde, la aplicación lo dice al arrancar con el detalle del
error en vez de abrirse vacía.

## Estructura (por capas)

| Capa | Responsabilidad | Depende de |
|---|---|---|
| `Entidades` | Datos del dominio (`Pedido`, `Plato`, `Cliente`) y los contratos (interfaces). | — |
| `Datos` | Implementaciones: repositorios de PostgreSQL y de archivos, canal de Telegram. | `Entidades` |
| `Logica` | Reglas del negocio: servicios, comandos del bot, notificador, controlador y fábrica. | `Entidades` |
| `BotTelegram` | Ventanas de Windows Forms. | `Logica` |

La regla se respeta: los formularios solo hablan con `RestauranteControlador`, y
`Logica` solo conoce las interfaces de `Entidades`. Cambiar de motor de base de
datos significa escribir nuevos repositorios, no tocar la lógica.

### Clases clave

- `RestauranteFabrica`: arma todo en un solo lugar.
- `ProcesadorMensajes` + `Comandos/`: cada comando es una clase; agregar uno es
  crear la clase y registrarla en la fábrica.
- `PedidoServicio`: caché en memoria de los pedidos, con candado, respaldada por
  la tabla `pedidos` en cada cambio.
- `NotificadorPedidos`: revisa los pedidos vencidos, los encola y envía la cola.
- `INotificacionRepositorio`: la cola de avisos (`tabla notificaciones`).

## Base de datos

Cuatro tablas: `clientes`, `platos`, `pedidos` y `notificaciones`. Los detalles de
diseño, los índices y las consultas útiles están en [`db/`](db/README.md):

- `01_esquema.sql`: tablas, columna generada `estado`, cola e índices.
- `02_menu.sql`: menú inicial (idempotente).
- `03_migracion_txt.sql`: importa `chats.txt` y `pedidos.txt`.
- `04_rollback.sql`: borra todo (solo desarrollo).
- `05_consultas.sql`: consultas de apoyo y limpieza.

Todas las fechas se guardan como hora local del restaurante (`timestamp` sin zona
horaria). El id de cada pedido lo asigna la base de datos.

## Paquetes NuGet

- `Npgsql` 8.0.9: último driver compatible con .NET Framework 4.7.2.
- `Telegram.Bot` 18.0.0.

Los paquetes están en `packages/`. Si clonas el proyecto en otra máquina, Visual
Studio los restaura desde `packages.config`.
