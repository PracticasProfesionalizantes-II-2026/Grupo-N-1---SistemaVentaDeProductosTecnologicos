# Matriz de trazabilidad de TotalTech

Revisión estática: 04/10/2026. Fuentes: documentos oficiales y archivos actuales del repositorio interno. No se ejecutó nuevamente la suite de pruebas del proyecto.

RF y RNF abrevían los números de las listas de requisitos de las páginas 8 y 9. No son identificadores nuevos. Los cinco escenarios seleccionados combinan capacidades del alcance actual.

| Capacidad | Requisito | Caso / escenario |
|---|---|---|
| Registro de clientes | RF5 | UC-01 |
| Autenticación y roles | RF5 | UC-02 |
| Catálogo y detalle | RF2 | UC-07, 08 y 09 |
| Carrito propio | RF3 | API actual |
| Productos, stock e imágenes | RF1 | UC-19 |

## Registro

**Actor:** Visitante. **Referencia:** RF5 / UC-01.

**Documentación:** [Documentación - Grupo 1 -TotalTech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación - Grupo 1 -TotalTech.pdf>) p. 8; [Casos de Uso- Totaltech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Casos de Uso- Totaltech.pdf>) p. 2; [Documentación de API.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación de API.pdf>) p. 7

**Contrato:** POST /auth/registro

**Métodos:** HomeController.Register; UsuariosLogica.RegistrarAsync / CrearAsync; UsuariosRepositorio.CrearAsync

| Capa | Archivo real |
|---|---|
| view | [Frontend/Views/Home/Register.cshtml](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Views/Home/Register.cshtml>) |
| mvc | [Frontend/Controllers/HomeController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/HomeController.cs>) |
| service | IHttpClientFactory.CreateClient("TotaltechApi") |
| endpointFile | [Totaltech/Endpoints/AuthEndpoints.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Endpoints/AuthEndpoints.cs>) |
| logic | [Totaltech/Logica/UsuariosLogica.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/UsuariosLogica.cs>) |
| repo | [Totaltech/Repositorios/UsuariosRepositorio.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Repositorios/UsuariosRepositorio.cs>) |
| testFile | [Tests/Totaltech.IntegrationTests/Endpoints/SeguridadEndpointsTests.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Tests/Totaltech.IntegrationTests/Endpoints/SeguridadEndpointsTests.cs>) |

**Regla:** La API fija el rol Cliente y almacena un hash de la contraseña.

**Resultado:** 201: usuario público. MVC redirige a Login. Email duplicado: 409.

**Pruebas existentes:** RegistroConRolAdministrador_FuerzaClienteYHasheaPassword. La existencia del test se verificó estáticamente; no equivale a una nueva ejecución aprobada.

**Recorrido de ida y vuelta:**

1. **Documento:** RF5, Usuarios; UC-01, Registrarse. Documentación general p. 8; casos de uso p. 2.
2. **Pantalla:** Register.cshtml RegisterViewModel reúne los campos. POST MVC y token antiforgery.
3. **Controlador:** HomeController.Register Valida ModelState, normaliza email y crea el cliente TotaltechApi.
4. **API:** POST /auth/registro AuthEndpoints convierte UsuarioRequest y llama RegistrarAsync.
5. **Persistencia:** UsuariosLogica → UsuariosRepositorio Rol Cliente, control de duplicado y hash. CrearAsync guarda mediante EF Core en SQL Server.
6. **Respuesta:** 201 / 409 → MVC Usuario público y redirección a Login; el conflicto de email vuelve como error de campo.

## Inicio de sesión

**Actor:** Usuario registrado. **Referencia:** RF5 / UC-02.

**Documentación:** [Documentación - Grupo 1 -TotalTech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación - Grupo 1 -TotalTech.pdf>) p. 8; [Casos de Uso- Totaltech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Casos de Uso- Totaltech.pdf>) p. 2; [Documentación de API.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación de API.pdf>) p. 5; [Documentación de API.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación de API.pdf>) p. 7

**Contrato:** POST /auth/login

**Métodos:** HomeController.Login; UsuariosLogica.LoginAsync; UsuariosRepositorio.ObtenerPorEmailAsync; JwtTokenService.Crear; ApiBearerTokenHandler.SendAsync

| Capa | Archivo real |
|---|---|
| view | [Frontend/Views/Home/Login.cshtml](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Views/Home/Login.cshtml>) |
| mvc | [Frontend/Controllers/HomeController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/HomeController.cs>) |
| service | IHttpClientFactory.CreateClient("TotaltechApi") |
| endpointFile | [Totaltech/Endpoints/AuthEndpoints.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Endpoints/AuthEndpoints.cs>) |
| logic | [Totaltech/Logica/UsuariosLogica.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/UsuariosLogica.cs>) |
| repo | [Totaltech/Repositorios/UsuariosRepositorio.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Repositorios/UsuariosRepositorio.cs>) |
| testFile | [Tests/Totaltech.IntegrationTests/Endpoints/SeguridadEndpointsTests.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Tests/Totaltech.IntegrationTests/Endpoints/SeguridadEndpointsTests.cs>) |

**Regla:** La cookie autentica MVC; el JWT autoriza llamadas a la API.

**Resultado:** 200: datos públicos, JWT y vencimiento. MVC crea cookie y redirige. Contraseña incorrecta: 401.

**Pruebas existentes:** LoginClienteCorrecto_DevuelveTokenRolClienteYSinPassword; LoginConPasswordIncorrecto_DevuelveUnauthorized. La existencia del test se verificó estáticamente; no equivale a una nueva ejecución aprobada.

**Recorrido de ida y vuelta:**

1. **Documento:** RF5, Usuarios; UC-02, Iniciar sesión. Documentación general p. 8; casos de uso p. 2.
2. **Pantalla:** Login.cshtml Email y contraseña viajan al controlador MVC.
3. **Controlador:** HomeController.Login IHttpClientFactory envía auth/login y recibe AuthUserResponse.
4. **API:** AuthEndpoints / UsuariosLogica.LoginAsync Consulta email, verifica el hash y JwtTokenService.Crear emite JWT.
5. **Persistencia:** UsuariosRepositorio.ObtenerPorEmailAsync Consulta el usuario en SQL Server. El token se genera después de validar las credenciales.
6. **Respuesta:** JSON → cookie MVC → Bearer StoreTokens conserva access_token en las propiedades de autenticación. SignInAsync crea la cookie. ApiBearerTokenHandler envía Bearer a la API.

## Catálogo y detalle

**Actor:** Visitante o cliente. **Referencia:** RF2 / UC-07, UC-08, UC-09.

**Documentación:** [Documentación - Grupo 1 -TotalTech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación - Grupo 1 -TotalTech.pdf>) p. 8; [Casos de Uso- Totaltech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Casos de Uso- Totaltech.pdf>) p. 2; [Documentación de API.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación de API.pdf>) p. 6; [Documentación de API.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación de API.pdf>) p. 7; [Documentación de API.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación de API.pdf>) p. 8

**Contrato:** GET /productos/catalogo; GET /productos/{id}

**Métodos:** ProductosController.Index / Detalle; ProductosApiService.ObtenerCatalogoAsync; ProductosLogica.ObtenerCatalogoAsync; ProductosRepositorio.ObtenerCatalogoAsync

| Capa | Archivo real |
|---|---|
| view | [Frontend/Views/Productos/Index.cshtml](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Views/Productos/Index.cshtml>) |
| view | [Frontend/Views/Productos/Detalle.cshtml](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Views/Productos/Detalle.cshtml>) |
| mvc | [Frontend/Controllers/ProductosController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/ProductosController.cs>) |
| service | [Frontend/Services/ProductosApiService.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Services/ProductosApiService.cs>) |
| endpointFile | [Totaltech/Endpoints/ProductosEndpoints.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Endpoints/ProductosEndpoints.cs>) |
| logic | [Totaltech/Logica/ProductosLogica.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/ProductosLogica.cs>) |
| repo | [Totaltech/Repositorios/ProductosRepositorio.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Repositorios/ProductosRepositorio.cs>) |
| testFile | [Tests/Totaltech.IntegrationTests/E2E/CatalogoE2ETests.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Tests/Totaltech.IntegrationTests/E2E/CatalogoE2ETests.cs>) |

**Regla:** Filtros por texto, categoría, precio y disponibilidad; orden estable y paginación en SQL.

**Resultado:** 200: página de productos y totales. El detalle inexistente devuelve 404.

**Pruebas existentes:** Catalogo_BuscaFiltraPaginaYAbreDetalle; Catalogo_CeroResultadosSePuedeLimpiarYProductoInexistenteEs404. La existencia del test se verificó estáticamente; no equivale a una nueva ejecución aprobada.

**Recorrido de ida y vuelta:**

1. **Documento:** RF2, Catálogo; UC-07/08/09. Documentación general p. 8; casos de uso p. 2. Marca y destacados no se incluyen en el alcance demostrado.
2. **Pantalla:** Productos/Index.cshtml y Detalle.cshtml La consulta GET conserva filtros en la URL; Ver producto abre el identificador.
3. **Controlador:** ProductosController.Index / Detalle Index consulta categorías y catálogo con Task.WhenAll; construye CatalogoViewModel.
4. **API:** ProductosApiService → ProductosEndpoints GET /productos/catalogo crea el filtro y llama ProductosLogica.ObtenerCatalogoAsync.
5. **Persistencia:** ProductosRepositorio.ObtenerCatalogoAsync AsNoTracking, Where, CountAsync, OrderBy, Skip, Take y Select. EF Core ejecuta la consulta en SQL Server.
6. **Respuesta:** CatalogoProductosResponse → Razor JSON con Items y totales. El cliente deserializa y MVC devuelve el HTML de las tarjetas o el detalle.

## Carrito propio

**Actor:** Cliente autenticado. **Referencia:** RF3 / API actual.

**Documentación:** [Documentación - Grupo 1 -TotalTech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación - Grupo 1 -TotalTech.pdf>) p. 8; [Casos de Uso- Totaltech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Casos de Uso- Totaltech.pdf>) p. 5; [Documentación de API.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación de API.pdf>) p. 9

**Contrato:** GET /carritos/actual; POST /carritos/actual/productos; PATCH y DELETE /carritos/actual/productos/{idProducto}

**Métodos:** CarritoController.Agregar / ActualizarCantidad / Eliminar; CarritosApiService.EnviarAsync; CarritosLogica.ModificarCarritoActivoAsync / CrearResumenAsync

| Capa | Archivo real |
|---|---|
| view | [Frontend/Views/Carrito/Index.cshtml](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Views/Carrito/Index.cshtml>) |
| view | [Frontend/Views/Productos/Detalle.cshtml](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Views/Productos/Detalle.cshtml>) |
| mvc | [Frontend/Controllers/CarritoController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/CarritoController.cs>) |
| service | [Frontend/Services/CarritosApiService.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Services/CarritosApiService.cs>) |
| endpointFile | [Totaltech/Endpoints/CarritosEndpoints.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Endpoints/CarritosEndpoints.cs>) |
| logic | [Totaltech/Logica/CarritosLogica.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/CarritosLogica.cs>) |
| repo | [Totaltech/Datos/TotaltechDbContext.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Datos/TotaltechDbContext.cs>) |
| testFile | [Tests/Totaltech.IntegrationTests/E2E/CatalogoE2ETests.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Tests/Totaltech.IntegrationTests/E2E/CatalogoE2ETests.cs>) |

**Regla:** El usuario se obtiene del JWT. Precio y total se calculan en el servidor. Agregar no reserva ni descuenta stock.

**Resultado:** 200: resumen actualizado. Stock insuficiente: 409. Cantidad inválida: 400.

**Pruebas existentes:** Carrito_DetalleAgregarModificarEliminarYEstadoVacio. La existencia del test se verificó estáticamente; no equivale a una nueva ejecución aprobada.

**Recorrido de ida y vuelta:**

1. **Documento:** RF3 y API actual, p. 9. El documento histórico de casos de uso p. 5 describe checkout sin carrito. No se le atribuye un UC nuevo.
2. **Pantalla:** Detalle.cshtml → Carrito/Index.cshtml Agregar, actualizar cantidad y eliminar mediante formularios MVC.
3. **Controlador:** CarritoController + CarritosApiService Authorize y antiforgery. EnviarAsync realiza POST, PATCH o DELETE con Bearer.
4. **API:** CarritosEndpoints Obtiene el idUsuario autenticado y llama las operaciones del carrito activo.
5. **Persistencia:** CarritosLogica → TotaltechDbContext Valida cantidad, stock y precio. Ciertas escrituras usan DbContext directamente y transacción serializable.
6. **Respuesta:** CrearResumenAsync → CarritoViewModel.Desde Devuelve items y totales. MVC redirige a Index y consulta el resumen actualizado. Sin carrito: resumen vacío.

## Administración de productos

**Actor:** Administrador. **Referencia:** RF1 / UC-19.

**Documentación:** [Documentación - Grupo 1 -TotalTech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación - Grupo 1 -TotalTech.pdf>) p. 8; [Casos de Uso- Totaltech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Casos de Uso- Totaltech.pdf>) p. 3; [Casos de Uso- Totaltech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Casos de Uso- Totaltech.pdf>) p. 5; [Documentación de API.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación de API.pdf>) p. 7; [Documentación de API.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación de API.pdf>) p. 8

**Contrato:** POST /productos; PUT /productos/{id}; DELETE /productos/{id}

**Métodos:** ProductosController.Crear / Editar / Eliminar; ProductoImagenStorage.GuardarAsync; ProductosLogica.CrearAsync / ActualizarAsync / EliminarAsync

| Capa | Archivo real |
|---|---|
| view | [Frontend/Views/Productos/Crear.cshtml](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Views/Productos/Crear.cshtml>) |
| view | [Frontend/Views/Productos/Editar.cshtml](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Views/Productos/Editar.cshtml>) |
| mvc | [Frontend/Controllers/ProductosController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/ProductosController.cs>) |
| service | [Frontend/Services/ProductosApiService.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Services/ProductosApiService.cs>) |
| service | [Frontend/Services/ProductoImagenStorage.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Services/ProductoImagenStorage.cs>) |
| endpointFile | [Totaltech/Endpoints/ProductosEndpoints.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Endpoints/ProductosEndpoints.cs>) |
| logic | [Totaltech/Logica/ProductosLogica.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/ProductosLogica.cs>) |
| repo | [Totaltech/Repositorios/ProductosRepositorio.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Repositorios/ProductosRepositorio.cs>) |
| testFile | [Tests/Totaltech.IntegrationTests/E2E/CatalogoAdministracionE2ETests.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Tests/Totaltech.IntegrationTests/E2E/CatalogoAdministracionE2ETests.cs>) |

**Regla:** Solo administrador. Archivo válido hasta 5 MB; SQL guarda su URL. El borrado actual es físico con control de relaciones.

**Resultado:** Creación 201; edición 200; eliminación sin relaciones 204, con relaciones 409.

**Pruebas existentes:** Imagen_SePrevisualizaReemplazaYConservaTrasRenombrarYReiniciar; Eliminar_ProductoAsociadoAUnCarritoSeConservaYMuestraElMotivo. La existencia del test se verificó estáticamente; no equivale a una nueva ejecución aprobada.

**Recorrido de ida y vuelta:**

1. **Documento:** RF1, Productos; UC-19, ABM. Requisitos p. 8; casos de uso pp. 3 y 5. La baja lógica prevista difiere del borrado actual.
2. **Pantalla:** Productos/Crear.cshtml y Editar.cshtml Administrador completa precio, stock, categoría y proveedor. Editar admite archivo de imagen.
3. **Controlador:** ProductosController + ProductoImagenStorage Validación MVC y CSRF. GuardarAsync comprueba extensión, MIME, firma y tamaño, y guarda el archivo local.
4. **API:** ProductosApiService → ProductosEndpoints POST crea, PUT actualiza, DELETE elimina. Endpoint y lógica comprueban permisos y relaciones.
5. **Persistencia:** ProductosLogica → ProductosRepositorio EF Core guarda el producto y la URL de imagen en SQL Server. El archivo queda en wwwroot/uploads/productos.
6. **Respuesta:** 201 / 200 / 204 / 409 → mensaje MVC Vuelve al catálogo con confirmación. Sin nueva imagen se conserva la anterior. Las relaciones bloquean la eliminación.

## Requerimientos no funcionales

[Documentación - Grupo 1 -TotalTech.pdf](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación - Grupo 1 -TotalTech.pdf>) p. 9

| Requisito | Implementación | Evidencia / alcance |
|---|---|---|
| Seguridad · RNF4 | Hash, JWT, roles y CSRF | Controles en código |
| Rendimiento · RNF1 | Paginación y proyección | Medición local histórica |
| Usabilidad · RNF2 | Validación y mensajes | Pantallas y pruebas |
| Compatibilidad · RNF3 | Seis anchos de pantalla | Chromium |
| Mantenibilidad · RNF5 | Responsabilidades y DI | Código y pruebas |

| Tema | Archivo | Referencia |
|---|---|---|
| Seguridad | [Frontend/Program.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Program.cs>) | cookie HttpOnly y configuración MVC |
| Seguridad | [Frontend/Services/ApiBearerTokenHandler.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Services/ApiBearerTokenHandler.cs>) | SendAsync |
| Seguridad | [Totaltech/Seguridad/JwtTokenService.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Seguridad/JwtTokenService.cs>) | Crear |
| Rendimiento | [Tests/Totaltech.IntegrationTests/E2E/CatalogoE2ETests.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Tests/Totaltech.IntegrationTests/E2E/CatalogoE2ETests.cs>) | CatalogoYDetalle_TresCargasQuedanBajoTresSegundos |
| Compatibilidad | [Tests/Totaltech.IntegrationTests/E2E/CatalogoAdministracionE2ETests.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Tests/Totaltech.IntegrationTests/E2E/CatalogoAdministracionE2ETests.cs>) | Administracion_CatalogoYEdicionSonUsablesEnSeisAnchosYConTeclado |
| Mantenibilidad | [Totaltech/Program.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Program.cs>) | registro de dependencias |
| Pendiente | [Frontend/Controllers/CheckoutController.cs](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/CheckoutController.cs>) | controlador reservado |
| Pendiente | [Frontend/wwwroot/js/contacto.js](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/wwwroot/js/contacto.js>) | confirmación de formulario sin envío a API |
| Pendiente | [Frontend/wwwroot/js/garantia.js](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/wwwroot/js/garantia.js>) | confirmación de formulario sin envío a API |

## Diferencias y límites

- Casos de uso, p. 5: checkout directo sin carrito. RF3 y la API actual documentan el carrito. Se explica la evolución sin inventar un UC para él.
- UC-19 y la nota de ABM prevén baja lógica y auditoría. ProductosRepositorio.EliminarAsync ejecuta Remove, sujeto a restricciones de relaciones en la base y manejo de DbUpdateException en el endpoint.
- UC-07/08 incluyen destacados y marca, que no forman parte del catálogo demostrado.
- Seguridad: el hash no es cifrado reversible. Los controles citados no certifican protección integral de datos ni cumplimiento normativo.
- Compatibilidad: seis anchos en Chromium. No hay una nueva validación de Firefox o Edge.
- Rendimiento: la prueba de tres cargas locales no demuestra carga concurrente de producción.
- Las operaciones backend de pedido/pago no equivalen a checkout MVC terminado.

## Procedencia de las 105 pruebas

El reporte de la preparación anterior del 01/10/2026 figura en [output/presentacion/LEEME.md](<C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/output/presentacion/LEEME.md>) y en su guion: 8 unitarias + 78 de integración y frontend + 19 E2E = 105. La fecha identifica ese material, no una fecha reconstruida del runner. Se cita como resultado histórico comunicado y no como conteo verificado nuevamente. Los archivos de tests se verificaron para localizar escenarios concretos. Esta tarea no vuelve a ejecutar la suite.
