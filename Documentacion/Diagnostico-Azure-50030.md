# TotalTech: diagnóstico HTTP 500.30

Fecha: 27/10/2026 (Argentina). Estado: **bloqueado por configuración de secretos y validación de Azure SQL**. El fallo todavía no está solucionado en Azure.

## Causa confirmada

El archivo `LogFiles/eventlog.xml` descargado de TotaltechApp contiene esta primera excepción de la aplicación, emitida por `.NET Runtime` e `IIS AspNetCore Module V2` a las 11:22:39 y 11:29:00 de Argentina:

```text
System.InvalidOperationException: Authentication requiere Issuer, Audience y SigningKey de al menos 32 bytes. Configure la clave mediante Authentication__SigningKey o User Secrets.
```

La consulta autenticada de App Service devuelve listas vacías tanto para app settings como para connection strings. `appsettings.json` contiene Issuer y Audience, pero deliberadamente no contiene SigningKey ni DefaultConnection. La ausencia de la clave activa la excepción en `Totaltech/Program.cs:39`. Después de configurarla, faltará la conexión y se producirá la excepción de `Program.cs:87`.

Ambas excepciones se reprodujeron ejecutando el DLL publicado en Production. Se usó exclusivamente la clave ficticia ya existente en los tests; no se generaron claves reales. No se cambió la validación ni se agregaron secretos al repositorio.

## Infraestructura y persistencia verificadas

- TotaltechApp: Windows, estado administrativo Running, `netFrameworkVersion=v10.0`, proceso de 32 bits. Running no significa que ASP.NET haya iniciado.
- GET públicos a `/categorias/` y `/productos/catalogo`: HTTP 500 con página 500.30 durante esta revisión.
- Servidor existente: `cancho.database.windows.net`, grupo `Servidores`.
- Base existente: **`TotatechDB`** (nombre exacto, sin la segunda «l» de Totaltech), estado `Paused`. Confirmar que sea la base destinada a esta aplicación antes de configurarla. No se inició una conexión ni se reanudó expresamente.
- SQL: acceso de red pública habilitado y TLS mínimo 1.2. No se verificaron reglas de firewall, credenciales, esquema ni conectividad desde App Service.
- Resource Health no pudo consultarse: el proveedor respondió `Conflict/AuthError`. No se registraron proveedores ni modificaron permisos.

LocalDB solo está en `appsettings.Development.json`; no es una configuración válida para producción en Azure. El contexto utiliza SQL Server y EF Core 10.0.7. Migraciones existentes: `20260914231145_InicialActualizada`, `20260929214228_AgregarImagenProducto` y `20261008215344_GestionAdministrativaUsuarios`. No se aplicaron migraciones ni se comprobó `__EFMigrationsHistory` en Azure.

El arranque siempre consulta e inicializa categorías canónicas, aunque `Database:ApplyMigrations=false`. Sus fallos se capturan como Warning, pero conexión y reintentos pueden demorar el inicio; además puede insertar categorías cuando SQL esté disponible. Por eso una aplicación que escucha HTTP no prueba que el catálogo funcione. En una prueba negativa con conexión vacía se observó ese Warning y luego el inicio de Kestrel: no se considera una validación funcional exitosa.

`Database:ApplyMigrations` es false por defecto en Production y true en Development; cuando se habilita, un fallo de migración se relanza. `BootstrapAdmin:Enabled=true` exige credenciales y acceso SQL y puede abortar el inicio. `DemoData:Enabled=true` se rechaza expresamente fuera de Development. Mantenerlos deshabilitados para este diagnóstico.

## Cambios y Git

Raíz efectiva: repositorio interior que contiene AGENTS.md, Totaltech y Frontend. Rama local `Rama--Facu`, inicialmente limpia en `7d4dcd3`. No se modificó el repositorio exterior ni se exploró Rama--Dai, main o Develop.

Se recuperó el workflow de `origin/Rama--Facu` y se corrigió `Releasee` a `Release` en la copia de trabajo. Durante la investigación la referencia remota avanzó a `97749a5`, que ya contiene exactamente esa corrección. La copia local del workflow es idéntica a ese commit, pendiente de revisión; aparece como archivo nuevo porque HEAD local antecede a su incorporación. No duplicar la corrección al integrar con el remoto.

El workflow conserva push automático a Rama--Facu, workflow_dispatch, OIDC (`azure/login@v2`, `id-token: write`), `azure/webapps-deploy@v3`, y build/publish exclusivos de `Totaltech/Totaltech.csproj`, con artefacto `publish/api`. El typo no explica la excepción JWT del proceso desplegado.

Se agregó este informe. No se modificaron fuentes del backend, dependencias, migraciones ni configuración de Azure. Sin commit, push, merge ni cambio de rama. Los logs descargados permanecen en `publish/`, ignorado por Git; no publicarlos sin revisión.

## Validación local

Desde la raíz del repositorio interior:

```powershell
dotnet restore ./Totaltech/Totaltech.csproj
dotnet build ./Totaltech/Totaltech.csproj -c Release --no-restore
dotnet publish ./Totaltech/Totaltech.csproj -c Release -o ./publish/api
dotnet test ./Tests/Totaltech.UnitTests/Totaltech.UnitTests.csproj -c Release --no-restore -m:1
dotnet test ./Tests/Totaltech.IntegrationTests/Totaltech.IntegrationTests.csproj -c Release --filter 'FullyQualifiedName~Totaltech.IntegrationTests.Endpoints' -m:1
```

Restore, build y publish correctos; build con 0 errores y 0 advertencias. Tests: **21 unitarios y 60 HTTP correctos**, ninguno omitido. Los primeros intentos de tests dentro de la restricción local fallaron por ejecución/IPC; los reintentos autorizados finalizaron correctamente. Las pruebas HTTP existentes usan EF InMemory aislado: no validan semántica SQL ni reemplazan Azure SQL en la aplicación. No se ejecutaron las suites SQL/E2E ni pruebas con Azure SQL.

Artefacto comprobado: `Totaltech.dll`, `Totaltech.deps.json`, `Totaltech.runtimeconfig.json`, dependencias y `web.config` generado por Web SDK. No contiene `Frontend.dll`. Runtimeconfig requiere Microsoft.NETCore.App y Microsoft.AspNetCore.App 10.0.0 con resolución de parches del runtime. `web.config` usa `processPath="dotnet"`, `arguments=".\Totaltech.dll"`, AspNetCoreModuleV2, hosting inprocess y stdout deshabilitado. Los logs de Azure prueban que se alcanzó código administrado, no un fallo inicial de carga del runtime.

## Configuración pendiente en el portal

Azure Portal → App Services → **TotaltechApp** → Settings/Configuración → **Environment variables / Variables de entorno** → **App settings / Configuración de aplicación**. Agregar los valores y guardar/aplicar (reinicia la aplicación). Los secretos se cargan allí; no en GitHub ni en este informe.

| Nombre exacto | Valor |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Authentication__SigningKey` | Secreto privado fuerte, aleatorio, de al menos 32 caracteres ASCII/32 bytes UTF-8. No usar la clave de tests. |
| `ConnectionStrings__DefaultConnection` | Cadena real de Azure SQL para la base confirmada, con credenciales válidas, cifrado y validación del certificado. |
| `Database__ApplyMigrations` | `false` |
| `BootstrapAdmin__Enabled` | `false` |
| `DemoData__Enabled` | `false` |

Issuer `Totaltech.Api` y Audience `Totaltech.Frontend` ya están en el paquete. Evitar valores conflictivos de DOTNET_ENVIRONMENT. Alternativamente, la conexión puede configurarse en la pestaña Connection strings con nombre `DefaultConnection` y tipo SQLAzure; elegir una sola fuente para evitar discrepancias.

En SQL databases → base confirmada → Connection strings, obtener la plantilla ADO.NET e introducir en privado las credenciales correctas. Ejemplo NO ejecutable, solo estructura:

```text
Server=tcp:cancho.database.windows.net,1433;Initial Catalog=TotatechDB;User ID=<usuario>;Password=<secreto>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

No se conocen esas credenciales. No se generó una clave JWT porque la tarea lo prohíbe. La base Paused puede requerir tiempo de reanudación al conectarse; comprobarlo antes de la demostración. Si SQL rechaza conexión por red o permisos, revisar el error y solicitar autorización para el cambio específico: no abrir firewall indiscriminadamente.

Revisar las tres migraciones contra `__EFMigrationsHistory`. Si faltan, preparar y revisar un script idempotente con copia de seguridad y autorización de aplicación; no activar migraciones automáticas permanentes como atajo.

## Obtener registros y verificar la corrección

Portal → TotaltechApp → Development Tools / Herramientas de desarrollo → Advanced Tools / Herramientas avanzadas → Go → Kudu → Debug console → CMD → `D:\home\LogFiles\eventlog.xml`. Buscar el primer evento .NET Runtime/IIS AspNetCore Module V2 después de aplicar la configuración. También puede descargarse mediante:

```powershell
az webapp log download -g Servidores -n TotaltechApp --log-file ./publish/azure-startup-logs.zip
```

Si no alcanza, habilitar temporalmente stdout en el `web.config` desplegado con destino `\\?\%home%\LogFiles\stdout`, reproducir una vez y volver a deshabilitarlo. No poner Development ni exponer excepciones al público. Los registros existentes alcanzaron para este diagnóstico; no se cambió el logging.

Después de configurar secretos y comprobar SQL:

```powershell
$base = 'https://totaltechapp-ghd2etambcgcfgcb.brazilsouth-01.azurewebsites.net'
Invoke-WebRequest "$base/categorias/" | Select-Object StatusCode, Content
Invoke-WebRequest "$base/productos/catalogo" | Select-Object StatusCode, Content
```

Aceptar solo HTTP 200 y JSON con el contrato esperado en ambos endpoints, sin nuevas excepciones de arranque. Revisar datos del catálogo para la demostración. `/` no es una prueba adecuada porque no hay una ruta raíz definida. La ejecución de GitHub Actions y el posterior arranque en Azure no se revalidaron con un nuevo despliegue durante esta tarea.

Referencias oficiales: [diagnóstico ASP.NET Core/IIS](https://learn.microsoft.com/en-us/aspnet/core/test/troubleshoot-azure-iis?view=aspnetcore-10.0) y [configuración App Service](https://learn.microsoft.com/en-us/azure/app-service/configure-common).
