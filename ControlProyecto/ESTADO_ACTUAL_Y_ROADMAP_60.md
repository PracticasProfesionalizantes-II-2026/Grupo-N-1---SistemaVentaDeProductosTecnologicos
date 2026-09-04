# TotalTech — Estado Actual y Roadmap hacia ≥60%

> Auditoría maestra de solo lectura realizada el 2026-09-04 sobre la rama `Rama--Facu`, HEAD `4da26d2`.
> Fuente técnica primaria: código, migraciones y esquema/datos consultados en modo lectura.
> Fuente funcional: `Documentación - Grupo 1 -TotalTech.pdf`; la documentación de API y los mockups se usaron como referencias secundarias.

## 1. Resumen ejecutivo

**Avance general estimado: 40,6%** (`9,75 / 24` unidades funcionales de igual peso).
**Solidez Backend estimada: 56,8%** (`6,25 / 11` dimensiones técnicas de igual peso).
**Etapas necesarias hasta ≥60%: 5.**
**Estado global: ROJO — el proyecto compila y tiene una API amplia, pero el flujo principal de compra no es seguro ni utilizable de extremo a extremo.**

**Principal bloqueo:** el Backend acepta `PrecioUnitario` y `Pago.Monto` desde requests, no modela un `Pedido.Total` autoritativo y no controla concurrencia de stock. Al mismo tiempo, las pantallas MVC de carrito, checkout, cuenta y pedidos son placeholders y no existe integración real con Mercado Pago.

**Hallazgos positivos principales:**

- Hay separación consistente `Endpoint → Lógica → Repositorio → EF Core → SQL Server` para 13 entidades y 14 grupos de recursos.
- JWT, hashing, autorización por rol y ownership están implementados en código. Registro fuerza rol Cliente y las respuestas de usuario no incluyen la contraseña.
- La base configurada respondió a consultas `SELECT`: tiene las cinco migraciones aplicadas, los índices únicos esperados, las seis categorías canónicas y un Admin canónico persistido con rol Administrador y hash de 84 caracteres.
- Backend y Frontend compilan por separado en .NET 10 con 0 warnings y 0 errores.

**Riesgos inmediatos:**

- `Totaltech/appsettings.json` contiene una cadena SQL con usuario y contraseña y está versionado; `Credenciales.txt` también está versionado y no está vacío. No se reproducen valores en este reporte. Deben rotarse y retirarse del historial operativo antes de continuar.
- No existe ningún proyecto de tests; `dotnet test` termina correctamente porque la solución no contiene ensamblados de prueba, no porque haya pruebas exitosas.
- La documentación de API está desactualizada: afirma que no existe JWT, mientras que el código actual sí lo implementa.

**Próxima etapa:** **Etapa 1 — Configuración segura y red mínima de pruebas**. Primero debe eliminarse la exposición de credenciales y fijarse una base automatizada para validar login, registro, roles y ownership antes de cambiar el contrato económico.

## 2. Snapshot de auditoría

| Dato | Resultado |
|---|---|
| Fecha | 2026-09-04 (`America/Buenos_Aires`) |
| Repositorio real | `Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos` |
| Rama | `Rama--Facu` |
| HEAD | `4da26d2 Backend` |
| Estado inicial | Limpio; `git status --short` no devolvió cambios |
| Último merge visible | `fdb7548 Merge branch 'Develop' into Rama--Facu` |
| Proyectos .NET | `Totaltech/Totaltech.csproj`, `Frontend/Frontend.csproj` |
| Solución | `Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln`; incluye solo `Totaltech/Totaltech.csproj` |
| Tests | Directorios y `.gitkeep`; 0 `.cs`, 0 `.csproj`, 0 tests ejecutables |
| Documentos locales | 2 PDF, README, 4 guías de `ControlProyecto`, mockups PNG en `Frontend/photos` |
| Documentos externos referenciados | Google Docs, Canva y Lucidchart desde `README.md`; **NO VERIFICADOS** en esta auditoría |
| Base configurada | SQL Server accesible mediante consulta de solo lectura; no se ejecutaron escrituras |
| API levantada | No. Evitado deliberadamente porque `Program.cs` ejecuta bootstrap de categorías y Admin al arrancar |

Se respetó el repositorio Git anidado indicado por `AGENTS.md`. El repositorio contenedor no fue modificado. Antes de crear este informe no existía `ControlProyecto/ESTADO_ACTUAL_Y_ROADMAP_60.md`.

## 3. Stack y arquitectura real

### Stack confirmado

| Componente | Evidencia | Estado |
|---|---|---|
| Runtime | Ambos `.csproj` usan `net10.0` | Confirmado |
| C# | No hay `LangVersion`; se usa la versión predeterminada del SDK .NET 10 instalado (`10.0.400`) | Confirmado hasta ese nivel; versión de lenguaje exacta no fijada |
| Backend | ASP.NET Core Minimal APIs | Confirmado en `Totaltech/Program.cs` y `Totaltech/Endpoints/**` |
| Persistencia | EF Core 10.0.7 + proveedor SQL Server 10.0.7 | Confirmado en `Totaltech/Totaltech.csproj` |
| Auth Backend | `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.7 | Confirmado |
| Documentación API en runtime | OpenAPI 10.0.7 + Scalar 2.14.11, solo en Development | Confirmado |
| Frontend | ASP.NET Core MVC con cookie local y `HttpClient` hacia la API | Confirmado |
| CSS/UI | Bootstrap vendorizado + `Frontend/wwwroot/css/site.css` | Confirmado |
| Base | SQL Server; conexión configurada y consulta `SELECT` exitosa | Confirmado |

### Arquitectura Backend real

```text
Request HTTP
    ↓
Minimal API Endpoint (mapeo, auth/ownership, status HTTP)
    ↓
Logica (reglas y validación de negocio)
    ↓
Repositorio (consultas EF y SaveChangesAsync)
    ↓
TotaltechDbContext
    ↓
SQL Server
```

`CarritosLogica` es la excepción relevante: además de repositorios, recibe directamente `TotaltechDbContext` para abrir la transacción de confirmación del carrito. No es un defecto por sí mismo, pero concentra la orquestación crítica en esa clase.

### Arquitectura Frontend real

```text
Razor View
    ↓
MVC Controller
    ↓
ApiService / IHttpClientFactory
    ↓  Bearer recuperado de la cookie de autenticación
Backend HTTP API
```

Login y registro llaman a la API directamente desde `HomeController`. Productos, categorías y proveedores usan servicios dedicados. Los demás servicios/controladores previstos están vacíos o reservados.

### Dependencias y límites

- La solución no incluye el proyecto `Frontend`; CI/build de la solución por sí solo no valida la UI.
- Backend y Frontend no tienen referencia de proyecto directa: se integran únicamente por HTTP y contratos JSON duplicados.
- No hay proyecto compartido de contratos ni generación de cliente OpenAPI.
- No se detectó infraestructura de CI para build/tests funcionales; `.github` no aporta evidencia de una pipeline ejecutada en esta auditoría.

## 4. Estado general del proyecto

Los porcentajes de esta tabla son indicadores auxiliares por área; **no** se usan para calcular el 40,6% general.

| Área | Estado | Avance estimado | Evidencia | Principal pendiente |
|---|---|---:|---|---|
| Backend | PARCIAL ALTO | 70% | 87 rutas, capas completas, build limpio | Economía autoritativa, checkout concurrente, recuperación real |
| Frontend | PARCIAL | 32% | Login, registro, catálogo y ABM parcial | Carrito, checkout, cuenta, pedidos, contacto y Admin completo |
| Base de datos | PARCIAL ALTO | 80% | 13 tablas, 5 migraciones aplicadas, FK/índices | `Pedido.Total`, imagen de producto, tokens de concurrencia/idempotencia |
| Seguridad | PARCIAL | 55% | JWT, hash, rol, ownership | Secretos versionados, cero tests de seguridad, sin revocación/refresh |
| Flujo de compra | INICIAL | 25% | `POST /carritos/{id}/confirmar` crea pedido/detalles y descuenta stock | Precio/total seguro, concurrencia, pago real y UI |
| Tests | NO IMPLEMENTADO | 0% | 0 proyectos y 0 fuentes de tests | Unitarios, integración, API, seguridad y Frontend |
| Documentación | PARCIAL | 60% | Requisitos y mockups locales utilizables | API PDF desactualizado; referencias externas no versionadas |

Lectura global: la amplitud del Backend es mayor que la madurez del producto. Hay CRUDs persistentes y seguridad estructural, pero faltan las cadenas funcionales que convierten esos recursos en una compra real. El Frontend cubre el acceso y parte del catálogo, no el caso de uso central.

## 5. Avance general estimado

### Metodología

- Fuente de unidades: alcance y RF del PDF `Documentación - Grupo 1 -TotalTech.pdf`, descompuestos en resultados funcionales comprobables.
- Pesos: 24 unidades, todas con el mismo peso porque la documentación no asigna prioridades cuantitativas.
- Escala: `COMPLETO = 1,00`; `PARCIAL ALTO = 0,75`; `PARCIAL = 0,50`; `INICIAL = 0,25`; `NO IMPLEMENTADO = 0,00`.
- Un build correcto prueba compilabilidad, no comportamiento. En ausencia total de tests y de validación HTTP actual, ninguna unidad recibió `1,00`.
- Los recursos auxiliares Proveedores, Compras y Reportes se auditan, pero no se agregan como unidades independientes porque el RF principal solo exige administración básica de productos, pedidos y usuarios. Esto evita inflar el porcentaje por cantidad de CRUDs.

| # | Unidad funcional evaluable | Score | Clasificación | Evidencia principal |
|---:|---|---:|---|---|
| 1 | Publicar y administrar datos de producto | 0,75 | PARCIAL ALTO | API + MVC CRUD; sin test/runtime HTTP |
| 2 | Asociar y mostrar imágenes de producto | 0,25 | INICIAL | Hay imágenes estáticas, pero `Producto` no tiene imagen ni flujo de carga/asociación |
| 3 | Gestionar y descontar stock | 0,75 | PARCIAL ALTO | CRUD/patch Admin y descuento al confirmar; sin control concurrente |
| 4 | Navegar catálogo por categoría | 0,75 | PARCIAL ALTO | Endpoints, servicio, controlador y vista |
| 5 | Filtrar por tipo, precio y disponibilidad | 0,50 | PARCIAL | Categoría y disponibilidad sí; precio y tipo separado no |
| 6 | Buscar por palabra clave | 0,75 | PARCIAL ALTO | `/productos/buscar` + UI; sin test |
| 7 | Ver detalle de producto | 0,50 | PARCIAL | Cadena API/MVC existe; vista mínima, sin imagen ni compra |
| 8 | Agregar, modificar y quitar productos del carrito | 0,25 | INICIAL | Backend existe; Frontend placeholder y precio manipulable |
| 9 | Calcular resumen/subtotal/total del carrito | 0,25 | INICIAL | Subtotal se calcula con precio recibido; no hay total autoritativo |
| 10 | Finalizar compra de extremo a extremo | 0,25 | INICIAL | Confirmación Backend parcial; no UI, pago ni total seguro |
| 11 | Procesar pago digital/Mercado Pago | 0,00 | NO IMPLEMENTADO | Solo entidad/CRUD `Pago`; sin SDK, preferencia, webhook ni credenciales de proveedor |
| 12 | Confirmar pago y entregar número de pedido | 0,25 | INICIAL | Se crea ID de pedido y Admin puede registrar pago; no flujo cliente/gateway |
| 13 | Registrar usuario | 0,75 | PARCIAL ALTO | API + MVC + hash + rol forzado; sin test actual |
| 14 | Iniciar sesión y conservar identidad | 0,75 | PARCIAL ALTO | JWT Backend + cookie Frontend + Bearer handler; sin test actual |
| 15 | Recuperar contraseña | 0,25 | INICIAL | Endpoint devuelve mensaje neutro, pero solo consulta existencia y no genera recuperación |
| 16 | Gestionar perfil y direcciones | 0,25 | INICIAL | Backend con ownership; `CuentaController`/vista y servicio de direcciones pendientes |
| 17 | Ver historial y estado de pedidos | 0,25 | INICIAL | Endpoints con ownership; UI de pedidos pendiente |
| 18 | Administración de productos | 0,75 | PARCIAL ALTO | API Admin + pantallas MVC; sin test |
| 19 | Administración de pedidos/estados | 0,50 | PARCIAL | API Admin existe; panel y vistas son placeholders |
| 20 | Administración de usuarios | 0,25 | INICIAL | API Admin existe; panel/servicio MVC pendientes |
| 21 | Notificar cambios de pedido por email/plataforma | 0,00 | NO IMPLEMENTADO | No hay servicio, outbox ni UI de notificaciones |
| 22 | Enviar consulta desde formulario web | 0,25 | INICIAL | API permite alta anónima; Frontend contacto pendiente |
| 23 | Acceso directo por WhatsApp | 0,00 | NO IMPLEMENTADO | No se encontró enlace funcional en Layout/Home |
| 24 | Uso responsive en PC y móvil | 0,50 | PARCIAL | Meta viewport y breakpoints; flujos centrales no implementados ni probados visualmente en runtime |

```text
24 unidades evaluables
9,75 puntos obtenidos

9,75 / 24 × 100 = 40,625%
Avance general estimado: 40,6%
```

La cifra es deliberadamente conservadora: reconoce código funcional sin otorgar completitud a cadenas sin tests ni interfaz utilizable.

## 6. Solidez actual del Backend

Se aplicó la misma escala y pesos iguales a las 11 dimensiones exigidas. Esta métrica no mide avance visual ni cantidad de endpoints.

| Dimensión | Score | Clasificación | Fundamento |
|---|---:|---|---|
| Arquitectura | 0,75 | PARCIAL ALTO | Capas coherentes; orquestación crítica acoplada a varios repositorios/DbContext y contratos duplicados |
| Persistencia | 1,00 | COMPLETO al alcance actual | Esquema real accesible, 5 migraciones aplicadas, FK/índices y datos persistidos |
| Validación | 0,50 | PARCIAL | Reglas manuales útiles, pero DTOs Backend casi sin DataAnnotations y validación no uniforme |
| Auth | 0,75 | PARCIAL ALTO | JWT, hash, bootstrap y cookie/Bearer existen; sin test HTTP actual ni recuperación real |
| Autorización | 0,75 | PARCIAL ALTO | Fallback autenticado y política Admin extendida; sin suite que evite regresiones |
| Ownership | 0,75 | PARCIAL ALTO | Controles sistemáticos sobre recursos personales; sin pruebas A/B automatizadas |
| Integridad económica | 0,25 | INICIAL | Precio de línea y monto de pago son confiados al request; no existe `Pedido.Total` |
| Stock | 0,50 | PARCIAL | Valida y descuenta en servidor, pero sin token/UPDATE condicional contra overselling |
| Transacciones | 0,50 | PARCIAL | Confirmación usa transacción explícita, pero faltan idempotencia y control concurrente |
| Errores | 0,50 | PARCIAL | 400/404/409 razonables en varios casos; sin middleware global/ProblemDetails y contratos dispares |
| Tests | 0,00 | NO IMPLEMENTADO | 0 proyectos de test |

```text
11 dimensiones
6,25 puntos obtenidos

6,25 / 11 × 100 = 56,818%
Solidez Backend estimada: 56,8%
```

Aunque el promedio se acerca a 60%, el Backend **no debe considerarse sólido todavía**: integridad económica, stock concurrente y tests son criterios de cierre, no dimensiones compensables por tener muchos CRUDs.

## 7. Inventario de Backend

Leyenda de estado: `FUNCIONAL SIN TESTS` significa que existe la cadena Endpoint/Lógica/Repositorio/Persistencia y pasó build, pero no fue verificada con test automatizado ni HTTP en esta auditoría.

| Recurso | Endpoint(s) | Lógica | Repositorio | Persistencia | Validaciones | Seguridad | Tests | Estado |
|---|---|---|---|---|---|---|---|---|
| Auth | 3 | `UsuariosLogica`, `JwtTokenService` | `UsuariosRepositorio` | `Usuarios` | Login/registro útiles; recuperación simulada | Público por diseño; emite JWT | 0 | PARCIAL |
| Usuarios | 5 | Sí | Sí | `Usuarios` | Campos básicos, email duplicado, hash | Admin para lista/alta; owner/Admin para unidad | 0 | FUNCIONAL SIN TESTS |
| Direcciones | 5 | Sí | Sí | `Direcciones` | Campos, enum, usuario existente | Owner/Admin y listado filtrado | 0 | FUNCIONAL SIN TESTS |
| Proveedores | 5 | Sí | Sí | `Proveedores` | Razón, CUIT, contacto, plazos, FK | Admin en todo el grupo | 0 | FUNCIONAL SIN TESTS |
| Productos | 9 | Sí | Sí | `Productos` | Nombre, precio/stock no negativos, FK | Lectura pública; mutación Admin | 0 | FUNCIONAL SIN TESTS |
| Categorías | 5 | Sí + bootstrap | Sí | `Categorias` | Nombre obligatorio | Lectura pública; mutación Admin | 0 | FUNCIONAL SIN TESTS |
| Pedidos | 10 | Sí | Sí | `Pedidos` | Usuario/dirección/estado | Lectura owner/Admin; mutación sensible Admin | 0 | PARCIAL |
| DetallePedidos | 5 | Sí | Sí | `DetallePedidos` | Cantidad/precio/FK; subtotal calculado | Lectura owner/Admin; mutación Admin | 0 | PARCIAL |
| Carritos | 9 | Sí + orquestación | Sí | `Carritos` | Usuario, estado, stock y dirección | Owner/Admin y listados filtrados | 0 | PARCIAL |
| DetalleCarritos | 6 | Sí | Sí | `DetalleCarritos` | Cantidad/stock/FK/único | Owner/Admin | 0 | PARCIAL |
| Pagos | 6 + 2 anidadas en pedidos | Sí | Sí | `Pagos` | Monto positivo, enums, pedido existente | Lectura owner/Admin; mutación Admin | 0 | PARCIAL |
| Compras | 5 | Sí | Sí | `Compras` | Total no negativo, estado, proveedor | Admin en todo el grupo | 0 | FUNCIONAL SIN TESTS |
| Reportes | 8 | Sí | Sí | `Reportes` + agregados | Tipo, fechas, usuario | Admin en todo el grupo | 0 | PARCIAL |
| Consultas | 6 | Sí | Sí | `Consultas` | Email/mensaje/estado/usuario | Alta pública; owner para propias; gestión Admin | 0 | FUNCIONAL SIN TESTS |

Observaciones:

- Existen 13 entidades persistentes y 14 recursos HTTP contando Auth.
- Los agregados de Reportes funcionan sobre `DetallePedido.Subtotal` y `Pago.Monto`; por ello su cálculo técnico existe, pero hereda la falta de confiabilidad económica de esos valores.
- Los repositorios hacen `SaveChangesAsync` por operación. Es suficiente para CRUD individual; en confirmación de carrito quedan contenidos por una transacción externa, pero aumenta round-trips.
- Las consultas de lectura no usan `AsNoTracking`; es deuda de rendimiento menor, no bloqueante para 60%.

## 8. Inventario de endpoints y seguridad

**Total real: 87 rutas.** `HTTP NV` significa “implementado en código, build correcto, comportamiento HTTP no verificado en esta auditoría”.

| Método | Ruta | Recurso | Auth | Rol | Ownership | Estado |
|---|---|---|---|---|---|---|
| POST | `/auth/login` | Auth | Pública | — | — | HTTP NV |
| POST | `/auth/registro` | Auth | Pública | — | Rol Cliente forzado | HTTP NV |
| POST | `/auth/recuperar-contrasena` | Auth | Pública | — | Respuesta no enumera email | PARCIAL; no recupera |
| GET | `/usuarios/` | Usuarios | JWT | Admin | No aplica | HTTP NV |
| GET | `/usuarios/{id}` | Usuarios | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| POST | `/usuarios/` | Usuarios | JWT | Admin | No aplica | HTTP NV |
| PUT | `/usuarios/{id}` | Usuarios | JWT | Cliente/Admin | Owner/Admin; Cliente no cambia rol | HTTP NV |
| DELETE | `/usuarios/{id}` | Usuarios | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| GET | `/direcciones/` | Direcciones | JWT | Cliente/Admin | Cliente recibe propias | HTTP NV |
| GET | `/direcciones/{id}` | Direcciones | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| POST | `/direcciones/` | Direcciones | JWT | Cliente/Admin | Cliente queda como owner | HTTP NV |
| PUT | `/direcciones/{id}` | Direcciones | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| DELETE | `/direcciones/{id}` | Direcciones | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| GET | `/proveedores/` | Proveedores | JWT | Admin | No aplica | HTTP NV |
| GET | `/proveedores/{id}` | Proveedores | JWT | Admin | No aplica | HTTP NV |
| POST | `/proveedores/` | Proveedores | JWT | Admin | No aplica | HTTP NV |
| PUT | `/proveedores/{id}` | Proveedores | JWT | Admin | No aplica | HTTP NV |
| DELETE | `/proveedores/{id}` | Proveedores | JWT | Admin | No aplica | HTTP NV |
| GET | `/productos/` | Productos | Pública | — | — | HTTP NV |
| GET | `/productos/{id}` | Productos | Pública | — | — | HTTP NV |
| POST | `/productos/` | Productos | JWT | Admin | No aplica | HTTP NV |
| PUT | `/productos/{id}` | Productos | JWT | Admin | No aplica | HTTP NV |
| DELETE | `/productos/{id}` | Productos | JWT | Admin | No aplica | HTTP NV |
| GET | `/productos/buscar` | Productos | Pública | — | — | HTTP NV |
| GET | `/productos/categoria/{idCategoria}` | Productos | Pública | — | — | HTTP NV |
| GET | `/productos/disponibles` | Productos | Pública | — | — | HTTP NV |
| PATCH | `/productos/{id}/stock` | Productos | JWT | Admin | No aplica | HTTP NV |
| GET | `/categorias/` | Categorías | Pública | — | — | HTTP NV |
| GET | `/categorias/{id}` | Categorías | Pública | — | — | HTTP NV |
| POST | `/categorias/` | Categorías | JWT | Admin | No aplica | HTTP NV |
| PUT | `/categorias/{id}` | Categorías | JWT | Admin | No aplica | HTTP NV |
| DELETE | `/categorias/{id}` | Categorías | JWT | Admin | No aplica | HTTP NV |
| GET | `/pedidos/` | Pedidos | JWT | Cliente/Admin | Cliente recibe propios | HTTP NV |
| GET | `/pedidos/{id}` | Pedidos | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| POST | `/pedidos/` | Pedidos | JWT | Cliente/Admin | Cliente queda como owner | PARCIAL; permite pedido vacío |
| PUT | `/pedidos/{id}` | Pedidos | JWT | Admin | No aplica | HTTP NV |
| DELETE | `/pedidos/{id}` | Pedidos | JWT | Admin | No aplica | HTTP NV |
| GET | `/pedidos/usuario/{idUsuario}` | Pedidos | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| GET | `/pedidos/estado/{estado}` | Pedidos | JWT | Admin | No aplica | HTTP NV |
| PATCH | `/pedidos/{id}/estado` | Pedidos | JWT | Admin | No aplica | HTTP NV |
| POST | `/pedidos/{idPedido}/pagos` | Pagos | JWT | Admin | No aplica | PARCIAL; monto manual |
| GET | `/pedidos/{idPedido}/pagos` | Pagos | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| GET | `/detallepedidos/` | DetallePedidos | JWT | Admin | No aplica | HTTP NV |
| GET | `/detallepedidos/{id}` | DetallePedidos | JWT | Cliente/Admin | Owner del pedido/Admin | HTTP NV |
| POST | `/detallepedidos/` | DetallePedidos | JWT | Admin | No aplica | PARCIAL; precio manual Admin |
| PUT | `/detallepedidos/{id}` | DetallePedidos | JWT | Admin | No aplica | PARCIAL; precio manual Admin |
| DELETE | `/detallepedidos/{id}` | DetallePedidos | JWT | Admin | No aplica | HTTP NV |
| GET | `/carritos/` | Carritos | JWT | Cliente/Admin | Cliente recibe propios | HTTP NV |
| GET | `/carritos/{id}` | Carritos | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| POST | `/carritos/` | Carritos | JWT | Cliente/Admin | Cliente queda como owner | PARCIAL; Cliente puede enviar estado |
| PUT | `/carritos/{id}` | Carritos | JWT | Cliente/Admin | Owner/Admin | PARCIAL; Cliente puede cambiar estado |
| DELETE | `/carritos/{id}` | Carritos | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| GET | `/carritos/usuario/{idUsuario}` | Carritos | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| POST | `/carritos/{idCarrito}/productos` | Carritos | JWT | Cliente/Admin | Owner/Admin | PARCIAL; precio manipulable |
| DELETE | `/carritos/{idCarrito}/productos/{idProducto}` | Carritos | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| POST | `/carritos/{idCarrito}/confirmar` | Carritos | JWT | Cliente/Admin | Owner/Admin | PARCIAL; sin total/concurrencia/pago |
| GET | `/detallecarritos/` | DetalleCarritos | JWT | Cliente/Admin | Cliente recibe propios | HTTP NV |
| GET | `/detallecarritos/{id}` | DetalleCarritos | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| GET | `/detallecarritos/carrito/{idCarrito}` | DetalleCarritos | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| POST | `/detallecarritos/` | DetalleCarritos | JWT | Cliente/Admin | Owner/Admin | PARCIAL; precio manipulable |
| PUT | `/detallecarritos/{id}` | DetalleCarritos | JWT | Cliente/Admin | Owner de origen y destino/Admin | PARCIAL; precio manipulable |
| DELETE | `/detallecarritos/{id}` | DetalleCarritos | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| GET | `/pagos/` | Pagos | JWT | Admin | No aplica | HTTP NV |
| GET | `/pagos/{id}` | Pagos | JWT | Cliente/Admin | Owner del pedido/Admin | HTTP NV |
| POST | `/pagos/` | Pagos | JWT | Admin | No aplica | PARCIAL; monto/estado manual |
| PUT | `/pagos/{id}` | Pagos | JWT | Admin | No aplica | PARCIAL; monto/estado manual |
| DELETE | `/pagos/{id}` | Pagos | JWT | Admin | No aplica | HTTP NV |
| PATCH | `/pagos/{id}/estado` | Pagos | JWT | Admin | No aplica | HTTP NV |
| GET | `/compras/` | Compras | JWT | Admin | No aplica | HTTP NV |
| GET | `/compras/{id}` | Compras | JWT | Admin | No aplica | HTTP NV |
| POST | `/compras/` | Compras | JWT | Admin | No aplica | HTTP NV |
| PUT | `/compras/{id}` | Compras | JWT | Admin | No aplica | HTTP NV |
| DELETE | `/compras/{id}` | Compras | JWT | Admin | No aplica | HTTP NV |
| GET | `/reportes/` | Reportes | JWT | Admin | No aplica | HTTP NV |
| GET | `/reportes/{id}` | Reportes | JWT | Admin | No aplica | HTTP NV |
| POST | `/reportes/` | Reportes | JWT | Admin | No aplica | HTTP NV |
| PUT | `/reportes/{id}` | Reportes | JWT | Admin | No aplica | HTTP NV |
| DELETE | `/reportes/{id}` | Reportes | JWT | Admin | No aplica | HTTP NV |
| GET | `/reportes/ventas` | Reportes | JWT | Admin | No aplica | PARCIAL; datos económicos no confiables |
| GET | `/reportes/ingresos` | Reportes | JWT | Admin | No aplica | PARCIAL; datos económicos no confiables |
| GET | `/reportes/productos-mas-vendidos` | Reportes | JWT | Admin | No aplica | PARCIAL; datos económicos no confiables |
| GET | `/consultas/` | Consultas | JWT | Admin | No aplica | HTTP NV |
| GET | `/consultas/{id}` | Consultas | JWT | Admin | No aplica | HTTP NV |
| GET | `/consultas/usuario/{idUsuario}` | Consultas | JWT | Cliente/Admin | Owner/Admin | HTTP NV |
| POST | `/consultas/` | Consultas | Pública | — | Usuario autenticado queda como owner; anónimo sin owner | HTTP NV |
| PUT | `/consultas/{id}` | Consultas | JWT | Admin | No aplica | HTTP NV |
| DELETE | `/consultas/{id}` | Consultas | JWT | Admin | No aplica | HTTP NV |

### Documentación vs rutas reales

- `Documentación de API.pdf` inventaría los mismos 14 recursos generales, pero representa un estado anterior: en su sección Auth indica que no se implementa JWT; el código actual sí lo hace.
- El PDF contiene ejemplos de contratos y status codes que ya no coinciden en todos los casos: por ejemplo, borrados actuales devuelven normalmente `204`, y login usa `404` para email desconocido y `401` para contraseña incorrecta.
- El documento antiguo permite `rol` en registro como si fuera efectivo; el Backend actual lo fuerza a `Cliente`, lo cual es una mejora implementada con documentación desactualizada.
- Rutas especializadas de ownership, stock, confirmar carrito y pagos anidados existen en código y deben volver a documentarse desde OpenAPI actual.

## 9. Autenticación, autorización y ownership

### Cadena de identidad

```text
Registro público
  → UsuariosLogica fuerza RolUsuario.Cliente
  → PasswordHasher<Usuario> persiste hash
  → Login consulta Usuario en BD y verifica hash
  → JwtTokenService firma JWT HS256
  → claims: sub, NameIdentifier, Name, Email, Role, jti
  → Frontend guarda el access_token dentro del ticket de cookie HttpOnly
  → ApiBearerTokenHandler adjunta Bearer en llamadas posteriores
  → Backend valida issuer, audience, firma, lifetime y rol
```

La clave JWT debe tener al menos 32 bytes y no está en el `appsettings.json` versionado; debe proveerse por variable `Authentication__SigningKey` o User Secrets. La cookie Frontend es `HttpOnly`, `SameSite=Lax`, expira con el JWT y no es persistente.

El nombre del rol difiere entre capas de forma deliberada: el JWT Backend emite `Administrador`; la cookie MVC traduce el valor numérico `1` a `Admin` para los atributos `[Authorize(Roles = "Admin")]` del Frontend. Funciona por separación de esquemas, pero es frágil por duplicación de convenciones.

### Admin bootstrap

- `Program.cs` invoca `AsegurarAdministradorAsync` al iniciar.
- La cuenta académica deliberada no se considera defecto.
- Si no existe, se crea; si existe con otro rol, se promueve; si encuentra la contraseña histórica en texto plano, la reemplaza por hash.
- La consulta de solo lectura confirmó 1 fila para el email canónico con rol `1` y longitud de hash 84.
- El login siempre consulta BD; no hay autorización por email ni bypass adicional detectado.
- Riesgo operativo: si la base no está disponible el bootstrap Admin registra Critical y detiene la aplicación. El bootstrap de categorías, en cambio, solo registra Warning y continúa.

### Matriz de ownership

| Recurso | Clasificación | Evidencia y límite |
|---|---|---|
| Usuarios | PROTEGIDO | Unidad: owner/Admin; lista/alta: Admin; autoedición conserva rol persistido |
| Direcciones | PROTEGIDO | Listado filtrado; ID, update y delete validan owner/Admin; alta pisa `IdUsuario` del Cliente |
| Carritos | PROTEGIDO en identidad; PARCIAL en estado | Listados/IDs/nested validan owner/Admin; el owner aún puede enviar estados sensibles |
| DetalleCarritos | PROTEGIDO en identidad | Se valida ownership del carrito actual y, en PUT, también del carrito destino |
| Pedidos | PROTEGIDO | Listados e ID filtran/validan owner; Cliente no puede cambiar estado por endpoints administrativos |
| DetallePedidos | PROTEGIDO | GET resuelve pedido padre; mutaciones solo Admin |
| Pagos | PROTEGIDO | GET resuelve pedido padre; listados y mutaciones solo Admin |

No se encontró una ruta obvia por la que Cliente A pueda leer o mutar recursos de Cliente B con solo conocer un ID. Como no existen tests A/B ni se levantó la API, la conclusión es **IMPLEMENTADO PERO NO VERIFICADO**.

### Datos sensibles

- `UsuarioResponse` y `LoginResponse` no exponen `Contrasena`.
- Los endpoints de usuarios proyectan explícitamente a esos DTOs.
- La recuperación responde igual exista o no el email, lo cual evita enumeración, pero no ejecuta recuperación.
- No hay rate limiting, lockout, refresh/revocación de tokens ni auditoría de accesos. Son mejoras de seguridad, no todas bloqueantes para el alcance académico.
- La exposición de secretos versionados sí es bloqueante y requiere rotación, no solo borrado del archivo actual.

## 10. Integridad económica y stock

| Valor | Origen actual | Cálculo/validación Backend | Persistencia | Clasificación |
|---|---|---|---|---|
| `Producto.Precio` | Request Admin | Solo `>= 0` | `decimal(18,2)` | AUTORITATIVO PARA CATÁLOGO, por rol Admin |
| `DetalleCarrito.PrecioUnitario` | Request Cliente/Admin | Usa request si `> 0`; solo cae a `Producto.Precio` si no | Sí | CONFIADO AL CLIENTE |
| `DetalleCarrito.Subtotal` | Backend | `PrecioUnitario recibido × Cantidad` | Sí | PARCIAL; fórmula server, insumo no confiable |
| `DetallePedido.PrecioUnitario` al confirmar | Copia detalle de carrito | No relee `Producto.Precio` | Sí | CONFIADO INDIRECTAMENTE AL CLIENTE |
| `DetallePedido.Subtotal` al confirmar | Copia detalle de carrito | No recalcula con precio vigente | Sí | CONFIADO INDIRECTAMENTE AL CLIENTE |
| `Pedido.Total` | No existe | No se calcula | No hay columna/propiedad | NO IMPLEMENTADO |
| `Pago.Monto` | Request Admin | Solo `> 0`; no compara con total de pedido | Sí | CONFIADO AL CLIENTE ADMIN; no autoritativo |
| `Compra.Total` | Request Admin | Solo `>= 0` | Sí | CONFIADO AL CLIENTE ADMIN |
| `Producto.Stock` | Request Admin o confirmación | Alta/edición no negativos; confirmación verifica y resta | Sí | PARCIAL |
| `Carrito.Estado` | Request owner/Admin | Enum válido | Sí | CONFIADO AL CLIENTE para una transición sensible |
| `Pedido.Estado` | Cliente forzado a Pendiente; Admin después | Enum válido | Sí | AUTORITATIVO POR ROL, pero transición no modelada |
| `Pago.Estado` | Request Admin | Enum válido; sincroniza pedido | Sí | AUTORITATIVO POR ROL, sin proveedor externo |
| `Usuario.Rol` | Request, según ruta | Registro fuerza Cliente; self-update conserva rol | Sí | AUTORITATIVO EN SERVIDOR para clientes |

### Consecuencia concreta

Un cliente autenticado puede llamar `POST /carritos/{id}/productos` o los endpoints directos de `detallecarritos` con un `PrecioUnitario` positivo arbitrario. El Backend persiste ese precio y subtotal y luego los copia al pedido. Aunque crear el pago es Admin-only, el pedido y los reportes pueden quedar económicamente incorrectos.

### Stock, atomicidad y concurrencia

- `AgregarProductoAsync` y `ConfirmarAsync` validan stock en Backend.
- `ConfirmarAsync` abre una transacción y dentro crea pedido, detalles, actualiza productos y marca el carrito Confirmado. Si hay una excepción, el `await using` revierte al disponer; la atomicidad básica está presente.
- Cada repositorio ejecuta `SaveChangesAsync`, pero todos usan el mismo DbContext/transacción; esto no rompe la atomicidad, aunque genera múltiples round-trips.
- No hay `rowversion`, concurrency token, `UPDATE ... WHERE Stock >= cantidad`, aislamiento serializable ni mecanismo equivalente. Dos confirmaciones concurrentes pueden superar el stock después de validar ambas contra el mismo valor.
- Tampoco hay clave/idempotency token de checkout. El estado del carrito reduce repeticiones secuenciales, pero no cierra la ventana concurrente.

## 11. Estado del flujo de compra

```text
Producto público
  ↓ IMPLEMENTADA
Carrito persistido con owner
  ↓ PARCIAL: no existe UI y el owner puede elegir estado
DetalleCarrito
  ↓ PARCIAL: cantidad/stock se validan; precio es manipulable
POST /carritos/{id}/confirmar
  ↓ PARCIAL: existe operación real de finalización Backend
Pedido + DetallePedido
  ↓ PARCIAL: transaccional, sin total autoritativo ni concurrencia
Pago
  ↓ NO IMPLEMENTADA como pago real: solo CRUD/manual Admin
Estado de pedido
  ↓ PARCIAL: sincroniza ante pago manual aprobado
Historial/confirmación Frontend
  → NO IMPLEMENTADO
```

| Transición | Estado | Evidencia | Brecha |
|---|---|---|---|
| Producto → carrito | PARCIAL | Endpoint nested y detalle directo | Sin UI; precio manipulable |
| Cambiar cantidad | PARCIAL | PUT de detalle y suma en endpoint nested | Sin UX; contrato permite cambiar producto/carrito/precio |
| Quitar producto | IMPLEMENTADA PERO NO VERIFICADA | DELETE nested/directo | Sin UI/test |
| Carrito → pedido | PARCIAL | `CarritosLogica.ConfirmarAsync` | Sin total, idempotencia o control concurrente |
| Crear detalles | IMPLEMENTADA PERO NO VERIFICADA | Copia todas las líneas dentro de transacción | Copia importes no confiables |
| Descontar stock | PARCIAL | Valida y decrementa | Riesgo de overselling concurrente |
| Pedido → pago | PARCIAL | CRUD de pago y endpoint nested Admin | No accesible como checkout Cliente; monto manual |
| Pago → estado pedido | PARCIAL | Pago Aprobado mueve Pendiente a Pagado | No webhook/proveedor; no valida suma pagada |
| Confirmación/número | INICIAL | El ID de `Pedido` existe | No pantalla ni comunicación al usuario |

Sí existe una operación equivalente parcial a `FinalizarCompra`: `POST /carritos/{idCarrito}/confirmar`. No es todavía una finalización comercial completa porque termina antes del pago y puede persistir importes manipulados.

## 12. Estado de base de datos

### Modelo y esquema

- `TotaltechDbContext` expone 13 `DbSet`: Usuarios, Direcciones, Proveedores, Productos, Categorías, Pedidos, DetallePedidos, Carritos, DetalleCarritos, Pagos, Compras, Reportes y Consultas.
- Relaciones explícitas con `DeleteBehavior.Restrict` evitan cascadas accidentales entre recursos principales.
- Índices únicos definidos para `Usuarios.Email` y `(DetalleCarrito.IdCarrito, DetalleCarrito.IdProducto)`.
- Campos monetarios usan `decimal(18,2)` en Producto, Compra, DetalleCarrito, DetallePedido y Pago.
- `Usuario.Email` tiene máximo 256 y `Usuario.Contrasena` máximo 500.
- No hay índices únicos de categoría por nombre ni proveedor por CUIT; la idempotencia de categorías depende de lectura/normalización en aplicación y puede sufrir carrera.
- No hay tokens de concurrencia.
- No existe propiedad/columna `Pedido.Total` ni campo de imagen/URL en Producto.

### Migraciones

El repositorio contiene y la base reportó aplicadas estas cinco migraciones:

1. `20260613220646_Inicial`
2. `20260613221117_CorregirRelaciones`
3. `20260615211910_AgregarDetalleCarrito`
4. `20260615230438_AgregarConsultasYFlujos`
5. `20260622030413_AjustarContratosValidacionesYRestricciones`

### Snapshot de datos de solo lectura

| Tabla | Filas |
|---|---:|
| Usuarios | 13 |
| Direcciones | 10 |
| Proveedores | 10 |
| Productos | 11 |
| Categorías | 16 |
| Pedidos | 10 |
| DetallePedidos | 10 |
| Carritos | 10 |
| DetalleCarritos | 10 |
| Pagos | 10 |
| Compras | 10 |
| Reportes | 10 |
| Consultas | 10 |

No se evaluó la calidad semántica de cada fila para evitar exponer datos personales. Los conteos demuestran persistencia y datos de prueba, no que cada flujo haya sido producido por la aplicación actual.

### Categorías canónicas

La base contiene exactamente las seis esperadas: `Notebooks`, `Celulares`, `Almacenamiento`, `Gabinetes`, `Periféricos` y `Placas de Video`. También contiene 10 categorías simuladas marcadas como testing; por eso el total es 16.

`CategoriasIniciales.InicializarAsync`:

- normaliza `Trim + FormKC + ToUpperInvariant`;
- agrega solo categorías canónicas ausentes;
- conserva categorías adicionales;
- es idempotente en ejecución secuencial;
- no está respaldado por índice único, por lo que la idempotencia concurrente es parcial.

### Configuración y secretos

- La cadena real está dentro de `Totaltech/appsettings.json`, archivo versionado, e incluye credenciales SQL. Esto es un defecto crítico de gestión de secretos.
- `Credenciales.txt` está versionado y contiene material no vacío con forma de credenciales. Debe tratarse como comprometido.
- La clave JWT no está versionada en `appsettings.json`, decisión correcta.
- La remediación requiere rotar credenciales en el proveedor y, si corresponde, limpiar historial coordinadamente. Quitar el texto del último commit no invalida una credencial ya expuesta.

## 13. Estado del Frontend

| Flujo/pantalla | Estado | Cadena real | Observación |
|---|---|---|---|
| Home | PARCIAL | Controller + vista | Solo logo; no portada/catálogo/promociones del mockup |
| Login | PARCIAL ALTO | MVC → `/auth/login` → cookie/JWT | Diseño trabajado y responsive; sin test/runtime actual |
| Registro | PARCIAL ALTO | MVC → `/auth/registro` | Validaciones y antiforgery; sin auto-login ni test |
| Recuperar contraseña | NO IMPLEMENTADO en UI | Placeholder | Backend también es solo simulación |
| Productos | PARCIAL ALTO | Service + Controller + Index | Lista, búsqueda, categoría y disponibilidad con datos API |
| Detalle producto | PARCIAL | Service + Controller + vista mínima | No imagen, galería, carrito ni ficha equivalente al mockup |
| Categorías | PARCIAL ALTO | Service en `CarritosApiService.cs`, Controller, vistas | Funciona conceptualmente; nombre del archivo/clase es confuso |
| Proveedores | PARCIAL ALTO, Admin | Service + Controller + vistas | Integrado con Bearer; no forma parte del flujo cliente |
| Modo Admin | PARCIAL | Cookie role + landing | Solo enlaces a categorías, productos y proveedores |
| Carrito | NO IMPLEMENTADO | Controller/view/service reservados | Mockup local disponible, sin código funcional |
| Checkout | NO IMPLEMENTADO | Controller/views reservados | Mockups locales disponibles, sin integración |
| Pedidos/historial | NO IMPLEMENTADO | Controller/views/service reservados | Backend sí expone rutas propias |
| Perfil/cuenta | NO IMPLEMENTADO | Placeholder | Backend de usuarios/direcciones disponible |
| Contacto | NO IMPLEMENTADO en UI | Placeholder | Backend de consultas disponible |
| Administración usuarios/pedidos/compras/reportes/consultas | NO IMPLEMENTADO en UI | Vistas de una línea/placeholders | Backend Admin disponible |

### Integración y sesión

- `ApiBaseUrl` está en configuración Frontend.
- `ApiBearerTokenHandler` recupera `access_token` del ticket y añade `Authorization: Bearer`.
- El Frontend tiene política cookie y `[Authorize(Roles="Admin")]` en controladores administrativos reales.
- No se encontró acción Logout, por lo que la sesión solo termina por expiración/cierre del ticket.
- Solo Categorías, Productos y Proveedores están registrados como ApiServices. Los servicios previstos para Auth, Carritos, Compras, Consultas, Direcciones, Pagos, Reportes y Usuarios son placeholders.

### Responsive y mockups

Se inspeccionaron los mockups locales de portada, login, carrito y las fases de finalizar compra. `site.css` tiene breakpoints a 1050, 1024, 900, 768, 700, 620 px y por altura. Login/registro poseen una implementación visual sustancial; catálogo usa la grilla responsive de Bootstrap.

| Intención de mockup | Estado funcional |
|---|---|
| Portada con navegación/búsqueda | PARCIAL: header/búsqueda existen; contenido Home no |
| Catálogo por categorías | PARCIAL: datos y filtros básicos; imágenes no vinculadas al modelo |
| Detalle de producto | PARCIAL: vista mínima sin experiencia del mockup |
| Inicio de sesión | PARCIAL ALTO: flujo real rediseñado, no pixel-perfect ni testado en runtime actual |
| Carrito | NO IMPLEMENTADO |
| Checkout por pasos | NO IMPLEMENTADO |
| Confirmación/pago | NO IMPLEMENTADO |
| Administración completa | PARCIAL: tres ABM visibles; resto placeholder |

Los enlaces de Canva del README no se abrieron; la comparación se hizo contra los PNG versionados. Responsive del flujo completo es **NO VERIFICADO** porque los flujos centrales no existen.

## 14. Matriz de requerimientos funcionales

Trazabilidad basada en el PDF funcional local, no en la mera documentación de API.

| RF | Requerimiento documentado | Evidencia código | Evidencia test | Estado |
|---|---|---|---|---|
| RF1.1 | CRUD de productos | Endpoints/lógica/repositorio + MVC Productos | Ninguna | PARCIAL |
| RF1.2 | Imágenes de productos | Archivos estáticos aislados; entidad sin imagen | Ninguna | NO IMPLEMENTADO como función |
| RF1.3 | Stock, precio y descripción | Entidad/CRUD; stock descontado al confirmar | Ninguna | PARCIAL |
| RF2.1 | Mostrar catálogo por categoría | Rutas y UI de categoría | Ninguna | PARCIAL |
| RF2.2 | Filtrar por tipo, precio y disponibilidad | Categoría/disponibilidad; sin rango de precio/tipo específico | Ninguna | PARCIAL |
| RF2.3 | Buscar por palabra clave | `/productos/buscar` + formularios | Ninguna | PARCIAL |
| RF3.1 | Agregar productos al carrito | Endpoints Backend | Ninguna | PARCIAL |
| RF3.2 | Modificar cantidad | PUT detalle Backend | Ninguna | PARCIAL |
| RF3.3 | Eliminar del carrito | DELETE nested/directo | Ninguna | PARCIAL |
| RF3.4 | Resumen y total automático | Subtotal no confiable; no `Pedido.Total`; UI ausente | Ninguna | NO IMPLEMENTADO de forma segura |
| RF4.1 | Pago digital/Mercado Pago | Entidad/CRUD manual solamente | Ninguna | NO IMPLEMENTADO |
| RF4.2 | Confirmación de pago y número de pedido | ID pedido + estado por pago manual | Ninguna | PARCIAL |
| RF5.1 | Registro | API + MVC + hash + rol forzado | Ninguna | NO VERIFICADO |
| RF5.2 | Login | JWT + cookie/Bearer | Ninguna | NO VERIFICADO |
| RF5.3 | Recuperación de contraseña | Endpoint neutro sin token/envío/reset | Ninguna | NO IMPLEMENTADO funcionalmente |
| RF5.4 | Historial y estado de compras | Backend owner-scoped; Frontend pendiente | Ninguna | PARCIAL |
| RF6.1 | Admin ve pedidos | Endpoint Admin; vista placeholder | Ninguna | PARCIAL |
| RF6.2 | Admin actualiza estado | PATCH Admin | Ninguna | NO VERIFICADO |
| RF6.3 | Notificar cambios | Sin implementación | Ninguna | NO IMPLEMENTADO |
| RF7.1 | Formulario de contacto | POST público Backend; UI pendiente | Ninguna | PARCIAL |
| RF7.2 | WhatsApp directo | Sin enlace funcional | Ninguna | NO IMPLEMENTADO |
| RF8 | Administración básica productos/pedidos/usuarios | Productos UI; pedidos/usuarios solo API | Ninguna | PARCIAL |
| RF9 | Responsive PC/móvil | CSS/Bootstrap en flujos existentes | Ninguna/inspección runtime ausente | NO VERIFICADO integralmente |

Funcionalidades explícitamente excluidas por el documento —reseñas, mayoristas/empresas, chat en vivo y estadísticas avanzadas— se clasifican **FUERA DEL ALCANCE ACTUAL** y no afectan el porcentaje.

## 15. Requerimientos no funcionales

| RNF documentado | Evidencia disponible | Estado |
|---|---|---|
| Respuesta menor a 3 segundos en uso normal | Timeout Frontend de 10 s, sin benchmark/APM/carga | NO VERIFICADO |
| Interfaz intuitiva | Mockups y algunos formularios reales; sin prueba de usabilidad | NO VERIFICADO |
| Compatibilidad navegadores modernos | HTML/CSS/Bootstrap convencionales; sin matriz de navegadores | NO VERIFICADO |
| Compatibilidad móvil | viewport + media queries; flujos incompletos | PARCIAL / NO VERIFICADO E2E |
| Contraseñas seguras | `PasswordHasher<Usuario>` y hashes persistidos | PARCIAL ALTO |
| Datos personales protegidos/privacidad | JWT/cookie HttpOnly; cadena SQL expuesta en Git, sin política de privacidad implementada | PARCIAL BAJO |
| Mantenibilidad | Capas claras y nullable; duplicación de contratos y placeholders/nombres confusos | PARCIAL |
| Escalabilidad | SQL/DI async y retry; sin pruebas, caché, paginación o diseño de escala | NO VERIFICADO |
| Disponibilidad 99% | Sin hosting/monitoring/SLO evidenciado | NO VERIFICADO |

No se declara cumplimiento de rendimiento, disponibilidad, seguridad integral ni escalabilidad sin evidencia objetiva.

## 16. Tests y validaciones

### Inventario de tests

| Tipo | Ubicación prevista | Cantidad ejecutable | Cobertura |
|---|---|---:|---|
| Unitarios Backend | `Tests/Totaltech.UnitTests/Logica`, `Validaciones` | 0 | Ninguna |
| Integración Backend/API | `Tests/Totaltech.IntegrationTests/Endpoints`, `Persistencia` | 0 | Ninguna |
| Frontend | `Tests/Frontend.UnitTests/Controllers`, `Services` | 0 | Ninguna |
| Seguridad | No hay proyecto | 0 | Ninguna |

Los directorios contienen únicamente `.gitkeep`. No existe `.csproj` de test ni archivo `.cs` bajo `Tests`.

### Resultados ejecutados

- `dotnet build Totaltech/Totaltech.csproj --nologo`: **EXIT 0**, 0 warnings, 0 errores.
- `dotnet build Frontend/Frontend.csproj --nologo`: **EXIT 0**, 0 warnings, 0 errores.
- `dotnet test Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln --no-build --nologo`: **EXIT 0 sin salida de tests**; la solución solo contiene el Backend y no contiene proyecto de pruebas.
- Consulta SQL de solo lectura: **EXIT 0**; confirmó conteos, categorías, Admin, índices y migraciones.
- No se ejecutaron requests HTTP ni pruebas destructivas.

Áreas críticas sin cobertura: tampering de precio/monto/rol/estado; Cliente A vs Cliente B; Admin vs Cliente; stock insuficiente y carreras; rollback; doble checkout; sincronización pago-pedido; login/registro; contratos MVC/API.

## 17. Bloqueantes actuales

### BLOQUEANTES PARA 60%

1. **Secretos versionados:** la credencial SQL debe considerarse comprometida. Bloquea un ciclo de desarrollo seguro y CI reproducible.
2. **Precio y total no autoritativos:** un cliente puede persistir un precio unitario arbitrario; no existe `Pedido.Total`.
3. **Pago no real:** Mercado Pago/digital payment no está integrado; `Pago` es un registro manual Admin con monto confiado.
4. **Concurrencia de stock:** la transacción aporta atomicidad, pero no evita overselling entre confirmaciones simultáneas.
5. **Frontend del flujo principal ausente:** carrito, checkout, dirección, confirmación, cuenta e historial son placeholders.
6. **Cero tests:** no puede sostenerse “verificable” ni prevenir regresiones de auth/ownership/economía.

No son bloqueantes para 60% por sí solos: refresh tokens, paginación, patrones enterprise, una capa de servicios compartidos perfecta o coincidencia pixel-perfect con Canva.

## 18. Deuda técnica no bloqueante

- `Frontend/Services/CarritosApiService.cs` contiene realmente `CategoriasApiService`; el nombre induce errores de mantenimiento.
- Interfaces de servicios Frontend y múltiples archivos de controller/view son placeholders de una línea.
- `AuthApiService` está vacío mientras `HomeController` concentra llamadas HTTP, parsing y manejo de sesión.
- La solución excluye Frontend, por lo que `dotnet build` de la solución no cubre todo.
- DTOs Backend tienen poca validación declarativa; las reglas están dispersas en lógica.
- No hay middleware uniforme de excepciones/ProblemDetails; se repite `catch (DbUpdateException)` en deletes.
- `EsNoEncontrado` infiere 404 inspeccionando texto de errores.
- Repositorios de lectura no usan `AsNoTracking` y algunos flujos hacen N+1, por ejemplo detalles de todos los carritos del cliente.
- Fechas mezclan `DateTime.Now` y `DateTime.UtcNow`.
- No hay paginación para listados.
- La política de borrado permite autoeliminación de usuario, sujeta a FK/409; la decisión de negocio no está documentada.
- Los nombres de rol `Administrador` (API) y `Admin` (cookie MVC) están duplicados.
- `ControlProyecto/CleanCode.md` termina de forma abrupta con una lista/fence incompleta; debe repararse como documentación, no como refactor funcional.
- README referencia documentos externos que pueden cambiar fuera del control de versiones.
- El PDF de API requiere regeneración o actualización tras JWT/ownership.
- Categorías de prueba conviven en la base con canónicas; no bloquean, pero deben identificarse por ambiente.

## 19. Roadmap priorizado hacia ≥60%

Las contribuciones estimadas se expresan contra las 24 unidades del apartado 5. Solo se acreditan si se cumplen los criterios y tests de cada etapa; no son puntos por archivos creados.

### Etapa 1 — Configuración segura y red mínima de pruebas

**Prioridad:** P0

**Objetivo:**
Retirar secretos del repositorio, rotar las credenciales expuestas y crear una base de tests que haga verificables autenticación, rol y ownership.

**Estado que la motiva:**
Hay credenciales SQL versionadas y cero tests. Cambiar economía/checkout sin proteger configuración ni congelar las reglas de acceso aumenta el riesgo de regresión y exposición.

**Alcance:**
- Rotar credenciales SQL en el proveedor; invalidar las actualmente versionadas.
- Remover valores sensibles de archivos trackeados y usar User Secrets/variables/CI secrets con ejemplos seguros.
- Definir configuración reproducible de test que no apunte a la base compartida.
- Crear proyectos de unit/integration tests e incluirlos junto con Backend y Frontend en la solución o pipeline.
- Probar registro Cliente forzado, hash/login, Admin por rol, 401/403 y accesos owner vs usuario ajeno en recursos críticos.

**Fuera de alcance:**
- Cambiar el algoritmo de JWT o agregar refresh tokens.
- Implementar checkout, pagos o pantallas nuevas.
- Reescribir el historial Git sin coordinación del equipo.

**Dependencias:**
- Acceso del equipo al servicio SQL para rotar el secreto.
- Acordar una base efímera/aislada para integración.

**Áreas/archivos probables:**
- `Totaltech/appsettings*.json`, `Credenciales.txt`, `.gitignore`.
- Configuración de secretos local/CI.
- `Tests/Totaltech.UnitTests/**`, `Tests/Totaltech.IntegrationTests/**`.
- `Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln` y eventual workflow CI.

**Criterios de aceptación:**
- [ ] Ninguna credencial válida queda en archivos versionados; las anteriores fueron rotadas.
- [ ] Backend/Frontend levantan con configuración externa documentada y fail-fast sin secretos.
- [ ] Tests prueban registro sin escalación, login válido/inválido, Admin/Cliente y Cliente A vs Cliente B.
- [ ] Una base de test aislada se crea/descarta sin tocar la base compartida.
- [ ] Build y tests de ambos proyectos corren en un único flujo reproducible.

**Validación:**
- Escaneo de secretos sobre árbol e historial relevante.
- `dotnet build` de Backend y Frontend.
- `dotnet test` con evidencia de cantidad y resultados.
- HTTP integration tests para 401, 403, 404 defensivo, owner 2xx y rol Admin.

**Riesgos:**
- Rotar sin actualizar entornos puede cortar acceso temporalmente.
- Limpiar historial afecta clones/ramas; requiere coordinación y no debe improvisarse.

**Contribución estimada al avance:**
+2,1 puntos porcentuales aprox., si registro y login pasan de PARCIAL ALTO a COMPLETO verificado. Su mayor aporte es de seguridad y verificabilidad.

**Desbloquea:**
Cambios de contrato económico protegidos por pruebas y desarrollo sin secretos compartidos.

### Etapa 2 — Precio, subtotal, total y estados autoritativos

**Prioridad:** P0

**Objetivo:**
Hacer que el Backend sea la única autoridad de importes y transiciones sensibles del carrito/pedido.

**Estado que la motiva:**
`PrecioUnitario` llega desde el cliente, el subtotal usa ese valor, la confirmación lo copia al pedido, `Pedido.Total` no existe y el owner puede enviar `Carrito.Estado`.

**Alcance:**
- Rediseñar DTOs públicos de carrito para recibir solo producto/cantidad.
- Tomar el precio del producto en servidor y recalcular subtotal en cada mutación/confirmación.
- Definir `Pedido.Total` o una proyección autoritativa inequívoca y su migración.
- Derivar/validar `Pago.Monto` contra el saldo/total del pedido; no aceptar monto libre en el flujo cliente.
- Restringir transiciones de Carrito/Pedido/Pago mediante comandos específicos.
- Adaptar Reportes para basarse únicamente en valores confiables.
- Agregar pruebas de tampering para precio, subtotal, total, monto, rol y estados.

**Fuera de alcance:**
- Integrar todavía el SDK/API de Mercado Pago.
- Resolver concurrencia de stock de manera completa.
- Construir UI de carrito.

**Dependencias:**
- Etapa 1 cerrada.
- Decisión explícita sobre precio histórico y cálculo de envío/impuestos dentro del alcance académico.

**Áreas/archivos probables:**
- `Totaltech/Logica/DTOs/**`.
- `Totaltech/Logica/CarritosLogica.cs`, `DetalleCarritosLogica.cs`, `DetallePedidosLogica.cs`, `PagosLogica.cs`.
- `Totaltech/Entidades/Pedido.cs`, `Totaltech/Datos/TotaltechDbContext.cs`, nueva migración.
- Endpoints de carritos, detalles, pedidos, pagos y reportes.
- Tests de lógica e integración.

**Criterios de aceptación:**
- [ ] Ningún request Cliente puede elegir precio unitario, subtotal, total, monto final ni estado sensible.
- [ ] El total del pedido coincide con la suma de detalles calculados en servidor y queda estable como precio histórico.
- [ ] Un precio manipulado es ignorado/rechazado por contrato y una prueba lo demuestra.
- [ ] Un pago no puede aprobar un pedido por un monto arbitrario.
- [ ] Reportes usan únicamente importes autoritativos.

**Validación:**
- Unit tests de cálculo y transiciones.
- Integration tests con payloads manipulados.
- Verificación de migración sobre base efímera.
- Build Backend/Frontend y validación de contratos consumidores.

**Riesgos:**
- Romper contratos actuales del Frontend/documentación.
- Decidir incorrectamente si el precio se congela al agregar o al confirmar; debe documentarse.

**Contribución estimada al avance:**
+4,2 puntos porcentuales aprox.

**Desbloquea:**
Checkout económicamente confiable, pagos reales y reportes defendibles.

### Etapa 3 — Checkout atómico, concurrente e idempotente

**Prioridad:** P0

**Objetivo:**
Consolidar la confirmación del carrito para que no genere overselling, duplicados ni estados parciales bajo carreras o reintentos.

**Estado que la motiva:**
La transacción actual aporta atomicidad básica, pero el stock se valida antes de actualizar sin token de concurrencia y no hay idempotency key/garantía de única confirmación concurrente.

**Alcance:**
- Mantener una única operación de checkout Backend a partir de carrito activo y dirección propia.
- Implementar actualización condicional o token de concurrencia para stock.
- Hacer idempotente la confirmación/reintento y asegurar un solo pedido por carrito.
- Definir rollback verificable ante falla en cualquier detalle.
- Evitar el endpoint genérico de pedido vacío en el flujo Cliente o separarlo claramente del checkout.
- Probar dos compradores sobre el último stock, doble confirmación y fallo intermedio.

**Fuera de alcance:**
- UI completa.
- Integración Mercado Pago.
- Reservas de stock de larga duración si no son necesarias para el alcance.

**Dependencias:**
- Etapa 2: importes y total autoritativos.
- Elección de estrategia SQL/EF de concurrencia.

**Áreas/archivos probables:**
- `CarritosLogica.cs`, repositorios de carrito/producto/pedido/detalles.
- `Producto`, `Carrito` y/o relación Pedido-Carrito.
- `TotaltechDbContext`, migración de concurrencia/unicidad.
- Tests de integración concurrente y rollback.

**Criterios de aceptación:**
- [ ] Dos confirmaciones concurrentes no dejan stock negativo ni venden más unidades disponibles.
- [ ] Repetir la misma confirmación devuelve el mismo resultado o un conflicto estable, sin pedido duplicado.
- [ ] Una falla inducida no deja pedido, detalles, stock o carrito parcialmente persistidos.
- [ ] La dirección debe pertenecer al owner y el carrito debe estar Activo.
- [ ] El pedido creado conserva importes autoritativos de la Etapa 2.

**Validación:**
- Test de integración paralelo sobre base SQL aislada.
- Test de rollback con excepción controlada.
- Verificación de constraint/token en migración y esquema.
- HTTP tests de conflicto/idempotencia.

**Riesgos:**
- Los proveedores EF en memoria no reproducen concurrencia SQL; el test debe usar SQL Server real/efímero.
- Un retry mal diseñado puede duplicar efectos.

**Contribución estimada al avance:**
+4,2 puntos porcentuales aprox.

**Desbloquea:**
Consumo seguro del checkout desde Frontend y posterior vinculación con proveedor de pagos.

### Etapa 4 — Flujo cliente MVC: carrito, dirección, checkout e historial

**Prioridad:** P1

**Objetivo:**
Convertir la API segura en un flujo utilizable por el cliente desde producto hasta pedido confirmado, incluyendo cuenta/direcciones e historial.

**Estado que la motiva:**
Controladores, servicios, ViewModels y vistas de Carrito, Checkout, Cuenta y Pedidos son placeholders, aunque gran parte de la API ya existe y hay mockups locales.

**Alcance:**
- Implementar ApiServices y contratos Frontend para carrito, detalle, direcciones y pedidos.
- Agregar al carrito desde catálogo/detalle; cambiar cantidad y eliminar.
- Mostrar resumen calculado por Backend, seleccionar/crear dirección propia y confirmar.
- Mostrar número y estado del pedido e historial propio.
- Implementar logout y estados 401/403/409 coherentes.
- Completar responsive funcional en móvil/escritorio para este recorrido.
- Incorporar tests de controllers/services y una prueba E2E o integración del happy path sin pago real.

**Fuera de alcance:**
- Capturar datos crudos de tarjeta.
- Panel Admin completo, notificaciones y recuperación de contraseña.
- Pixel-perfect de todos los mockups.

**Dependencias:**
- Etapas 1 a 3 cerradas y contrato OpenAPI estabilizado.
- Datos de catálogo/proveedor de prueba reproducibles.

**Áreas/archivos probables:**
- `Frontend/Controllers/CarritoController.cs`, `CheckoutController.cs`, `CuentaController.cs`, `PedidosController.cs`.
- `Frontend/Services/**`, `Models/**`, `Views/Carrito/**`, `Checkout/**`, `Cuenta/**`, `Pedidos/**`.
- `Views/Productos/**`, `_Layout.cshtml`, CSS/JS de flujo.
- Tests Frontend y de integración API.

**Criterios de aceptación:**
- [ ] Un Cliente autenticado agrega, actualiza y elimina líneas sin poder alterar importes.
- [ ] Solo puede elegir sus direcciones y ver sus pedidos.
- [ ] Confirmar muestra un número de pedido estable y el stock actualizado.
- [ ] Errores de stock/conflicto se muestran sin duplicar pedidos.
- [ ] El flujo funciona en viewport móvil y escritorio y cuenta con una prueba automatizada principal.

**Validación:**
- Tests unitarios de controllers/services con respuestas 2xx/4xx.
- Integration/E2E del recorrido producto → carrito → dirección → pedido.
- Prueba visual manual en breakpoints representativos.
- Build Frontend y Backend.

**Riesgos:**
- Contratos duplicados pueden desalinearse; conviene congelarlos con tests/OpenAPI.
- No debe mostrarse como “pago completado” un pedido solo creado.

**Contribución estimada al avance:**
+7,3 puntos porcentuales aprox.

**Desbloquea:**
El primer flujo cliente end-to-end y la conexión final con Mercado Pago.

### Etapa 5 — Pago digital y confirmación idempotente

**Prioridad:** P1

**Objetivo:**
Integrar Mercado Pago en modo sandbox y cerrar pedido/pago con monto autoritativo, callback/webhook validado e idempotencia.

**Estado que la motiva:**
El RF exige pago digital; hoy solo existen registros `Pago` administrados manualmente. El mockup muestra captura de tarjeta, pero el sistema no debe procesar datos crudos si el proveedor puede tokenizarlos/alojarlos.

**Alcance:**
- Crear preferencia/orden de pago desde el total Backend.
- Redirigir o usar componente tokenizado del proveedor; no persistir datos de tarjeta.
- Validar firma/origen del webhook y consultar al proveedor cuando corresponda.
- Mapear estados externos a `EstadoPago`/`EstadoPedido` mediante transiciones idempotentes.
- Mostrar pendiente/aprobado/rechazado y número de pedido en Frontend.
- Probar sandbox, webhook repetido, monto discordante y orden desconocida.

**Fuera de alcance:**
- Producción real antes de revisión de credenciales/compliance.
- Reembolsos avanzados, cuotas complejas o conciliación contable completa.
- Guardar PAN/CVV o construir un procesador de tarjetas propio.

**Dependencias:**
- Etapas 1 a 4.
- Cuenta/credenciales sandbox de Mercado Pago y URL pública de webhook para pruebas.

**Áreas/archivos probables:**
- Nuevo adaptador/configuración de pagos Backend.
- `PagosLogica`, endpoints de pago/webhook, DTOs y persistencia de referencia externa/idempotencia.
- Checkout/Confirmación Frontend.
- Tests de contrato y sandbox.

**Criterios de aceptación:**
- [ ] El monto enviado al proveedor surge exclusivamente del pedido autoritativo.
- [ ] Un callback del navegador no aprueba por sí solo el pedido.
- [ ] Webhooks válidos y repetidos producen una única transición consistente.
- [ ] Monto/orden discordante se rechaza y registra sin marcar Pagado.
- [ ] El usuario ve número y estado correcto sin que TotalTech almacene datos crudos de tarjeta.

**Validación:**
- Suite automatizada con cliente de proveedor simulado.
- Prueba sandbox documentada de aprobado/rechazado/pendiente.
- Reenvío del mismo webhook y verificación de idempotencia.
- E2E producto → carrito → checkout → pago → historial.

**Riesgos:**
- Dependencia de credenciales/red del proveedor.
- Webhooks expuestos sin validación permitirían falsificar pagos.
- La semántica asíncrona exige que UI no confunda “retorno exitoso” con “pago aprobado”.

**Contribución estimada al avance:**
+4,2 puntos porcentuales aprox.

**Desbloquea:**
Cumplimiento del flujo comercial principal y un avance general estimado de aproximadamente 62,5%.

## 20. Resumen de etapas

| # | Etapa | Prioridad | Dependencias | Avance acumulado estimado | Estado |
|---:|---|---|---|---:|---|
| Base | Estado auditado | — | — | 40,6% | ACTUAL |
| 1 | Configuración segura y red mínima de pruebas | P0 | Rotación externa/BD test | 42,7% | PLANIFICADA |
| 2 | Precio, subtotal, total y estados autoritativos | P0 | Etapa 1 | 46,9% | PLANIFICADA |
| 3 | Checkout atómico, concurrente e idempotente | P0 | Etapa 2 | 51,1% | PLANIFICADA |
| 4 | Flujo cliente MVC: carrito, dirección, checkout e historial | P1 | Etapas 1–3 | 58,3% | PLANIFICADA |
| 5 | Pago digital y confirmación idempotente | P1 | Etapas 1–4 + sandbox | **62,5%** | PLANIFICADA |

Las cifras son proyecciones condicionadas: si una etapa no incluye sus pruebas y criterios de aceptación, no corresponde acreditar todo el incremento. Luego de 60%, el siguiente bloque útil sería recuperación de contraseña, contacto/WhatsApp, Admin de usuarios/pedidos, notificaciones y documentación API actualizada, apuntando a 70%.

## 21. Próxima etapa recomendada

**Etapa 1 — Configuración segura y red mínima de pruebas**

Motivo:
La credencial SQL versionada es el riesgo más urgente y la ausencia de tests impide modificar contratos de auth/economía con seguridad. Esta etapa produce una base verificable sin mezclar todavía el rediseño del checkout.

No comenzar otra etapa antes de cerrar:

- rotación efectiva de toda credencial expuesta, no solo borrado del archivo;
- configuración local/CI sin secretos trackeados;
- tests verdes de login, registro, rol Admin/Cliente y ownership Cliente A/Cliente B;
- base de integración aislada de la base compartida.

## 22. Criterio para considerar Backend sólido

Para el alcance académico, Backend puede considerarse sólidamente listo cuando todos estos criterios sean verdaderos:

- [x] **Arquitectura:** endpoints, lógica, repositorios y persistencia tienen responsabilidades reconocibles.
- [x] **Persistencia:** entidades/relaciones principales y migraciones están aplicadas.
- [~] **Validación:** IDs, cantidades, enums, duplicados y relaciones se validan de forma coherente y probada.
- [~] **Auth:** registro/login/bootstrap funcionan contra BD y están cubiertos por tests.
- [~] **Autorización:** mutaciones Admin y fallback autenticado cuentan con pruebas de 401/403.
- [~] **Ownership:** Cliente A no accede a Cliente B y existe suite automatizada por recurso sensible.
- [ ] **Economía:** precio, subtotal, total y monto son calculados/validados por Backend sin confiar en navegador.
- [ ] **Stock:** actualización atómica/concurrente impide overselling.
- [~] **Compra:** existe confirmación de carrito transaccional, pero falta pago y contrato completo.
- [~] **Atomicidad:** rollback e idempotencia están probados, no solo implementados parcialmente.
- [~] **Errores:** contratos 400/401/403/404/409 y errores inesperados son consistentes y no filtran detalles.
- [ ] **Testing:** auth, ownership, economía, concurrencia, checkout y pago tienen pruebas automáticas.
- [ ] **Configuración segura:** no quedan secretos válidos versionados y los ambientes son reproducibles.

Leyenda: `[x]` satisfecho con evidencia actual; `[~]` parcial/no verificado; `[ ]` faltante/bloqueante. Para declarar “sólido” no se exige perfección enterprise, pero los ítems de Economía, Stock, Testing y Configuración segura no pueden quedar abiertos.

## 23. Riesgos

| Severidad | Riesgo | Probabilidad/impacto | Mitigación prevista |
|---|---|---|---|
| CRÍTICA | Credenciales SQL válidas en Git | Exposición/acceso no autorizado | Rotar, externalizar, escanear y coordinar historial |
| ALTA | Precio unitario manipulable | Pedidos/reportes económicamente falsos | Etapa 2, contratos mínimos y tests de tampering |
| ALTA | Overselling concurrente | Stock negativo o doble venta | Etapa 3, control condicional/token + test paralelo |
| ALTA | Cero tests | Regresiones de seguridad y negocio invisibles | Etapa 1 y tests obligatorios en cada etapa |
| ALTA | Pago manual presentado como pago | Estado Pagado sin confirmación externa | Etapa 5, webhook validado e idempotente |
| MEDIA | Backend inicia con escrituras de bootstrap | Auditorías/smoke tests pueden mutar BD compartida | Ambientes aislados y bootstrap controlado |
| MEDIA | Base compartida con datos simulados | Resultados/reportes contaminados | Separar ambientes/fixtures; no borrar sin autorización |
| MEDIA | Documentación API obsoleta | Clientes implementan contratos incorrectos | Regenerar desde OpenAPI tras estabilizar etapas 2–3 |
| MEDIA | Frontend/API con DTOs duplicados | Rupturas silenciosas | Contract tests o cliente generado |
| MEDIA | Errores no uniformes | UX/diagnóstico inconsistente | ProblemDetails/middleware después de cerrar P0 funcional |
| MEDIA | RNF sin evidencia | Se promete rendimiento/disponibilidad no probados | Benchmarks/monitoring en etapa posterior |
| BAJA | Nombres/placeholders/deuda de estilo | Mantenibilidad | Lotes pequeños después del flujo principal |

## 24. Evidencias y archivos relevantes

### Instrucciones y documentación

- `AGENTS.md`: topología del repositorio anidado y reglas de trabajo.
- `ControlProyecto/Auditor.md`: severidad, evidencia y modo READ_ONLY.
- `ControlProyecto/Backend.md`, `Frontend.md`, `CleanCode.md`: criterios técnicos locales.
- `Documentación - Grupo 1 -TotalTech.pdf`: alcance, RF, RNF y exclusiones.
- `Documentación de API.pdf`: inventario histórico; desactualizado respecto de JWT/ownership actuales.
- `README.md`: referencias externas a Google Docs, Canva y Lucidchart (**NO VERIFICADAS**).
- `Frontend/photos/*.png`: mockups locales de portada, categorías, producto, login, carrito y checkout.

### Backend

- `Totaltech/Program.cs`: DI, JWT, políticas, SQL Server, bootstrap y mapeo de 14 recursos.
- `Totaltech/Seguridad/{JwtOptions,JwtTokenService,Autorizacion}.cs`: claims, firma, rol y ownership.
- `Totaltech/Endpoints/**`: 87 rutas inventariadas.
- `Totaltech/Logica/CarritosLogica.cs`: confirmación transaccional, stock e importes actuales.
- `Totaltech/Logica/{Usuarios,Pagos,DetalleCarritos,DetallePedidos,Productos,Reportes}Logica.cs`: reglas principales.
- `Totaltech/Repositorios/ReportesRepositorio.cs`: agregados que heredan valores económicos persistidos.
- `Totaltech/Datos/TotaltechDbContext.cs`: relaciones, índices y restricciones.
- `Totaltech/Entidades/**`: ausencia de `Pedido.Total`/imagen y presencia de decimales.
- `Totaltech/Logica/DTOs/**`: campos de request manipulables.
- `Totaltech/Migrations/**`: cinco migraciones y snapshot.
- `Totaltech/appsettings.json`, `Credenciales.txt`: evidencia de secretos versionados; valores omitidos.

### Frontend

- `Frontend/Program.cs`: cookie, HttpClient y servicios registrados.
- `Frontend/Services/ApiBearerTokenHandler.cs`: propagación de JWT.
- `Frontend/Controllers/HomeController.cs`: login/registro y ticket de cookie.
- `Frontend/Controllers/{Productos,Categorias,Proveedores}Controller.cs`: flujos reales.
- `Frontend/Controllers/{Carrito,Checkout,Cuenta,Pedidos,Consultas}Controller.cs`: placeholders.
- `Frontend/Services/**`: solo Categorías, Productos y Proveedores tienen implementación relevante.
- `Frontend/Views/**`: login/registro/catálogo/ABM parciales; flujos principales pendientes.
- `Frontend/wwwroot/css/site.css`: responsive existente.

### Base y tests

- Consulta read-only a SQL Server: 13 tablas con datos, 5 migraciones aplicadas, dos índices únicos y categorías/Admin verificados.
- `Tests/**`: estructura prevista sin proyectos ni código de tests.

## 25. Comandos de validación ejecutados

Todos se ejecutaron desde el repositorio Git anidado. No se ejecutó `commit`, `push`, `merge`, `rebase`, migración ni escritura SQL.

| Comando/acción | Resultado |
|---|---|
| `git status --short` (inicio) | Sin salida: árbol limpio |
| `git branch --show-current` | `Rama--Facu` |
| `git rev-parse --show-toplevel` | Confirmó el repositorio anidado |
| `git log --oneline -15` | HEAD `4da26d2`; historial inspeccionado |
| `dotnet sln Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln list` | Solo `Totaltech/Totaltech.csproj` |
| Descubrimiento `Tests/**/*.csproj` y `Tests/**/*.cs` | 0 proyectos, 0 fuentes |
| `dotnet build Totaltech/Totaltech.csproj --nologo` | EXIT 0; 0 warnings; 0 errores |
| `dotnet build Frontend/Frontend.csproj --nologo` | EXIT 0; 0 warnings; 0 errores |
| `dotnet test Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln --no-build --nologo` | EXIT 0; ningún test descubierto/ejecutado |
| `sqlcmd` con credenciales leídas en memoria y consultas exclusivamente `SELECT` | EXIT 0; esquema/datos resumidos en sección 12 |
| Inspección visual de páginas relevantes de ambos PDF y mockups PNG | Completada; sin modificación |
| `git diff --check` | EXIT 0; sin errores |
| `git status --short` (cierre) | Solo `?? ControlProyecto/ESTADO_ACTUAL_Y_ROADMAP_60.md` |

No se levantó la API ni el Frontend. Por lo tanto, este documento no afirma haber probado HTTP, navegación o responsive en runtime. El build y la consulta SQL se registran como evidencias separadas, sin convertirlos en pruebas funcionales.
