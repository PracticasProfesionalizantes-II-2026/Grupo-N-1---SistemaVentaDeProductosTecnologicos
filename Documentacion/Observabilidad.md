# Observabilidad local de TotalTech

Actualización: 09/10/2026. UC-46 queda parcialmente cubierto con métricas y alertas visibles en Prometheus. Grafana, Docker, trazas distribuidas, Alertmanager, notificaciones externas y despliegue en producción quedan fuera de esta entrega. No requiere migraciones.

## Arranque en Windows

1. Configurar e iniciar API y frontend con el perfil **TotalTech completo**, en `http://localhost:5070` y `http://localhost:5087`. La API conserva sus prerrequisitos de SQL Server y JWT.
2. Conservar la distribución oficial **Prometheus 3.15.0, Windows AMD64** bajo `ControlProyecto/prometheus-3.15.0.windows-amd64/prometheus-3.15.0.windows-amd64/`, con `prometheus.exe` y `promtool.exe`. El ZIP y los ejecutables no se versionan. [Release oficial](https://github.com/prometheus/prometheus/releases/tag/v3.15.0).
3. Abrir PowerShell en la raíz Git interna, que contiene `Frontend/`, `Totaltech/` y `Observabilidad/`, y ejecutar:

```powershell
.\Observabilidad\iniciar-prometheus.ps1
```

El script valida configuración y reglas antes de iniciar Prometheus en primer plano. No inicia aplicaciones, aplica migraciones ni modifica datos SQL. Detener con **Ctrl+C** en esa terminal. Para otra ubicación de ejecutables:

```powershell
.\Observabilidad\iniciar-prometheus.ps1 -DistributionPath 'C:\Herramientas\prometheus-3.15.0.windows-amd64'
```

Abrir [Prometheus](http://127.0.0.1:9090), [Targets](http://127.0.0.1:9090/targets) y [Alertas](http://127.0.0.1:9090/alerts). Los jobs `totaltech_api`, `totaltech_frontend` y `prometheus` deben aparecer `UP`. La extracción/evaluación usa **15 s**, con timeout de **5 s**.

Prometheus escucha únicamente en `127.0.0.1:9090`. Guarda series en `%LOCALAPPDATA%\Totaltech\Prometheus\data`, con retención de **15 días** y `--storage.tsdb.retention.size=1GB`. El límite de retención controla bloques históricos; WAL y datos en memoria pueden superar ese tamaño. `-DataPath` permite elegir otra carpeta.

El ZIP local fue cotejado con `sha256sums.txt` oficial: SHA256 `5d333b385557d9adc2ff015d13da9809baccc52fb800a82d1e5c94b79258f87e`.

## Contrato y configuración

| Aplicación | Extracción |
|---|---|
| Backend | `GET http://localhost:5070/metrics` |
| Frontend | `GET http://localhost:5087/metrics` |

Exportan texto Prometheus y permiten solo conexiones loopback IPv4/IPv6, incluidas direcciones IPv4 mapeadas. Origen remoto: `403`; método distinto de GET: `405`. Las cabeceras reenviadas no conceden acceso. La rama se ejecuta antes de HTTPS, cookies y JWT: extraer no redirige, autentica ni consulta SQL o API remota.

`Observability:Enabled=false` es el valor base; Development usa `true` y las pruebas lo habilitan explícitamente. Deshabilitado, `/metrics` devuelve `404` y no se recolectan métricas. Se puede deshabilitar con `Observability__Enabled=false`.

Cada host tiene un registro independiente por DI. La extracción utiliza memoria y lecturas locales del proceso; el frontend continúa exportando cuando falla la API. Los ejemplares de trazas están desactivados. Los contratos JSON de negocio permanecen vigentes.

## Qué se mide

| Métrica | Etiquetas |
|---|---|
| `totaltech_http_requests_total` | `http_method, route, code` |
| `totaltech_http_request_duration_seconds` | `http_method, route, code` |
| `totaltech_http_requests_in_progress` | `http_method, route` |
| `totaltech_api_client_requests_total` | `client, method, result` |
| `totaltech_api_client_request_duration_seconds` | `client, method, result` |
| `totaltech_db_commands_total` | `execution, result` |
| `totaltech_db_command_duration_seconds` | `execution, result` |
| `totaltech_dependency_up` | `dependency="sqlserver"` |
| `totaltech_auth_operations_total` | `operation, result` |
| `totaltech_checkout_operations_total` | `result` |
| `totaltech_checkout_duration_seconds` | `result` |
| `totaltech_user_operations_total` | `operation, result` |
| `process_cpu_seconds_total`, `process_working_set_bytes`, `process_num_threads` | — |
| `dotnet_total_memory_bytes` | — |
| `dotnet_collection_count_total` | `generation` |
| `totaltech_threadpool_threads`, `totaltech_threadpool_pending_work_items` | — |

Los histogramas exportan `_bucket`, `_sum` y `_count`. Límites en segundos: `0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10, +Inf`. Prometheus añade `job` e `instance`.

- **HTTP:** una observación por solicitud terminada, incluyendo excepciones no controladas como `500` y entradas inválidas como `400`. La API etiqueta plantillas de ruta y MVC controlador/acción. Las rutas inexistentes y métodos desconocidos usan valores fijos. La cancelación de la solicitud usa `code="cancelled"`. Excluye extracción y archivos estáticos, incluido el CSS generado con fingerprint.
- **Clientes:** mide `TotaltechApi` y `TotaltechSessionApi` hasta recibir cabeceras, sin consumir cuerpo ni cambiar Bearer, excepciones o cancelación. Resultados: clases `http_1xx` a `http_5xx`, error de transporte, cancelación —incluido timeout— y error inesperado.
- **SQL:** ejecución `reader/scalar/nonquery`, síncrona, asíncrona y `ExecuteUpdateAsync`. Cada reintento cuenta otro comando. La comprobación de disponibilidad conecta en un scope independiente al iniciar y cada 30 s, con límite de 2 s. Publica 1/0; un fallo no detiene el host.
- **Negocio:** login/registro distinguen éxito, credenciales inválidas, cuenta inexistente/inactiva, entrada inválida, conflicto y error. Checkout distingue creado/repetido/inválido/inexistente/conflicto/error/cancelación. Usuarios mide edición/baja/reactivación por resultado.
- **Transacciones:** los contadores de negocio se registran al terminar la operación exterior, después de reintentos/transacción. Un rollback no cuenta como pedido creado. Una edición sin cambios o baja repetida cuenta como operación exitosa, sin otro cambio efectivo ni auditoría.
- **Privacidad:** no hay IDs, emails, nombres, búsquedas, URLs concretas, SQL, parámetros, credenciales, tokens ni trazas como etiquetas.

Los contadores reinician con el proceso y miden actividad, no cifras contables ni históricos. Los reportes SQL siguen siendo la fuente de negocio.

## Consultas listas para usar

Pegar cada expresión en la interfaz de Prometheus. Generar tráfico desde el sitio; `rate` necesita al menos dos extracciones.

Disponibilidad de aplicaciones y SQL:

```promql
up{job=~"totaltech_api|totaltech_frontend"}
```

```promql
totaltech_dependency_up{job="totaltech_api",dependency="sqlserver"}
```

Solicitudes por segundo y porcentaje de 5xx:

```promql
sum by (job, route) (rate(totaltech_http_requests_total[5m]))
```

```promql
100 * sum by (job) (rate(totaltech_http_requests_total{code=~"5.."}[5m]))
/ sum by (job) (rate(totaltech_http_requests_total[5m]))
```

Latencia p95 HTTP en segundos y solicitudes en curso:

```promql
histogram_quantile(0.95, sum by (job, le) (rate(totaltech_http_request_duration_seconds_bucket[5m])))
```

```promql
sum by (job) (totaltech_http_requests_in_progress)
```

MVC → API, tasa de llamadas por cliente/resultado y latencia p95:

```promql
sum by (client, result) (rate(totaltech_api_client_requests_total{job="totaltech_frontend"}[5m]))
```

```promql
histogram_quantile(0.95, sum by (client, le) (rate(totaltech_api_client_request_duration_seconds_bucket{job="totaltech_frontend"}[5m])))
```

SQL, tasa de comandos por ejecución/resultado y latencia p95:

```promql
sum by (execution, result) (rate(totaltech_db_commands_total{job="totaltech_api"}[5m]))
```

```promql
histogram_quantile(0.95, sum by (le) (rate(totaltech_db_command_duration_seconds_bucket{job="totaltech_api"}[5m])))
```

CPU usada por proceso, memoria en MiB e hilos:

```promql
rate(process_cpu_seconds_total{job=~"totaltech_api|totaltech_frontend"}[5m])
```

```promql
process_working_set_bytes{job=~"totaltech_api|totaltech_frontend"} / 1024 / 1024
```

```promql
process_num_threads{job=~"totaltech_api|totaltech_frontend"}
```

GC por segundo y trabajo pendiente del ThreadPool:

```promql
sum by (job, generation) (rate(dotnet_collection_count_total[5m]))
```

```promql
totaltech_threadpool_pending_work_items
```

Operaciones de autenticación y usuarios en 30 minutos:

```promql
sum by (operation, result) (increase(totaltech_auth_operations_total{job="totaltech_api"}[30m]))
```

```promql
sum by (operation, result) (increase(totaltech_user_operations_total{job="totaltech_api"}[30m]))
```

Confirmaciones de carrito por resultado y latencia p95:

```promql
sum by (result) (increase(totaltech_checkout_operations_total{job="totaltech_api"}[30m]))
```

```promql
histogram_quantile(0.95, sum by (le) (rate(totaltech_checkout_duration_seconds_bucket{job="totaltech_api"}[5m])))
```

## Alertas y diagnóstico

| Alerta | Condición | Permanencia |
|---|---|---|
| `TotaltechServicioCaido` | `up=0` de API/MVC | 1 min |
| `TotaltechSqlNoDisponible` | Dependencia SQL en 0 | 1 min |
| `TotaltechErroresElevados` | Más de 5 % de 5xx y ≥20 solicitudes en 5 min | 5 min |
| `TotaltechLatenciaElevada` | p95 >1 s y ≥20 solicitudes en 5 min | 5 min |

Las alertas se ven en Prometheus; no envían mensajes. Con poco tráfico no alertan errores/latencia. Revisar umbrales con evidencia antes de otro entorno.

Si un target aparece `DOWN`, inspeccionar **Last error** en Targets:

- Conexión rechazada: iniciar el proyecto y comprobar puerto.
- `404`: comprobar Development y `Observability:Enabled`.
- `403`: comprobar que la conexión se origine desde loopback; cabeceras proxy no la habilitan.
- Redirección/login/HTML: confirmar proceso, puerto y versión de TotalTech; `/metrics` no pasa por autenticación.
- Timeout: revisar proceso/carga. Extraer no depende de SQL ni de la API remota del MVC.
- SQL en 0 con target `UP`: revisar conexión y servicio SQL. No aplicar migraciones como solución automática.

En esta máquina, Smart App Control bloqueó la DLL existente de Scalar con `0x800711C7`. La referencia interactiva se carga únicamente cuando se utiliza. Para iniciar Development sin ejecutar esa dependencia opcional, manteniendo OpenAPI, la API y las métricas:

```powershell
$env:ApiReference__Enabled = 'false'
dotnet run --project Totaltech/Totaltech.csproj --configuration Release --no-build --launch-profile http
```

La referencia interactiva conserva su valor predeterminado habilitado en Development. Esta opción no modifica políticas de seguridad ni desbloquea archivos. Las pruebas finales se ejecutaron con esa variable; no prueban la interfaz de Scalar.

## Validación y trazabilidad

```powershell
$promtool = '.\ControlProyecto\prometheus-3.15.0.windows-amd64\prometheus-3.15.0.windows-amd64\promtool.exe'
& $promtool check config Observabilidad/prometheus.yml
& $promtool check rules Observabilidad/alertas.yml
& $promtool test rules Observabilidad/tests/alertas.test.yml
```

El CI descarga 3.15.0 desde el release oficial, verifica SHA256 contra `sha256sums.txt` y ejecuta estos controles. Las series sintéticas cubren permanencia, activación, recuperación, mínimo de tráfico y exclusión de jobs ajenos. No requieren aplicaciones ni SQL. Las pruebas relacionales del proyecto usan bases desechables.

Comprobación operativa: iniciar ambos proyectos, extraer `/metrics` y comprobar ambos targets `UP`. Detener un proceso de desarrollo, esperar extracción y un minuto de permanencia, observar `TotaltechServicioCaido`, reiniciar y comprobar recuperación. Detenerlo no borra datos.

Evidencia ejecutada el **09/10/2026**:

- Compilación Release de la solución completa: cero advertencias y errores.
- xUnit: **21 pruebas unitarias y 181 de integración aprobadas**, sin fallos ni omisiones; incluye SQL Server desechable y navegador. Resultados locales en `Tests/*/TestResults/prometheus-final.trx` (no versionados).
- `promtool 3.15.0`: `check config`, `check rules` (cuatro reglas) y `test rules` (cuatro escenarios) correctos. El workflow remoto de GitHub Actions no se ejecutó durante esta validación local.
- Ambos `/metrics` respondieron `200` con formato Prometheus, sin credenciales. Los tres targets estuvieron `UP`; el servidor escuchó únicamente en `127.0.0.1:9090` y SQL publicó `1`.
- Caída real de la API: su target pasó a `DOWN` y `TotaltechServicioCaido` pasó por `pending` y `firing`. El frontend continuó `UP`, exportó métricas y registró el error de transporte. Tras reiniciar la API, los tres targets recuperaron `UP` y no quedaron alertas activas.

La comprobación operativa no aplicó migraciones ni semillas. La instancia temporal de Prometheus se detuvo al finalizar; iniciar la herramienta con el script anterior. Los cambios locales de los botones de Usuarios y la distribución descargada se preservan. UC-46 permanece parcial por notificaciones externas y operación en producción pendientes.

## Referencias

[Configuración de Prometheus](https://prometheus.io/docs/prometheus/latest/configuration/), [pruebas de reglas](https://prometheus.io/docs/prometheus/latest/configuration/unit_testing_rules/), [instrumentación](https://prometheus.io/docs/practices/instrumentation/), [prometheus-net 8.2.1](https://www.nuget.org/packages/prometheus-net.AspNetCore/8.2.1) y [DotNetStats 8.2.1](https://github.com/prometheus-net/prometheus-net/blob/v8.2.1/Prometheus/DotNetStats.cs).

