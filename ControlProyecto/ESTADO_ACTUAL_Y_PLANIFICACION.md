# TotalTech - Estado actual y planificación hasta el 25 de octubre de 2026

Fecha de corte: **17 de septiembre de 2026**

Rama y revisión analizadas: `Rama--Facu` en `d6afef3`

Alcance de esta actualización: auditoría del código y la documentación, medición del estado y planificación. No se modificó código funcional, esquema ni datos.

## 1. Dictamen ejecutivo

El proyecto tiene una base Backend considerablemente más madura que su experiencia Frontend. La autenticación, el catálogo administrativo y el checkout Backend tienen implementación real; sin embargo, cuenta, direcciones, carrito, checkout, pedidos y consultas todavía carecen de recorridos MVC completos.

El catálogo documental contiene **46 casos de uso (UC)**. Al contrastarlo con el código actual:

- **8 UC tienen un núcleo construido y utilizable**, aunque conservan brechas respecto del alcance formal.
- **15 UC están desarrollados parcialmente**, casi siempre porque existe Backend pero falta Frontend o porque el flujo cubre sólo una parte del requisito.
- **23 UC no tienen evidencia de implementación**.
- **0 UC están acreditados como cerrados de forma sólida extremo a extremo** con el criterio de aceptación definido en este documento, porque todavía no hay una suite de navegador ni UAT trazada a un SHA.

La última medición formal posterior a Wave 2 fue **Backend sólido 58% / Frontend sólido 12%**. Al incorporar de forma conservadora el catálogo local y el modo Administrador desarrollados después, la estimación al 17/09 es **Backend 64% / Frontend 19%**. Esta estimación no es un gate aprobado: debe recalcularse y quedar trazada durante la primera etapa del plan.

### Meta obligatoria al 25/10/2026

- **Backend sólido >= 70%**.
- **Frontend sólido >= 70%**.
- **Mínimo comprometido: 14 UC de negocio aceptados**; supera el pedido de más de 10 y deja margen ante un rechazo de alcance.
- **Objetivo operativo: 18 UC de negocio aceptados**.
- UC-43 Autorización por rol es obligatorio como control transversal, pero **no se usa para inflar el conteo funcional**.
- Cero defectos P0/P1 abiertos en los recorridos acreditados.
- Build, pruebas unitarias, HTTP, SQL relacional y E2E verdes sobre el mismo SHA candidato.

No se utilizará el promedio entre Backend y Frontend: cada capa debe superar 70% por separado.

## 2. Fuentes y criterio de auditoría

Se revisaron:

- `Documentacion/Casos de Uso- Totaltech.pdf` completo: 46 UC en cinco páginas.
- `Documentacion/Documentación - Grupo 1 -TotalTech.pdf` completo: alcance y requerimientos funcionales/no funcionales.
- `Documentacion/Documentación de API.pdf` completo: 54 páginas; se lo considera referencia histórica, no contrato vigente.
- Código propio de `Totaltech/`, `Frontend/` y `Tests/`.
- Solución, proyectos, configuración, migración vigente, workflow y el historial consolidado en el roadmap anterior.

Cuando la documentación contradice el repositorio, el código probado en `d6afef3` prevalece para describir el estado actual. Las diferencias documentales se registran como trabajo pendiente; no se presentan como funcionalidad.

### Estados usados

| Estado | Significado |
|---|---|
| **Construido (C\*)** | El flujo principal tiene implementación utilizable, pero aún puede deber requisitos secundarios o evidencia E2E/UAT. |
| **Parcial (P)** | Hay un fragmento real —frecuentemente API, persistencia o una pantalla—, pero no un recorrido completo. |
| **No implementado (N)** | No existe comportamiento ejecutable suficiente; modelos, comentarios, mockups o archivos reservados no cuentan. |
| **Aceptado sólido** | Cumple íntegramente la Definition of Done de la sección 8. Es el único estado que contará para la meta de octubre. |

## 3. Inconsistencias documentales que deben resolverse

1. **Checkout directo versus carrito.** El catálogo de UC afirma que UC-10/11/12 reemplazan el carrito por checkout directo. El documento funcional, el PDF de API, los mockups y el código sí usan carrito. La decisión recomendada y presupuestada es conservar el carrito: UC-10 selecciona/agrega ítems y cantidades; UC-11 confirma el carrito y genera el pedido; UC-12 queda reservado al pago externo.
2. **Colisión de identificadores.** El caso detallado “Finalizar compra” usa `UC-002`, mientras el catálogo formal asigna UC-02 a “Iniciar sesión”. La trazabilidad nueva usará exclusivamente UC-01 a UC-46 del catálogo formal.
3. **API histórica desactualizada.** El PDF afirma que Auth no tiene JWT y documenta CRUD genérico de pedidos, detalles y pagos. El código actual sí usa JWT/cookie/Bearer y retiró mutaciones genéricas que violaban invariantes.
4. **Baja lógica y auditoría.** El catálogo declara baja lógica y auditoría para todos los ABM. Hoy existen `DELETE` físicos en varios módulos y no existe UC-32 Bitácora.
5. **Imágenes de producto.** Las 20 imágenes actuales se resuelven en Frontend por nombre; no existe todavía un contrato persistido y administrable de imagen.
6. **Versión canónica.** Los PDF locales y los enlaces de Drive no declaran cuál manda. Antes de cambiar contratos debe fijarse una fuente canónica versionada en el repositorio.
7. **Migraciones.** El árbol conserva únicamente `20260914231145_InicialActualizada`. Una base que tenga las migraciones históricas eliminadas puede intentar recrear tablas. Debe decidirse y probarse “base nueva” o una ruta incremental antes de promover.

## 4. Inventario completo de casos de uso

### Cliente: UC-01 a UC-18

| UC | Estado | Evidencia actual | Brecha para cierre |
|---|---|---|---|
| UC-01 Registrarse | C* | Formulario MVC, `POST /auth/registro`, rol Cliente forzado, hash y pruebas. | Validaciones límite, manejo homogéneo de errores, E2E y UAT. |
| UC-02 Iniciar sesión | C* | Login MVC/API, JWT, cookie protegida y propagación Bearer. | Vencimiento/errores E2E y prueba de navegador. |
| UC-03 Cerrar sesión | N | No existe acción ni enlace de logout. | POST con antiforgery, `SignOutAsync`, borrado de sesión y regresión. |
| UC-04 Recuperar contraseña | P | Endpoint de solicitud con respuesta genérica. | Token de un solo uso, expiración, cambio real, canal de entrega y UI. |
| UC-05 Gestionar perfil | P | API permite lectura/edición propia. | Servicio, controller y vista MVC; validaciones y E2E de ownership. |
| UC-06 Gestionar direcciones | P | CRUD Backend propio, ownership y pruebas HTTP; snapshot de envío en pedido. | Servicio, formularios y navegación MVC; casos vacíos/error y E2E. |
| UC-07 Explorar catálogo | Aceptado sólido | Recorrido navegador → MVC → API → SQL, paginación determinista, estado vacío, imágenes locales y E2E. | Productos destacados quedan diferidos por falta de modelo/regla. |
| UC-08 Filtrar y buscar productos | Aceptado sólido | Filtros combinables por texto, categoría, precio inclusivo y disponibilidad; estado preservado al paginar y pruebas HTTP/E2E. | Marca queda diferida por falta de modelo/regla. |
| UC-09 Ver detalle de producto | Aceptado sólido | Detalle con nombre de categoría, precio, stock, imagen/fallback, estado sin stock y 404 coherente, cubierto por E2E. | La compra pertenece al flujo de carrito/checkout, no a este UC. |
| UC-10 Seleccionar ítems a comprar | P | API de carrito agrega/elimina y protege owner/precio. | Servicio y UI de carrito, modificar cantidad, resumen y decisión documental. |
| UC-11 Confirmar compra | P | Backend idempotente, total/snapshot, stock, rollback y concurrencia probados. | Checkout MVC con dirección, feedback de conflicto y E2E a SQL. |
| UC-12 Pagar compra/MercadoPago | N | Sólo existe un modelo de pagos manuales administrativos. | Pasarela, retorno seguro, conciliación, idempotencia y sandbox externo. |
| UC-13 Ver historial de pedidos | P | Endpoints de pedidos propios y detalles. | Listado/servicio/vista MVC, paginación y E2E de aislamiento. |
| UC-14 Ver estado de pedido | P | Estados y transiciones Backend. | Estado visible al Cliente, detalle y actualización consistente en MVC. |
| UC-15 Descargar comprobantes | N | Sin generador PDF/email. | Definir comprobante, generación, autorización y almacenamiento/entrega. |
| UC-16 Dejar reseña | N | Fuera del alcance inicial del documento funcional. | Mantener excluido o diseñar módulo completo. |
| UC-17 Aplicar promoción | N | Sin promociones. | Entidad, reglas, vigencia, cálculo autoritativo y UI. |
| UC-18 Consultar soporte | P | `POST /consultas` público y gestión API administrativa. | Formulario MVC, confirmación, WhatsApp si aplica y bandeja Admin. |

### Administrador: UC-19 a UC-33

| UC | Estado | Evidencia actual | Brecha para cierre |
|---|---|---|---|
| UC-19 ABM de productos | C* | CRUD MVC/API Admin y stock/precio/categoría/proveedor. | Imagen administrable, baja lógica acordada, errores de API y E2E. |
| UC-20 ABM de categorías | C* | CRUD plano MVC/API Admin. | Acordar jerarquía/subcategoría o excluirla; baja lógica, errores y E2E. |
| UC-21 ABM de promociones | N | Sin implementación. | Módulo completo. |
| UC-22 Gestionar pedidos | P | Listado, detalle y transiciones válidas en Backend. | Pantallas Admin, datos de envío, feedback y E2E de transiciones. |
| UC-23 Emitir reembolso | N | Sin implementación. | Política, integración de pago, autorización e idempotencia. |
| UC-24 Gestionar usuarios | P | CRUD y autorización Backend. | Pantalla Admin, bloqueo/desbloqueo, reset administrativo y auditoría. |
| UC-25 Gestionar proveedores | C* | CRUD MVC/API Admin y condiciones comerciales. | Baja lógica coherente, errores/timeouts y E2E. |
| UC-26 Asociar productos a proveedor | P | Cada producto referencia un proveedor. | Múltiples/preferente/costo o exclusión formal del alcance ampliado. |
| UC-27 Ajuste de stock | P | Edición y endpoint Admin de stock. | Caso dedicado, motivo, historial de movimientos y pruebas de concurrencia. |
| UC-28 Importar/exportar catálogo | N | Sin implementación. | CSV/Excel, validación, informe de errores y autorización. |
| UC-29 Reportes operativos | P | Agregados Backend de ventas, ingresos y más vendidos. | Períodos, conciliación SQL, estados/promociones y UI Admin. |
| UC-30 Zonas y costos de envío | N | Sin implementación. | Reglas y UI de administración. |
| UC-31 Configurar medios de pago | N | Sin implementación. | Configuración segura por ambiente y sin exponer secretos. |
| UC-32 Auditoría del sistema | N | Sin bitácora funcional. | Eventos, actor, fecha, datos mínimos y consulta protegida. |
| UC-33 Gestionar reseñas | N | Reseñas no implementadas. | Depende de UC-16. |

### Proveedor: UC-34 a UC-36

| UC | Estado | Evidencia actual | Brecha para cierre |
|---|---|---|---|
| UC-34 Actualizar datos propios | N | Sólo Admin administra proveedores. | Rol/identidad Proveedor y portal propio. |
| UC-35 Consultar productos vinculados | N | No existe acceso del actor Proveedor. | Rol, endpoint propio y vista. |
| UC-36 Ver/actualizar condiciones | N | No existe acceso del actor Proveedor. | Rol, ownership y reglas de edición. |

### Integraciones y automatizaciones: UC-37 a UC-42

| UC | Estado | Evidencia actual | Brecha para cierre |
|---|---|---|---|
| UC-37 Webhook de pago | N | Sin callback externo. | Firma/autenticidad, idempotencia y conciliación. |
| UC-38 Notificaciones por email | N | Sin servicio de correo. | Proveedor, plantillas, reintentos y trazabilidad. |
| UC-39 Validación de promoción | N | Sin promociones. | Depende de UC-17/21. |
| UC-40 Cálculo de envío | N | Sin tarifario. | Depende de UC-30. |
| UC-41 Generación de comprobante | N | Sin PDF ni envío. | Depende de UC-15. |
| UC-42 Tareas programadas | N | Sin worker/scheduler. | Casos concretos, observabilidad y reintentos. |

### Seguridad y plataforma: UC-43 a UC-46

| UC | Estado | Evidencia actual | Brecha para cierre |
|---|---|---|---|
| UC-43 Autorización por rol | C* | Cliente/Admin protegidos en API y MVC; tests 401/403. | Matriz completa de recorridos; rol Proveedor sólo si entra en alcance. |
| UC-44 Sesiones y tokens | P | JWT con expiración, cookie MVC y handler Bearer. | Logout, renovación o decisión de no renovarla, expiración E2E. |
| UC-45 Backup/restore | N | Sin procedimiento probado. | Política, automatización y prueba de restauración. |
| UC-46 Monitoreo y alertas | N | Sin observabilidad acreditada. | Logs, métricas, salud y alertas mínimas. |

### Resumen cuantitativo del inventario

| Clasificación actual | Cantidad | UC |
|---|---:|---|
| Núcleo construido (C*) | 8 | 01, 02, 07, 09, 19, 20, 25, 43 |
| Parcial (P) | 15 | 04, 05, 06, 08, 10, 11, 13, 14, 18, 22, 24, 26, 27, 29, 44 |
| No implementado (N) | 23 | 03, 12, 15, 16, 17, 21, 23, 28, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 45, 46 |

## 5. Estado técnico comprobado al 17/09/2026

### Fortalezas

- Arquitectura activa: Razor/MVC -> servicios HTTP -> Minimal API -> lógica -> repositorios -> EF Core -> SQL Server.
- Registro y login con hashing, JWT y rol Cliente forzado.
- Políticas de Admin y ownership de recursos sensibles.
- Confirmación de carrito protegida: pedido único, stock condicionado, total y dirección históricos, rollback y recuperación idempotente.
- Transiciones acotadas de pedido/pago y reposición única al cancelar.
- Catálogo local idempotente con 20 productos y rutas de imagen comprobadas.
- CRUD MVC/API de productos, categorías y proveedores disponible para Admin.
- CI .NET 10 y pruebas separadas rápidas/relacionales.

### Cuellos de botella

- `CarritoController`, `CheckoutController`, `CuentaController`, `PedidosController` y `ConsultasController` son estructuras reservadas sin acciones.
- Sus ViewModels, servicios y vistas principales también son placeholders.
- `CarritosApiService.cs` contiene realmente `CategoriasApiService`; no existe un servicio MVC de carrito.
- `AdministracionController` sólo ofrece el índice; pedidos, usuarios y consultas no tienen recorrido Admin MVC.
- No existe suite E2E con navegador ni prueba automatizada de accesibilidad/responsive.
- No hay recuperación real, pasarela, correo, promociones, logística, auditoría, backup ni monitoreo.

### Gates ejecutados durante esta revisión

| Gate | Resultado |
|---|---|
| Restore de la solución | PASS; los cuatro proyectos estaban actualizados. |
| Build Release | PASS; 4 proyectos, 0 errores, 0 advertencias. |
| Unit tests | PASS; 8/8. |
| Integración rápida sin SQL Server | PASS; 27/27. |
| Integración SQL Server aislada | PASS; 13/13 en LocalDB fuera del sandbox. |
| Ejecución SQL dentro del sandbox | Falló por no poder crear la instancia automática de LocalDB; no fue un fallo del producto. |
| E2E de navegador | NO EXISTE/NO EJECUTADO. |
| UAT | NO EJECUTADO. |

## 6. Medición del 70% sólido

El porcentaje no se calculará por cantidad de archivos, endpoints ni UC nominales. Se mantiene la matriz ponderada de capacidades para no ocultar los flujos críticos.

| Capacidad | Peso |
|---|---:|
| C01 Registro/login, roles y errores | 12 |
| C02 Sesión MVC y logout | 4 |
| C03 Home y catálogo integrado | 10 |
| C04 Búsqueda y filtros | 6 |
| C05 Detalle e inicio de compra | 5 |
| C06 Carrito propio | 12 |
| C07 Direcciones y snapshot de envío | 7 |
| C08 Confirmación única, stock y total | 12 |
| C09 Historial y detalle de pedido | 8 |
| C10 Perfil y recuperación | 5 |
| C11 Administración de catálogo/categorías/proveedores | 6 |
| C12 Administración de pedidos/usuarios | 4 |
| C13 Contacto y consultas | 3 |
| C14 Pago digital | 3 |
| C15 Compra a proveedor | 1 |
| C16 Reportes conciliados | 2 |
| **Total** | **100** |

Escala por capa y capacidad: `0` ausente; `0,25` fragmento; `0,50` subflujo sustancial; `0,75` recorrido principal probado con pendientes acotados; `1` alcance cerrado con pruebas representativas. Fórmula: `Σ(peso × puntuación)`; los pesos suman 100.

La estimación actual usa estas puntuaciones conservadoras. BE/FE son solidez Backend/Frontend, no porcentaje de código escrito.

| Capacidad | Peso | BE 17/09 | FE 17/09 | Evidencia/límite principal |
|---|---:|---:|---:|---|
| C01 | 12 | 0,75 | 0,25 | Auth probado en API; MVC sin E2E. |
| C02 | 4 | 0,25 | 0,25 | Cookie/JWT presentes; logout ausente. |
| C03 | 10 | 0,75 | 0,50 | Catálogo demo real; faltan E2E/estados finales. |
| C04 | 6 | 0,50 | 0,50 | Texto/categoría/stock; faltan marca/precio. |
| C05 | 5 | 0,75 | 0,50 | Detalle funcional; falta inicio de compra y E2E. |
| C06 | 12 | 0,75 | 0 | Backend concurrente; Frontend placeholder. |
| C07 | 7 | 0,50 | 0 | API propia y snapshot; Frontend placeholder. |
| C08 | 12 | 1 | 0 | Confirmación Backend cubierta en SQL; sin checkout MVC. |
| C09 | 8 | 0,50 | 0 | API de historial/detalle; Frontend placeholder. |
| C10 | 5 | 0,25 | 0 | API de perfil parcial; recuperación/UI ausentes. |
| C11 | 6 | 0,75 | 0,50 | Tres CRUD utilizables; faltan errores/E2E y cierre de alcance. |
| C12 | 4 | 0,50 | 0,25 | API de pedidos/usuarios; panel MVC casi vacío. |
| C13 | 3 | 0,50 | 0 | API de consultas; Frontend placeholder. |
| C14 | 3 | 0,25 | 0 | Pago manual contenido, sin pasarela. |
| C15 | 1 | 0,25 | 0 | Sólo cabecera de compra a proveedor. |
| C16 | 2 | 0,25 | 0 | Agregados Backend sin período/UI/prueba SQL específica. |
| **Resultado** | **100** | **63,5 -> 64%** | **18,5 -> 19%** | Estimación a confirmar en el gate 20/09. |

### Línea de base y proyección condicionada

| Corte | Backend sólido | Frontend sólido | Condición |
|---|---:|---:|---|
| Medición formal posterior a Wave 2 | 58% | 12% | Evidencia histórica ya ejecutada. |
| Estimación conservadora 17/09 | 64% | 19% | Incluye catálogo/Admin posteriores; pendiente de recalcular y aceptar. |
| Meta 27/09 | 74% | 37% | Sesión, perfil/direcciones y validaciones cerrados. |
| Meta 04/10 | 75% | 43% | Catálogo y tres ABM endurecidos y probados. |
| Meta 11/10 | 80% | 73% | Carrito, checkout e historial E2E. Primer corte que supera 70 en ambas capas. |
| Meta 18/10 | 82% | 81% | Admin de pedidos/usuarios y consultas; regresión por roles. |

Las proyecciones no conceden avance por calendario. Sólo se actualizan si el gate de la etapa pasa sobre un SHA identificado.

## 7. Casos de uso comprometidos para octubre

### Compromiso mínimo: 14 UC de negocio

`UC-01`, `UC-02`, `UC-03`, `UC-06`, `UC-07`, `UC-08`, `UC-09`, `UC-10`, `UC-11`, `UC-13`, `UC-14`, `UC-19`, `UC-20` y `UC-25`.

Este conjunto entrega un recorrido demostrable de Cliente —identidad, catálogo, carrito, confirmación e historial— y tres ABM administrativos. No depende de fingir pagos, correos o promociones.

### Objetivo operativo: 18 UC de negocio

Al compromiso mínimo se agregan `UC-05`, `UC-18`, `UC-22` y `UC-24`.

`UC-43` se verifica de forma transversal para anónimo, Cliente y Admin, pero no se suma a las 14/18 funciones de negocio.

### Fuera del gate de 70%, salvo exigencia académica explícita

- UC-04 recuperación real por correo.
- UC-12/37 pago externo y webhook.
- UC-15/41 comprobantes.
- Promociones, reseñas, logística, portal Proveedor y funciones de plataforma.

Si MercadoPago es obligatorio para la evaluación de octubre, debe decidirse antes del 20/09. En ese caso, la etapa 12–18/10 se reasigna a UC-12/37 y UC-24 deja de ser compromiso, sin reducir las pruebas del checkout ni bajar la meta mínima de 14 UC.

## 8. Definition of Done de un caso de uso

Un UC cuenta como **aceptado sólido** sólo si cumple todo lo aplicable:

- [ ] El recorrido principal existe; no es placeholder ni únicamente un endpoint aislado.
- [ ] MVC, API y persistencia están conectados cuando corresponda.
- [ ] El servidor valida identidad, rol, ownership, importes, stock y estados sensibles.
- [ ] Formularios mutantes usan antiforgery; errores 400/401/403/404/409/timeout tienen experiencia recuperable.
- [ ] Tiene pruebas automatizadas de éxito y negativos relevantes.
- [ ] Si posee UI, pasa un E2E real navegador -> MVC -> API -> SQL aislado.
- [ ] Se verifican estados vacío, carga, error y responsive/teclado aplicables.
- [ ] Build y todas las suites pertinentes pasan en el mismo SHA.
- [ ] Contrato y documentación coinciden con el comportamiento actual.
- [ ] UAT fue aceptada y existe trazabilidad `UC -> código -> prueba -> evidencia -> SHA`.
- [ ] No queda ningún P0/P1 abierto dentro del recorrido.

## 9. Plan de acciones del 17/09 al 25/10/2026

### 17–20 de septiembre - Baseline, alcance y capacidad de entrega

**Objetivo:** eliminar ambigüedades antes de abrir más implementación.

Acciones:

1. Aprobar por escrito el modelo con carrito y el mapeo UC-10/11/12.
2. Definir la fuente documental canónica y crear la matriz `UC -> C01-C16 -> módulo -> prueba -> SHA`.
3. Decidir “base nueva” o migración incremental; ensayar la decisión sobre una copia desechable, nunca sobre Azure compartido.
4. Confirmar si MercadoPago, recuperación por email y notificaciones son obligatorios para el hito del 25/10.
5. Incorporar desde ahora un arnés E2E de navegador para los recorridos que se cierren; no postergarlo a la última semana.
6. Recalcular la línea de base C01-C16 y registrar el SHA.

Gate de salida:

- Alcance y exclusiones firmados.
- Estrategia de migración demostrada.
- Build 0/0, 8 unitarias, 27 rápidas y 13 SQL verdes.
- Baseline Backend/Frontend reproducible.

### 21–27 de septiembre - Identidad, sesión, perfil y direcciones

**Objetivo:** cerrar el prerrequisito de cualquier compra identificada.

Acciones:

1. Implementar logout mediante POST, antiforgery, `SignOutAsync` y navegación coherente.
2. Completar Cuenta MVC, edición del perfil y CRUD MVC de direcciones propias.
3. Corregir validaciones antes de `Trim`, límites persistidos y cambio de contraseña; preservar entrada ante error.
4. Traducir 401/403/timeout sin filtrar respuestas sensibles.
5. Probar dos usuarios, dirección ajena, expiración, CSRF, logout y snapshot histórico.

UC a aceptar: **01, 02, 03 y 06**. Objetivo adicional: **05**.

Gate: identidad y dirección funcionan E2E; después de logout no se envía cookie/token; meta orientativa **74% Backend / 37% Frontend**.

### 28 de septiembre–4 de octubre - Catálogo y ABM administrativos

**Objetivo:** convertir los flujos ya utilizables en casos sólidos.

Acciones:

1. Acordar filtros obligatorios; implementar precio/marca o documentar su exclusión aprobada.
2. Incorporar inicio de compra en el detalle y estados sin stock/sin imagen/sin resultados.
3. Definir imagen administrable o acotar formalmente el resolver local de la demo.
4. Endurecer Productos, Categorías y Proveedores: errores, timeout, referencias, baja lógica decidida y mensajes de validación.
5. Probar anónimo/Cliente/Admin, teclado y 320/375/425/768/1024/1280 px.

UC a aceptar: **07, 08, 09, 19, 20 y 25**.

Gate: acumulado mínimo de **10 UC núcleo aceptados**; meta orientativa **75% Backend / 43% Frontend**.

### 5–11 de octubre - Carrito, checkout e historial extremo a extremo

**Objetivo:** completar el recorrido comercial principal sin simular un pago aprobado.

Acciones:

1. Renombrar la deuda de `CarritosApiService.cs` y crear el servicio real de carrito.
2. Implementar agregar, modificar cantidad, eliminar, vaciar y resumen autoritativo.
3. Implementar checkout con dirección propia, confirmación y pantalla de resultado “Pedido pendiente de pago”.
4. Implementar historial y detalle propio con total, dirección histórica, líneas y estado.
5. Manejar stock cambiado, sesión vencida, conflicto y reenvío sin duplicar pedido.
6. Ejecutar E2E navegador -> MVC -> API -> SQL: compra, modificación, confirmación repetida, historial y aislamiento entre clientes.

UC a aceptar: **10, 11, 13 y 14**.

Gate: acumulado comprometido de **14 UC de negocio aceptados**; meta orientativa **80% Backend / 73% Frontend**.

### 12–18 de octubre - Atención y administración operativa

**Objetivo:** alcanzar el objetivo con margen y cerrar el segundo rol de demostración.

Acciones:

1. Implementar formulario de contacto y confirmación; bandeja Admin y transiciones de consulta.
2. Implementar listado/detalle/transiciones de pedidos Admin con cancelación única.
3. Implementar gestión básica de usuarios según alcance aprobado; no inventar reset por email si UC-04 queda diferido.
4. Completar perfil si no cerró en septiembre.
5. Ejecutar matriz E2E anónimo/Cliente/Admin y probar 401/403, transiciones prohibidas y errores recuperables.

UC objetivo: **05, 18, 22 y 24**.

Gate: objetivo de **18 UC de negocio aceptados** y UC-43 verificado; meta orientativa **82% Backend / 81% Frontend**.

### 19–22 de octubre - Congelamiento y regresión

**Objetivo:** consolidar, no sumar alcance.

Acciones:

1. Congelar funcionalidades el 19/10; sólo corregir defectos que afecten los UC comprometidos.
2. Repetir unitarias, HTTP, SQL, concurrencia, E2E, ownership, antiforgery y regresión de login/logout.
3. Verificar Chrome, Firefox y Edge; responsive, teclado, estados vacíos y errores.
4. Medir carga <=3 s con ambiente, dataset y método documentados; no extrapolar disponibilidad 99% desde una demo.
5. Auditar secretos, bootstrap, logs y datos ficticios.

Gate: >=70% Backend y >=70% Frontend sobre un mismo SHA, 14 UC como mínimo, cero P0/P1 y lista cerrada de riesgos residuales.

### 23–25 de octubre - UAT, documentación y versión candidata

**Objetivo:** entregar una versión demostrable y reproducible.

Acciones:

1. Ejecutar UAT con guiones Cliente y Admin y datos ficticios.
2. Corregir únicamente bloqueantes; repetir todos los gates afectados.
3. Actualizar API, alcance, matriz UC/evidencia y manual de arranque seguro.
4. Preparar guion de defensa, contingencia sin Internet y evidencia por SHA.
5. Marcar la versión candidata sólo si cumple Definition of Done; no declarar funciones diferidas como desarrolladas.

Salida final: versión candidata con **14–18 UC aceptados**, Backend/Frontend >=70%, documentación coherente y evidencia reproducible.

## 10. Dependencias, responsables y disciplina de ejecución

El equipo debe asignar nombres concretos en la reunión del 17/09. Hasta entonces se usan roles:

| Frente | Responsable de ejecución | Revisor obligatorio |
|---|---|---|
| Backend, reglas e integridad SQL | Backend | Auditor/otro integrante |
| MVC, UX y accesibilidad | Frontend | Otro integrante |
| E2E, matriz UC y evidencias | QA/Documentación | Backend + Frontend |
| Alcance, migración y excepciones | Responsable de proyecto | Equipo completo/docente cuando corresponda |

Reglas de trabajo:

- Límite WIP: máximo dos UC activos por persona.
- Cada PR debe declarar UC, capacidad C, pruebas y riesgo.
- Un UC no pasa a “aceptado” por compilación o demostración manual aislada.
- Ningún cambio de contrato comienza sin productor, consumidor, migración y pruebas acordados.
- No usar Azure compartido para pruebas destructivas o relacionales.

## 11. Riesgos y respuesta concreta

| Riesgo | Fecha límite | Respuesta |
|---|---|---|
| Migración consolidada incompatible | 20/09 | Elegir base nueva o ruta incremental y ensayarla en copia aislada. |
| Carrito versus checkout directo | 18/09 | Adoptar carrito y corregir trazabilidad, salvo decisión contraria explícita. |
| Pago externo obligatorio | 20/09 | Reservar 12–18/10 para UC-12/37 y reducir sólo objetivos de margen. |
| Frontend concentrado en placeholders | Diario | Implementar cortes verticales y cerrar E2E antes de abrir otro flujo. |
| Errores/timeouts en CRUD Admin | 04/10 | Política común de errores y pruebas negativas en los tres ABM. |
| LocalDB dependiente del entorno | 20/09 | Documentar prerrequisitos y validar el job SQL en CI/host autorizado. |
| Imágenes/acceso sin Internet | 04/10 | Mantener assets locales/fallback y probar demo offline. |
| Secretos o bootstrap inseguro | Cada PR | Configuración externa, guardias de ambiente y revisión automática/manual. |
| Expansión de alcance | 19/10 | Congelamiento funcional; nuevas ideas van al backlog posterior. |

## 12. Criterio de éxito del 25/10/2026

El hito se considera logrado sólo cuando exista evidencia de todos estos puntos:

- [ ] Al menos 14 UC de negocio cumplen la Definition of Done; objetivo 18.
- [ ] UC-43 está probado transversalmente y no se cuenta como función de negocio.
- [ ] Backend y Frontend alcanzan cada uno >=70% en la matriz C01-C16.
- [ ] Registro/login/logout, catálogo/búsqueda/detalle, carrito/dirección/confirmación e historial son utilizables.
- [ ] Productos, categorías, proveedores y al menos pedidos Admin son utilizables según el alcance aceptado.
- [ ] No hay P0/P1 abiertos en los recorridos acreditados.
- [ ] Build y suites unitarias, HTTP, SQL y E2E pasan en el mismo SHA.
- [ ] UAT Cliente/Admin, responsive, teclado y errores están documentados.
- [ ] Migración y arranque de demo son reproducibles sin secretos ni datos productivos.
- [ ] Documentación, API y matriz UC coinciden con el producto entregado.

## 13. Registro de avances ya ejecutados

- **Wave 1:** CI .NET 10, configuración segura, pruebas rápidas/SQL aisladas y tratamiento de credenciales.
- **Wave 2:** checkout Backend idempotente y transaccional, total/dirección históricos, stock, pagos manuales contenidos y DTO explícitos.
- **Modo Administrador:** autenticación y autorización confirmadas; CRUD MVC/API de productos, categorías y proveedores utilizable.
- **Catálogo demo:** 20 productos con imágenes locales, carga idempotente y consultas públicas de catálogo/detalle/búsqueda.

Este registro reemplaza las secciones históricas y contradictorias del roadmap anterior. A partir de esta fecha, toda actualización debe modificar la matriz de UC, la medición C01-C16 y la evidencia del SHA en este único archivo.

## Evidencia del hito público 20/09

Implementado sobre la línea base `d6afef3fceb9714832b36ed80f3ca2bf133a885b` (cambios aún no consolidados en un commit al momento de esta medición).

- Contrato agregado: `GET /productos/catalogo`, con texto, categoría, precios inclusivos, disponibilidad, página y tamaño de página; las rutas históricas permanecen operativas.
- La consulta usa una única composición `IQueryable`, orden por nombre/ID, total previo a `Skip/Take` y proyección explícita con categoría y proveedor.
- El Frontend usa el contrato paginado, conserva filtros, diferencia vacío/error, resuelve imágenes locales con fallback y presenta detalle/404 comprensibles.
- Resultado verificado: build Release, 0 errores y 0 advertencias; 8/8 unitarias; 36/36 integraciones rápidas; 13/13 SQL; 4/4 E2E (incluidas tres cargas de catálogo y detalle por debajo de 3 segundos, los seis anchos responsive y navegación por teclado). El job `catalog-e2e` quedó encadenado después de las suites rápida y SQL.
- C01-C16: este hito mejora principalmente C05 (UX pública), C06 (catálogo/productos), C11 (integración HTTP), C12 (persistencia consultada), C13 (manejo de errores), C14 (pruebas), C15 (CI) y C16 (evidencia). No acredita UC-19 ni casos administrativos.
- Alcance diferido explícito: marca y productos destacados. No existe actualmente modelo persistente ni regla de negocio aprobada; no se agregaron columnas ni migraciones.
