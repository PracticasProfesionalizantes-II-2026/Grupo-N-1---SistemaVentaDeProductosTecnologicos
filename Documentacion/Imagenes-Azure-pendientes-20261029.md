# Imágenes de Azure: asociaciones verificadas y omisiones acordadas

Estado revisado después de resolver la Vivobook ID 10. Azure conserva once
productos. Después de la revisión manual se confirmaron seis fotos adicionales;
sus asociaciones SQL se aprobaron, aplicaron y verificaron. El usuario indicó que
los cuatro productos restantes no tienen imagen y deben omitirse de las asociaciones,
conservando productos y placeholders. La Vivobook ID 10 ya está aplicada y verificada.
Local contiene 42 productos, todos con archivo local disponible. No son el mismo
catálogo y no se deben relacionar por ID, categoría o precio. El usuario confirmó
que el alcance es exclusivamente los once productos actuales de Azure, sin altas
del catálogo local ni cambios de datos comerciales.

## Identificación de los diez productos actuales

Las seis selecciones confirmadas ya están aplicadas con aprobación SQL específica.
Los cuatro IDs sin fotografía se omiten por indicación del usuario. No se les
asignan imágenes de la misma marca o categoría ni se alteran sus registros.

La certeza evalúa la **asociación con el producto de Azure**, no la existencia del
archivo. **Baja**: solo hay compatibilidad de marca/tipo, sin SKU identificable;
no habilita asociación. **Nula**: no se identificó un archivo correspondiente o la
denominación es contradictoria. **Alta**: correspondencia seleccionada explícitamente
por el usuario; todavía requiere aprobación independiente antes de escribir SQL.

| ID Azure | Nombre | Imagen propuesta / archivos para revisar | Certeza | Estado y dato faltante |
|---|---|---|---|---|
| 1 | asus | A1 ASUS TUF Gaming A16 | Alta | Aplicada; vistas y persistencia verificadas |
| 2 | Samsung | S2 S26 Ultra Cobalt Violet | Alta | Aplicada; vistas y persistencia verificadas |
| 3 | Disco ssd | D1 Kingston NV3 1 TB | Alta | Aplicada; vistas y persistencia verificadas |
| 4 | Naxido | Sin imagen | Nula | Asociación omitida por indicación del usuario |
| 5 | Teclado | K1 Logitech TKL inalámbrico | Alta | Aplicada; vistas y persistencia verificadas |
| 6 | NVIDIA G4050 | Sin imagen | Nula | Asociación omitida por indicación del usuario |
| 7 | HP | H1 Victus Gaming 15 | Alta | Aplicada; vistas y persistencia verificadas |
| 8 | DELL | Sin imagen | Nula | Asociación omitida por indicación del usuario |
| 9 | LENOVO VICTUS | Sin imagen | Nula | Asociación omitida por indicación del usuario |
| 11 | Mouse | M1 Logitech M280 inalámbrico | Alta | Aplicada; vistas y persistencia verificadas |

Los 19 códigos referencian archivos concretos con ruta y SHA-256 en
[el CSV de candidatas](Imagenes-Azure-candidatas-20261029.csv). La
[galería local de revisión](../publish/evidencia-imagenes-20261029/revision-imagenes-pendientes.html)
incluye las 19 fotos originales embebidas, agrupadas por ID, sin llamadas de red
ni acciones de escritura. Las seis elecciones se muestran destacadas. Sus textos
se simplificaron a una frase profesional de una línea; los detalles de archivo/hash
quedaron en desplegables. Esta edición es solo de la galería: ninguna descripción
almacenada en Azure se modifica. Confirmar un código identifica la foto real;
**no autoriza ejecutar SQL**. Para estas seis filas se obtuvo luego una aprobación
SQL específica, registrada junto con la ejecución en
[el informe de seis asociaciones](Imagenes-Azure-lote-seis-20261029.md).

La Vivobook ID 10 conserva certeza alta por confirmación explícita y verificación
visual/HTTP tras reinicio/redeploy. No necesita otra actualización.

### Originales adicionales encontrados

Se localizaron 18 imágenes existentes en `C:\Users\facu_\Downloads\Mouses`, sin
descargar material de Internet. Solo se transfirió la foto M1 confirmada al
directorio persistente existente; las demás permanecen locales. Sus nombres identifican
Logitech M280 Wireless, Logitech G903 Wireless, Redragon Centrophorus M601 RGB,
Glorious Model D Wireless, Glorious I 2 Wireless y Razer Basilisk V3. Hay ángulos
adicionales de algunos modelos. El
[inventario adicional](Imagenes-Mouses-originales-locales-20261029.csv) registra
archivo, modelo según nombre, tamaño y SHA-256. Ninguno está asociado por inferencia
al ID 11; la selección M1 procede de confirmación explícita del usuario.

Los cinco originales de `Downloads/GABINETES` son idénticos por SHA-256 a los cinco
gabinetes estáticos ya inventariados; no aportan una correspondencia con Naxido.
En `Downloads/1 proyecto` se encontró el original
`E1504FA-WS51_800 vivobooookk.webp`, idéntico al WebP ya publicado del ID 10.
No aparecieron archivos identificados como Dell, Naxido o G4050 en la búsqueda
por nombre de imágenes de Downloads ni en las carpetas web inspeccionadas.

## Próximo paso y límites

Las filas 4, 6, 8 y 9 quedan sin imagen por decisión expresa del usuario. Las seis fotos
confirmadas ya se aplicaron mediante el lote transaccional autorizado, limitado a
`ImagenUrl`. Se comprobaron Home, catálogo y detalle, reinicio y redeploy del mismo
ZIP. Hay siete productos con imagen, contando la Vivobook ID 10.

En esta fase se generó documentación, se simplificó la galería local y se transfirió
solo la fotografía M1 al almacenamiento persistente. Después de la autorización
se actualizaron únicamente seis ImagenUrl. No hubo altas de productos, cambios de precio,
stock, descripción, categoría, proveedor o relaciones, ni cambios al código de
producción. No hubo commit ni push. El manifiesto local provisional se conservó
únicamente como referencia ignorada en `publish/evidencia-imagenes-20261029/`;
incorporar ese catálogo queda fuera del alcance elegido.
