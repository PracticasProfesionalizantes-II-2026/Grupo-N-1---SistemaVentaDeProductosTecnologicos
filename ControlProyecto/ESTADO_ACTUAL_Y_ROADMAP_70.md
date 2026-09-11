# TotalTech — Estado actual y roadmap hacia ≥70% sólido

Fecha de revisión: 2026-09-11. Encargo ejecutado: auditoría, validación aislada y documentación. **No se implementaron correcciones del producto.**

## 1. Pre-flight y dictamen

**Dictamen: FAIL para aceptar el checkout actual; auditoría estática transversal con límites de verificación.** Hay defectos de integridad confirmados y faltan recorridos MVC. La compilación y las pruebas rápidas de B son correctas; no prueban la experiencia completa ni la semántica de SQL Server.

Raíz auditada: `C:\Users\facu_\Documents\GitHub\Grupo-N-1---SistemaVentaDeProductosTecnologicos\Grupo-N-1---SistemaVentaDeProductosTecnologicos`.
El directorio padre también es Git, en `codex/rama_facu`, con cambios previos extensos y un repositorio anidado no registrado. No se modificó ese repositorio. La raíz anidada comenzó limpia, en `Rama--Facu`; no había cambios locales de código ni de documentos que sumar a B.

| Fuente | Referencia local | SHA | Árbol |
|---|---|---|---|
| A integrado | main / origin/main | f0a9e1ad7fc2f865da32ebd57b3e0fb3d0b6d446 | 26b61affb823a9ececee0affa4e225b21b7e4d5a |
| Integración intermedia | Develop / origin/Develop | 642aaf58dc8b4cf02f26a629cc3bb8f68d77dbd2 | Igual a A |
| B desarrollo | HEAD / Rama--Facu / origin/Rama--Facu | 61ee39b4fb5d1c650f85fab710b14e8b726085e0 | f9163f4da0d2d0351e3295964b3296bc8d4a9a64 |
| Otra contribución | origin/Rama--Dai | 442febd66a8f042ccbe32e7d4cd184c79170ec81 | No acreditado a A/B |

La consulta `git ls-remote --heads origin main Develop Rama--Facu Rama--Dai` falló por conexión a github.com:443. **Todas son referencias locales de vigencia remota no verificada.** No se hizo fetch, checkout, merge ni commit. Main y Develop comparten árbol, no SHA. B tiene un commit exclusivo frente a main: 61ee39b.

SDK instalado: 10.0.400. Los cuatro proyectos apuntan a net10.0; no se encontró global.json versionado. La solución incluye API, MVC, UnitTests e IntegrationTests. EF Core SQL Server, JwtBearer, OpenApi y Mvc.Testing están en 10.0.7; Microsoft.OpenApi 2.12.2, Scalar 2.14.11; xUnit 2.9.3, runner 3.1.4 y Test SDK 17.14.1. No se actualizaron dependencias.

## 2. Cobertura y fuentes

Inventario reproducido con `rg --files Totaltech Frontend Tests`, excluyendo bin, obj y wwwroot/lib; conteo de archivos .cs/.cshtml/.css/.js: Backend 78, Frontend 118, tests 5. Los 78 del Backend incluyen 11 archivos de migraciones/snapshot. No confundir este conteo con funcionalidades ni cobertura de líneas.

| Grupo homogéneo | Revisión realizada | Límite |
|---|---|---|
| Backend Program, Endpoints, Logica y DTOs, Repositorios, Entidades, Datos, Seguridad | Lectura del código propio y recorrido productores/consumidores | Sin ejecutar SQL real |
| Frontend Program, todos los Controllers, Services e interfaces, Models, Views, CSS/JS propios | Lectura estática, incluidas estructuras reservadas | Sin navegador de la aplicación ni evaluación de contraste/overflow en ejecución |
| Cinco fuentes de tests y dos csproj | Lectura de todos los casos, fake y factory; ejecución B | No se ejecutó A ni Dai |
| Migraciones y snapshot | Inventario, restricciones actuales en DbContext; lectura de la migración 20260622030413 con SQL manual y revisión parcial de otras migraciones | Designer/snapshot y DDL generado excluidos de lectura exhaustiva; no se validó aplicación/reversión de toda la cadena |
| Configuración, launchSettings, solución y workflow | Revisión de estructura, puertos y presencia de material sensible con valores redactados | No se abrieron User Secrets ni se probaron credenciales |
| AGENTS, Auditor, Frontend, CleanCode | Lectura de gobierno; correcciones mínimas de hechos | Auditor permanece READ_ONLY para producto; las escrituras documentales provienen del encargo explícito |
| Backend.md y roadmap 60 | Inspección de encabezado/estructura y secciones relevantes | Backend.md sigue siendo un encargo de reescritura; no se lo ejecutó ni reescribió como nueva tarea |
| PDF funcional | Texto pp.1–10, especialmente alcance pp.7–9 | Sin revisión visual completa del PDF ni validación de versiones en Drive |
| PDF API | Extracción e inspección parcial, incluidos Auth/consultas pp.47–54 | No se certifican todos sus contratos; el código actual prevalece |
| Mockups locales | Inspección visual de Carrito de Compras.png y Finalizar Compra -3.png | Resto de mockups, fotos de producto y enlaces externos no revisados visualmente |
| .git, bin, obj, wwwroot/lib | Excluidos de revisión manual del código | Bibliotecas vendorizadas no auditadas por seguridad actual |

Los archivos vacíos/.gitkeep son reservas, no implementación. En particular, `Frontend/Services/CarritosApiService.cs` contiene **CategoriasApiService**, pese a su nombre y comentario inicial. No existe allí un servicio de carrito utilizable. Interfaces y ViewModels reservados se abrieron y se clasificaron por su contenido.

Quedan áreas materiales **no verificadas**: persistencia/concurrencia SQL, UI/E2E, vigencia remota, vulnerabilidades actuales, despliegue y cadena completa de migraciones. No se presenta este informe como certificación exhaustiva de ausencia de fallas.

## 3. Alcance académico y arquitectura

El PDF funcional pp.7–9 exige catálogo con imágenes, búsqueda/filtros, carrito, pagos digitales, cuenta/historial, recuperación, administración, contacto y responsive. Sus requisitos de carga ≤3 s y disponibilidad ≥99% necesitan medición definida, no se acreditan por compilación. El encargo actual permite diferir pago externo y notificaciones para el hito intermedio. Recuperación real queda después del 70%, visible como pendiente; no se elimina del denominador.

Cadena real:

```text
Razor → Controller MVC → IHttpClientFactory / ApiService → API
Endpoint → Lógica → Repositorio → EF Core → SQL Server
```

Excepción estructural relevante: CarritosLogica coordina directamente DbContext/transacción y varios repositorios. Cada repositorio guarda cambios por operación. HomeController usa IHttpClientFactory directamente para Auth; no usa el AuthApiService placeholder.

Frontend escucha por defecto en 5087 (HTTPS 7169); ApiBaseUrl apunta a localhost:5070, puerto HTTP del Backend (HTTPS 7038). Cookie MVC contiene el token protegido; ApiBearerTokenHandler lo propaga como Bearer. Rol API Administrador se traduce a Admin MVC; el control efectivo reside en API. Política fallback autenticada, lecturas públicas de catálogo y altas públicas Auth/consultas; operaciones administrativas con política de rol y recursos de Cliente con ownership.

Program inicializa categorías y cuenta Admin al arrancar. **No se levantó el Backend normal**, no se tocó Azure SQL y no se aplicaron migraciones. Los tests reemplazan DbContext por InMemory antes del arranque y usan claves de prueba y DataProtection efímera.

### Estado funcional por módulo

Los estados indican implementación, no certificación. PARCIAL puede contener un subflujo roto; sus condiciones aparecen en hallazgos. Ningún módulo completo se acredita como COMPLETO VERIFICADO de extremo a extremo.

| Módulo | Backend A/B salvo nota | Frontend A/B | Entrada, consumidor, persistencia, autorización y prueba |
|---|---|---|---|
| Registro / login | PARCIAL | PARCIAL | AuthEndpoints → UsuariosLogica → UsuariosRepositorio; Home.Login/Register → POST /auth/login y /auth/registro. Público, hashing/JWT; tests Auth. Pendientes F06. |
| Logout | NO IMPLEMENTADO | NO IMPLEMENTADO | No se exige endpoint de revocación para Auth académico; falta SignOut MVC y navegación, F08. |
| Home / catálogo | PARCIAL | PARCIAL | Home.Index sólo logo. Productos.Index → ProductosApiService → GET /productos → ProductosRepositorio → Productos. Lectura pública; sin fotos en modelo ni pruebas UI. |
| Categorías / búsqueda | PARCIAL | PARCIAL | Categorias.Index y Productos.Buscar/Categoria/Disponibles → GET /categorias y /productos/{buscar,categoria,disponibles}. Filtros texto/categoría/stock; precio/marca faltan. |
| Detalle producto | PARCIAL | PARCIAL | Productos.Detalle → GET /productos/{id}. Precio y stock, sin imagen ni acción carrito; sin prueba UI. |
| Proveedores | PARCIAL | PARCIAL | ProveedoresController → ProveedoresApiService → /proveedores → ProveedoresLogica/Repositorio. Admin; FK opcional dirección, formulario por ID; no tests específicos. |
| Carrito / líneas | PARCIAL; A inseguro | PLACEHOLDER | /carritos y /detallecarritos → lógicas/repositorios homónimos → Carritos/DetalleCarritos. Owner/Admin; un caso HTTP feliz en B. F01/F02/F04/F06. |
| Checkout | PARCIAL con P0 | PLACEHOLDER | POST /carritos/{id}/confirmar → CarritosLogica → Pedidos, DetallePedidos y stock. Dirección propia. Sin garantía de idempotencia SQL, F02/F03. |
| Pedidos / líneas / historial | PARCIAL | PLACEHOLDER | GET /pedidos y /pedidos/usuario/{id}: propietario/Admin. GET detalle por ID, pero no listado de líneas del pedido para Cliente. Mutaciones genéricas Admin en B. Historial sin pantalla, F05/F08. |
| Perfil | PARCIAL | PLACEHOLDER | /usuarios/{id} proyecta UsuarioResponse y permite propio/Admin. B preserva rol/fecha para Cliente. Recuperación sólo consulta; F06/F12. |
| Direcciones | PARCIAL | PLACEHOLDER | /direcciones → DireccionesLogica/Repositorio → Direcciones; identidad autoritativa y lectura propia, 3 casos HTTP específicos. Falta formulario y snapshot histórico de envío. |
| Contacto / consultas | PARCIAL | PLACEHOLDER | POST /consultas público, B fuerza identidad/fecha/estado; GET propio por usuario, CRUD Admin. Un test B. Contactos enlaza Home. |
| Administración | PARCIAL | PARCIAL | Panel MVC sólo enlaza categorías/productos/proveedores. API gestiona usuarios/pedidos, pero transiciones débiles. Vistas Administracion/Categorias y Productos no tienen acciones consumidoras reales. |
| Pagos | PARCIAL | PLACEHOLDER | /pagos y /pedidos/{id}/pagos → PagosLogica/Repositorio; altas Admin, lecturas owner/Admin. Sólo registros manuales, no pasarela ni pruebas. |
| Compras | PARCIAL | PLACEHOLDER | /compras → ComprasLogica/Repositorio → Compra; Admin. Cabecera total manual; no líneas ni reposición automática. |
| Reportes | PARCIAL | PLACEHOLDER | /reportes/{ventas,ingresos,productos-mas-vendidos} → ReportesLogica/Repositorio. Admin, agregados reales sin filtros temporales ni tests SQL. |

Los borrados suelen capturar DbUpdateException y devolver 409; no hay manejo homogéneo equivalente para todas las altas/actualizaciones. Las relaciones usan Restrict, índice único de email y de (carrito,producto), pero no hay restricción de pedido único por carrito. Pedido no persiste total ni snapshot de dirección. Tener FK a una dirección mutable no preserva la dirección histórica.

## 4. Comparación de ramas

| Rama / SHA | Diferencia y evidencia | Acción propuesta | Dependencias y riesgo |
|---|---|---|---|
| Develop 642aaf5 | Árbol igual a main f0a9e1a | No hay delta que integrar | No atribuirle las pruebas de B |
| Rama--Facu 61ee39b | 17 archivos; 363 líneas añadidas y 54 eliminadas frente a A. Precio autoritativo, estado/fechas, alta pedidos Admin, bootstrap/validaciones y 7 casos adicionales | Adaptar e integrar en wave 1 con regresión; checkout requiere wave 2 | No afirmar atomicidad/idempotencia completa; ignorar warning InMemory no valida SQL |
| Rama--Facu Frontend | Quita aceptación obligatoria de términos de modelo/controller/vista; conserva MVC/Auth | Mantener coherencia con alcance académico, sin asignar aumento de funcionalidad por borrar el checkbox | Privacidad de entrega final debe tratarse explícitamente |
| Rama--Dai 442febd | Base común 2ae7b1e7e7758a06bca63672dfc6e8881d355da3. Comparación total: 263 archivos, +421/-9225; aporta imágenes en photos/Categorias e inicio | Evaluar/adaptar únicamente assets útiles en wave 4 | No servidos automáticamente desde photos; verificar licencia/procedencia, tamaño y mapeo |
| Rama--Dai código | Home sólo Index/Privacy/Login GET/Error; Program MVC sin cookie, cliente HTTP ni handler; faltan controllers CRUD y tests del árbol A | No incorporar como reemplazo de A/B | Sustituir archivos completos perdería Auth y CRUD reales; es incompatibilidad funcional, no mero conflicto textual |

Commits exclusivos de Dai observados: 442febd, 535c11e, dfdb587, 8764679, 3a6b7c1 y dcf39e7. No se ejecutó su código. Sus imágenes no cuentan como catálogo implementado y su conjunto no fue auditado exhaustivamente. La recomendación de no incorporar código no autoriza borrarlo.

## 5. Hallazgos y cierre

Prioridades del encargo: P0 impide aceptar integridad/seguridad básica; P1 para el 70%; P2 entrega final; P3 mejora; P4 opcional. CONFIRMADO por código no significa reproducido en Azure. Donde el comportamiento depende de fallos del proveedor se mantiene SOSPECHA. Las rutas sin prefijo corresponden a Totaltech salvo indicación.

### F01 — Precio y estado controlables por el cliente en main

- Baseline: A; corregido parcialmente en B. SHA A/B: tabla del pre-flight.
- Evidencia: **CONFIRMADO**. Prioridad: **P0**.
- Hecho y condición: En A un cliente puede enviar un precio positivo menor al real, que se guarda en la línea y se copia al pedido. También puede reabrir su carrito cambiando Estado. B fuerza precio de producto y conserva el estado para Cliente.
- Archivo/símbolo: Totaltech/Logica/CarritosLogica.cs, AgregarProductoAsync y ConfirmarAsync; DetalleCarritosLogica.CrearAsync/ActualizarAsync; CarritosEndpoints POST/PUT. Diff main..HEAD.
- Impacto: Importes históricos manipulables y posibilidad de reutilizar un carrito confirmado.
- Remediación: Adaptar las correcciones de B como unidad y mantener invariantes en todos los CRUD, sin acreditar la corrección completa de checkout por este cambio.
- Dependencia: waves 1 y 2; requiere tarea de implementación posterior.
- Cierre: Los dos contratos de líneas ignoran precios manipulados; Cliente no puede reabrir el carrito; pruebas HTTP negativas y SQL aisladas.

### F02 — Confirmación sin exclusión del mismo carrito

- Baseline: A y B. SHA A/B: tabla del pre-flight.
- Evidencia: **CONFIRMADO por revisión de la secuencia; no reproducido en SQL**. Prioridad: **P0**.
- Hecho y condición: Dos solicitudes pueden leer Estado=Activo antes de la transacción. En B cada una inserta su pedido y descuenta stock si alcanza; la escritura final del carrito no compara el estado original. No existe relación única pedido-carrito ni clave de idempotencia.
- Archivo/símbolo: CarritosLogica.ConfirmarAsync; Entidades/Pedido.cs; Datos/TotaltechDbContext.OnModelCreating; CarritosRepositorio.ActualizarAsync.
- Impacto: Dos pedidos y dos descuentos para un solo acto de compra, incluso con actualización de stock atómica.
- Remediación: Diseñar una confirmación con exclusión persistente y resultado recuperable por carrito/clave. Incluir lecturas y validación en la unidad consistente; coordinar las otras mutaciones del carrito.
- Dependencia: waves 2; requiere tarea de implementación posterior.
- Cierre: Dos contextos SQL sincronizados antes de confirmar producen un único pedido y un único descuento. Reenvío secuencial devuelve el resultado existente o conflicto documentado.

### F03 — Reintento con contexto y lecturas de un intento anterior

- Baseline: B; A además inicia transacción fuera de la estrategia. SHA A/B: tabla del pre-flight.
- Evidencia: **SOSPECHA sobre el resultado concreto tras fallos; estructura confirmada**. Prioridad: **P1**.
- Hecho y condición: Rollback SQL no restaura automáticamente el grafo CLR ni las identidades ya aceptadas por SaveChanges. El delegado reintentado reutiliza ese contexto. Tampoco verifica si un commit de respuesta perdida fue efectivo.
- Archivo/símbolo: CarritosLogica.ConfirmarAsync captura carrito, dirección, detalles y productos fuera de ExecuteAsync; repositorios llaman SaveChanges por operación; Program habilita EnableRetryOnFailure.
- Impacto: Replay sobre estado obsoleto, errores de persistencia o duplicación tras commit ambiguo; falta una prueba representativa para determinar cada resultado.
- Remediación: Usar una unidad reintentable con estado fresco y verificación del resultado persistido. Validar también que A no falle por transacción manual bajo estrategia SQL.
- Dependencia: waves 2; requiere tarea de implementación posterior.
- Cierre: Inyectar fallo antes de commit, después de un SaveChanges y pérdida de respuesta de commit; verificar una sola operación completa, sin registros parciales ni duplicados.

### F04 — Movimiento de una línea fuera de un carrito confirmado

- Baseline: A y B. SHA A/B: tabla del pre-flight.
- Evidencia: **CONFIRMADO por código**. Prioridad: **P1**.
- Hecho y condición: El endpoint comprueba ownership del origen y destino, pero reemplaza IdCarrito antes de validar. La lógica comprueba solamente que el carrito destino esté activo.
- Archivo/símbolo: Endpoints/DetalleCarritosEndpoints.cs, MapPut y EsCarritoAccesibleAsync; DetalleCarritosLogica.ValidarDetalleAsync.
- Impacto: Un propietario puede retirar una línea de un carrito confirmado moviéndola a otro activo. No equivale a cambiar automáticamente DetallePedido, pero rompe la inmutabilidad del carrito de origen.
- Remediación: Prohibir reasignación o validar explícitamente estado original y destino antes de mutar; cerrar carreras con la confirmación.
- Dependencia: waves 2; requiere tarea de implementación posterior.
- Cierre: Mover una línea desde origen confirmado/cancelado es rechazado y no altera ninguna fila, también bajo concurrencia.

### F05 — Pagado no representa un importe conciliado

- Baseline: A y B. SHA A/B: tabla del pre-flight.
- Evidencia: **CONFIRMADO por código**. Prioridad: **P1 para contener antes del 70%; P2 para pago externo**.
- Hecho y condición: Cualquier pago aprobado de monto positivo hace pasar un pedido pendiente a Pagado. No se compara monto con total. Pago y estado se guardan en operaciones separadas. Admin puede cambiar estados o líneas históricas sin máquina de transiciones ni compensación de stock.
- Archivo/símbolo: PagosLogica.ValidarPagoAsync y SincronizarEstadoPedidoAsync; PagosRepositorio y PedidosRepositorio; PedidosEndpoints/DetallePedidosEndpoints.
- Impacto: Un registro administrativo de importe insuficiente puede declarar una venta pagada; errores intermedios dejan estados incongruentes. El acceso está restringido a Admin, no se demuestra aprobación pública por un Cliente.
- Remediación: Antes del 70%, definir totales históricos, transiciones y contención de pagos manuales; impedir estados incoherentes por CRUD alternativos. Después, integrar pago externo con idempotencia y conciliación.
- Dependencia: waves 2 y 6; cierre externo posterior; requiere tarea de implementación posterior.
- Cierre: Importe insuficiente no liquida; cancelación/reversión aplica la política una sola vez; errores no dejan pago/pedido divergentes; edición de líneas pagadas queda prohibida.

### F06 — Validaciones no alineadas con null y límites persistidos

- Baseline: A y B; B añade validación al alta. SHA A/B: tabla del pre-flight.
- Evidencia: **CONFIRMADO por código; HTTP malformado no ejecutado**. Prioridad: **P1**.
- Hecho y condición: Se usa Trim antes de validar null en registro/API y formularios. En B el mínimo de contraseña sólo se exige al crear, no al cambiar. La suma de cantidades int no está comprobada. Se aceptan decimales superiores a decimal(18,2), y no se valida el máximo 256 de email en la lógica.
- Archivo/símbolo: UsuariosLogica.NormalizarEmail/CrearAsync/ActualizarAsync/ValidarUsuario; HomeController.Login/Register; CarritosLogica.AgregarProductoAsync; ProductoRequest MVC; DbContext.
- Impacto: Entradas inválidas pueden producir excepciones o valores incoherentes; una contraseña nueva corta evita la regla del alta. La protección del descuento B evita descontar cantidades no positivas, pero no corrige la línea corrupta.
- Remediación: Validar antes de normalizar, aplicar límites compartidos y aritmética comprobada; alinear precisión/redondeo con SQL y manejar conflictos.
- Dependencia: waves 2, 3 y 4; requiere tarea de implementación posterior.
- Cierre: Null, vacíos, email excesivo, nueva contraseña corta, int.MaxValue acumulado y decimal fuera de rango devuelven error controlado sin cambios persistidos.

### F07 — Entidades navegables en respuestas de negocio

- Baseline: A y B. SHA A/B: tabla del pre-flight.
- Evidencia: **SOSPECHA de exposición sensible; contrato confirmado**. Prioridad: **P1 para verificar y acotar**.
- Hecho y condición: El hash es serializable si una navegación Usuario queda cargada. No se observó Include de usuario en estos repositorios ni una respuesta HTTP actual que lo exponga; no se afirma fuga demostrada.
- Archivo/símbolo: Entidades/Usuario.Contrasena; navegaciones Usuario de Carrito/Consulta/Direccion/Pedido/Reporte; endpoints devuelven esas entidades. Auth y Usuarios sí proyectan UsuarioResponse.
- Impacto: Un cambio de tracking o de carga puede incorporar datos sensibles a un contrato público sin modificar el endpoint. Las pruebas actuales sólo recorren registro/login/usuarios para ausencia de hashes.
- Remediación: Proyectar respuestas de negocio explícitas y probar serialización con relaciones cargadas; evitar devolver grafos completos.
- Dependencia: waves 2 y 3; requiere tarea de implementación posterior.
- Cierre: Recorrer recursivamente respuestas de carritos, direcciones, pedidos, pagos, consultas y reportes con relaciones cargadas; ningún campo de contraseña/hash/salt.

### F08 — Compra y cuenta sin recorrido MVC

- Baseline: A y B. SHA A/B: tabla del pre-flight.
- Evidencia: **CONFIRMADO**. Prioridad: **P1**.
- Hecho y condición: Los cinco controllers contienen únicamente un comentario. No existe cierre de sesión. El detalle de producto no ofrece agregado al carrito. Contactos y el icono de carrito del layout llevan a Home.
- Archivo/símbolo: Frontend/Controllers/{Carrito,Checkout,Pedidos,Cuenta,Consultas}Controller.cs; Views correspondientes; Services e interfaces reservadas; HomeController sin Logout.
- Impacto: El usuario puede consultar catálogo y acceder, pero no completar una compra ni consultar su historial desde la aplicación MVC.
- Remediación: Implementar sesión, cuenta/direcciones y luego la cadena carrito-confirmación-historial; enlazar navegación real.
- Dependencia: waves 3, 5 y 6; requiere tarea de implementación posterior.
- Cierre: Un usuario nuevo termina con pedido propio pendiente, historial y logout; otro usuario no puede acceder a sus recursos.

### F09 — Errores de API sin experiencia consistente

- Baseline: A y B. SHA A/B: tabla del pre-flight.
- Evidencia: **CONFIRMADO por código**. Prioridad: **P1**.
- Hecho y condición: Las lecturas usan GetFromJsonAsync/EnsureSuccessStatusCode sin tratamiento local de transporte o status salvo 404 puntual. Algunas mutaciones trasladan el cuerpo API sin sanitizar a ModelState; Productos.Index no presenta TempData tras crear/editar/eliminar.
- Archivo/símbolo: Frontend/Services/{Productos,Proveedores}ApiService.cs y CarritosApiService.cs (clase CategoriasApiService); controllers consumidores; HomeController.LeerErrorApiAsync.
- Impacto: Una caída o 401 se convierte en error genérico; feedback inconsistente. Mostrar detalles internos depende del cuerpo que envíe la API; no se afirma un stack trace observado.
- Remediación: Unificar resultados tipados y mensajes seguros, distinguir 400/401/403/404/409/transporte/timeout y preservar formularios.
- Dependencia: waves 3 y 4; requiere tarea de implementación posterior.
- Cierre: Pruebas con respuestas simuladas y navegador para cada estado; no se pierde input ni aparecen datos de infraestructura.

### F10 — Validación automática concentrada en Auth y memoria

- Baseline: A y B. SHA A/B: tabla del pre-flight.
- Evidencia: **CONFIRMADO**. Prioridad: **P1**.
- Hecho y condición: B ejecuta 23 casos; SQL se sustituye por InMemory y se ignora TransactionIgnoredWarning. No hay suite MVC/E2E. El workflow usa SDK 8 y rutas your-solution-name/your-test-project-path, además de empaquetado WAP ajeno a la solución.
- Archivo/símbolo: Tests/Totaltech.IntegrationTests/Infrastructure/TotaltechWebApplicationFactory.cs y ambas suites; .github/workflows/dotnet-desktop.yml.
- Impacto: Una suite verde no garantiza constraints, rollback, SQL, concurrencia ni UX. CI no constituye una validación reproducible de esta solución tal como está configurado.
- Remediación: Conservar tests rápidos; añadir SQL aislado y MVC/E2E por wave, configurar CI web .NET según los proyectos reales.
- Dependencia: waves 1 a 6; requiere tarea de implementación posterior.
- Cierre: CI compila los cuatro proyectos y ejecuta las suites con datos aislados; pruebas fallan ante doble confirmación, autorización rota y contrato incorrecto.

### F11 — Material sensible versionado y bootstrap no apto para exposición pública

- Baseline: A y B. SHA A/B: tabla del pre-flight.
- Evidencia: **CONFIRMADO en archivos; vigencia externa NO VERIFICADA**. Prioridad: **P1 antes de exposición; no cambia credenciales en esta auditoría**.
- Hecho y condición: Hay material de conexión versionado y credenciales académicas canónicas en el arranque. No se probaron credenciales ni se consultaron User Secrets. El bootstrap normal puede escribir categorías y administrador.
- Archivo/símbolo: Totaltech/appsettings.json contiene ConnectionStrings.DefaultConnection con campo Password; Credenciales.txt; Program y UsuariosLogica.AsegurarAdministradorAsync.
- Impacto: Riesgo de acceso si los secretos son vigentes y peligro de modificar la base compartida durante un smoke test.
- Remediación: Mantener esta auditoría sin ejecución normal. Planificar gestión/rotación de secretos con autorización del propietario y entorno privado de demostración; preservar Auth académico acordado.
- Dependencia: waves 1; antes de cualquier despliegue; requiere tarea de implementación posterior.
- Cierre: No se publican secretos ni se usa la base compartida para pruebas; propietario confirma tratamiento de exposición y acepta los límites de la demo.

### F12 — Alcance final incompleto y documentación ambigua

- Baseline: A y B. SHA A/B: tabla del pre-flight.
- Evidencia: **CONFIRMADO**. Prioridad: **P2**.
- Hecho y condición: Recuperación responde sin registrar solicitud ni enviar recuperación; reportes agregan sin aplicar el período guardado. Imágenes, compras detalladas, pago externo y notificaciones no están implementados. Backend.md es un encargo de reescritura; CleanCode termina con fence abierto.
- Archivo/símbolo: Producto sin campo imagen; ReportesRepositorio.ObtenerVentasAsync/ObtenerIngresosAsync sin fechas; Compra sin líneas; UsuariosLogica.RecuperarContrasenaAsync sólo consulta existencia; PDF funcional pp.7–9 y API pp.47–49; Backend.md encabezado; CleanCode.md final.
- Impacto: No se puede presentar la lista de CRUD como cumplimiento integral de requisitos. Los perfiles pueden inducir tareas fuera de alcance.
- Remediación: Mantener pendientes explícitos, no simular recuperación ni cobro; completar alcance final en iteraciones posteriores. Corregir sólo contradicciones documentales objetivas.
- Dependencia: waves 4 y 6; etapa final; requiere tarea de implementación posterior.
- Cierre: Trazabilidad requisito-prueba; recuperación verificable o exclusión aceptada; reportes concilian por período; gobierno utilizable.


Observaciones menores P3: etiqueta Precio que imprime Descripcion en Categorias/Detalle; vistas administrativas huérfanas; naming CarritosApiService; CSS reservado y estilos sin consumidores. No son fallas críticas. La respuesta distinta 404/401 de login revela existencia de cuenta, pero responde al flujo académico vigente; registrar su aceptación y revisar antes de exposición pública, sin rediseñar Auth en esta auditoría.

## 6. Método de medición antes de los resultados

La matriz conserva **100 unidades de peso por capa** en todas las etapas. Se asigna 12 a identidad, 12 a carrito y 12 a checkout por su carácter transversal; catálogo y descubrimiento suman 21; cuenta/dirección/historial suman 20; administración suma 10. El resto mantiene sesión y funciones complementarias, incluidos pagos digitales, compras y reportes. Los CRUD accesorios no dominan la puntuación. No hay promedio general que oculte una capa débil.

| ID | Capacidad y límite de alcance | Peso |
|---|---|---|
| C01 | Registro/login, hash, roles, bootstrap y errores; sin MFA/refresh | 12 |
| C02 | Sesión MVC, Bearer, vencimiento y logout local seguro; no exige revocar todo JWT | 4 |
| C03 | Home y catálogo integrado con fotos, estados vacíos y navegación | 10 |
| C04 | Búsqueda, categoría, disponibilidad y filtros funcionales acordados | 6 |
| C05 | Detalle completo, imagen, precio/stock e inicio de compra | 5 |
| C06 | Carrito propio, cantidades, eliminar, resumen y concurrencia de mutaciones | 12 |
| C07 | Direcciones propias completas y preservación histórica del envío | 7 |
| C08 | Confirmación única, stock atómico, rollback/retry, total histórico; termina pendiente | 12 |
| C09 | Historial y detalle propio, importes/estados consistentes y actualización visible | 8 |
| C10 | Perfil propio y recuperación real; sin recuperación máximo 0,75 | 5 |
| C11 | CRUD catálogo/categorías/proveedores, imágenes, relaciones y errores | 6 |
| C12 | Administración de pedidos y usuarios con transiciones válidas | 4 |
| C13 | Contacto público y gestión/respuesta de consultas | 3 |
| C14 | Pago digital confirmado, conciliación e idempotencia; manual máximo 0,25 | 3 |
| C15 | Compra a proveedor con líneas y reposición; cabecera máximo 0,25 | 1 |
| C16 | Reportes correctos por período y conciliados | 2 |

Escalas de evaluación:

- **Funcional:** 0 ausente/placeholder; 0,25 fragmento útil; 0,50 subflujo sustancial con huecos; 0,75 recorrido principal implementado con pendientes explícitos; 1 alcance completo de la capacidad. Es inspección de implementación, no prueba de ejecución.
- **Solidez:** 0 ausencia o P0 que invalida el flujo principal; 0,25 implementación revisada con controles insuficientes; 0,50 contratos/reglas relevantes revisados pero sin pruebas representativas suficientes; 0,75 implementación con pruebas relevantes ejecutadas y pendientes acotados; 1 alcance cerrado y pruebas representativas de éxito, errores, autorización y persistencia/concurrencia cuando aplique. Ningún valor parcial significa “completamente verificado”.
- Toda puntuación de solidez se limita por la implementación y por evidencia faltante. En A se da crédito estático, **no se acredita ejecución**. En Frontend el máximo actual es 0,25 porque sólo se revisó fuente y compilación.
- **E2E acreditado:** peso de capacidades demostradas por navegador→MVC→API→persistencia aislada, dividido por 100. Un test directo API no acredita MVC. Sin demostración se puntúa 0.
- **Madurez de testing:** matriz de riesgos separada más abajo. No es cobertura de líneas ni porcentaje de tests pasados.

Fórmula por indicador/capa/baseline: `redondear(100 × Σ(peso × puntuación) / Σ(peso))`; Σ(peso)=100. Redondeo convencional al entero más próximo, mitad hacia arriba. Conservar las puntuaciones originales para recalcular, no sumar deltas ya redondeados.

### Puntuaciones y evidencia por capacidad

AF/AS = funcional/solidez Backend A; BF/BS = Backend B; FF/FS = Frontend igual en A y B. Evidencia específica: matriz funcional y F01–F12; tests ejecutados sólo B.

| ID | Peso | AF | AS | BF | BS | FF A/B | FS A/B |
|---|---|---|---|---|---|---|---|
| C01 | 12 | 0,75 | 0,5 | 0,75 | 0,75 | 0,75 | 0,25 |
| C02 | 4 | 0,5 | 0,25 | 0,5 | 0,25 | 0,25 | 0,25 |
| C03 | 10 | 0,75 | 0,5 | 0,75 | 0,5 | 0,5 | 0,25 |
| C04 | 6 | 0,75 | 0,5 | 0,75 | 0,5 | 0,75 | 0,25 |
| C05 | 5 | 0,5 | 0,5 | 0,5 | 0,5 | 0,5 | 0,25 |
| C06 | 12 | 0,75 | 0,25 | 0,75 | 0,5 | 0 | 0 |
| C07 | 7 | 0,75 | 0,5 | 0,75 | 0,5 | 0 | 0 |
| C08 | 12 | 0,5 | 0 | 0,75 | 0,25 | 0 | 0 |
| C09 | 8 | 0,5 | 0,25 | 0,5 | 0,25 | 0 | 0 |
| C10 | 5 | 0,5 | 0,25 | 0,5 | 0,25 | 0 | 0 |
| C11 | 6 | 0,75 | 0,5 | 0,75 | 0,5 | 0,75 | 0,25 |
| C12 | 4 | 0,5 | 0,25 | 0,5 | 0,25 | 0,25 | 0,25 |
| C13 | 3 | 0,5 | 0,25 | 0,75 | 0,5 | 0 | 0 |
| C14 | 3 | 0,25 | 0,25 | 0,25 | 0,25 | 0 | 0 |
| C15 | 1 | 0,25 | 0,25 | 0,25 | 0,25 | 0 | 0 |
| C16 | 2 | 0,5 | 0,25 | 0,5 | 0,25 | 0 | 0 |

Operaciones sin redondear: A Backend funcional=62,25 y solidez=33,50; B Backend funcional=66 y solidez=43,25. Frontend funcional=27,50 y solidez=11,75 en ambos. Las puntuaciones de C06/C08 siguen limitadas por F02–F04 pese a la mejora B. C14 no se retira del denominador para facilitar el 70%.

| Indicador | A integrado local | B rama actual | Lectura |
|---|---|---|---|
| Funcional Backend | 62% | 66% | Implementación, no certificación |
| Solidez Backend | 34% | 43% | Incremento estático/pruebas B, P0 pendiente |
| Funcional Frontend | 28% | 28% | Pantallas de compra ausentes |
| Solidez Frontend | 12% | 12% | No validación UI |
| E2E acreditado | 0% | 0% | No se ejecutó recorrido MVC completo |
| Madurez testing | 10% | 29% | Matriz de riesgos, no cobertura de código |

### Riesgos de testing y evidencia

Escala: 0 sin caso representativo; 0,25 casos existentes sólo leídos; 0,50 casos ejecutados en sustituto útil con huecos materiales; 0,75 éxitos/negativos ejecutados representativos del riesgo acotado; 1 incluye regresión adversa/relacional/E2E que el riesgo requiere.

| Riesgo | Peso | A | B | Evidencia y hueco |
|---|---|---|---|---|
| Credenciales, token y bootstrap | 20 | 0,25 | 0,75 | Hash/login/Admin, errores y rol; faltan expiración/null/cambio corto |
| Ownership y autorización | 20 | 0,25 | 0,50 | Dirección propia/ajena y categoría Admin; no matriz completa de recursos |
| Importes y estado autoritativo | 15 | 0 | 0,25 | Caso carrito B ejecutado, pero sin precisión SQL/pagos/CRUD alternativos representativos |
| Stock, rollback, retry e idempotencia SQL | 20 | 0 | 0 | InMemory no demuestra este riesgo |
| MVC, CSRF y experiencia E2E | 15 | 0 | 0 | No suite de Frontend ni navegador |
| CI reproducible y regresión de entrega | 10 | 0 | 0 | Workflow no ajustado a solución |
| Total ponderado | 100 | 10 | 28,75 | Redondeado: 10% y 29% |

## 7. Roadmap secuencial

Las proyecciones **no son resultados logrados**. Parten de A; wave 1 recupera/adapta el trabajo B antes de avanzar. Se usa la misma matriz y el mismo denominador; se cambia únicamente la puntuación de capacidades explícitas. Cada avance se acredita al SHA integrado que pase sus pruebas. Si un gate falla, no se concede el incremento.

### Wave 1 — Consolidar la base y las pruebas

Prioridad: P0/P1. Área: Ambas y validación. Esfuerzo relativo: **M**, no estimación de días.

Archivos/módulos: Diff 61ee39b de Totaltech/Endpoints, Logica, Repositorios; Tests; Frontend/Home y AuthViewModels; .github/workflows/dotnet-desktop.yml (futura modificación).

Tareas ordenadas:

1. Revalidar main remoto y el diff de B sin mezclar Rama--Dai. Preparar una integración revisable de las correcciones de precio, fechas, estado y alta directa de pedidos; no asumir que el checkout de B está cerrado.
2. Conservar los tests de B, corregir el pipeline web en una tarea autorizada y especificar una instancia SQL desechable con datos ficticios y guardia que rechace el servidor compartido.
3. Registrar casos de reproducción F01–F07 y acordar idempotencia, total/dirección históricos, cancelación y pago pendiente antes de cambiar contratos.
4. Definir el límite de exposición de la demo y tratamiento de secretos con el propietario, sin publicar ni copiar sus valores.

Dependencias y riesgos: Parte de A. Adaptar B; su delta funcional BE es +4 puntos redondeados y de solidez +10, no sumarlos al porcentaje como dos mejoras independientes. No recuperar código de Dai. Instancia SQL aislada y decisiones de negocio son prerrequisitos para wave 2.

Pruebas obligatorias: Build de cuatro proyectos; ejecutar y descubrir tests en el nuevo SHA integrado; al menos los 23 casos de B y casos negativos de precio/owner/estados. Comprobar que CI nunca apunta a Azure compartido.

Cambios de solidez: C01: BE 0.5→0.75, FE 0.25→0.25; C06: BE 0.25→0.5, FE 0→0; C08: BE 0→0.25, FE 0→0; C13: BE 0.25→0.5, FE 0→0. Sin otros cambios de puntuación.
**Proyección acumulada: Backend 43%; Frontend 12%.**

Gate de salida: SHA de integración documentado y regresión rápida verde; defectos transaccionales siguen abiertos y con reproducciones preparadas. No pasar si se pierde hashing/roles/ownership.

Fuera de alcance de esta wave: No UI de checkout, pasarela, migraciones aplicadas a Azure ni refactor general.

### Wave 2 — Proteger carrito y confirmación

Prioridad: P0/P1. Área: Backend. Esfuerzo relativo: **L**, no estimación de días.

Archivos/módulos: CarritosLogica, DetalleCarritosLogica, PedidosLogica, DetallePedidosLogica, PagosLogica; endpoints/repositories asociados; DTOs y entidades Pedido/DetallePedido; DbContext y migración sólo si la futura tarea los autoriza; Tests/Persistencia.

Tareas ordenadas:

1. Cerrar F02/F03 con relación persistente pedido-carrito o clave única, estado fresco por intento y recuperación del resultado tras commit ambiguo.
2. Cerrar F04: validar origen antes de mutación; coordinar agregar/editar/borrar con confirmar. Tratar conflictos de duplicados y overflow sin efectos parciales.
3. Definir importe y dirección históricos; centralizar transiciones de pedido y prohibir modificar líneas confirmadas por CRUD alternativo. Cancelación debe tener política de stock explícita e idempotente.
4. Contener F05 en API existente: importe conciliado para Pagado, escritura consistente de pago/pedido y rechazo de cambios históricos incompatibles, aunque la pasarela quede pendiente.
5. Añadir respuestas seguras de negocio y listado de líneas por pedido propio. No declarar F07 fuga real sin test de serialización.

Dependencias y riesgos: Wave 1. Contratos y posibles restricciones de esquema aprobados para implementar en ambiente aislado; recuperación del checkout B exige adaptación, no cherry-pick ciego.

Pruebas obligatorias: SQL Server aislado: dos contextos y barreras para mismo carrito y último stock; reenvíos; cambios concurrentes; fallo en mitad de líneas; rollback; retry y commit ambiguo; montos extremos; historial inmutable; owner; serialización con navegaciones.

Cambios de solidez: C06: BE 0.5→0.75, FE 0→0; C08: BE 0.25→1, FE 0→0; C09: BE 0.25→0.5, FE 0→0; C12: BE 0.25→0.5, FE 0.25→0.25. Sin otros cambios de puntuación.
**Proyección acumulada: Backend 58%; Frontend 12%.**

Gate de salida: F01/F02 cerrados en el SHA integrado y pruebas relacionales reproducibles; F03 demostrado y tratado. Pedido único, stock no negativo, total estable y cero escrituras parciales.

Fuera de alcance de esta wave: Pantallas nuevas, cobro externo y aplicación de migraciones al servidor compartido.

### Wave 3 — Completar sesión cuenta y direcciones

Prioridad: P1. Área: Ambas. Esfuerzo relativo: **M**, no estimación de días.

Archivos/módulos: Frontend/HomeController, CuentaController, AuthViewModels, Views/Home, Views/Cuenta y Shared/_Layout; servicios de usuarios/direcciones; UsuariosLogica, DireccionesLogica, DTOs; Tests.

Tareas ordenadas:

1. Completar logout POST con antiforgery y eliminar cookie/token local; mostrar identidad actual y navegación coherente. No agregar refresh/MFA.
2. Validar campos antes de Trim y aplicar mínimo de contraseña también al cambiarla. Cubrir vencimiento, 401 y destino local de retorno seguro.
3. Implementar perfil y CRUD de direcciones propias, con mensajes por campo y conservación de entrada. Validar campos de envío necesarios y errores de relaciones.
4. Probar respuestas sin hashes y que dirección editada no cambie el snapshot de un pedido ya confirmado.

Dependencias y riesgos: Wave 2 y contratos de identidad/envío estables. Recuperación por correo sigue diferida: C10 sólo llega a 0.75.

Pruebas obligatorias: Unitarios, HTTP y MVC/navegador: registro válido/inválido/null, email repetido, login erróneo, vencimiento, logout, CSRF, dos usuarios, dirección ajena, cambio de contraseña e inmutabilidad de envío.

Cambios de solidez: C01: BE 0.75→1, FE 0.25→0.75; C02: BE 0.25→1, FE 0.25→1; C07: BE 0.5→0.75, FE 0→0.75; C10: BE 0.25→0.75, FE 0→0.75. Sin otros cambios de puntuación.
**Proyección acumulada: Backend 69%; Frontend 30%.**

Gate de salida: Cuenta y dirección propias funcionan en entorno aislado; cookie y token ya no se envían después de logout; F06 de Auth cerrado.

Fuera de alcance de esta wave: Cambio de credencial canónica, proveedor externo de identidad y recuperación real de contraseña.

### Wave 4 — Completar catálogo y administración de productos

Prioridad: P1. Área: Ambas. Esfuerzo relativo: **M**, no estimación de días.

Archivos/módulos: Frontend/ProductosController, CategoriasController, ProveedoresController, Services y Views correspondientes; Models/Api; wwwroot/css/site.css; Backend Productos/Categorias/Proveedores y contratos.

Tareas ordenadas:

1. Completar catálogo, búsqueda, filtros acordados y detalle; incorporar contrato de imagen con alternativa válida si no hay foto. Resolver referencias de proveedor con selector usable.
2. Homogeneizar manejo de errores y feedback; rehidratar listas; validar importes según precisión SQL y entradas nulas/extremas.
3. Revisar contraste, labels, foco y tablas móviles en los recorridos activos. Adaptar material gráfico de Dai sólo con mapeo a producto, procedencia y rutas servidas verificadas.
4. Eliminar confusión de vistas administrativas huérfanas en la futura implementación: conectar o retirar dentro de su alcance, no duplicar controllers.

Dependencias y riesgos: Wave 3. Usar contratos reales; imágenes de Rama--Dai son candidatas, no integración automática. Decidir almacenamiento de imágenes antes de añadir campos.

Pruebas obligatorias: API y MVC para CRUD Admin, Cliente403, borrado referenciado409, búsquedas vacías/sin resultados, producto404, errores400/401/403/409/500 y timeout. Navegador en 320/375/425/768/1024/1280 px y teclado.

Cambios de solidez: C03: BE 0.5→0.75, FE 0.25→0.75; C04: BE 0.5→0.75, FE 0.25→0.75; C05: BE 0.5→0.75, FE 0.25→0.75; C11: BE 0.5→0.75, FE 0.25→0.75. Sin otros cambios de puntuación.
**Proyección acumulada: Backend 75%; Frontend 43%.**

Gate de salida: F09 cerrado en catálogo; catálogo y Admin verificables, sin secretos en errores. C03/C04/C05/C11 a 0.75 exige todo el alcance de esta etapa; lo pendiente final queda registrado.

Fuera de alcance de esta wave: Promociones, recomendaciones y rediseño global; no usar porcentajes para ocultar filtros pendientes.

### Wave 5 — Conectar la compra de extremo a extremo

Prioridad: P1. Área: Ambas. Esfuerzo relativo: **L**, no estimación de días.

Archivos/módulos: Frontend/CarritoController, CheckoutController, PedidosController; Services/CarritosApiService y nuevos servicios reales de pedidos; Models/ViewModels/{Carrito,Checkout,Pedidos}; Views correspondientes y Productos/Detalle; API contratos wave 2.

Tareas ordenadas:

1. Crear o recuperar carrito propio y agregar/editar/eliminar productos desde catálogo. Reubicar con cuidado CategoriasApiService actualmente alojado en CarritosApiService.cs.
2. Mostrar resumen autoritativo, dirección elegida y confirmación; botón deshabilitado es sólo UX, la garantía de reenvío pertenece al Backend.
3. Mostrar número e importe históricos y texto Pedido pendiente de pago. Implementar historial y detalle propio sin enumerar IDs.
4. Manejar stock cambiado, sesión vencida, conflictos y reintentos sin duplicar pedido ni mostrar un cobro aprobado ficticio.

Dependencias y riesgos: Wave 4 y cierre SQL wave 2. Usa recuperación por carrito/clave y listado de líneas del pedido; no existe hoy un servicio PedidosApiService útil que pueda darse por hecho.

Pruebas obligatorias: E2E navegador→MVC→API→SQL aislado: usuario nuevo compra, modifica cantidades, vacía carrito, cambia stock desde otra sesión, reenvía confirmación, consulta historial; otro cliente no ve el pedido. Repetir después de reiniciar host.

Cambios de solidez: C06: BE 0.75→1, FE 0→1; C08: BE 1→1, FE 0→1; C09: BE 0.5→0.75, FE 0→0.75. Sin otros cambios de puntuación.
**Proyección acumulada: Backend 80%; Frontend 73%.**

Gate de salida: Recorrido cliente completo y reproducible con evidencia de persistencia. Proyección supera 70 en ambas capas, pero no se declara meta aceptada hasta cerrar Admin y regresión de wave 6.

Fuera de alcance de esta wave: Pasarela, falsa pantalla de pago aprobado y pruebas en Azure compartido.

### Wave 6 — Cerrar administración contacto y regresión

Prioridad: P1. Área: Ambas. Esfuerzo relativo: **M**, no estimación de días.

Archivos/módulos: Frontend/AdministracionController y Views/Administracion/{Pedidos,Usuarios,Consultas}; ConsultasController/Views; services reales; Pedidos/Pagos/Consultas/Usuarios Backend; Tests y ControlProyecto.

Tareas ordenadas:

1. Completar listado y transiciones de pedidos Admin, gestión básica de usuarios y contacto público con bandeja de consultas. No habilitar aprobación de pagos sin regla conciliada.
2. Cerrar las pruebas de autorización de todos los recorridos acreditados y la regresión de login/logout. Resolver o contener cada P0 confirmado antes de aceptar la demo.
3. Actualizar contrato API y evidencias de la matriz; revisar responsive/teclado, errores y recorridos de dos roles con un usuario evaluador.
4. Registrar límites aceptados: pago externo y recuperación pendientes, demo privada, secretos tratados con el propietario y pendientes fuera del 70.

Dependencias y riesgos: Wave 5. Políticas de estados aprobadas y aplicadas; F11 resuelto para cualquier exposición. No incorporar el backend antiguo de Dai.

Pruebas obligatorias: E2E Admin/Cliente/anónimo, contacto inválido/válido, permisos, transiciones prohibidas, cancelación una vez, regresión SQL concurrente, CI y UAT de los recorridos incluidos.

Cambios de solidez: C01: BE 1→1, FE 0.75→1; C12: BE 0.5→0.75, FE 0.25→0.75; C13: BE 0.5→0.75, FE 0→0.75. Sin otros cambios de puntuación.
**Proyección acumulada: Backend 82%; Frontend 81%.**

Gate de salida: Backend y Frontend ≥70 sin P0 en los recorridos acreditados, Admin de pedidos/usuarios utilizable y acta de límites. Proyección 82/81, no resultado actual.

Fuera de alcance de esta wave: Cobro externo, métricas avanzadas y funciones opcionales.


### Resumen de gates

| Etapa terminada | Backend sólido proyectado | Frontend sólido proyectado | Qué habilita |
|---|---|---|---|
| 1 | 43% | 12% | Consolidar la base y las pruebas |
| 2 | 58% | 12% | Proteger carrito y confirmación |
| 3 | 69% | 30% | Completar sesión cuenta y direcciones |
| 4 | 75% | 43% | Completar catálogo y administración de productos |
| 5 | 80% | 73% | Conectar la compra de extremo a extremo |
| 6 | 82% | 81% | Cerrar administración contacto y regresión |

Wave 5 supera el umbral aritmético (80/73), **pero el hito se acepta al finalizar wave 6** (82/81) por la exigencia de administración y regresión. Las capacidades C03/C04/C05/C07/C09/C10/C11/C12/C13 quedan como máximo en 0,75 y mantienen obligaciones de cierre. La meta exige además ausencia de P0 abiertos en recorridos acreditados, evidencia de dos roles, demo privada y límites aceptados. Ni promedio ni número de archivos sustituyen estos gates.

## 8. Del 70% a la evaluación

1. **Completar 80–90 con evidencia, no sólo puntuación.** La proyección wave 6 ya entra en esa banda, pero todavía falta ejecutar el trabajo. Luego completar C03/C04/C05/C07/C09/C10/C11/C12/C13 de 0,75 a 1 según sus requisitos: imágenes/filtros, direcciones e historial, recuperación real, Admin y atención al cliente. Mantener pruebas en cada entrega.
2. **Pago digital y notificaciones.** C14 de 0,25 a 1 requiere proveedor de prueba, validación de notificaciones entrantes, reconciliación de importe/moneda/pedido, reenvíos y estados cancelado/rechazado. Definir quién autoriza credenciales y entorno. Nunca simular aprobación para compensar integración pendiente.
3. **Compras y reportes.** C15 de 0,25 a 1 requiere detalle/reposición consistente; C16 de 0,25 a 1 requiere período, conciliación y pruebas SQL de agregación. Confirmar con docentes si compras detalladas forman parte del alcance de evaluación; cualquier exclusión se documenta sin recalcular retrospectivamente este denominador.
4. **Validación final y UAT.** Pruebas relacionales y E2E, regresión de errores y permisos, teclado/viewports, navegadores definidos, medición de carga bajo dataset/entorno acordados. El requisito de disponibilidad necesita observación operacional; no prometerlo con una demo.
5. **Preparar defensa.** Matriz requisito→evidencia→SHA, datos ficticios, guion Cliente/Admin, instrucciones de arranque seguras y contingencia de conectividad. Presentar límites y decisiones académicas, no funciones no implementadas.

Reseñas, mayoristas, chat en tiempo real y estadísticas avanzadas quedan opcionales conforme a exclusiones del PDF. MFA/captcha/refresh no son requisitos automáticos del 70%. No se autoriza desplegar, cobrar ni cambiar Azure mediante este roadmap.

### Definition of Done para entrega

- [ ] SHA y configuración de demostración identificados, sin secretos en documentos/artefactos.
- [ ] Backend y Frontend compilan; tests unitarios, HTTP, relacionales y E2E relevantes ejecutados con resultados conservados.
- [ ] Registro/login/logout, catálogo/búsqueda/detalle, carrito, dirección, pedido/historial y administración utilizables.
- [ ] Confirmación única, stock y total correctos; errores/reintentos no dejan datos parciales.
- [ ] Propietario y roles comprobados; respuestas sin material sensible; antiforgery y expiración revisados.
- [ ] Pago externo/recuperación/notificaciones implementados o excepción de alcance de evaluación aceptada explícitamente.
- [ ] UI móvil, teclado, estados vacíos/errores y navegadores acordados comprobados visualmente.
- [ ] Requisitos y contratos documentales coinciden con código; demo usa datos ficticios y no depende de alterar producción.
- [ ] UAT y guion de defensa completados; P0/P1 relevantes cerrados y riesgos residuales aceptados.

## 9. Validaciones ejecutadas y límites

Todos los comandos de solución se ejecutaron desde la raíz anidada, en **B 61ee39b**. No se atribuyen resultados a A.

| Comando o inspección | Resultado | Qué demuestra / qué no |
|---|---|---|
| git rev-parse, status, branch, diff, log, merge-base | Lectura correcta; comienzo limpio | Identidad y diferencias locales |
| git ls-remote --heads origin main Develop Rama--Facu Rama--Dai | Falló conexión a GitHub | No permite afirmar vigencia remota |
| dotnet --version | 10.0.400 | SDK disponible |
| dotnet build Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln --no-restore | Éxito; 0 warnings, 0 errores | Compila Backend, MVC y ambas suites B; no levanta host normal |
| dotnet test Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln --no-build --no-restore --logger "console;verbosity=minimal" | 8 unitarios + 15 integración superados; 0 fallidos, 0 omitidos | 23 casos descubiertos/ejecutados por runner; sólo fake/InMemory |
| Inventario de tests A mediante diff | 5 unitarios + 11 integración definidos en fuente | 16 casos por inspección; no descubrimiento runtime ni ejecución en A |
| PDFs con pypdf del runtime existente | Extracción disponible | PyMuPDF no instalado; texto API parcialmente revisado, no layout certificado |
| Análisis de vulnerabilidades actuales | No ejecutado contra fuente actual | Las versiones se inventariaron; no se certifica ausencia de vulnerabilidades |
| Navegador/E2E/SQL/Azure | No ejecutados | Protege base compartida; hueco de evidencia explícito |

No se añadieron tests para reproducir defectos porque el encargo prohíbe modificar tests. Los casos propuestos pertenecen a las waves. No se instalaron herramientas ni se restauraron paquetes para conseguir el build.

## 10. Registro documental de esta tarea

| Archivo | Cambio autorizado | Evidencia |
|---|---|---|
| ControlProyecto/ESTADO_ACTUAL_Y_ROADMAP_70.md | Nuevo informe, matriz y plan | Código y comandos descritos; no sustituye auditoría 60 |
| AGENTS.md | Corrige membresía de Frontend/tests en solución y descripción de suites | Solución contiene cuatro proyectos y build los compila |
| ControlProyecto/CleanCode.md | Cierra el fence de ejemplo truncado al final | Archivo terminaba dentro de bloque text |
| Backend.md, Auditor.md, Frontend.md, roadmap 60 | Sin modificación | No hay necesidad de reescritura amplia; Backend.md requiere una tarea específica |
| Guía personal fuera de Git | DOCX en la carpeta de artefactos autorizada | Mismos baselines, indicadores y seis waves |

Las credenciales académicas, appsettings, código, tests, dependencias, DbContext, migraciones y Git permanecen sin cambios. Revisión final de diff y formato: ver nota de cierre añadida tras generar artefactos.

Primera sesión recomendada: **wave 1**. No comenzar por implementar cinco pantallas en paralelo ni ejecutar checkout contra la base compartida.

## 11. Nota de cierre

La guía `Hoja_de_Ruta_Personal_TotalTech.docx` se guardó fuera de cualquier
repositorio Git en la carpeta de artefactos autorizada. Se reabrió con
`python-docx`: 130 párrafos, 10 tablas, una sección y siete saltos manuales,
que organizan ocho páginas previstas. La auditoría estructural de accesibilidad
de Documents informó cero hallazgos y las diez tablas tienen fila de encabezado.
El paquete OOXML es íntegro.

No fue posible renderizar el DOCX a imágenes porque el entorno Windows no tiene
`soffice.exe` disponible. Por tanto, la apertura y la estructura están
validadas, pero el ajuste visual final en Word permanece pendiente. Este límite
no altera las métricas técnicas del proyecto.

## 12. Ejecución de Wave 1 — consolidación local

Fecha de ejecución: 2026-09-11. Rama de trabajo: `Rama--Facu`.

### Baseline Git revalidada

`git fetch --prune origin` completó correctamente antes de modificar archivos.
El código funcional de la rama continúa identificado por `61ee39b`; el estado
documental desde el cual comenzó esta ejecución es `8dcb4b1`. Las referencias
remotas observadas fueron:

| Referencia | SHA revalidado |
|---|---|
| `origin/Rama--Facu` | `8dcb4b1fcfb10351859c4f0c1cf75feb9c3f8a44` |
| `origin/Develop` | `0b4eecff5b4d7476874f81480b754f74e433c01b` |
| `origin/main` | `f0a9e1ad7fc2f865da32ebd57b3e0fb3d0b6d446` |

`origin/Rama--Facu` ya era ancestro de `origin/Develop`; Develop tenía tres
commits adicionales. La simulación de merge de la baseline de la rama hacia
Develop no mostró marcadores de conflicto. `origin/main` y `origin/Develop`
estaban divergidos, por lo que la promoción final requiere PR y sus gates; no se
debe fusionar main directamente desde una referencia local desactualizada.

### Decisiones aprobadas para Wave 2

- La identidad idempotente será el carrito. Existirá un único pedido por carrito,
  respaldado por `Pedido.IdCarrito` obligatorio y único.
- La primera confirmación devolverá `201`; un reintento equivalente recuperará el
  mismo pedido con `200`; una dirección distinta sobre un carrito confirmado
  producirá `409`.
- El pedido almacenará `Total decimal(18,2)` y un snapshot completo de la
  dirección. El historial no dependerá de precios o direcciones mutables.
- Sólo un pedido pendiente podrá cancelarse directamente. La reposición de stock
  será transaccional e idempotente; pedidos pagados requerirán un flujo de
  reversión posterior.
- Un pago parcial no cambiará el pedido a Pagado. La suma aprobada debe coincidir
  exactamente con el total y se rechazará el sobrepago.
- No se agregará una clave de idempotencia enviada por el cliente en el alcance
  académico inicial. Estos acuerdos son diseño de Wave 2; Wave 1 no modifica DTO,
  endpoint, entidad ni esquema.

### Casos de reproducción preparados

| ID | Preparación | Acción | Resultado que debe exigir Wave 2 |
|---|---|---|---|
| F01 | Producto con precio servidor distinto del valor cliente | Agregar línea y confirmar | Precio y subtotal proceden únicamente del producto persistido |
| F02 | Dos contextos SQL sincronizados sobre el mismo carrito activo | Confirmar en paralelo | Un pedido, un descuento de stock y respuesta idempotente o conflicto seguro |
| F03 | Fallo transitorio y resultado de commit ambiguo | Reintentar la unidad completa | Estado releído por intento y recuperación del pedido sin duplicación |
| F04 | Carrito confirmado o cancelado con una línea existente | Mover, editar o borrar la línea | Operación rechazada sin alterar origen, destino ni stock |
| F05 | Pedido con total histórico y pago aprobado inferior o superior | Conciliar el pago | Parcial continúa pendiente; sobrepago se rechaza; sólo igualdad marca Pagado |
| F06 | Null, texto excedido, cantidad extrema y multiplicación monetaria | Enviar requests límite | Validación segura sin truncamiento, overflow ni escritura parcial |
| F07 | Entidades con navegaciones cargadas | Serializar respuestas de negocio | Contrato explícito sin ciclos ni datos internos no previstos |

### Cambios ejecutados

- El workflow WPF/.NET 8 fue sustituido por CI web .NET 10 con jobs separados
  para regresión rápida y SQL Server relacional, ejecutados en secuencia.
- `global.json` fija SDK 10.0.400 con avance al último patch compatible.
- Las pruebas relacionales crean `TotaltechTests_<GUID>` exclusivamente en
  `(localdb)\\MSSQLLocalDB`, aplican las migraciones existentes y eliminan la base
  al terminar. La validación rechaza cualquier otro servidor o nombre, incluidos
  Azure SQL y `TotatechDB`.
- La cadena Azure fue retirada de `appsettings.json`. Desarrollo usa LocalDB sin
  contraseña; otros entornos deben proporcionar la conexión mediante variables o
  User Secrets.
- El bootstrap Admin quedó deshabilitado por defecto y exige `Enabled`, `Email` y
  `Password` desde configuración externa cuando se habilita.
- `Credenciales.txt` dejó de estar versionado; se incorporó un ejemplo sin
  valores reales y se ignora el archivo local. Cualquier secreto que haya sido
  válido debe rotarse por el propietario antes de exponer el sistema.

### Evidencia local y estado del gate

| Gate | Resultado local |
|---|---|
| Restore de la solución | PASS; cuatro proyectos actualizados |
| Build Release secuencial | PASS; 0 errores y 0 advertencias |
| Regresión rápida | PASS; 8 unitarias + 15 HTTP, 23/23 |
| SQL Server relacional | PASS; 3/3: guardia, migraciones e índice único |
| Azure SQL | NO EJECUTADO; prohibido por la guardia de pruebas |
| CI alojado y promoción | PENDIENTE hasta push/PR y ejecución de GitHub Actions |

Wave 1 queda implementada y validada localmente. Su cierre integrado sólo puede
acreditarse cuando los dos jobs pasen en el PR `Rama--Facu` → `Develop`, se
registre el SHA resultante y luego se promueva `Develop` → `main` con los mismos
gates. F02–F07 permanecen abiertos para Wave 2; no se presentan como corregidos.
