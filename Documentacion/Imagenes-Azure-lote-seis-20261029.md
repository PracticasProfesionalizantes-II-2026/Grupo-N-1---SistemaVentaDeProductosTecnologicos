# Seis asociaciones aplicadas — ejecución y persistencia

## Estado y alcance

El usuario confirmó `1=A1, 2=S2, 3=D1, 5=K1, 7=H1, 11=M1` y pidió frases de una
línea **solo en la galería de revisión**. Después autorizó específicamente ejecutar
el lote de seis ImagenUrl, conservando las descripciones comerciales. El script se
ejecutó sin cambios y las seis imágenes quedaron verificadas en Azure, incluyendo
reinicio y redeploy. No se hicieron commits ni push.

El usuario indicó después que los IDs 4, 6, 8 y 9 no tienen imagen y deben omitirse
de las asociaciones. Se conservan sus productos y placeholders, sin ocultarlos ni
eliminarlos. El ID 10 conserva la imagen ya aplicada y verificada. No se incorporan
los 42 productos locales.

## Galería simplificada

La galería local `publish/evidencia-imagenes-20261029/revision-imagenes-pendientes.html`
conserva las 19 fotografías originales, muestra las seis selecciones y usa frases
breves de una sola línea. Por ejemplo: «Notebook ASUS TUF Gaming A16.» y «Mouse
inalámbrico Logitech M280.». Los nombres de archivo, hashes y la descripción real
almacenada quedan en desplegables. No se trasladan estas frases a la base.
La lectura visual automatizada de la galería local quedó bloqueada por la política
del navegador para URLs `file://`; no se intentó eludirla. Se comprobó el HTML y sus
archivos embebidos; la revisión visual de esa edición queda NOT TESTED.

## Valores anteriores y aplicados

Las seis `ImagenUrl` estaban NULL. Solo cambió ese campo, en los seis IDs autorizados.

| ID | Nombre intacto | Foto confirmada | Descripción intacta | Precio intacto | Stock intacto | Categoría / proveedor intactos |
|---|---|---|---|---:|---:|---|
| 1 | asus | A1 ASUS TUF Gaming A16 | Inalambrico | 155000,00 | 101 | 11 / 1 |
| 2 | Samsung | S2 Samsung S26 Ultra Cobalt Violet | 564 | 160000,00 | 102 | 12 / 2 |
| 3 | Disco ssd | D1 Kingston NV3 1 TB | 65446 | 165000,00 | 103 | 13 / 3 |
| 5 | Teclado | K1 Logitech TKL inalámbrico | 312131 | 175000,00 | 105 | 15 / 5 |
| 7 | HP | H1 HP Victus Gaming 15 | 3121231 | 185000,00 | 107 | 11 / 7 |
| 11 | Mouse | M1 Logitech M280 inalámbrico | Inalambrico | 12000,00 | 20 | 15 / 6 |

URLs nuevas exactas:

| ID | ImagenUrl aplicada (anterior NULL) |
|---|---|
| 1 | `/images/categorias/Notebooks/2 compragamer_Imganen_general_56118_Notebook_ASUS_TUF_Gaming_A16.jpg` |
| 2 | `/images/categorias/Celulares/Celular Samsung S26 Ultra 12gb 256gb Cobalt Violet.png` |
| 3 | `/images/categorias/Almacenamiento/snv3s-1000g.png` |
| 5 | `/images/categorias/Perisfericos/Teclados/1compragamer_Imganen_general_42700_Teclado_Mecanico_.jpg` |
| 7 | `/images/categorias/Notebooks/3compragamer_Imganen_general_56094_Notebook_HP_Victus_Gaming_15_15.jpg` |
| 11 | `/uploads/productos/936ad1ba8e8d425492a6fe2058abbe0b.jpg` |

Se conservan los espacios literales de las rutas estáticas: el validador existente
de la API acepta esas rutas y rechaza el carácter `%`. No se debilita la validación.
El navegador codifica los espacios al solicitarlas; las seis URLs públicas dan HTTP
200 y sus SHA-256 coinciden con los originales locales.

## Archivos y persistencia

El [manifiesto de seis archivos](Imagenes-Azure-seis-confirmadas-20261029.csv)
registra rutas locales, URL nueva, SHA-256, modelos confirmados y precondiciones.
Hay copias verificadas de las seis imágenes fuera de publish en
`C:\Users\facu_\.codex\artifacts\totaltech-imagenes-20261029\seis-confirmadas\`.
Las cinco imágenes estáticas ya pertenecen al paquete versionado.

Solo M1 requirió transferencia: JPG original de 13.200 bytes, firma válida, dentro
del límite existente de 5 MB, hash
`E2DFA606188C8B5DA8E91AFB9EBB7F34A3E074BB7985D432E0F129E53C906436`.
Kudu autenticado con Entra creó el archivo con HTTP 201, sin sobrescribir otro.
La descarga pública da HTTP 200 y MIME image/jpeg con el mismo hash. Se mantiene
`%HOME%\data\Totaltech\productos` y no se crean recursos ni cambian configuraciones.

## Scripts completos, concurrencia y rollback

- [Lote transaccional de seis imágenes](SQL/Totaltech-SeisImagenes-20261029.sql).
- [Rollback acotado](SQL/Totaltech-SeisImagenes-20261029-rollback.sql).

El script exige TotatechDB, ninguna transacción exterior y exactamente los seis
IDs. Dentro de la transacción usa XACT_ABORT, UPDLOCK y HOLDLOCK, con timeout de
bloqueo de diez segundos. Revalida por bytes nombre, descripción y URL, y por valor
precio, stock y ambas relaciones. Cada ImagenUrl debe ser NULL o la nueva exacta.
Solo acepta las seis NULL (actualiza exactamente seis) o las seis ya aplicadas
(repetición idempotente sin escribir). Un estado parcial u otra diferencia abortan.
Se verifica `@@ROWCOUNT=6` y el estado posterior antes del COMMIT. Cualquier
excepción produce ROLLBACK de todo el lote.

El rollback exige que las seis filas conserven todos los valores y las seis URLs
nuevas. Solo entonces restaura NULL en ImagenUrl de esos seis IDs, exigiendo seis
filas afectadas. No elimina archivos. Si hubo cambios, se detiene; no fuerza la
reversión ni toca los otros cinco productos. No detecta una modificación posterior
que vuelva a exactamente el mismo estado; no se agrega esquema para resolver ese
caso. Se revalidará antes de un eventual rollback autorizado.

SHA-256 del script aprobado y ejecutado:
`B7D6A7469295F00AFAF7EDFCD0798744A938E91E7BCC300D2CC2A9757EE241D5`.
SHA-256 del rollback:
`AD36CF05722818A4C76FF409FB2D7C2F83322206ECADF8DC0BBFC297336893F4`.

Se revalidó recuperación antes de ejecutar: DB Online, retención de siete días,
inicio de ventana `2026-10-22T16:28:23.463905Z` y referencia anterior
`2026-10-29T16:13:27.2598871Z`. No se realizó un simulacro de restauración ni se
creó otra base. ApplyMigrations sigue false.

El script se ejecutó una sola vez el **29/10/2026 a las 13:28:32 ART**
(16:28:32 UTC). Resultado: COMMIT, **seis filas afectadas**. Se verificaron el
hash aprobado, ausencia de triggers activos y todos los valores de referencia
antes de ejecutar. La comparación tipada de los once productos después de la
transacción encontró únicamente las seis diferencias autorizadas en ImagenUrl.
Los conteos de metadatos de las quince tablas permanecieron iguales, incluyendo
cinco filas de historial EF. Descripciones y demás campos permanecieron intactos.
El rollback no fue necesario y no se ejecutó.

Evidencia local ignorada por Git: `lote-seis-ejecucion-autorizada.json`,
`lote-seis-sql-resultsets.json`, `lote-seis-inmediato-{antes,despues}.json`,
`lote-seis-conteos-{antes,despues}.json` y `lote-seis-recuperacion.json`, todos en
`publish/evidencia-imagenes-20261029/` y sin credenciales.

## Validaciones y próximos pasos

| Verificación | Estado |
|---|---|
| Seis originales y copias: tamaño, firma, SHA-256 | PASS |
| Seis imágenes públicas: HTTP 200 y hashes originales | PASS |
| Lectura SQL previa: seis ImagenUrl NULL y valores comerciales de referencia | PASS |
| Sintaxis del lote y rollback con PARSEONLY | PASS; sin ejecutar DML |
| Descripciones de la galería: frases breves, una línea, 19 fotos intactas | PASS en HTML; visual NOT TESTED por bloqueo file:// |
| UPDATE productivo autorizado | PASS: seis filas, solo ImagenUrl, valores comerciales intactos |
| Seis productos con foto en Home, catálogo y detalle | PASS: 18 comprobaciones iniciales con capturas |
| Persistencia después de reinicio | PASS: 18 comprobaciones; capturas idénticas por SHA-256 |
| Persistencia después de redeploy | PASS: 18 comprobaciones; capturas idénticas por SHA-256 |
| Seis URLs en cada etapa | PASS: HTTP 200, MIME correcto y SHA-256 original |
| Regresión Vivobook ID 10 | PASS: URL y demás datos conservados |
| Consola de las vistas inspeccionadas | PASS: sin errores ni advertencias capturados |
| Fotografía de IDs 4, 6, 8 y 9 | NOT TESTED: sin imagen; asociaciones omitidas por indicación del usuario |

## Reinicio, redeploy y evidencias visuales

Se reinició exclusivamente TotaltechFrontend a las **13:31:58 ART**. Ambos App
Services estaban Running antes y después. Se redesplegó exactamente el mismo ZIP
validado, SHA-256 `CA7AFACA87AB22BAEB0A769B89FF8F92709B936E8404B0B7EE795A7B6A69FFD4`.
Redeploy `c5dec4478d6d44d3b8dfb0669c9d9c5c`, Succeeded a las **13:33:01 ART**.
No se desplegó ni reinició el backend, ni se alteraron configuraciones o recursos.

Se inspeccionaron visualmente las seis fotos en Home, catálogo y detalle (18
capturas iniciales). Tras reinicio y tras redeploy se repitieron las 18 lecturas
de cada etapa: URL exacta, imagen completa y dimensiones naturales válidas.
Las 36 capturas posteriores son idénticas por SHA-256 a las 18 iniciales
inspeccionadas. Total: **54 comprobaciones visuales registradas**.

| Etapa | Home (6) | Catálogo (6) | Detalles (6) | URLs de imágenes |
|---|---|---|---|---|
| Asociación | PASS | PASS | PASS | 6 HTTP 200 + hash original |
| Reinicio | PASS | PASS | PASS | 6 HTTP 200 + hash original |
| Redeploy | PASS | PASS | PASS | 6 HTTP 200 + hash original |

Las imágenes decodificadas miden 450×402 (ASUS, teclado, HP y mouse), 604×582
(Samsung) y 1600×1200 (SSD). El directorio persistente conserva el JPG M1 con su
hash original. Las cinco imágenes estáticas siguen dentro del paquete validado.

Capturas y registros:
`lote-seis-{asociadas,reinicio,redeploy}-{home,catalogo,detalle}-{ID}.jpg`,
`lote-seis-visual-results.json`, `lote-seis-http-{asociadas,reinicio,redeploy}.json`,
`lote-seis-reinicio.json`, `lote-seis-redeploy.json` y
`lote-seis-redeploy-persistencia.json`, dentro del mismo directorio de evidencia.

![ASUS en Home tras redeploy](../publish/evidencia-imagenes-20261029/lote-seis-redeploy-home-1.jpg)

![Samsung en catálogo tras redeploy](../publish/evidencia-imagenes-20261029/lote-seis-redeploy-catalogo-2.jpg)

![Mouse en detalle tras redeploy](../publish/evidencia-imagenes-20261029/lote-seis-redeploy-detalle-11.jpg)

## Estado global y cierre del alcance

**Siete de once productos tienen imagen:** IDs 1, 2, 3, 5, 7, 10 y 11.
Los IDs 4 Naxido, 6 NVIDIA G4050, 8 DELL y 9 LENOVO VICTUS quedan sin foto y con
placeholder por indicación expresa del usuario («no tiene imagen, omite»).
El alcance de imágenes disponibles queda completado: siete verificadas y cuatro
asociaciones omitidas. No se afirma que los once productos tengan fotografía.
Nombres, descripciones comerciales, precios, stock, categorías, proveedores y
relaciones se conservaron. No se incorporaron productos locales.

La validación .NET de la preparación previa fue 21 unitarias y 209 rápidas, build
sin errores/advertencias y actionlint correcto; no se repitió porque desde entonces
solo se modificaron documentación/galería y se ejecutó el lote autorizado. El
workflow definitivo sigue preparado sin commit/push; no se afirma que un pipeline
remoto haya validado estos cambios pendientes. La sesión productiva autenticada
tras reinicio sigue NOT TESTED; se comprobaron las rutas anónimas/protegidas.
