# TotalTech · Guion de 20 minutos

Facundo: 10:00. Daiana: 10:00. Navegación manual. Las demostraciones forman parte del tiempo asignado.

La duración de 20:00 es el presupuesto y el límite del temporizador. La duración oral necesita ensayo conjunto. Los párrafos son apuntes para desarrollar, no texto que deba leerse literalmente a velocidad fija.

## Preparación de las demostraciones

Usar una base LocalDB independiente con nombre de prueba y datos ficticios. No conectar la demo a la base real. Preparar una sesión Cliente y otra Admin, un email de registro aún no usado, un producto con stock y otro sin relaciones para CU05. Mantener categorías y un proveedor activo. Abrir los archivos enumerados en las notas. Preparar una imagen PNG/JPG/WebP menor de 5 MB. Las credenciales deben permanecer fuera de las diapositivas.

El acceso del navegador al entorno local fue denegado durante esta preparación. No se obtuvieron nuevas capturas de registro, login o carrito. Los cuatro PNG de respaldo son capturas reales de la ejecución anterior, con datos E2E. Se incluyen completas, sin reconstruir pantallas. El material sigue permitiendo explicar los cinco recorridos, pero esos tres respaldos visuales quedan pendientes de captura autorizada.

## Cronograma

| Diapositiva | Tema | Expositor | Intervalo | Duración |
|---|---|---|---|---|
| 1 | TotalTech | Facundo | 0:00–0:30 | 0:30 |
| 2 | Problema y alcance actual | Daiana | 0:30–1:00 | 0:30 |
| 3 | Documentación y trazabilidad | Facundo | 1:00–2:00 | 1:00 |
| 4 | Cinco requerimientos funcionales | Daiana | 2:00–2:45 | 0:45 |
| 5 | Cinco requerimientos no funcionales | Facundo | 2:45–4:15 | 1:30 |
| 6 | Cinco casos de uso de la demostración | Daiana | 4:15–5:00 | 0:45 |
| 7 | Recorrido de una solicitud | Facundo | 5:00–6:30 | 1:30 |
| 8 | CU01 · Registro de clientes | Daiana | 6:30–8:00 | 1:30 |
| 9 | CU02 · Login, cookie y JWT | Facundo | 8:00–9:30 | 1:30 |
| 10 | CU03 · Catálogo y detalle | Daiana | 9:30–12:00 | 2:30 |
| 11 | CU04 · Carrito propio | Facundo | 12:00–14:30 | 2:30 |
| 12 | CU05 · Administración de productos | Daiana | 14:30–17:00 | 2:30 |
| 13 | Pruebas y tratamiento de errores | Facundo | 17:00–18:30 | 1:30 |
| 14 | Funciones logradas y pendientes | Daiana | 18:30–20:00 | 1:30 |

## Intervenciones y pasos

### 1. TotalTech · Facundo · 0:00–0:30

0:00–0:30. Facundo. Presentar a Facundo y Daiana. Hoy mostramos cómo un requisito llega a una pantalla y cómo esa acción recorre el código hasta SQL Server. Usaremos cinco casos completos del estado actual del proyecto. En veinte minutos alternamos documentación, implementación y demostración. No enumerar todas las entidades. Ceder la palabra a Daiana al llegar a 0:30.

Archivos para abrir:

- [README.md](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/README.md>)

### 2. Problema y alcance actual · Daiana · 0:30–1:00

0:30–1:00. Daiana. Explicar el problema: una tienda necesita mostrar su oferta y permitir que cada cliente arme su carrito, mientras el administrador mantiene la información. El alcance actual que defendemos son cinco flujos verificables. La documentación original incluye objetivos más amplios como pagos, pedidos y soporte. Tener un documento o una vista reservada no significa que el flujo MVC esté terminado. El cierre delimita esos pendientes. Transición: Facundo explica cómo elegimos evidencia.

Archivos para abrir:

- [Documentacion/Documentación - Grupo 1 -TotalTech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación - Grupo 1 -TotalTech.pdf>)
- [Documentacion/API.md](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/API.md>)

### 3. Documentación y trazabilidad · Facundo · 1:00–2:00

1:00–2:00. Facundo. Primero leímos Documentación - Grupo 1 -TotalTech.pdf, requisitos funcionales en página 8 y no funcionales en página 9, y Casos de Uso- Totaltech.pdf. Después contrastamos los objetivos con la documentación de API actualizada y el código de esta versión. La documentación original describe más funciones que las demostrables hoy. RF01 a RF05 y CU01 a CU05 son identificadores de selección para esta presentación, no una renumeración del documento histórico. Para cada caso seguimos pantalla, contrato HTTP, controlador, lógica, persistencia y respuesta. El ejemplo es gestión de usuarios: en la interfaz elegimos registro, enviamos POST /auth/registro y encontramos UsuariosLogica.RegistrarAsync. El resultado observable es la cuenta creada y la redirección a login. La evidencia de pruebas pertenece a una ejecución anterior. Una captura acredita una pantalla y un estado, no todos los requisitos de seguridad. Ceder a Daiana.

Archivos para abrir:

- [Documentacion/Documentación - Grupo 1 -TotalTech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación - Grupo 1 -TotalTech.pdf>)
- [Documentacion/Casos de Uso- Totaltech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Casos de Uso- Totaltech.pdf>)
- [Documentacion/API.md](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/API.md>)

### 4. Cinco requerimientos funcionales · Daiana · 2:00–2:45

2:00–2:45. Daiana. Leer el listado agrupando las acciones. RF01 permite crear un cliente. RF02 permite iniciar sesión y distinguir permisos. RF03 permite buscar, filtrar, paginar y ver un producto. RF04 permite modificar solamente el carrito propio. RF05 permite administrar productos, su stock y su imagen. Los cinco aparecen implementados en las rutas y pantallas que vamos a recorrer. En la demostración no incluimos pago ni compra completa. Conectar cada número con CU01 a CU05. Evitar entrar todavía en todos los archivos.

Archivos para abrir:

- [Documentacion/API.md](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/API.md>)

### 5. Cinco requerimientos no funcionales · Facundo · 2:45–4:15

2:45–4:15. Facundo. Dedicar unos 18 segundos a cada requisito. Seguridad: PasswordHasher guarda un hash, no un cifrado reversible. El frontend usa cookie HttpOnly y los servicios HTTP agregan el JWT. La API valida permisos y propiedad. Los formularios MVC de escritura validan antiforgery. Rendimiento: el catálogo aplica filtros y proyecta DTO con AsNoTracking, Count, Skip y Take. La prueba histórica midió cargas locales repetidas de catálogo y detalle bajo tres segundos. Es una prueba controlada de carga de páginas, no una certificación de capacidad concurrente en producción. Usabilidad: errores por campo, mensajes de éxito, carrito vacío y confirmación de eliminar. Compatibilidad: los anchos verificados fueron 320, 375, 425, 768, 1024 y 1280. Las pruebas automatizadas usaron Chromium. No asegurar validación en todos los navegadores. Mantenibilidad: controladores coordinan, servicios HTTP llaman contratos, lógica aplica reglas y repositorios acceden a datos, con la excepción real de operaciones activas del carrito que usan DbContext. La inyección conecta estas dependencias. El soporte son pruebas unitarias, de integración y E2E de la ejecución anterior.

Archivos para abrir:

- [Frontend/Program.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Program.cs>)
- [Totaltech/Logica/UsuariosLogica.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/UsuariosLogica.cs>)
- [Tests/Totaltech.IntegrationTests/E2E/CatalogoE2ETests.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Tests/Totaltech.IntegrationTests/E2E/CatalogoE2ETests.cs>)

### 6. Cinco casos de uso de la demostración · Daiana · 4:15–5:00

4:15–5:00. Daiana. Enumerar CU01 registro, CU02 acceso, CU03 catálogo y detalle, CU04 carrito y CU05 administración de un producto sin relaciones. Aclarar actores: visitante para registro, usuario para login, visitante o cliente para catálogo, cliente autenticado para carrito y administrador para productos. Cada demostración está incluida en el tiempo de su diapositiva. Las capturas reales disponibles de catálogo y edición permiten respaldar esos estados. Registro, login y carrito completan el recorrido en la demostración en vivo. Presentar la arquitectura antes de comenzar.

Archivos para abrir:

- [Documentacion/Casos de Uso- Totaltech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Casos de Uso- Totaltech.pdf>)

### 7. Recorrido de una solicitud · Facundo · 5:00–6:30

5:00–6:30. Facundo. Seguir el diagrama en el orden de ida. El navegador presenta un formulario Razor. El controlador MVC recibe y valida los datos. El cliente HTTP llama a la API, el endpoint interpreta el contrato y la identidad, la lógica aplica las reglas y la persistencia llega a SQL Server. Seguir el regreso: el endpoint devuelve JSON, el cliente lo deserializa, MVC construye el modelo de pantalla y devuelve la vista o una redirección. El navegador recibe HTML, no consulta SQL directamente. Program.cs configura las dependencias. En registro y login HomeController usa directamente IHttpClientFactory.CreateClient, aunque el proyecto también tenga un archivo AuthApiService. En catálogo y carrito existen servicios específicos. En la lógica de carrito activo algunos accesos pasan directamente por TotaltechDbContext. No dibujar un repositorio ficticio para uniformar la arquitectura. Cookie MVC y JWT de la API tienen responsabilidades diferentes. Abrir el recurso interactivo preparado para recorrer cada caso manualmente.

Archivos para abrir:

- [Frontend/Program.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Program.cs>)
- [Totaltech/Program.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Program.cs>)
- [Frontend/Controllers/HomeController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/HomeController.cs>)
- [Totaltech/Logica/CarritosLogica.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/CarritosLogica.cs>)

### 8. CU01 · Registro de clientes · Daiana · 6:30–8:00

6:30–8:00. Daiana. DEMO 30 segundos. Abrir /Home/Register con datos ficticios preparados, mostrar Nombre, Apellido, Email, Teléfono y contraseña confirmada. Enviar Crear mi cuenta y mostrar la redirección a login. Si la demo no está disponible, describir el resultado esperado sin fingir una captura. CÓDIGO 60 segundos. Register.cshtml enlaza RegisterViewModel. ModelState controla errores, HomeController normaliza email y valida antiforgery. Crear el cliente TotaltechApi mediante IHttpClientFactory y enviar POST /auth/registro. AuthEndpoints convierte UsuarioRequest y llama RegistrarAsync. UsuariosLogica fija rol Cliente y delega en CrearAsync, que verifica duplicado, valida contraseña y usa PasswordHasher.HashPassword. UsuariosRepositorio persiste con SaveChangesAsync. La API devuelve 201 con el usuario público, nunca el hash. MVC redirige a Login con mensaje. Un email duplicado responde 409 y se muestra en el campo. Evidencia histórica en SeguridadEndpointsTests.RegistroConRolAdministrador_FuerzaClienteYHasheaPassword y RespuestasDeRegistroLoginYUsuario_NoExponenDatosSensibles. No seleccionar Admin en la solicitud pública.

Archivos para abrir:

- [Frontend/Views/Home/Register.cshtml](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Views/Home/Register.cshtml>)
- [Frontend/Controllers/HomeController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/HomeController.cs>)
- [Totaltech/Endpoints/AuthEndpoints.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Endpoints/AuthEndpoints.cs>)
- [Totaltech/Logica/UsuariosLogica.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/UsuariosLogica.cs>)
- [Totaltech/Repositorios/UsuariosRepositorio.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Repositorios/UsuariosRepositorio.cs>)

### 9. CU02 · Login, cookie y JWT · Facundo · 8:00–9:30

8:00–9:30. Facundo. DEMO 30 segundos. Entrar con el cliente recién registrado y mostrar sesión iniciada. El navegador de administración debe quedar preparado aparte. No mostrar el token ni contraseñas en la exposición. CÓDIGO 60 segundos. Login.cshtml envía a HomeController.Login. El controlador llama POST /auth/login usando IHttpClientFactory. AuthEndpoints consulta existencia de email y UsuariosLogica.LoginAsync verifica el hash. JwtTokenService.Crear firma el token con identidad y rol. El JSON contiene datos públicos, AccessToken y ExpiresAtUtc. MVC construye claims y convierte el rol Administrador de la API al texto Admin de la cookie. Guarda access_token en AuthenticationProperties y llama HttpContext.SignInAsync. En llamadas posteriores ApiBearerTokenHandler.SendAsync lo obtiene y lo coloca en Authorization Bearer. La cookie protege rutas MVC y el JWT protege endpoints de la API. Email inexistente produce 404 y redirige a registro. Contraseña equivocada produce 401 y mensaje en el formulario. Pruebas históricas cubren credenciales, roles y respuestas.

Archivos para abrir:

- [Frontend/Views/Home/Login.cshtml](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Views/Home/Login.cshtml>)
- [Frontend/Controllers/HomeController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/HomeController.cs>)
- [Frontend/Services/ApiBearerTokenHandler.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Services/ApiBearerTokenHandler.cs>)
- [Totaltech/Seguridad/JwtTokenService.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Seguridad/JwtTokenService.cs>)

### 10. CU03 · Catálogo y detalle · Daiana · 9:30–12:00

9:30–12:00. Daiana. DEMO 60 segundos. Abrir /Productos. Buscar notebook, activar Sólo disponibles y aplicar. Elegir categoría y precio si el conjunto lo permite. Mostrar el contador, avanzar de página con filtros y abrir Ver producto. Mostrar nombre, precio, stock y regreso al catálogo. Respaldo: capturas/catalogo-1280.png y catalogo-320.png son capturas reales de la ejecución anterior. CÓDIGO 90 segundos. Index.cshtml envía filtros por query string a ProductosController.Index. El controlador solicita categorías y catálogo en paralelo con Task.WhenAll. ProductosApiService.ObtenerCatalogoAsync construye GET /productos/catalogo. ProductosEndpoints crea FiltroCatalogoProductos. ProductosLogica valida límites. ProductosRepositorio usa AsNoTracking, Where por texto/categoría/precio/stock, CountAsync, orden estable Nombre e IdProducto, Skip, Take y Select a ProductoCatalogoResponse. SQL retorna solamente la página y los campos proyectados. JSON agrega Items, Pagina, TamanoPagina, TotalItems y TotalPaginas. El servicio deserializa, MVC llena CatalogoViewModel y Razor dibuja las tarjetas. Detalle llama GET /productos/{id}. Producto inexistente retorna 404, consulta vacía tiene mensaje y limpiar filtros. La imagen del catálogo puede resolverse mediante ProductoImagenResolver. Evidencia histórica: CatalogoE2ETests.Catalogo_BuscaFiltraPaginaYAbreDetalle y pruebas de catálogo por endpoint.

Archivos para abrir:

- [Frontend/Controllers/ProductosController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/ProductosController.cs>)
- [Frontend/Services/ProductosApiService.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Services/ProductosApiService.cs>)
- [Totaltech/Endpoints/ProductosEndpoints.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Endpoints/ProductosEndpoints.cs>)
- [Totaltech/Repositorios/ProductosRepositorio.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Repositorios/ProductosRepositorio.cs>)
- [Tests/Totaltech.IntegrationTests/E2E/CatalogoE2ETests.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Tests/Totaltech.IntegrationTests/E2E/CatalogoE2ETests.cs>)

### 11. CU04 · Carrito propio · Facundo · 12:00–14:30

12:00–14:30. Facundo. DEMO 60 segundos. En sesión Cliente, abrir detalle de un producto con stock, agregar una unidad, mostrar badge y resumen. Cambiar cantidad a dos y actualizar. Mostrar subtotal y total. Eliminar y mostrar Tu carrito está vacío. CÓDIGO 90 segundos. Detalle.cshtml envía POST MVC a CarritoController.Agregar, protegido por Authorize y ValidateAntiForgeryToken. CarritosApiService convierte las acciones a POST /carritos/actual/productos, PATCH y DELETE con idProducto. ApiBearerTokenHandler agrega JWT. CarritosEndpoints toma el idUsuario del claim, no del formulario. CarritosLogica usa operaciones de carrito activo: AgregarProductoActivoAsync y ActualizarCantidadActivaAsync llaman ModificarCarritoActivoAsync. La escritura relacional usa una transacción serializable y ciertos accesos directos a TotaltechDbContext. El servidor verifica cantidad positiva y stock y toma precio desde Producto, sin confiar en un precio enviado por el cliente. Agregar el mismo producto consolida cantidades. No decir que el carrito reserva o descuenta stock. CrearResumenAsync proyecta items y calcula totales. CarritoResumenResponse vuelve al servicio, CarritoViewModel.Desde arma la pantalla y el controlador redirige a Index con mensajes. La ausencia de carrito produce resumen vacío. Evidencia histórica: Carrito_DetalleAgregarModificarEliminarYEstadoVacio y pruebas de propiedad, cantidades y stock. 

Archivos para abrir:

- [Frontend/Controllers/CarritoController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/CarritoController.cs>)
- [Frontend/Services/CarritosApiService.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Services/CarritosApiService.cs>)
- [Totaltech/Endpoints/CarritosEndpoints.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Endpoints/CarritosEndpoints.cs>)
- [Totaltech/Logica/CarritosLogica.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/CarritosLogica.cs>)

### 12. CU05 · Administración de productos · Daiana · 14:30–17:00

14:30–17:00. Daiana. DEMO 70 segundos. Usar una sesión Admin preparada y un producto ficticio sin carrito, pedido ni compra asociados. Abrir Crear, ingresar nombre Notebook exposición TotalTech, descripción, precio 125000, stock 5, categoría y proveedor activo. Guardar y buscarlo. Abrir Editar, pasar stock a 7, elegir una imagen PNG real de prueba y guardar. Verificar nombre, stock e imagen. Pulsar Eliminar y aceptar confirmación. Verificar que desapareció. No eliminar productos de datos reales. Respaldo disponible: editar-1280.png y editar-320.png, capturas reales anteriores de la edición. CÓDIGO 80 segundos. ProductosController.Crear valida ModelState y llama ProductosApiService.CrearAsync, que envía POST /productos. Endpoint y lógica comprueban permisos Admin, precios/stock y relaciones válidas. Editar recibe ProductoEdicionViewModel y archivo multipart. ProductoImagenStorage.GuardarAsync verifica extensión, MIME, firma y tamaño hasta 5 MB, genera nombre y guarda en wwwroot/uploads/productos. Envía la URL junto con datos a PUT /productos/{id}. SQL guarda esa URL, no los bytes de la imagen. Si no se elige archivo nuevo, se conserva la imagen. Eliminar usa POST MVC con antiforgery y DELETE API. ProductosLogica verifica relaciones y retorna conflicto 409 o éxito 204. El controlador muestra mensaje y vuelve a una URL local. Evidencia histórica: CatalogoAdministracionE2ETests cubre edición, imagen, roles y eliminación.

Archivos para abrir:

- [Frontend/Controllers/ProductosController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/ProductosController.cs>)
- [Frontend/Services/ProductoImagenStorage.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Services/ProductoImagenStorage.cs>)
- [Totaltech/Logica/ProductosLogica.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/ProductosLogica.cs>)
- [Tests/Totaltech.IntegrationTests/E2E/CatalogoAdministracionE2ETests.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Tests/Totaltech.IntegrationTests/E2E/CatalogoAdministracionE2ETests.cs>)

### 13. Pruebas y tratamiento de errores · Facundo · 17:00–18:30

17:00–18:30. Facundo. Explicar 105 aprobadas en la ejecución anterior: ocho unitarias, setenta y ocho de integración y frontend y diecinueve E2E. Esta presentación no declara que se hayan ejecutado nuevamente las 105. Las unitarias apoyan reglas de negocio, integración apoya endpoints y contratos y E2E verifica el recorrido de pantalla. Mostrar ejemplos de respuestas: 400 validación, 401 autenticación, 403 permisos, 404 inexistente y 409 conflicto. Relacionar duplicado de email con campo Email, stock insuficiente con mensaje en carrito y producto con relaciones con rechazo de eliminación. Las pruebas de compatibilidad usan seis anchos y Chromium. Las cargas bajo tres segundos corresponden a entorno local, sin promesa de producción. ModelState y TempData convierten errores en indicaciones al usuario, con manejo de timeout y servicio inaccesible. La captura de pantalla sirve para continuar la explicación si falla la demo. La evidencia verificable son archivos de pruebas y registros de aquella ejecución, no una métrica nueva inventada.

Archivos para abrir:

- [Tests/Totaltech.UnitTests](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Tests/Totaltech.UnitTests>)
- [Tests/Totaltech.IntegrationTests](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Tests/Totaltech.IntegrationTests>)

### 14. Funciones logradas y pendientes · Daiana · 18:30–20:00

18:30–20:00. Daiana. Dedicar 30 segundos a lo logrado: cliente que se registra e inicia sesión, catálogo consultable y carrito propio, administrador que mantiene productos y stock e imagen. Los cinco requisitos se trazaron hasta el código y el resultado. Dedicar 40 segundos a pendientes. Existen operaciones de confirmación de carrito y entidades de pedidos/pagos en la API, pero las pantallas y controladores reservados de checkout MVC no completan ese recorrido. RecuperarContrasenaAsync consulta existencia pero no completa envío y restablecimiento. Contacto y garantía muestran confirmaciones simuladas sin envío a la API. Estos puntos delimitan honestamente el trabajo que falta. Dedicar los últimos 20 segundos al cierre: aprendimos a validar lo que documentamos con contratos, reglas y pruebas. La separación de responsabilidades permite seguir la solicitud de ida y vuelta y corregir cada parte. Agradecer al llegar a 20:00. Las preguntas posteriores quedan fuera de los veinte minutos.

Archivos para abrir:

- [Frontend/Controllers/CheckoutController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/CheckoutController.cs>)
- [Frontend/Controllers/PedidosController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/PedidosController.cs>)
- [Totaltech/Logica/UsuariosLogica.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/UsuariosLogica.cs>)
- [Frontend/wwwroot/js/contacto.js](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/wwwroot/js/contacto.js>)
- [Frontend/wwwroot/js/garantia.js](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/wwwroot/js/garantia.js>)

## Ensayo cronometrado

Abrir Recorrido-TotalTech.html, pulsar Iniciar y navegar manualmente por las 14 diapositivas según los intervalos de la tabla. Facundo cede a Daiana y viceversa en cada cambio. El temporizador avisa el bloque actual y el final a 20:00. Cronometrar especialmente los cinco bloques de demo. Si una operación demora, continuar con el resultado esperado o la captura disponible al agotarse su ventana. Las preguntas se atienden después del cierre.

La comprobación técnica suma 1200 segundos y 600 por expositor. No reemplaza un ensayo oral de Facundo y Daiana. Registrar desvíos y acortar ejemplos dentro de su bloque, sin redistribuir minutos entre integrantes.

## Capturas de respaldo

[Catálogo en 1280 px](capturas/catalogo-1280.png), [catálogo en 320 px](capturas/catalogo-320.png), [edición en 1280 px](capturas/editar-1280.png) y [edición en 320 px](capturas/editar-320.png). Origen: Tests/Totaltech.IntegrationTests/TestResults/catalogo-admin. Pertenecen a una ejecución anterior. No prueban por sí solas creación, eliminación, registro, login ni carrito. Capturar esos estados cuando se autorice el navegador local.
