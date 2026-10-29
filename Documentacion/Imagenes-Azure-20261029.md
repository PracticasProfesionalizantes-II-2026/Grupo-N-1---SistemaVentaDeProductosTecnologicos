# Imágenes de productos en Azure — 29/10/2026

## Estado

**PASS para la Vivobook ID 10: asociación aplicada y persistencia verificada.**
La imagen se comprobó visualmente y por HTTP en Home, catálogo y detalle después
del UPDATE autorizado, después de reiniciar solo el frontend y después de
redesplegar exactamente el mismo ZIP validado. Este informe documenta la fase inicial
del ID 10. Actualización posterior: seis asociaciones adicionales se aprobaron,
aplicaron y verificaron; hay **7/11 productos con imagen**. El usuario indicó omitir
las asociaciones de los IDs 4, 6, 8 y 9, que no tienen foto; se mantienen sus
productos y placeholders. El alcance de imágenes disponibles está completado.
Ver el [informe del lote de seis](Imagenes-Azure-lote-seis-20261029.md).
No se afirma que los once productos tengan fotografía. Sin commit ni push.

## Causa raíz y evidencia

- API local: 42 productos. Azure SQL `TotatechDB`: 11 productos, todos con
  `ImagenUrl=NULL` al tomar la referencia. No se pueden trasladar IDs entre bases.
- El resolver usa nombres exactos normalizados, sin coincidencias parciales.
  Ninguno de los 11 nombres de Azure está en su diccionario. Los servicios MVC
  respetan una URL explícita de la API y solo usan el resolver cuando está vacía.
- Las vistas Home, catálogo y detalle muestran un placeholder cuando falta la URL;
  no se observó un fallo de descarga de estas tarjetas: no tenían un `img` de producto.
- Se inspeccionaron 63 imágenes versionadas, las 38 referencias del resolver,
  el artefacto GitHub Actions 12900625089 del run 40878841586 (commit 83e9a268),
  y el paquete activo Kudu `20261029131644.zip` durante el diagnóstico inicial.
  Las 63 imágenes coincidían por SHA-256 y las 38 URLs respondieron HTTP 200.
- El artefacto GitHub tenía digest
  `35207F2CA074C8838D835BCD9DFA29DC9483825CF39C808FDCCB430269D46909`.
- Cuatro imágenes subidas existen solamente en el directorio local ignorado por Git.
  El pipeline no las tenía; una publicación local anterior sí incluía las cuatro,
  incluso referencias en `Frontend.staticwebassets.endpoints.json`.
- Kudu confirmó inicialmente vacío `data/Totaltech/productos/`. El directorio web
  del paquete se inspecciona en `site/wwwroot/wwwroot/images/`, no en
  `site/wwwroot/images/`.
- La comparación local se obtuvo mediante la API operativa. La consulta directa a
  LocalDB desde el contexto de esta terminal falló al iniciar la instancia; no se
  modificó su configuración ni se sustituyó por otra base.

## Archivos y asociaciones

El inventario completo de 67 imágenes (63 estáticas y cuatro uploads), tamaños y
SHA-256 está en [el inventario CSV](Imagenes-Azure-inventario-20261029.csv).
Las 42 asociaciones locales y su existencia física están en
[las asociaciones locales](Imagenes-Azure-asociaciones-locales-20261029.csv).

Dos uploads PNG son copias exactas de las imágenes estáticas de gabinetes Magnum
Tech 435R y 436b. Los otros dos son WebP de un mouse y una Vivobook. Esto no
autoriza a asociarlos a nombres genéricos en Azure. `Frontend/photos/` contiene
referencias visuales y mockups, no una fuente de asociaciones comerciales.

Única asociación confirmada explícitamente por el usuario:

| Dato | Valor |
|---|---|
| ID Azure / nombre | 10 / `asus vivobook` |
| ID local | 23 (solo referencia, nunca usado como ID de Azure) |
| Archivo | `dcd308fe347a4dcc86a2d59fb9d50098.webp` |
| Imagen | ASUS Vivobook, 15,6 pulgadas, Ryzen 5, 8 GB RAM, SSD 512 GB |
| Tamaño | 29.218 bytes |
| SHA-256 | `807093335C2DFA755CC7FD13E16462AD7F9A5FD96878B693F88A351F4AB07BE7` |
| URL aplicada | `/uploads/productos/dcd308fe347a4dcc86a2d59fb9d50098.webp` |

Después se confirmaron y aplicaron las fotos de los IDs 1, 2, 3, 5, 7 y 11.
Sin fotografía, con asociación omitida por indicación del usuario: ID 4 `Naxido`,
6 `NVIDIA G4050`, 8 `DELL` y 9 `LENOVO VICTUS`. No se aplican fotos por marca ni se
confunde `LENOVO VICTUS` con HP Victus o Lenovo LOQ.

## Implementación y persistencia

- `Frontend/Frontend.csproj`: `Content Update="wwwroot/uploads/**"` con
  `CopyToPublishDirectory="Never"`. Los cuatro items Content se conservan.
- `Desarrollo/Test-TotaltechFrontendArtifact.ps1`: control compartido por CLI y CI;
  verifica frontend, web.config, ausencia de backend/uploads/referencias en
  manifiestos e integridad de cada imagen estática respecto al código fuente.
- Workflow frontend: usa ese control antes de subir y después de descargar el
  artefacto. Conserva OIDC, validaciones y destino. No transfiere uploads ni ejecuta SQL.
- `ProductoImagenResolverTests`: verifica coincidencias exactas, rechazo de los
  once nombres no identificados, URLs explícitas y separación de IDs entre entornos.
- Sin cambios al resolver, storage, servicios, DTO, Razor, backend ni autenticación.
- Configuración verificada e intacta:
  `ProductoImagenes__Directorio=%HOME%\data\Totaltech\productos` y
  `WEBSITE_RUN_FROM_PACKAGE=1`. Las claves Data Protection existentes no se modifican.
- Copia verificable independiente del publish en
  `C:\Users\facu_\.codex\artifacts\totaltech-imagenes-20261029\`, con manifiesto.
- Transferido exclusivamente el WebP confirmado por Kudu con autenticación Entra,
  comprobación de destino previo y hash. HTTP 201 al crear; descarga pública HTTP
  200, MIME `image/webp`, 29.218 bytes y SHA-256 correcto el 29/10/2026 13:43 UTC.
  No se sobrescribió ningún archivo existente ni se publicaron otros uploads.
- El paquete se monta en solo lectura. `%HOME%` está fuera del paquete y persiste
  entre reinicios; no se agregan cuentas de almacenamiento, planes ni servicios.
  Referencias: [run from package](https://learn.microsoft.com/en-us/azure/app-service/deploy-run-package)
  y [persistencia de HOME](https://learn.microsoft.com/en-us/azure/app-service/operating-system-functionality).

## SQL autorizado y rollback preparado

Scripts completos conservados sin cambios respecto a lo aprobado:

- [Actualización transaccional](SQL/Totaltech-Producto10-ImagenUrl.sql).
- [Rollback transaccional](SQL/Totaltech-Producto10-ImagenUrl-rollback.sql).

| Campo | Antes | Después comprobado |
|---|---|---|
| IdProducto / Nombre | 10 / `asus vivobook` | Sin cambios |
| Descripcion | `87997` | Sin cambios |
| Precio / Stock | 200000,00 / 110 | Sin cambios |
| IdCategoria / IdProveedor | 11 / 10 | Sin cambios |
| ImagenUrl | NULL | URL WebP indicada arriba |

La transacción usa `XACT_ABORT`, `UPDLOCK`, `HOLDLOCK` y espera máxima de bloqueo
de 10 segundos. Exige base `TotatechDB`, ninguna transacción exterior y todos los
valores de referencia. Compara nombres, descripción y URL por bytes para evitar
aceptar diferencias de mayúsculas o espacios por la collation. Aborta si la fila
no coincide. El UPDATE cambia solo `ImagenUrl`; repetirlo con el resultado esperado
no vuelve a escribir. Toda excepción hace ROLLBACK.

El rollback verifica los mismos datos y la URL exacta aplicada antes de volver a
NULL; ante una asociación distinta o cambios de referencia se detiene. No elimina
archivos ni restaura toda la base. Una modificación posterior que coloque de nuevo
exactamente la misma URL no se distingue sin una versión de fila; no se agrega
esquema para este caso. Revalidar estado y autorización antes de cualquier rollback.

El usuario autorizó específicamente el script del ID 10. Su SHA-256 se comprobó
antes de ejecutarlo: `E8029D5C4128FDEB314A0B992AB0C87C420F3BBEFF42E89051974AD0739D1405`.
Se revalidaron WebP público, firma, MIME y hash; no había triggers activos sobre
Productos. Azure confirmó DB Online y retención de siete días. La comprobación de
recuperación a las 10:55:55 ART registró inicio de ventana
`2026-10-22T13:55:51.668697Z` y referencia anterior
`2026-10-29T13:40:55.3691251Z`, dentro de esa ventana. No se realizó un simulacro
de restauración ni se creó una base adicional. `Database__ApplyMigrations=false`.

Ejecución única a las **10:57:13 ART (13:57:13 UTC)**: COMMIT correcto, una fila
afectada con `@@ROWCOUNT=1` exigido dentro de la transacción. La comparación tipada
de los ocho campos de los once productos confirmó como única diferencia
`Productos[10].ImagenUrl`. Los conteos de metadatos de las quince tablas permanecieron
iguales, incluidos cinco registros de historial EF. No se ejecutó el rollback:
no fue necesario ni se probó mediante escrituras de reversión.

El primer intento de preflight se detuvo antes de ejecutar SQL porque comparaba
representaciones JSON de Decimal y Double. Una comparación por campos, con precio
decimal y cadenas ordinales, confirmó cero diferencias reales antes de continuar.
Ese rechazo preventivo no produjo escrituras. Resultados anteriores/posteriores y
horas están en `sql-ejecucion-autorizada.json` y `sql-inmediato-*.json`.

## Validaciones

| Prueba | Resultado | Evidencia |
|---|---|---|
| Restore/build Frontend Release | PASS | 0 errores, 0 advertencias |
| Build solución Release | PASS | 0 errores, 0 advertencias |
| Pruebas unitarias / integración rápidas | PASS | 21 / 209; TRX `imagenes-fast.trx` |
| Pruebas dirigidas de imagen, edición y sesión | PASS | 87; TRX `imagenes-persistentes.trx` (incluidas en rápidas) |
| Publicación nueva | PASS | SDK 10.0.400; 63 imágenes con hash correcto, 0 uploads |
| Items Content conservados | PASS | 4, todos CopyToPublishDirectory=Never |
| Rechazo de paquete anterior contaminado | PASS | Detecta referencias a uploads en manifiesto |
| Cuatro URLs locales después de publish | PASS | HTTP 200, archivos originales intactos |
| WebP público tras despliegue inicial | PASS | HTTP 200, MIME WebP y hash original; decodifica 800×800 |
| Inspección visual inicial del archivo público | PASS | `publish/evidencia-imagenes-20261029/vivobook-archivo-publico.jpg` |
| 63 imágenes estáticas tras despliegue | PASS | Todas HTTP 200; `static-http.json` |
| Paquete activo idéntico al validado | PASS | Comprobado inicialmente y tras redeploy; `paquete-activo.json`, `redeploy-persistencia.json` |
| Protección anónima y página login | PASS | Login 200; Administración y Productos/Crear 302 |
| Validación workflow | PASS | actionlint |
| Sintaxis SQL, actualización y rollback | PASS | PARSEONLY en conexión exclusiva; no ejecuta DML |
| UPDATE productivo acotado | PASS | Autorizado, una fila, solo ImagenUrl; demás campos y productos intactos |
| Rollback productivo | NOT TESTED | Preparado y sintaxis validada; no fue necesario ejecutarlo |
| Vivobook en Home, catálogo y detalle | PASS | Tres capturas y HTML HTTP 200 después de asociar |
| Persistencia tras reinicio | PASS | Tres vistas, imagen 800×800 y hash original |
| Persistencia tras redeploy | PASS | Tres vistas, mismo ZIP y archivo persistente con hash original |
| Consola de las vistas inspeccionadas | PASS | Sin errores ni advertencias capturados |
| Pipeline remoto con estos cambios | NOT TESTED | Sin commit ni push autorizados |
| Sesión productiva autenticada tras reinicio | NOT TESTED | Esta fase no usa credenciales privadas ni inicia una cuenta |
| Cuatro productos sin fotografía | NOT TESTED | IDs 4, 6, 8 y 9 omitidos por indicación del usuario; conservan placeholders |

La ejecución remota existente sobre 538bd30f concluyó correctamente (run
40881059466), pero no contiene estos cambios pendientes. No se presenta como prueba
de CI de la modificación actual. No se ejecutaron SQL relacional ni E2E completos
en esta fase: la regresión local está cubierta por las rápidas y por las nueve
comprobaciones visuales efectivas de Azure. La sesión autenticada productiva tras
reinicio no se afirma verificada; las comprobaciones anónimas y pruebas locales sí pasaron.

### Evidencia de persistencia

| Momento (ART) | Home | Catálogo | Detalle | Imagen |
|---|---|---|---|---|
| Asociación, 10:59 | PASS | PASS | PASS | HTTP 200, WebP 800×800, SHA-256 original |
| Reinicio, 10:59–11:00 | PASS | PASS | PASS | HTTP 200, WebP 800×800, SHA-256 original |
| Redeploy, 11:01 | PASS | PASS | PASS | HTTP 200, WebP 800×800, SHA-256 original |

Nueve capturas: `publish/evidencia-imagenes-20261029/{asociada,reinicio,redeploy}-{home,catalogo,detalle}.jpg`.
`visual-results.json` registra URL, carga completa, dimensiones y hora de cada
captura. `http-{asociada,reinicio,redeploy}.json` registra HTML e imagen HTTP 200,
once productos, seis categorías y ninguna diferencia ajena al ID 10.

![Vivobook en Home después del redeploy](../publish/evidencia-imagenes-20261029/redeploy-home.jpg)

![Vivobook en catálogo después del redeploy](../publish/evidencia-imagenes-20261029/redeploy-catalogo.jpg)

![Vivobook en detalle después del redeploy](../publish/evidencia-imagenes-20261029/redeploy-detalle.jpg)

## Operación y contingencia

Artefacto manual: `publish/totaltech-imagenes-persistentes-20261029.zip`, SHA-256
`CA7AFACA87AB22BAEB0A769B89FF8F92709B936E8404B0B7EE795A7B6A69FFD4`.
Base `538bd30fa3a1ea4f24c5949e124802b5f068ba6c` más diff pendiente, sin commit/push.
Despliegue manual inicial `24f97f00c1a34250b53e6c874a39ebe6`, Succeeded,
29/10/2026 13:44:43 UTC. Smoke 13:46:35 UTC: Home, catálogo y CSS HTTP 200;
6 categorías y 11 productos. Ambos App Services Running; catálogo idéntico a la
referencia anterior. Se comprobó que no había un pipeline frontend activo.
Tras la aprobación SQL, reinicio exclusivo del frontend a las 10:59:39 ART y
redeploy `0504cbbc4c9c46109ea9e6d216f65c32`, Succeeded a las 11:00:36 ART.
Ambos App Services Running antes y después. No se reinició ni desplegó el backend;
no hubo cambios de configuración, permisos ni recursos. Los diez productos sin
correspondencia conservan NULL y sus placeholders. Login HTTP 200 y rutas de
administración HTTP 302 se comprobaron nuevamente después del redeploy.
El manifiesto de archivos fuente y ZIP, referencias SQL y resultados están en
`publish/evidencia-imagenes-20261029/` (ignorado por Git, sin secretos).
Antes del despliegue se conservó el paquete activo en `azure-package-before.zip`
con su nombre y hash en `azure-package-before.json`.

Si falla la publicación, redesplegar ese último paquete válido exclusivamente al
frontend. Si falla la asociación, usar el rollback acotado después de revalidar
sus condiciones; no hacer restauraciones globales ni eliminar archivos por lotes.
No ejecutar otros cambios SQL ni el rollback sin autorización específica.
