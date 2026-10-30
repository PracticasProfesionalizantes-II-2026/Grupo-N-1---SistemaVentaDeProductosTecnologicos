# UC-28 — Handoff Backend

El Frontend implementa extracción y revisión. Estos endpoints **no existen todavía** en
Totaltech. La pantalla informa su ausencia y no reemplaza la importación atómica por
altas individuales. No se modificaron Backend, esquema ni migraciones.

## Contrato acordado

JSON camelCase, autenticación bearer vigente, autorización Admin en ambos endpoints.

`POST /productos/importacion/previsualizar`

```json
{
  "idProveedor": 1,
  "filas": [
    { "numeroFila": 1, "nombre": "Notebook", "descripcion": "Equipo portátil",
      "precio": 1234.56, "stock": 5, "categoriaNombre": "Notebooks", "idCategoria": 1 }
  ]
}
```

Cada número de fila es la posición lógica 1-based, único y consecutivo. `precio`
(decimal), `stock` (int) e `idCategoria` (int) admiten null durante la revisión;
`categoriaNombre` (string) también puede ser null. Nombre y descripción son strings.

Respuesta 200, o 422 si hay errores por fila:

```json
{
  "filas": [
    { "numeroFila": 1, "nombre": "Notebook", "descripcion": "Equipo portátil",
      "precio": 1234.56, "stock": 5, "categoriaNombre": "Notebooks", "idCategoria": 1 }
  ],
  "errores": [],
  "puedeConfirmar": true
}
```

Devolver todas las filas con sus números originales, aunque haya errores. Cada error:
`{ "numeroFila": 1, "campo": "Nombre", "mensaje": "El nombre ya existe." }`.
NumeroFila/campo pueden ser null para errores generales. No persistir durante preview.

`POST /productos/importacion/confirmar`: el mismo cuerpo más `idOperacion` (GUID).
Respuesta **200 solamente tras confirmar todo el lote**:

```json
{ "idOperacion": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", "importados": 1 }
```

422 usa la misma estructura de errores de preview. 409 indica conflicto del catálogo
o identificador reutilizado con otro contenido. 401/403 indican problemas de acceso.
Errores generales: `{ "mensaje": "Mensaje seguro" }`; el Frontend usa mensajes
propios por status. 404/405 se presentan como funcionalidad Backend no disponible.

## Garantías que debe implementar Backend

- Entre 1 y 1.000 altas nuevas; proveedor existente/activo y categorías existentes.
- Precio no negativo, compatible con decimal(18,2); stock entero no negativo.
- Nombre obligatorio; descripción opcional. Comparar duplicados por espacios
  normalizados, mayúsculas invariantes y sin marcas de acentuación (FormD).
- Rechazar duplicados dentro del lote y contra productos existentes. Garantizar
  también la corrección frente a importaciones concurrentes; preview no reserva nombres.
- Confirmación revalida el cuerpo completo y persiste todo o nada. No confiar en
  validaciones, categorías sugeridas ni flags enviados por el Frontend.
- Idempotencia por actor autenticado + idOperacion + contenido: mismo contenido
  devuelve el resultado original, contenido diferente devuelve 409. Esto debe cubrir
  solicitudes concurrentes y pérdida de la respuesta HTTP tras persistir.
- No exponer entidades de persistencia, excepciones ni secretos. Propagar cancelación.

La elección e implementación de persistencia de idempotencia y exclusión concurrente
corresponde al Backend y requiere su validación relacional. El Frontend no demuestra
atomicidad ni idempotencia mediante su API simulada.

## Extracción y ejecución

CSV UTF-8 (BOM opcional), `;` o `,`, encabezados Nombre/Precio/Stock obligatorios;
Descripcion y CategoriaNombre opcionales. Categoria es alias de CategoriaNombre.
XLSX lee exclusivamente sheet1.xml, sharedStrings e inlineStr, no evalúa fórmulas.
PDF admite texto tabular con 3 columnas (Nombre/Precio/Stock) o 4–5 columnas
(Nombre/Descripcion/Precio/Stock/Categoria), separadas por `;`, `|`, tabuladores o
espacios múltiples; también campos rotulados en una línea y Nombre $Precio Stock.
PDF no tiene OCR ni garantías para tablas arbitrarias: revisar siempre la extracción.
ICatalogoExtractorAgent usa una heurística local por defecto y permite reemplazo DI,
sin proveedor remoto ni credenciales. Sus fallos recuperables usan fallback local.

Límites: archivo 5 MiB reales; 1.000 productos; PDF 100 páginas; XML XLSX 20 MiB
acumulados. El formulario acepta 6 MiB de cuerpo y 20.000 valores.

Ejecutar desde la raíz Git interior:

```powershell
dotnet restore Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln
dotnet build Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln --no-restore
dotnet restore Tests/Frontend.UnitTests/Frontend.UnitTests.csproj
dotnet test Tests/Frontend.UnitTests/Frontend.UnitTests.csproj
dotnet test Tests/Totaltech.UnitTests/Totaltech.UnitTests.csproj --no-build --no-restore
dotnet test Tests/Totaltech.IntegrationTests/Totaltech.IntegrationTests.csproj --no-build --no-restore --filter FullyQualifiedName~Totaltech.IntegrationTests.Frontend
```

La nueva suite se ejecuta explícitamente, sin modificar la solución. Todas las
pruebas UC-28 usan API simulada: no requieren SQL Server ni agente remoto.
