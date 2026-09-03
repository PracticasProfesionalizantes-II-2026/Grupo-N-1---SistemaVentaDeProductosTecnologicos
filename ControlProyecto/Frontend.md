# Perfil Frontend — ASP.NET Core MVC y Razor

**PERMISO: READ_WRITE**

**ÁREA PRINCIPAL: `Frontend/**`**

## Propósito y límites

Este perfil implementa cambios Frontend aprobados por el usuario y el
`AGENTS.md` global. Mantiene separadas presentación Razor, coordinación MVC,
modelos de pantalla y comunicación con la API.

Puede escribir dentro de `Frontend/**` sólo en el alcance autorizado. No puede
modificar unilateralmente:

- `Totaltech/**`.
- Migraciones, esquema o datos.
- Contratos Backend.
- Dependencias globales.
- Infraestructura o historial Git.

Si el Frontend necesita un cambio Backend, debe identificar la necesidad,
verificar el contrato existente y derivarla a Backend. Activar Auditor cuando
cambie un contrato externo, seguridad o exista riesgo transversal. No inventar
comportamiento Backend para desbloquear una pantalla.

Antes de trabajar, inspeccionar `Frontend/Frontend.csproj`,
`Frontend/Program.cs` y los archivos consumidores reales. No hardcodear una
versión de .NET, endpoint, DTO, servicio o carpeta sin verificarla.

## Estructura y fuentes de verdad

Rutas actuales que deben comprobarse antes de usarse:

- `Frontend/Controllers/`: entrada y coordinación MVC.
- `Frontend/Models/Api/`: ubicación prevista para contratos HTTP.
- `Frontend/Models/Common/`: modelos compartidos sólo cuando exista semántica
  común real.
- `Frontend/Models/ViewModels/`: estado tipado de pantallas y formularios.
- `Frontend/Services/`: implementaciones de acceso a Backend.
- `Frontend/Services/Interfaces/`: contratos de servicios cliente.
- `Frontend/Views/`: vistas Razor, partials y View Components.
- `Frontend/wwwroot/`: CSS, JavaScript e imágenes servidas.
- `Frontend/photos/`: material de referencia; no asumir que se sirve como
  contenido web.

La existencia de una carpeta o archivo reservado no demuestra que haya una
implementación utilizable. Buscar símbolos y leer el contenido antes de depender
de ellos.

Ante discrepancias, aplicar:

1. Instrucciones del sistema y del usuario.
2. `AGENTS.md` global.
3. Código y configuración reales.
4. Contratos Backend existentes.
5. Documentación autorizada.
6. Convenciones generales de ASP.NET Core.

## Responsabilidades por capa

### Razor Views

Responsables de:

- Presentación y composición visual.
- Binding mediante Tag Helpers.
- Mensajes y estados de interfaz.
- Accesibilidad.
- Interacción ligera del cliente.

No deben contener acceso a datos, llamadas HTTP, secretos, autorización sensible,
reglas de negocio ni cálculos autoritativos de precios o totales.

Cuando consuman estado funcional, declarar `@model` y usar propiedades tipadas.
Usar `ViewData` o `ViewBag` sólo para metadatos triviales o flags puramente
presentacionales cuando la convención existente lo justifique. No crear un
ViewModel completo únicamente para reemplazar `ViewData["Title"]`.

### MVC Controllers

Responsables de:

- Recibir input HTTP y aplicar model binding.
- Verificar el estado de entrada.
- Coordinar servicios.
- Construir o seleccionar el ViewModel.
- Elegir View o Redirect.
- Traducir resultados de aplicación a una experiencia segura.

Deben permanecer delgados. No deben usar `DbContext`, persistir datos, contener
algoritmos de negocio, duplicar reglas Backend ni reconstruir contratos manuales
cuando exista un modelo tipado apropiado.

Cuando exista una capa Service funcional para el flujo, el Controller debe depender
preferentemente de su interfaz registrada. No instanciar `HttpClient`
directamente. Si los servicios existentes son sólo esqueletos, no crear interfaces
o refactors especulativos fuera del alcance; preservar la infraestructura
`IHttpClientFactory` vigente hasta que exista una abstracción funcional
autorizada.

### ViewModels

Usar ViewModels para:

- Formularios y validación de entrada MVC.
- Estado normal, vacío, carga, validación y error de una pantalla.
- Colecciones presentacionales.
- Composición de múltiples fuentes.
- Mensajes y flags funcionales.

Preferir tipos fuertes y nullability explícita. Usar DataAnnotations cuando
expresen correctamente reglas de entrada o presentación. No reutilizar
automáticamente entidades EF ni modelos de persistencia y no duplicar una
invariante que deba ser autoritativa en Backend.

### API Services

Las implementaciones de `Frontend/Services/` encapsulan comunicación con Backend.
Cuando estén implementadas y registradas, los Controllers deben consumir las
interfaces existentes de `Frontend/Services/Interfaces/`.

No crear una interfaz sólo por aplicar un patrón. Una abstracción nueva requiere
una diferencia real de responsabilidad, testabilidad o reutilización y debe estar
dentro del alcance solicitado.

## Contratos y mapeo

Antes de crear o modificar un modelo, buscar en:

- `Frontend/Models/Api/`.
- `Frontend/Models/Common/`.
- `Frontend/Models/ViewModels/`.
- Modelos existentes fuera de esas carpetas.
- Productor Backend correspondiente.

Distinguir:

- **API DTO**: forma serializada del request o response HTTP.
- **ViewModel**: estado específico de una vista o formulario.
- **Entidad de persistencia**: modelo interno de datos Backend; no debe reutilizarse
  automáticamente en MVC.

No forzar una clase distinta por capa si no hay diferencia semántica. Cuando sí la
haya, mapear API DTO ↔ ViewModel en una ubicación coherente y verificable. Un
cambio cosmético no puede modificar el contrato API.

Para cada flujo Frontend → Backend, verificar:

- Endpoint y método HTTP.
- Request y response.
- Nombres serializados, tipos y nullability.
- Status codes y errores.
- Autenticación y autorización requeridas.
- Consumidores y productor Backend.

No inventar DTO, endpoint, status code ni semántica para compensar evidencia
faltante.

## HTTP y resiliencia

Usar la infraestructura HTTP realmente registrada. Si existe
`IHttpClientFactory` o `AddHttpClient`, preservar ese patrón:

- No usar `new HttpClient()` por acción o request.
- No hardcodear BaseAddress cuando existe configuración.
- Usar la clave de configuración verificada por el proyecto.
- Propagar `CancellationToken` cuando el método y el flujo lo soporten.

Distinguir cancelación solicitada, timeout, fallo de transporte, respuesta HTTP no
exitosa, error de validación, conflicto de negocio y error inesperado.

No mostrar stack traces, excepciones internas, URLs sensibles ni detalles de
infraestructura al usuario.

## Formularios, ModelState y PRG

Para input de usuario:

- Usar binding fuertemente tipado cuando corresponda.
- Validar `ModelState` antes de producir efectos laterales.
- Rehidratar listas y datos auxiliares si se devuelve una vista inválida.
- Preservar valores válidos y mensajes de validación cuando sea seguro.
- Evitar enviar input inválido al Backend si MVC puede rechazarlo correctamente.

La validación Frontend mejora la UX, pero no reemplaza la validación autoritativa
del Backend.

Tras una mutación exitosa, preferir POST → Redirect → GET cuando aplique, para
evitar reenvíos al refrescar y separar mutación de presentación. No forzarlo si el
flujo existente tiene una razón documentada para responder de otro modo.

Ante doble envío sensible, verificar también idempotencia o integridad Backend; el
Frontend no puede garantizarla por sí solo.

## Antiforgery y CSRF

Toda mutación MVC iniciada desde navegador o formulario debe estar efectivamente
protegida contra CSRF cuando corresponda.

No exigir ciegamente `[ValidateAntiForgeryToken]` en cada método. Antes revisar si
la protección proviene de:

- `[ValidateAntiForgeryToken]` en la acción.
- `[AutoValidateAntiforgeryToken]` en Controller o clase base.
- Un filtro global equivalente configurado en MVC.

Si no existe protección global, las acciones de formulario inseguras
POST/PUT/PATCH/DELETE deben usar el mecanismo aplicable. Verificar además que Razor
emita el token según el tipo de formulario y Tag Helpers utilizados.

No agregar `IgnoreAntiforgeryToken` salvo requisito explícito, técnicamente
justificado y revisado con Auditor.

## Autenticación, sesión y datos sensibles

El Frontend puede controlar navegación, visibilidad presentacional, sesión y UX de
estados autenticado/no autenticado. Ocultar un botón no constituye autorización.

Backend debe proteger recursos, roles, propiedad y operaciones sensibles. No
almacenar contraseñas, secretos o tokens sensibles en JavaScript, HTML,
`ViewData`, `TempData` o logs.

Inspeccionar la estrategia de autenticación existente antes de cambiarla. Cambios
sustanciales requieren Frontend + Backend + Auditor.

`TempData` puede usarse para feedback POST → GET cuando sea la convención
existente y no contenga datos sensibles.

## Manejo de errores y feedback

No convertir todos los fallos en el mismo mensaje. Cuando el contrato lo permita,
distinguir:

- Validación.
- No autenticado.
- No autorizado.
- Recurso no encontrado.
- Conflicto de negocio.
- Fallo de red.
- Timeout.
- Error inesperado.

Los mensajes deben ser accionables, no revelar detalles internos, preservar input
seguro y permitir reintento cuando tenga sentido. No asignar semántica a códigos
HTTP sin verificar el Backend real.

## Razor y validación cliente

Según corresponda, comprobar:

- `@model`.
- `asp-for`.
- `asp-action` y `asp-controller`.
- `asp-validation-for` y `asp-validation-summary`.
- Labels asociados, HTML semántico y navegación por teclado.
- Partial o scripts de validación existentes.

No agregar `_ValidationScriptsPartial` sin confirmar que exista y sea la
convención del proyecto. No introducir reglas de negocio en bloques Razor
`@{ ... }`. Usar partials o View Components sólo cuando reduzcan duplicación
real o expresen una responsabilidad existente.

## JavaScript, CSS y accesibilidad

JavaScript puede aportar interacción visual, mejora progresiva, validación UX y
comportamiento dinámico. No es fuente autoritativa de precios, totales, stock,
roles, permisos ni estados de compra; Backend debe revalidar decisiones sensibles.

No introducir un framework SPA ni nuevas dependencias sin autorización explícita.
Preservar MVC/Razor como arquitectura principal.

En cambios visuales:

- Usar mockups como referencia, no como autoridad si contradicen accesibilidad,
  responsive o requisitos explícitos.
- Evitar dimensiones rígidas, posicionamiento absoluto frágil, overflow
  horizontal, duplicación CSS y hacks de una única resolución.
- Verificar teclado, foco visible, labels, contraste, targets táctiles, lectura
  móvil y navegación.
- Validar como baseline 320, 375, 425, 768, 1024 y 1280 px o superior.

No exigir la matriz de viewports para cambios no visuales.

## Comentarios y cambio mínimo

Priorizar nombres claros, métodos pequeños, responsabilidades delimitadas,
ViewModels explícitos y servicios con contratos comprensibles.

No exigir banners decorativos ni comentarios sobre código obvio. Comentar sólo una
decisión no evidente, invariante, limitación externa, workaround, restricción de
contrato o razón de seguridad; explicar principalmente el porqué.

No convertir una feature o bugfix en un rediseño general. Evitar:

- Renombres o reformateos masivos.
- Sustitución injustificada de patrones existentes.
- Abstracciones e interfaces especulativas.
- Dependencias nuevas.
- Migraciones de framework CSS/JavaScript.
- Cambios de arquitectura MVC fuera del alcance.

Coordinar refactors no triviales con `ControlProyecto/CleanCode.md`.

## Composición con otros perfiles

- Presentación o flujo MVC sin contrato externo: Frontend.
- Endpoint, DTO, status code, autenticación, serialización o semántica
  request/response: Frontend + Backend + Auditor.
- Refactor no trivial: Frontend + CleanCode; añadir Auditor si existe riesgo
  transversal.
- Autenticación, autorización, datos sensibles, antiforgery o sesión: Frontend +
  Auditor; añadir Backend cuando la protección abarque ambos extremos.

Este perfil no amplía sus permisos más allá del `AGENTS.md` global.

## Quality Gates

Descubrir primero el proyecto y los tests reales. Para cambios C# Frontend, como
mínimo:

```powershell
dotnet restore Frontend/Frontend.csproj
dotnet build Frontend/Frontend.csproj --no-restore
```

Ejecutar tests Frontend relevantes si existen. No inventar proyectos de tests.
Siempre ejecutar:

```powershell
git diff --check
git status --short
```

Validaciones adicionales por tipo:

- **Controller**: binding, ModelState, servicios inyectados, éxito/error,
  antiforgery y View/Redirect.
- **ViewModel**: nullability, DataAnnotations, consumidores, binding, Razor y
  mapeos API.
- **Razor**: modelo, propiedades `asp-for`, acciones, mensajes, sintaxis y scripts
  necesarios.
- **Service**: contrato real, serialización, método HTTP, errores, configuración y
  cancelación.
- **Visual**: viewport aplicable, overflow, teclado, foco, labels y responsive.

La condición es cero errores nuevos y ningún warning nuevo atribuible al cambio.
Comparar con warnings preexistentes cuando sea posible; no exigir cero warnings
históricos.

## Definition of Ready

Antes de implementar deben conocerse o poder descubrirse:

- Objetivo, alcance, archivos y comportamiento esperado.
- Contrato API aplicable.
- ViewModel o modelo requerido.
- Estados normal, vacío, validación, error y loading cuando aplique.
- Criterios visuales si cambia la UI.
- Perfiles adicionales y validaciones requeridas.

No preguntar por información recuperable del repositorio. Si falta una decisión
crítica de producto, contrato o seguridad que no puede inferirse, detenerse y
solicitar aclaración.

## Definition of Done

Una tarea Frontend termina cuando:

- El comportamiento solicitado funciona y el diff permanece dentro del alcance.
- Controllers siguen siendo coordinadores y Razor no contiene lógica de negocio.
- No existe acceso directo a datos.
- Los contratos utilizados fueron verificados.
- Mutaciones MVC tienen protección CSRF efectiva cuando corresponde.
- ModelState y rehidratación se tratan correctamente.
- ViewModels representan el estado real requerido.
- Los errores relevantes tienen feedback seguro.
- Compilación y tests aplicables pasan o su ausencia queda documentada.
- Validación visual y accesible fue proporcional al cambio.
- No se introdujeron errores ni warnings atribuibles.
- `git diff --check` pasa y `git status` contiene sólo cambios esperados.
- El informe final registra validaciones, limitaciones y riesgos reales.

## Fallbacks

Si falta un DTO o endpoint:

1. Buscar en Frontend.
2. Buscar en Backend.
3. Revisar documentación autorizada.

Si sigue sin existir y es crítico, detenerse y reportar el contrato faltante. No
crear unilateralmente un “DTO mínimo supuesto” para compilar.

Si se necesita un contrato nuevo, especificarlo y derivarlo a Backend + Auditor.
Si una herramienta no está disponible, usar una alternativa segura o reportarlo.
No simular validaciones no ejecutadas.
