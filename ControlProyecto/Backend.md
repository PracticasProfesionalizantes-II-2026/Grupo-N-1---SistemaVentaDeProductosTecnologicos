# TAREA — Refactorización de ControlProyecto/Backend.md

## IDENTIDAD

Actúa como Principal Backend Engineer especializado en:

- ASP.NET Core;
- Minimal APIs y APIs HTTP;
- Entity Framework Core;
- SQL Server;
- consistencia transaccional;
- concurrencia;
- seguridad defensiva;
- contratos REST;
- testing;
- Context Engineering para agentes de codificación.

Tu objetivo en ESTA tarea es mejorar exclusivamente:

ControlProyecto/Backend.md

para convertirlo en el perfil READ_WRITE especializado del Backend de
TotalTech, coherente con:

- AGENTS.md;
- Auditor.md;
- Frontend.md;
- CleanCode.md;
- y la arquitectura REAL del repositorio.

NO implementes funcionalidad del producto durante esta tarea.

---

# CONTEXTO

Repositorio:

PracticasProfesionalizantes-II-2026/
Grupo-N-1---SistemaVentaDeProductosTecnologicos

Rama esperada:

Rama--Facu

Arquitectura conocida del Backend:

./Totaltech/
    Configuracion/
    Datos/
    Endpoints/
    Entidades/
    Errores/
    Logica/
    Middlewares/
    Migrations/
    Repositorios/
    Seguridad/
    Servicios/Interfaces/
    Validaciones/
    Program.cs
    Totaltech.csproj

Arquitectura establecida actualmente:

Endpoints
    ↓
Logica
    ↓
Repositorios
    ↓
EF Core / Datos

El nuevo Backend.md debe preservar esta arquitectura mientras siga siendo
coherente con el código real.

IMPORTANTE:

No asumas:

- versión de .NET;
- versión de C#;
- versión de EF Core;
- proveedor de base de datos;
- nombres de DTO;
- interfaces;
- endpoints;
- esquema;
- mecanismo de concurrencia.

Descúbrelos en el repositorio.

---

# OBJETIVO

Reescribir `ControlProyecto/Backend.md` para establecer reglas precisas sobre:

1. separación de responsabilidades;
2. contratos HTTP;
3. DTOs y entidades;
4. validación;
5. reglas de negocio;
6. seguridad;
7. importes monetarios;
8. inventario;
9. concurrencia;
10. transacciones;
11. persistencia;
12. manejo de errores;
13. autenticación/autorización;
14. migraciones;
15. compatibilidad Frontend ↔ Backend;
16. pruebas y Quality Gates;
17. cambio mínimo;
18. fallbacks ante información insuficiente.

El documento debe controlar comportamientos observables.

Evita reglas basadas únicamente en estilo.

---

# FASE 0 — SEGURIDAD

Antes de modificar cualquier archivo ejecuta:

git rev-parse --show-toplevel
git branch --show-current
git status --short --branch

Verifica:

- repositorio;
- rama;
- working tree.

Si existen cambios locales ajenos que puedan verse afectados:

DETENTE.

Durante esta tarea:

- NO commit;
- NO push;
- NO merge;
- NO rebase;
- NO reset;
- NO clean;
- NO cambio de rama;
- NO migraciones;
- NO modificación de base de datos.

---

# FASE 1 — DESCUBRIMIENTO DEL BACKEND REAL

Antes de editar lee:

AGENTS.md
ControlProyecto/Backend.md
ControlProyecto/Auditor.md
ControlProyecto/Frontend.md
ControlProyecto/CleanCode.md

Totaltech/Totaltech.csproj
Totaltech/Program.cs

Inspecciona además muestras representativas de:

Totaltech/Endpoints/
Totaltech/Logica/
Totaltech/Repositorios/
Totaltech/Entidades/
Totaltech/Datos/
Totaltech/Validaciones/
Totaltech/Seguridad/
Totaltech/Middlewares/
Totaltech/Errores/

y Tests/ si existe.

Busca mediante `rg` o equivalente:

DbContext
DbSet
SaveChanges
SaveChangesAsync
BeginTransaction
TransactionScope
DbUpdateConcurrencyException
ConcurrencyCheck
Timestamp
rowversion
ExecuteUpdate
ExecuteSql
decimal
double
float
MapGet
MapPost
MapPut
MapDelete
Results.
TypedResults.
ProblemDetails
AddProblemDetails
UseExceptionHandler
Authorize
AllowAnonymous
Claims
DTO
Request
Response

No modifiques nada durante esta fase.

---

# FASE 2 — FUENTES DE VERDAD

Para redactar Backend.md utiliza este orden:

1. instrucciones del sistema/usuario;
2. AGENTS.md global;
3. estado real del repositorio;
4. contratos y tests existentes;
5. documentación del proyecto;
6. documentación oficial de ASP.NET Core / EF Core.

Si una descripción histórica contradice el proyecto actual:

prevalece el repositorio verificable.

No inventes una arquitectura futura.

---

# FASE 3 — RESPONSABILIDAD Y PERMISOS

El nuevo archivo debe declarar:

PERMISO: READ_WRITE

ÁREA PRINCIPAL:

Totaltech/**

Puede implementar cambios autorizados dentro del Backend.

No puede modificar unilateralmente:

Frontend/**
esquema de base de datos;
migraciones;
dependencias;
contratos públicos;
infraestructura Git;
datos reales.

Cuando un cambio de Backend afecte a Frontend:

coordinar:

Backend + Frontend

y activar Auditor cuando exista modificación o riesgo sobre:

- contrato;
- seguridad;
- datos;
- autenticación;
- concurrencia;
- inventario;
- pagos.

---

# FASE 4 — ARQUITECTURA EXISTENTE

Preserva por defecto:

Endpoint
    ↓
Logica
    ↓
Repositorio
    ↓
EF Core

## Endpoints

Responsabilidades:

- routing;
- binding;
- autenticación/autorización aplicable;
- delegación;
- traducción del resultado a HTTP.

No deben:

- contener reglas de negocio sustanciales;
- ejecutar consultas EF Core directamente si el patrón actual dispone de
  Logica/Repositorios;
- calcular totales;
- gestionar transacciones complejas;
- confiar en propiedades sensibles del cliente.

## Logica

Responsabilidades:

- casos de uso;
- invariantes;
- reglas de negocio;
- coordinación de operaciones;
- decisiones de dominio;
- resultados funcionales.

## Repositorios

Responsabilidades:

- queries;
- persistencia;
- interacción EF Core;
- operaciones específicas de datos.

No fuerces una interfaz para cada repositorio si la arquitectura actual no la
requiere.

No crees capas adicionales únicamente por seguir un patrón teórico.

---

# FASE 5 — ENTIDADES Y CONTRATOS HTTP

No expongas directamente entidades de persistencia mediante la API cuando
ello acople el contrato HTTP al modelo de datos o exponga campos no destinados
al consumidor.

Distingue:

ENTIDAD:
modelo persistente administrado por EF Core.

REQUEST DTO:
datos aceptados desde el cliente.

RESPONSE DTO:
datos deliberadamente expuestos por la API.

No obligues a implementar todos los DTOs como `record`.

Se permiten:

- record;
- class;
- otros tipos apropiados;

según las convenciones reales del proyecto.

La condición importante es:

- contrato explícito;
- tipado fuerte;
- campos mínimos necesarios;
- ausencia de propiedades sensibles;
- validación apropiada.

No aceptar directamente del cliente valores autoritativos como:

- rol;
- permisos;
- propietario real;
- precio final;
- total final;
- descuento autorizado;
- estado de pago;
- aprobación;
- stock definitivo.

---

# FASE 6 — VALIDACIÓN

Toda entrada externa debe considerarse no confiable.

Validar en Backend aunque Frontend también valide.

Evaluar según el endpoint:

- tipos;
- rangos;
- IDs;
- cantidades;
- strings;
- formato;
- existencia de recursos;
- propiedad;
- estado permitido;
- invariantes.

Diferenciar:

VALIDACIÓN DE INPUT

de:

REGLA DE NEGOCIO.

No duplicar validaciones de forma innecesaria entre Endpoint, Logica y
Repositorio.

Ubicar cada regla en la capa que tenga autoridad sobre ella.

---

# FASE 7 — DINERO

Para importes monetarios utilizar:

decimal

salvo que exista una razón técnica extraordinaria y documentada.

Aplicar a:

- precio;
- subtotal;
- descuento monetario;
- impuesto;
- total;
- reembolso;
- importes de pago.

No usar:

float
double

para representar importes financieros.

Verificar además la precisión y escala configuradas en SQL Server / EF Core.

No asumir una escala determinada:

descubrir la configuración existente.

Las reglas de:

- redondeo;
- impuestos;
- descuentos;
- promociones

deben ser explícitas cuando formen parte del flujo.

El servidor debe recalcular importes autoritativos.

Nunca confiar en el total calculado por el cliente.

---

# FASE 8 — CANTIDADES E INVENTARIO

Usar el tipo que corresponda al dominio real.

Para productos discretos, normalmente una cantidad integral positiva será
apropiada, pero NO modifiques tipos existentes únicamente por esta regla sin
analizar el dominio y los consumidores.

Mantener como invariante cuando corresponda:

stock >= 0

y:

cantidad solicitada > 0

La validación previa:

if (stock >= cantidad)

NO es por sí sola una garantía de concurrencia.

Dos requests concurrentes pueden observar el mismo stock.

Por ello, todo flujo que reduzca inventario debe analizar explícitamente:

- ventana de concurrencia;
- mecanismo de exclusión o detección;
- comportamiento ante conflicto;
- atomicidad de la operación.

---

# FASE 9 — CONCURRENCIA

NO asumas que:

"usar una transacción"

equivale a:

"resolver concurrencia".

Son problemas relacionados pero distintos.

Para inventario u otros recursos concurrentes selecciona el mecanismo mínimo
correcto según el código, proveedor y requisitos reales.

Mecanismos posibles incluyen:

- optimistic concurrency mediante concurrency token;
- rowversion cuando esté soportado y sea apropiado;
- actualización atómica condicionada;
- aislamiento transaccional apropiado;
- locking/pessimistic concurrency cuando esté justificado;
- constraints de base de datos;
- combinación de mecanismos.

No impongas uno de ellos de forma universal.

Si se utiliza optimistic concurrency:

manejar explícitamente:

DbUpdateConcurrencyException

y definir si corresponde:

- informar conflicto;
- reintentar;
- recargar;
- abortar.

No implementar retries ciegos sin límite.

Para stock:

la validación final debe continuar siendo correcta bajo solicitudes
concurrentes.

---

# FASE 10 — TRANSACCIONES EF CORE

No exijas:

BeginTransactionAsync

en todas las operaciones.

Recuerda la semántica real de EF Core:

si el proveedor soporta transacciones, un único `SaveChanges` ejecuta sus
cambios dentro de una transacción por defecto.

Usar transacción explícita únicamente cuando la unidad de trabajo lo requiera,
por ejemplo:

- múltiples SaveChanges que deben ser atómicos conjuntamente;
- múltiples operaciones dependientes;
- coordinación de varias acciones de persistencia;
- aislamiento específico requerido por concurrencia.

Antes de agregar una transacción explícita:

analiza si aporta una garantía que no exista ya.

Cuando exista una transacción manual:

- delimita correctamente su alcance;
- commit solo tras completar la unidad;
- rollback ante fallo cuando corresponda;
- propaga CancellationToken;
- evita dejar transacciones abiertas innecesariamente.

No exijas una llamada manual explícita a Rollback en rutas donde disposal o
la infraestructura ya garanticen rollback y la llamada no aporte semántica
adicional.

La condición de aceptación es ATOMICIDAD observable, no la presencia textual
de:

BeginTransactionAsync
CommitAsync
RollbackAsync

---

# FASE 11 — OPERACIONES EXTERNAS Y PAGOS

Una transacción SQL no vuelve atómica una llamada a un sistema externo.

Si un flujo combina:

- base de datos;
- pasarela de pago;
- servicio externo;
- mensajería;

NO mantengas automáticamente una transacción SQL abierta durante una llamada
remota.

Analiza según la arquitectura:

- idempotencia;
- estado de operación;
- compensación;
- reintentos;
- identificadores únicos;
- prevención de doble cobro.

No introduzcas patrones distribuidos complejos si el proyecto no los necesita.

Pero tampoco declares que una transacción EF Core protege una operación
externa.

---

# FASE 12 — SEGURIDAD DE DATOS

Preferir las APIs seguras y parametrizadas de EF Core.

LINQ sobre EF Core debe continuar siendo la vía normal para queries.

Si existe SQL raw necesario:

usar APIs parametrizadas apropiadas.

No concatenar input del usuario dentro de SQL.

No asumir que todo uso de SQL raw es automáticamente vulnerable.

Verificar el mecanismo real.

Nunca devolver:

- password;
- password hash;
- salt;
- token interno;
- secreto;
- connection string;
- información sensible no necesaria.

---

# FASE 13 — AUTENTICACIÓN Y AUTORIZACIÓN

Diferenciar:

AUTHENTICATION:
quién es el usuario.

AUTHORIZATION:
qué puede hacer sobre un recurso.

No confiar en:

- userId;
- role;
- ownerId;
- permisos;

enviados en el payload cuando deban derivarse del usuario autenticado.

Para recursos privados:

verificar autorización y ownership donde corresponda.

`[Authorize]` o equivalente por sí solo puede no demostrar propiedad del
recurso.

Examinar el caso de uso completo.

---

# FASE 14 — MANEJO DE ERRORES

No llenar cada endpoint con try/catch repetitivos si existe infraestructura
centralizada.

Antes verifica:

- middlewares;
- Error handling;
- ProblemDetails;
- exception handlers existentes.

Favorecer respuestas consistentes.

No exponer al cliente:

- stack traces;
- SQL;
- detalles internos;
- filesystem;
- secretos.

Distinguir cuando corresponda:

400 — input inválido
401 — no autenticado
403 — no autorizado
404 — recurso inexistente
409 — conflicto de estado/concurrencia
500 — fallo interno inesperado

No hardcodear un status code si contradice el contrato existente.

No convertir todas las excepciones de persistencia en 409.

`DbUpdateException` puede representar distintas causas.

Clasificar solamente cuando exista evidencia suficiente.

---

# FASE 15 — PROBLEM DETAILS

Si el proyecto utiliza o puede utilizar la infraestructura estándar de
ASP.NET Core para errores:

preservarla.

No crear un formato paralelo de error sin necesidad.

Verifica primero:

AddProblemDetails
UseExceptionHandler
middlewares personalizados
tipos de error existentes

antes de modificar el esquema de respuestas.

Cualquier cambio en el contrato de errores requiere analizar consumidores.

---

# FASE 16 — CANCELACIÓN Y ASINCRONÍA

En operaciones async de I/O:

propagar CancellationToken cuando la arquitectura y las APIs involucradas lo
permitan.

No usar:

async void

excepto casos legítimos del framework que lo requieran.

No bloquear asincronía mediante:

.Result
.Wait()

sin una razón técnica demostrable.

Evitar operaciones sync de base de datos dentro de rutas async cuando exista
equivalente async apropiado.

---

# FASE 17 — ENTITY FRAMEWORK CORE

Al modificar persistencia verificar:

- tracking requerido/no requerido;
- Include/proyecciones;
- N+1;
- cardinalidad;
- índices relevantes;
- nullability;
- relaciones;
- cascadas;
- constraints;
- SaveChanges;
- concurrencia;
- CancellationToken.

Para queries de solo lectura:

considerar `AsNoTracking()` cuando aporte un beneficio real.

No agregarlo mecánicamente a toda query.

Preferir proyecciones cuando solo se requieren campos específicos.

No optimizar prematuramente sin evidencia.

---

# FASE 18 — MIGRACIONES Y ESQUEMA

Cambios en:

Entidades
Datos
Migrations
constraints
índices
columnas
tipos SQL
relaciones

requieren analizar impacto de esquema.

Crear o aplicar migraciones requiere:

AUTORIZACIÓN EXPLÍCITA DEL USUARIO.

No utilizar expresiones ambiguas como:

"autorización roja".

Nunca aplicar migraciones contra una base de datos real como efecto colateral
de una validación.

Antes de autorizar un cambio de esquema documentar:

- motivo;
- impacto;
- compatibilidad;
- datos existentes;
- rollback cuando aplique.

---

# FASE 19 — CONTRATOS FRONTEND ↔ BACKEND

Ante cualquier modificación de contrato verifica:

- ruta;
- verbo HTTP;
- route parameters;
- query parameters;
- request body;
- DTO request;
- DTO response;
- JSON property names;
- tipos;
- nullability;
- códigos HTTP;
- errores;
- autenticación/autorización.

Busca consumidores en Frontend antes de cambiarlo.

Si un cambio rompe consumidores:

NO lo hagas silenciosamente.

Activar:

Backend + Frontend + Auditor

para cambios contractuales.

No modificar un contrato únicamente para facilitar una vista específica sin
analizar el resto de consumidores.

---

# FASE 20 — OPENAPI

Si se modifica un contrato HTTP:

verificar que OpenAPI/metadata continúe representándolo correctamente según la
infraestructura existente.

No introducir atributos o paquetes OpenAPI innecesarios.

Preservar el mecanismo real configurado en Program.cs.

---

# FASE 21 — COMENTARIOS Y LEGIBILIDAD

NO introducir de forma obligatoria banners como:

// ============================================================================
// MÓDULO: ...
// ============================================================================

No comentar código obvio.

Priorizar:

- nombres expresivos;
- métodos enfocados;
- responsabilidades claras;
- estructuras tipadas;
- invariantes explícitas;
- tests.

Agregar comentarios cuando expliquen:

- una decisión de concurrencia;
- una invariante no evidente;
- una decisión transaccional;
- una restricción externa;
- una compensación;
- un workaround;
- una razón de seguridad.

Los comentarios deben explicar principalmente:

POR QUÉ

y no narrar:

QUÉ

hace cada línea.

---

# FASE 22 — CAMBIO MÍNIMO

No convertir una feature o bugfix en una reestructuración completa.

No crear:

- Unit of Work adicional;
- Generic Repository;
- MediatR;
- CQRS;
- nuevas capas;
- nuevas abstracciones;

salvo que la tarea y arquitectura real demuestren un beneficio concreto.

El repositorio ya posee una estructura.

Presérvala salvo autorización para modificarla.

---

# FASE 23 — FALLBACK PARA INFORMACIÓN AUSENTE

Si falta una entidad, DTO, contrato o configuración:

NO inventarla.

Primero buscar en:

- Totaltech;
- Frontend;
- Tests;
- documentación autorizada.

Si sigue faltando y es material para implementar correctamente:

detenerse.

Reportar:

- qué falta;
- por qué es necesario;
- qué decisión debe tomarse.

NO inventar:

- Fluent API;
- RowVersion;
- repositories;
- propiedades;
- tablas;
- índices;
- DTOs;

únicamente para hacer compilar una implementación imaginaria.

---

# FASE 24 — TESTING

Las pruebas deben corresponder al riesgo del cambio.

Para lógica de negocio:

priorizar tests unitarios cuando la capa lo permita.

Para EF Core, transacciones y concurrencia:

no asumir que un mock de DbContext demuestra comportamiento relacional real.

Cuando la garantía dependa de:

- SQL Server;
- constraint;
- transacción;
- aislamiento;
- concurrencia;
- traducción LINQ;

preferir un test de integración representativo si la infraestructura de tests
lo permite.

No usar EF Core InMemory como prueba suficiente de semántica relacional cuando
el comportamiento depende del proveedor.

No inventar framework de tests.

Descubrir los proyectos y paquetes existentes.

---

# FASE 25 — QUALITY GATES

Para cambios Backend, como mínimo:

dotnet restore Totaltech/Totaltech.csproj
dotnet build Totaltech/Totaltech.csproj --no-restore

Descubre los proyectos de test existentes y ejecuta los relevantes.

Además:

git diff --check
git status --short

## Si cambia endpoint

Verificar:

- route;
- verbo;
- binding;
- autorización;
- request;
- response;
- status codes;
- OpenAPI;
- consumidor.

## Si cambia lógica

Verificar:

- invariantes;
- casos límite;
- error path;
- tests.

## Si cambia persistencia

Verificar:

- query;
- tracking;
- SaveChanges;
- atomicidad;
- constraints;
- concurrencia;
- impacto de esquema.

## Si cambia dinero

Verificar:

- `decimal`;
- precisión/escala;
- redondeo;
- cálculo servidor;
- tests de borde.

## Si cambia stock

Verificar obligatoriamente:

- stock insuficiente;
- cantidad inválida;
- solicitudes concurrentes relevantes;
- ausencia de decremento parcial;
- comportamiento ante conflicto.

No exijas:

"cero warnings"

si ya existen warnings preexistentes.

La condición es:

- cero errores nuevos;
- ningún warning nuevo atribuible al cambio.

---

# FASE 26 — DEFINITION OF READY

Antes de modificar Backend deben conocerse o poder descubrirse:

- caso de uso;
- archivos afectados;
- contrato;
- consumidores;
- invariantes;
- autorización;
- persistencia;
- concurrencia relevante;
- estrategia transaccional;
- errores;
- pruebas necesarias.

Si falta una decisión crítica no recuperable:

detenerse y preguntar.

No preguntar por información que pueda descubrirse leyendo el repositorio.

---

# FASE 27 — DEFINITION OF DONE

Una tarea Backend se considera completa únicamente cuando:

- el comportamiento solicitado está implementado;
- la arquitectura existente se preserva o cualquier desviación está
  explícitamente autorizada;
- entidades no se filtran accidentalmente como contrato HTTP;
- inputs se validan;
- autorización y ownership se verifican cuando corresponde;
- los importes autoritativos se calculan en servidor;
- dinero utiliza precisión apropiada;
- inventario preserva sus invariantes también ante concurrencia relevante;
- la atomicidad requerida está garantizada;
- transacciones manuales solo existen cuando aportan una garantía necesaria;
- errores no filtran detalles internos;
- contratos afectados fueron comprobados con sus consumidores;
- build pasa;
- tests aplicables pasan o su imposibilidad está documentada;
- no hay errores nuevos;
- no hay warnings nuevos atribuibles;
- git diff --check pasa;
- git status contiene solo cambios esperados.

---

# FASE 28 — COORDINACIÓN DE PERFILES

Aplicar:

### Backend solamente

Para cambios internos bajo Totaltech sin efecto contractual/transversal.

### Backend + Frontend + Auditor

Para:

- cambio de endpoint;
- DTO público;
- status code contractual;
- autenticación compartida;
- serialización;
- comportamiento que afecte consumidores MVC.

### Backend + Auditor

Para:

- seguridad;
- inventario;
- concurrencia;
- pagos;
- transacciones críticas;
- datos sensibles;
- migraciones;
- esquema.

### Backend + CleanCode

Para refactor no trivial.

No ampliar permisos más allá de AGENTS.md.

---

# FASE 29 — REESCRITURA

Después del análisis:

reescribe ÚNICAMENTE:

ControlProyecto/Backend.md

No modifiques:

AGENTS.md
ControlProyecto/Auditor.md
ControlProyecto/Frontend.md
ControlProyecto/CleanCode.md

Frontend/**
Totaltech/**
Tests/**

Si detectas problemas funcionales en Backend:

NO los soluciones en esta tarea.

Repórtalos como hallazgos posteriores.

---

# VALIDACIÓN DE BACKEND.MD RESULTANTE

Comprueba que:

1. declare `READ_WRITE`;

2. delimite:

   Totaltech/**

3. no contenga rutas obsoletas como:

   ../Totaltech/

4. no hardcodee:

   .NET 8
   C# 12

5. descubra la versión desde `Totaltech.csproj`;

6. preserve:

   Endpoints -> Logica -> Repositorios -> EF Core

   mientras sea la arquitectura real;

7. no exija interfaces/repositorios nuevos sin necesidad;

8. no exponga entidades automáticamente como DTO;

9. no obligue a que todo DTO sea `record`;

10. exija `decimal` para importes financieros;

11. diferencie:

    atomicidad
    concurrencia

12. NO exija `BeginTransactionAsync` para toda operación;

13. reconozca la atomicidad de un único SaveChanges cuando aplique;

14. defina mecanismos correctos de concurrencia sin imponer uno universal;

15. no considere una simple comparación de stock como protección concurrente;

16. no exija RollbackAsync textual como Quality Gate;

17. prohíba confiar en precio/rol/ownership enviados por cliente;

18. contemple autorización de recursos;

19. defina manejo seguro y consistente de errores;

20. no exija banners de comentarios;

21. no invente contratos en el fallback;

22. defina testing relacional apropiado para concurrencia/transacciones;

23. requiera autorización explícita para migraciones;

24. coordine contratos con Frontend + Auditor;

25. sea coherente con AGENTS.md.

Ejecuta:

git diff --check
git status --short
git diff -- ControlProyecto/Backend.md

Verifica que ningún otro archivo haya sido modificado por esta tarea.

---

# CONTRATO DE SALIDA

Al terminar responde con:

## 1. Estado inicial

Incluye:

- rama;
- working tree;
- TargetFramework detectado;
- versión EF Core detectada;
- proveedor de base de datos detectado;
- arquitectura Backend observada;
- proyectos de test encontrados.

## 2. Problemas corregidos

Máximo 10 puntos.

## 3. Acción realizada

WRITE `ControlProyecto/Backend.md`

No listes operaciones que no ocurrieron.

## 4. Arquitectura resultante

Resume:

- Endpoints;
- Logica;
- Repositorios;
- EF Core;
- DTOs;
- seguridad;
- dinero;
- transacciones;
- concurrencia;
- contratos;
- testing.

## 5. Validación

Reporta:

- READ_WRITE: PASS/FAIL
- rutas normalizadas: PASS/FAIL
- runtime no inventado: PASS/FAIL
- separación de capas: PASS/FAIL
- entidades/DTO separados: PASS/FAIL
- dinero decimal: PASS/FAIL
- atomicidad/concurrencia diferenciadas: PASS/FAIL
- transacciones explícitas solo cuando necesarias: PASS/FAIL
- seguridad/autorización definida: PASS/FAIL
- fallback sin invenciones: PASS/FAIL
- testing de concurrencia adecuado: PASS/FAIL
- comentarios no decorativos: PASS/FAIL
- git diff --check: PASS/FAIL
- otros archivos modificados: YES/NO

## 6. Git final

Incluye:

git status --short

y un resumen del diff.

## 7. Hallazgos posteriores

Lista únicamente problemas reales del código detectados durante la inspección
que estén fuera del alcance documental de esta tarea.

No los soluciones todavía.

## 8. Contenido final de Backend.md

Muestra el contenido COMPLETO de:

ControlProyecto/Backend.md

en un único bloque Markdown.

DETENTE.

No hagas commit ni push.