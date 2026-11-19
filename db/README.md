# Base de datos (PostgreSQL)

El bot guarda todo en PostgreSQL: clientes, menú, pedidos y una cola de avisos
pendientes de enviar por Telegram. Los `.txt` anteriores ya no se usan
(`03_migracion_txt.sql` los importa una sola vez).

## 1. Requisitos

- PostgreSQL 12 o superior (probado contra el dialecto de OCI/Neon/Supabase/local).
- Un usuario con permiso para crear tablas en su esquema.

## 2. Crear el esquema y el menú

Con `psql`:

```bash
psql "postgresql://USUARIO:CLAVE@HOST:5432/restaurante?sslmode=require" \
     -v ON_ERROR_STOP=1 -f 01_esquema.sql

psql "postgresql://USUARIO:CLAVE@HOST:5432/restaurante?sslmode=require" \
     -v ON_ERROR_STOP=1 -f 02_menu.sql
```

Desde DBeaver/pgAdmin: abre cada archivo y ejecútalo como script (los dos son
idempotentes, puedes repetirlos sin miedo).

Al terminar debes ver cuatro tablas: `clientes`, `platos`, `pedidos`,
`notificaciones`.

## 3. Configurar la cadena de conexión

> **Estado actual del proyecto:** `BotTelegram/App.config` ya apunta a la
> instancia de OCI (`157.137.206.223`, base `restaurante`, SSL obligatorio) y la
> base ya está creada con `01_esquema.sql` y `02_menu.sql`. Lo de abajo queda
> como referencia por si algún día cambias de servidor.

En `BotTelegram/App.config`, dentro de `<connectionStrings>`, reemplaza
`Host`, `Database`, `Username` y `Password`:

```xml
<add name="Restaurante"
     connectionString="Host=HOST;Port=5432;Database=restaurante;Username=USUARIO;Password=CLAVE;SSL Mode=Require;Trust Server Certificate=true;Timeout=15;Command Timeout=30;Pooling=true;Maximum Pool Size=10"
     providerName="Npgsql" />
```

Ejemplos de `SSL Mode`:

| Proveedor | Valor recomendado |
|---|---|
| Neon / Supabase | `Require` |
| OCI Database with PostgreSQL | `VerifyFull` si cargaste la CA, si no `Require` |
| PostgreSQL local | `Disable` (o no pongas la propiedad) |

`Trust Server Certificate=true` acepta certificados que el sistema no puede
validar; sirve para desarrollo. Si tu proveedor te da una CA, cárgala y usa
`SSL Mode=VerifyFull` sin esa propiedad.

## 4. Migrar los datos viejos (opcional)

Necesita `psql` porque usa `\copy` (los archivos están en tu PC, no en el
servidor). Copia `chats.txt` y `pedidos.txt` a una carpeta, entra en ella y:

```bash
psql "postgresql://USUARIO:CLAVE@HOST:5432/restaurante?sslmode=require" \
     -v ON_ERROR_STOP=1 -f 03_migracion_txt.sql
```

El script respeta los ids originales, conserva los cancelados, acepta fechas en
ticks de .NET y crea como inactivos los platos que ya no estén en el menú. Al
final imprime cuántas filas quedaron y cuántas líneas descartó.

## 5. Volver a empezar

```bash
psql "..." -f 04_rollback.sql   # borra las cuatro tablas
```

## Notas de diseño

- **Fechas sin zona horaria.** Todas las columnas son `timestamp` y guardan la
  hora local del restaurante, igual que hacía el código con `DateTime.Now`.
  Npgsql exige `DateTimeKind.Unspecified` al escribir; por eso el repositorio
  normaliza con `SqlPostgres.SinZona`.
  La base **no** usa `now()` ni `localtimestamp` para los datos del negocio: la
  aplicación es la única que pone la hora. Esto es importante porque el servidor
  de OCI está en UTC y el restaurante en UTC-5; si la base decidiera la hora, los
  avisos se adelantarían 5 horas. `05_consultas.sql` fija la zona al inicio para
  que las consultas manuales también cuadren.
  Si algún día el bot atiende varias zonas horarias, hay que pasar a
  `timestamptz` y a UTC en toda la aplicación.
- **La columna `estado` es generada.** No se escribe a mano: sale de
  `notificado_en` / `cancelado_en`. Así es imposible que un pedido quede en dos
  estados a la vez (además hay un `check`).
- **`notificaciones` es una bandeja de salida.** Si Telegram falla, el aviso
  queda en `pendiente` con espera creciente (30 s, 60 s, 2 min… hasta 10 min) y
  se reintenta solo. Tras 6 intentos queda `fallida` y se ve en
  `05_consultas.sql`.
- **Los pedidos no se borran.** Cancelar solo marca `cancelado_en`; el
  historial y las estadísticas siguen funcionando.
