# Corrección aplicada del catálogo en Azure

28/10/2026. Rama inspeccionada: Rama--Facu, HEAD 767784b.

## Resultado de la ejecución autorizada

El usuario autorizó ejecutar las acciones requeridas para TotaltechApp. El script se ejecutó contra cancho / TotatechDB el 28/10/2026 a las 17:46:12 UTC (14:46:12 Argentina).

- Antes: ImagenUrl ausente, 11 productos, 6 categorías, 5 registros de migraciones.
- Después: ImagenUrl nvarchar(500) NULL (1000 bytes máximos), mismos conteos y 0 productos huérfanos.
- GET /productos/catalogo: HTTP 200, totalItems=11, items=11, página 1, tamaño 12, campo imagenUrl presente.
- GET /productos/catalogo?pagina=2&tamanoPagina=3: HTTP 200, 3 elementos, página 2, totalItems=11.
- GET /productos/catalogo?idCategoria=11: HTTP 200, 5 elementos.
- GET /productos/catalogo?soloDisponibles=true&precioMin=0: HTTP 200, 11 elementos.
- GET /categorias/: HTTP 200, 6 categorías, hash SHA256 del cuerpo idéntico antes y después.
- Descarga posterior de eventlog.xml: 0 nuevas excepciones SQL, de arranque o no controladas desde la ejecución en el registro disponible.
- No se modificó código ni workflow; no fue necesario desplegar o reiniciar. Se conservó __EFMigrationsHistory y Database__ApplyMigrations=false.

Preflight de respaldo: Azure Resource Manager informó restorePointType=CONTINUOUS y retención de 7 días. Se registró como referencia anterior al cambio 2026-10-28T17:30:45Z, posterior al earliestRestoreDate informado 2026-10-21T17:45:46.5945681Z. Se verificó por API en lugar del portal. No se creó una base nueva ni se ensayó una restauración, por lo que esta evidencia no acredita una prueba de recuperación completa.

Evidencias locales adicionales, sin secretos: publish/imagenurl-preflight.json, publish/imagenurl-sql-before.json, publish/imagenurl-execution.json y publish/imagenurl-http-validation.json (directorio ignorado por Git).

Estado del incidente del catálogo: resuelto. La conciliación completa entre el historial de junio y las migraciones actuales permanece pendiente; no activar migraciones automáticas ni inferir que otros módulos no verificados están íntegramente actualizados.

## Evidencia

Reproducción pública del catálogo: HTTP 500. Los eventos descargados inmediatamente después, en LogFiles/eventlog.xml, registran a las 17:39:59 UTC (14:39:59 Argentina):

```text
Microsoft.Data.SqlClient.SqlException (0x80131904): Invalid column name 'ImagenUrl'.
Error Number:207,State:1,Class:16
```

El comienzo de esta revisión coincidió con una reanudación de Azure SQL; una petición a categorías agotó su tiempo de espera. La base pasó por Resuming y la conexión SQL posterior permitió todas las lecturas de diagnóstico. Repetir ambos endpoints al finalizar el cambio.

ProductosRepositorio.ObtenerCatalogoAsync cuenta en la línea 114 y materializa la proyección en las líneas 115–133. La proyección lee Producto.ImagenUrl en la línea 125; Producto.cs:17 declara string nullable con MaxLength(500). La migración 20260929214228_AgregarImagenProducto agrega nvarchar(500) NULL.

Comprobaciones SQL de solo lectura sobre TotatechDB:

- dbo.Productos, dbo.Categorias y dbo.Proveedores existen.
- Las columnas proyectadas existen salvo Productos.ImagenUrl.
- COUNT_BIG de productos: 11; no prueba por sí solo CountAsync, pero confirma que el conteo SQL básico no depende de la columna faltante.
- JOIN Producto–Categoria–Proveedor: 11 filas; productos huérfanos: 0.
- Ambas claves foráneas están habilitadas y son trusted.
- La proyección paginada equivalente sin ImagenUrl compila y devuelve 11 filas (se emitió solo el conteo).
- __EFMigrationsHistory contiene cinco migraciones de junio: Inicial, CorregirRelaciones, AgregarDetalleCarrito, AgregarConsultasYFlujos y AjustarContratosValidacionesYRestricciones.
- El historial no contiene las migraciones actuales de septiembre y octubre.

No es necesario agregar Include ni modificar CountAsync o las relaciones para resolver el error demostrado. No hay evidencia de un fallo de traducción EF; SQL está rechazando una columna inexistente.

## Alcance mínimo propuesto

Ejecutar Totaltech-ImagenUrl.sql únicamente contra TotatechDB, después de autorización y respaldo verificado. Agrega una columna nullable sin actualizar ni borrar filas. Valida el destino, tipo, longitud, nullability y conteo de productos. Si hay error o bloqueo superior a diez segundos, aborta y revierte la transacción.

No cambia código, CI/CD, categorías ni __EFMigrationsHistory. No declarar conciliado el historial por agregar esta columna. La migración inicial actual intenta crear tablas existentes: mantener Database__ApplyMigrations=false y planificar por separado una comparación completa de esquema antes de reconciliar el historial o aplicar GestiónAdministrativaUsuarios.

## Verificación y respaldo previos a la ejecución

1. Repetir las lecturas de esquema, historial, conteos y estado de la base. Confirmar que el despliegue siga usando la misma entidad y proyección.
2. Azure SQL informó retención de respaldos automáticos de 7 días, redundancia Local y earliestRestoreDate 2026-10-21T17:39:24.159842 UTC. Esto confirma configuración y límite inferior, no una restauración ensayada.
3. Antes del ALTER, verificar en Portal → SQL databases → TotatechDB → Restore, o mediante la API restorePoints de Azure Resource Manager, el intervalo de restauración continuo y registrar una hora UTC anterior al cambio. No iniciar una restauración en esta fase. Si se necesita ensayar recuperación o una copia independiente, solicitar autorización para una base temporal y su posible costo; no crear recursos automáticamente.
4. Registrar el conteo previo de productos y categorías y conservar el resultado de las verificaciones sin datos sensibles. Evitar escrituras concurrentes durante la breve ventana de cambio.
5. Si no se confirma un punto de restauración previo utilizable, no ejecutar el script.

## Después de la ejecución autorizada

1. Verificar sys.columns: nvarchar, max_length=1000 bytes, nullable.
2. Confirmar que los conteos de productos y categorías no cambiaron y que las relaciones siguen íntegras.
3. GET /productos/catalogo: HTTP 200 y JSON con Items/paginación según el contrato serializado (camelCase), totalItems=11 si no hubo cambios de negocio; ImagenUrl puede ser null.
4. GET /categorias/: HTTP 200 y mismo contenido/conteo que antes del cambio.
5. Repetir el GET con filtro/paginación y revisar eventlog.xml para esas peticiones: sin nuevas excepciones SQL 207.
6. No se requiere redeploy para que SQL reconozca la columna; confirmar que workflow y working tree no tengan cambios funcionales accidentales.

## Recuperación

Ante errores dentro del script, TRY/CATCH y XACT_ABORT revierten el cambio. Si aparece otro fallo tras el commit, conservar los datos y diagnosticar antes de actuar. No eliminar la columna como rollback automático, porque podría contener datos nuevos. Una restauración a un punto anterior crea otra base y requiere autorización, validación y un cambio de conexión controlado.
