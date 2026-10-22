# TotalTech: contenido de la presentación

**Integrantes:** Daiana Chinellato y Facundo Sola.

**Duración prevista:** 23:00, con demostración web incluida y preguntas posteriores.

**Cantidad:** 15 diapositivas.

**Fecha de preparación:** 05/10/2026.

Este documento contiene el texto para las diapositivas, las notas del expositor y los pasos de demostración. Los tiempos son una distribución para el ensayo, no una exposición cronometrada ya realizada. El archivo de PowerPoint y la grabación de respaldo quedan pendientes de producción.

## Idea central y forma de exposición

> TotalTech permite que el cliente encuentre y organice su selección de productos, mientras el comercio mantiene su catálogo desde una administración centralizada.

Presentar primero el beneficio para el comercio o el cliente. Después mostrar la acción, su resultado y la evidencia que la sostiene. El recorrido es: problema del negocio, requisito documentado, caso de uso, implementación y resultado en la página.

En cada diapositiva copiar únicamente el apartado **Texto en pantalla**. Las notas, pasos de demo y referencias son apoyo del expositor. Las notas son orientativas: parte de la explicación acompaña las acciones de demostración y no se lee íntegramente antes de operar. Usar capturas del sistema como evidencia, conservar los identificadores documentales y mostrar fragmentos breves de código. Los listados están en las diapositivas y las acciones se demuestran en el sitio real.

## Cronograma

| Diapositiva | Tiempo | Inicio | Fin |
|---|---:|---:|---:|
| 01. Presentación de TotalTech | 0:45 | 00:00 | 00:45 |
| 02. Problema del comercio | 1:15 | 00:45 | 02:00 |
| 03. Cliente y administrador | 1:00 | 02:00 | 03:00 |
| 04. Documentación e implementación | 1:30 | 03:00 | 04:30 |
| 05. Cinco requerimientos funcionales | 1:00 | 04:30 | 05:30 |
| 06. Cinco casos de uso | 0:45 | 05:30 | 06:15 |
| 07. Registro | 1:45 | 06:15 | 08:00 |
| 08. Acceso y responsabilidades | 1:45 | 08:00 | 09:45 |
| 09. Búsqueda y filtros | 2:15 | 09:45 | 12:00 |
| 10. Detalle y carrito | 2:30 | 12:00 | 14:30 |
| 11. Administración de productos | 3:00 | 14:30 | 17:30 |
| 12. Cinco criterios de calidad | 2:00 | 17:30 | 19:30 |
| 13. Recorrido del código | 1:00 | 19:30 | 20:30 |
| 14. Versión actual y próxima etapa | 1:15 | 20:30 | 21:45 |
| 15. Propuesta comercial | 1:15 | 21:45 | 23:00 |

Los tiempos de las diapositivas 7 a 11 incluyen explicar, cambiar de ventana y operar la web. En la 12 se incluye una breve vista móvil. Las preguntas comienzan después del cierre.

## Diapositiva 01: TotalTech: tu catálogo y tus clientes en un mismo sistema

**Tiempo:** 0:45. **Tramo:** 00:00 a 00:45.

### Texto en pantalla

**TotalTech**

Tu catálogo y tus clientes en un mismo sistema

Una plataforma para presentar productos tecnológicos y gestionar la oferta del comercio

Daiana Chinellato y Facundo Sola

### Notas del expositor

“TotalTech conecta dos necesidades de un comercio tecnológico: presentar sus productos al cliente y mantener esa oferta desde la administración. El cliente puede crear su cuenta, acceder, buscar productos y organizar su carrito. El comercio puede gestionar productos, precios, stock e imágenes. Hoy vamos a mostrar estas capacidades en la página y explicar cómo nacieron de la documentación hasta convertirse en funciones del sistema. La propuesta es que la información que administra el comercio acompañe la experiencia que recibe el cliente.”

**Apoyo:** portada sencilla con la identidad de TotalTech. La pantalla inicial del sitio puede acompañar la apertura.

**Transición:** “Para entender el valor de esta propuesta, empecemos por el problema del comercio”.

## Diapositiva 02: El problema que enfrenta el comercio

**Tiempo:** 1:15. **Tramo:** 00:45 a 02:00.

### Texto en pantalla

- Información dispersa.
- Actualización manual de productos.
- Dificultades para comunicar precio y disponibilidad.

**Necesidad:** reunir el catálogo y su administración en un mismo sistema.

### Notas del expositor

“El diagnóstico del proyecto describe operaciones manuales y herramientas separadas. Pensemos en una tienda que mantiene precios en un lugar y disponibilidad en otro: cuando un cliente pregunta por una notebook, la persona que atiende necesita reunir esa información antes de responder. Si los datos no coinciden, la atención se vuelve más difícil. TotalTech toma ese problema como punto de partida y propone un catálogo que el comercio pueda mantener desde la administración. La intención es facilitar la consulta y actualización de la información. La demostración mostrará las funciones que hoy permiten trabajar con esa propuesta.”

**Apoyo:** señalar la situación actual en la documentación del proyecto, sección 1.1, página 3. Hablar del diagnóstico documentado, sin porcentajes de ahorro, ventas o reducción de errores.

**Transición:** “Ese problema tiene dos protagonistas: quien busca un producto y quien mantiene la información”.

## Diapositiva 03: Una experiencia para el cliente y control para el comercio

**Tiempo:** 1:00. **Tramo:** 02:00 a 03:00.

### Texto en pantalla

**Cliente**

Encuentra productos, consulta información y organiza su carrito.

**Administrador**

Publica y actualiza productos, precios, imágenes y stock.

**Una misma información de catálogo para ambos recorridos.**

### Notas del expositor

“Seguiremos una situación sencilla: Ana quiere encontrar una notebook para trabajar, conocer su precio y disponibilidad, y agregarla a su carrito. Del otro lado, el administrador necesita que los productos tengan información correcta y una imagen adecuada. Ambos recorridos se encuentran en el catálogo. Lo que administra el comercio es lo que consulta el cliente. Usaremos una notebook como hilo conductor para la búsqueda y el carrito. Para mostrar la creación y eliminación desde administración utilizaremos otro producto, preparado exclusivamente para esa demostración.”

**Apoyo:** señalar el catálogo público y una captura de la edición administrativa. La notebook prevista es Lenovo Yoga 7 2 en 1.

**Transición:** “Ahora veremos cómo estas necesidades se transformaron en decisiones documentadas y código”.

## Diapositiva 04: El recorrido de la documentación al sistema

**Tiempo:** 1:30. **Tramo:** 03:00 a 04:30.

### Texto en pantalla

**Ejemplo: encontrar una notebook disponible dentro de un presupuesto**

| Etapa | Evidencia |
|---|---|
| Necesidad | Encontrar un producto adecuado. |
| Requerimiento | Catálogo: búsqueda y filtros. |
| Caso de uso | UC-08: Filtrar y buscar productos. |
| Implementación | Consulta del catálogo y validación de filtros. |
| Demostración | Productos que cumplen los criterios seleccionados. |

### Notas del expositor

“El punto de partida fue definir el problema, los usuarios y los requisitos. En el grupo de catálogo se documentaron la búsqueda y los filtros. UC-08 describe la acción de buscar y filtrar productos. La API vigente define la consulta que recibe esos criterios, y el código implementa la pantalla y la consulta de datos. El resultado se observa cuando la página muestra los productos que cumplen la búsqueda. Ese vínculo permite seguir una decisión desde el documento hasta el comportamiento. También permite identificar diferencias entre el alcance previsto y la versión construida. En esta exposición mostraremos búsqueda por texto, categoría, precio y disponibilidad.”

### Evidencia que se muestra

1. Documentación general, sección 5.1, grupo 2, página 8.
2. Casos de uso, UC-08, página 2.
3. API vigente, catálogo, páginas 6 a 8: `GET /productos/catalogo`.
4. Método `ProductosController.Index` y resultado del catálogo en la web.

**Apoyo:** mostrar el extracto pertinente de cada fuente, no páginas completas con texto pequeño. La búsqueda operativa se desarrolla en la diapositiva 9.

**Transición:** “Esa trazabilidad nos permite elegir cinco requisitos implementados y demostrar cada uno”.

## Diapositiva 05: Cinco requerimientos funcionales implementados

**Tiempo:** 1:00. **Tramo:** 04:30 a 05:30.

### Texto en pantalla

| Requerimiento concreto | Origen documental | Resultado en la web |
|---|---|---|
| Registrar usuarios | Grupo 5: Gestión de usuarios | Cuenta propia con validación de datos. |
| Autenticar usuarios | Grupo 5: Gestión de usuarios | Acceso y navegación según el rol. |
| Consultar y buscar productos | Grupo 2: Catálogo de productos | Catálogo, filtros y ficha de producto. |
| Administrar el carrito | Grupo 3: Carrito de compras | Agregar, modificar, eliminar y consultar totales. |
| Gestionar productos | Grupo 1: Gestión de productos | Alta, modificación, imágenes y eliminación permitida. |

**Selección de cinco requerimientos concretos derivados de los grupos funcionales 1, 2, 3 y 5.**

### Notas del expositor

“Seleccionamos cinco requisitos concretos que podemos recorrer en la página: registrar clientes, iniciar sesión, consultar el catálogo, gestionar un carrito propio y administrar productos. Registro y acceso derivan del grupo 5. Catálogo deriva del grupo 2, carrito del grupo 3 y administración del grupo 1. Conservamos esas referencias para volver al documento original. Esta selección descompone los grupos generales en capacidades específicas. Cada fila tiene una acción visible y una implementación que la sostiene. A continuación vamos a relacionarlas con los casos de uso y a mostrar sus resultados.”

**Fuente:** documentación general, sección 5.1, página 8. Registro y acceso son dos requisitos concretos del mismo grupo original. La selección no acredita como completo todo el alcance de Gestión de usuarios.

**Transición:** “Para organizar la demostración, identificamos también cinco casos de uso del documento”.

## Diapositiva 06: Cinco casos de uso que vamos a demostrar

**Tiempo:** 0:45. **Tramo:** 05:30 a 06:15.

### Texto en pantalla

| Caso de uso | Actor | Resultado esperado |
|---|---|---|
| UC-01: Registrarse | Visitante | Obtiene una cuenta de cliente. |
| UC-02: Iniciar sesión | Usuario registrado | Accede con sus credenciales. |
| UC-08: Filtrar y buscar productos | Cliente | Encuentra productos según sus criterios. |
| UC-09: Ver detalle de producto | Cliente | Consulta la información para decidir. |
| UC-19: ABM de productos | Administrador | Mantiene la información del catálogo. |

### Notas del expositor

“Demostraremos los cinco casos que figuran en este listado. Primero veremos el registro y el acceso. Después buscaremos una notebook y abriremos su detalle. Por último cambiaremos a la sesión administrativa para mantener un producto. En cada caso señalaremos el actor, la acción y el resultado. También mostraremos el carrito para acreditar su requisito funcional. El carrito se relaciona con el grupo 3 y la API vigente. Conservamos los identificadores originales de estos cinco casos de uso.”

**Fuente:** casos de uso, páginas 2 y 3. No atribuir el carrito a UC-10: ese documento describe UC-10 dentro de un checkout directo sin carrito. El catálogo y el detalle también permiten acceso público en la implementación actual.

**Transición:** “Comenzamos con el primer paso del cliente: crear su cuenta”.

## Diapositiva 07: Crear una cuenta para operar en la tienda

**Tiempo:** 1:45. **Tramo:** 06:15 a 08:00.

### Texto en pantalla

El cliente registra sus datos y obtiene una cuenta propia.

- Validación de los datos ingresados.
- Control de correo único.
- Confirmación del registro y acceso al inicio de sesión.

**UC-01: Registrarse. Origen: grupo 5, Gestión de usuarios.**

### Notas del expositor

“Ana comienza creando su cuenta. El formulario le indica qué datos necesita y valida la información. Primero mostraremos cómo responde ante una confirmación de contraseña que no coincide. Después completaremos el registro. Al terminar, el sistema informa que la cuenta fue creada y lleva al inicio de sesión. En el servidor, el registro crea un cliente y guarda la contraseña mediante hash. La cuenta queda identificada para las operaciones posteriores, como acceder a su propio carrito.”

### Demostración en la página

1. Abrir `/Home/Register` en la sesión destinada al cliente.
2. Ingresar nombre Ana, apellido Demo, un correo ficticio nuevo y un teléfono de demostración.
3. Escribir una contraseña de demostración de al menos 8 caracteres y una confirmación distinta. Mostrar el mensaje de validación y corregirla.
4. Enviar el formulario válido.
5. Señalar la confirmación y la redirección al inicio de sesión con el correo completado.
6. Mostrar brevemente `HomeController.Register`, `POST /auth/registro` y `UsuariosLogica.RegistrarAsync`.

**Distribución:** 25 s de explicación, 65 s de formulario y resultado, 15 s de evidencia y transición.

**Resultado esperado:** creación de un cliente. El registro no inicia sesión ni entrega token. Si el correo ya existe, mostrar el error y usar el correo nuevo previsto para este ensayo.

**Evidencia:** [registro MVC][registro-mvc], [endpoint de registro][registro-api] y [asignación del rol Cliente][registro-logica].

**Transición:** “Con la cuenta creada, veamos cómo accede el cliente y cómo se diferencian sus permisos”.

## Diapositiva 08: Cada usuario accede según su responsabilidad

**Tiempo:** 1:45. **Tramo:** 08:00 a 09:45.

### Texto en pantalla

Acceso con credenciales.

Funciones diferenciadas para cliente y administrador.

**El servidor controla los permisos.**

**UC-02: Iniciar sesión. Origen: grupo 5, Gestión de usuarios.**

### Notas del expositor

“La cuenta del cliente y la administración tienen responsabilidades diferentes. Ana accede con el correo que acaba de registrar. Si la contraseña es incorrecta, la página le permite corregirla. Con las credenciales válidas, accede como cliente. En la otra sesión ya está preparado el administrador, que dispone de las herramientas de mantenimiento. La API valida las credenciales y el servidor controla las operaciones permitidas. Esto organiza el acceso a las funciones del comercio.”

### Demostración en la página

1. En `/Home/Login`, mantener el correo del registro anterior.
2. Enviar una contraseña incorrecta y señalar el mensaje recuperable.
3. Enviar la contraseña válida y mostrar el acceso del cliente.
4. Abrir el catálogo y señalar la navegación correspondiente al cliente.
5. Cambiar a la sesión administrativa ya autenticada y contrastar los controles disponibles. Volver a la sesión del cliente para continuar.
6. Mostrar brevemente la validación de login y el control de rol del administrador.

**Distribución:** 25 s de explicación, 65 s de acceso y contraste, 15 s de evidencia y transición.

**Preparación:** las sesiones deben estar separadas por perfil o contexto de navegador. El guion no depende de cerrar sesión dentro de la aplicación.

**Evidencia:** [login MVC][login-mvc], [endpoint de acceso][login-api] y [verificación de credenciales][login-logica].

**Transición:** “Ahora Ana puede buscar el producto que necesita”.

## Diapositiva 09: Encontrar el producto adecuado

**Tiempo:** 2:15. **Tramo:** 09:45 a 12:00.

### Texto en pantalla

Búsqueda por palabra clave y filtros por categoría, precio y disponibilidad.

Resultados paginados con información de cada producto.

**UC-08: Filtrar y buscar productos. Origen: grupo 2, Catálogo.**

### Notas del expositor

“Ana busca una notebook y tiene un presupuesto. Puede combinar la palabra de búsqueda con la categoría, el rango de precios y la disponibilidad. La página muestra los resultados de esos criterios y conserva los filtros para seguir navegando. También veremos qué ocurre cuando no encuentra productos y cómo puede volver a una búsqueda útil. Para el cliente, el valor está en consultar una selección más pertinente. La implementación lleva esos criterios hasta la consulta de datos y devuelve una página de resultados.”

### Demostración en la página

1. Abrir `/Productos` en la sesión del cliente.
2. Buscar `notebook`, elegir categoría `Notebooks` y activar disponibilidad.
3. Aplicar precio mínimo `1000000` y máximo `1500000`. Estos son datos de demostración, no precios comerciales actuales.
4. Señalar los filtros conservados y los resultados. Con el catálogo inicial, Lenovo Yoga 7 2 en 1 queda dentro de ese rango.
5. Mostrar un resultado vacío usando el texto `sin-resultados-totaltech-demo`.
6. Limpiar los filtros y volver a buscar `Lenovo Yoga 7` para abrir su ficha en el siguiente paso.
7. Mostrar el endpoint del catálogo y el método de consulta. La paginación puede señalarse en el catálogo sin filtros, donde el conjunto de demostración contiene más de una página.

**Distribución:** 30 s de explicación, 85 s de búsqueda y recuperación, 20 s de evidencia y transición.

**Resultado esperado:** los productos corresponden a los criterios enviados. Si la base modificó los valores iniciales, ajustar el rango durante la preparación, antes de exponer. No mostrar filtro de marca como función implementada.

**Evidencia:** [controlador de catálogo][catalogo-mvc], [cliente HTTP][catalogo-servicio], [endpoint][catalogo-api], [validación de filtros][catalogo-logica] y [consulta paginada][catalogo-repositorio].

**Transición:** “Una vez encontrado el producto, Ana necesita revisar su información y organizar la selección”.

## Diapositiva 10: Información para decidir y un carrito para organizar

**Tiempo:** 2:30. **Tramo:** 12:00 a 14:30.

### Texto en pantalla

**Ficha del producto:** imagen, descripción, precio y disponibilidad.

**Carrito:** cantidades, subtotales y total de la selección.

**UC-09: Ver detalle de producto. Origen: grupo 2.**

**Gestión del carrito: origen en el grupo 3 y la API vigente.**

### Notas del expositor

“La ficha reúne la información que Ana necesita para decidir. Puede ver la imagen, la descripción, el precio y el stock informado. Después organiza la selección en su carrito. Mostraremos una unidad, cambiaremos la cantidad a dos y veremos cómo se actualiza el total. El servidor obtiene el precio y calcula los importes. Por último quitaremos el producto para mostrar la recuperación del estado vacío. Este recorrido permite revisar la selección. Agregar al carrito no reserva ni descuenta stock. La demostración de esta versión llega hasta el carrito.”

### Demostración en la página

1. Abrir `/Productos/Detalle/{id}` desde la tarjeta de Lenovo Yoga 7 2 en 1. Utilizar el ID que ofrece la página.
2. Señalar nombre, imagen, descripción, precio y disponibilidad.
3. Pulsar `Agregar al carrito` con la sesión cliente iniciada y el carrito vacío.
4. En `/Carrito`, mostrar una unidad y su subtotal.
5. Escribir cantidad `2` y pulsar `Actualizar`. Señalar cantidad, subtotal y total.
6. Pulsar `Eliminar`. Mostrar el estado vacío y el acceso para volver al catálogo.
7. Mostrar brevemente la operación MVC y el cálculo del resumen en el servidor.

**Distribución:** 35 s de explicación, 95 s de ficha y carrito, 20 s de evidencia y transición.

**Datos:** si el producto conserva el precio inicial de $1.450.000 y el carrito sólo contiene esa línea, dos unidades totalizan $2.900.000. Presentar el importe que realmente muestre la demo. Agregar suma a una línea existente, mientras que actualizar establece la cantidad indicada.

**Evidencia:** [ficha del producto][detalle-mvc], [operaciones del carrito][carrito-mvc], [API del carrito][carrito-api] y [reglas de cantidades y precio][carrito-logica].

**Transición:** “Lo que Ana consulta depende de la información que mantiene el comercio. Veamos esa administración”.

## Diapositiva 11: El comercio mantiene su oferta desde la administración

**Tiempo:** 3:00. **Tramo:** 14:30 a 17:30.

### Texto en pantalla

Crear, editar y eliminar productos.

Actualizar precio, descripción, imagen y stock.

**Los cambios guardados se reflejan al consultar el catálogo.**

**UC-19: ABM de productos. Origen: grupo 1, Gestión de productos.**

### Notas del expositor

“El comercio necesita mantener vigente la información que recibe el cliente. En la administración crearemos un producto exclusivo de demostración. Después modificaremos su precio y stock, y añadiremos una imagen desde la edición. Al recargar el catálogo público veremos la información guardada. Finalmente eliminaremos ese producto. Las operaciones requieren permisos de administrador y validan los datos. Cuando existen relaciones con carritos o pedidos, la eliminación puede rechazarse para proteger esas referencias. Por eso este ejemplo utiliza un producto separado del recorrido anterior.”

### Demostración en la página

1. Cambiar a la sesión administrativa y abrir `/Productos/Crear`.
2. Completar los campos del alta con los datos de demostración.
3. Crear `Producto de demostración TotalTech`, descripción `Teclado de prueba para la exposición`, precio `95000`, stock `3`, categoría `Periféricos` y proveedor activo `PixelWare Distribuidora Demo`.
4. Buscar el nuevo producto en el catálogo y abrir `Editar`.
5. En la edición, dejar el nombre vacío, intentar guardar y mostrar su mensaje de validación. Restaurar el nombre del producto, cambiar precio a `120000`, stock a `5` y seleccionar una imagen JPG, PNG o WebP de demostración. La imagen se carga en la edición, no en el formulario de alta.
6. Guardar. En la sesión del cliente, buscar el nombre del producto y recargar para comprobar precio, stock e imagen.
7. Volver a la administración, eliminar ese producto y confirmar la acción. Comprobar que dejó de aparecer en el catálogo.
8. Mostrar brevemente la autorización del endpoint y la validación de precio, stock y referencias.

**Distribución:** 30 s de explicación, 130 s de ABM y verificación, 20 s de evidencia y transición.

**Condiciones:** debe existir la categoría y el proveedor activo elegido. La imagen debe ser un archivo permitido de hasta 5 MB. Mantener el producto de prueba fuera de carritos y pedidos. Esta demostración muestra eliminación permitida, no acredita baja lógica ni auditoría administrativa completa.

**Evidencia:** [alta administrativa][admin-crear], [edición e imagen][admin-editar], [eliminación][admin-eliminar], [autorización de productos][admin-api] y [validación de producto][admin-logica].

**Transición:** “Además de funcionar, el sistema necesita criterios de calidad que acompañen estos recorridos”.

## Diapositiva 12: Cinco criterios de calidad contemplados

**Tiempo:** 2:00. **Tramo:** 17:30 a 19:30.

### Texto en pantalla

| RNF original | Cómo está contemplado |
|---|---|
| 1. Rendimiento | Consultas paginadas y carga paralela del catálogo. |
| 2. Usabilidad | Formularios claros, mensajes y filtros conservados. |
| 3. Compatibilidad | Diseño adaptable a distintos tamaños de pantalla. |
| 4. Seguridad | Hash de contraseñas, autenticación y permisos por rol. |
| 5. Mantenibilidad | Responsabilidades separadas por capas. |

**Objetivo de rendimiento documentado: carga de páginas en hasta 3 segundos bajo condiciones normales.**

### Notas del expositor

“La documentación establece criterios de calidad además de las funciones. Para rendimiento, el catálogo consulta páginas de resultados y carga información en paralelo. El objetivo de tres segundos requiere medirse bajo condiciones definidas. En usabilidad, observamos validaciones, mensajes comprensibles y una forma de recuperar búsquedas vacías. Para compatibilidad, el diseño se adapta al tamaño de la pantalla, como mostraremos ahora. La documentación prevé Chrome, Firefox y Edge. La evidencia de pruebas de navegador inspeccionada utiliza Chromium. En seguridad, el servidor verifica credenciales y permisos, y guarda las contraseñas mediante hash. Ese mecanismo protege contraseñas y no demuestra cifrado integral de todos los datos personales. Por último, la separación de capas facilita localizar responsabilidades y mantener el sistema. Estos cinco criterios están contemplados mediante mecanismos concretos.”

### Evidencia y demostración

- Señalar la paginación y la recuperación de búsqueda ya observadas.
- Mostrar el catálogo a 375 px de ancho y volver a la vista de escritorio.
- Recordar el contraste de permisos entre cliente y administrador.
- Mostrar la separación de responsabilidades en la siguiente diapositiva.

**Distribución:** 85 s de explicación de los cinco criterios, 20 s de vista móvil y 15 s de transición y apoyo técnico.

**Alcance de la evidencia:** existen pruebas de catálogo, navegación, seguridad y tamaños de pantalla. No se ejecutaron en esta preparación. La adaptación móvil no acredita por sí sola todos los navegadores previstos. No presentar como resultados medidos los objetivos de rendimiento o disponibilidad.

**Fuente:** documentación general, sección 5.2, página 9. Son los primeros cinco RNF originales. Escalabilidad y disponibilidad forman parte del alcance documentado, pero no integran esta selección.

**Transición:** “Para entender la mantenibilidad, sigamos una de las operaciones que acabamos de mostrar”.

## Diapositiva 13: La implementación que sostiene lo demostrado

**Tiempo:** 1:00. **Tramo:** 19:30 a 20:30.

### Texto en pantalla

**Ejemplo: buscar productos**

1. Pantalla de catálogo.
2. Controlador MVC y servicio HTTP.
3. API y lógica de validación.
4. Repositorio y base de datos.
5. Resultados en la página.

### Notas del expositor

“La pantalla recoge los filtros que eligió Ana. El controlador MVC solicita el catálogo mediante el servicio HTTP. La API recibe esos criterios y la lógica comprueba su validez. El repositorio compone la consulta, aplica los filtros y obtiene la página de productos desde la base de datos. La respuesta vuelve al controlador, que presenta los resultados. Cada capa tiene una responsabilidad identificable. Este recorrido conecta la necesidad documentada, el caso de uso y el comportamiento que vimos en la página.”

### Fragmentos breves para mostrar

Mostrar estos tres fragmentos reales, con una frase sobre su responsabilidad. Los recortes omiten código intermedio, identificado con `...`, y sirven para lectura durante la exposición.

**Controlador: coordina la consulta.**

```csharp
var categoriasTask = _categoriasApiService.ObtenerTodosAsync();
var catalogoTask = _productosApiService.ObtenerCatalogoAsync(
    texto, idCategoria, precioMin, precioMax,
    soloDisponibles, pagina, tamanoPagina);
await Task.WhenAll(categoriasTask, catalogoTask);
```

**Endpoint: publica la operación y delega.**

```csharp
group.MapGet("/catalogo", async (...,
    IProductosLogica logica, CancellationToken cancellationToken) =>
{
    ...
    var (catalogo, error) = await logica.ObtenerCatalogoAsync(
        filtro, cancellationToken);
    ...
}).AllowAnonymous();
```

**Repositorio: consulta una página de resultados.**

```csharp
var totalItems = await consulta.CountAsync(cancellationToken);
var items = await consulta
    .OrderBy(producto => producto.Nombre)
    .ThenBy(producto => producto.IdProducto)
    .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
    .Take(filtro.TamanoPagina)
    ...
```

**Distribución:** 40 s de recorrido y 20 s de fragmentos y transición. Mostrarlos de forma secuencial o señalar los métodos preparados en el editor.

**Evidencia:** [controlador][catalogo-mvc], [servicio HTTP][catalogo-servicio], [endpoint][catalogo-api], [lógica][catalogo-logica] y [repositorio][catalogo-repositorio].

**Transición:** “Este recorrido también nos permite precisar qué alcanza la versión actual y cuál es su siguiente etapa”.

## Diapositiva 14: La versión actual y su próxima etapa

**Tiempo:** 1:15. **Tramo:** 20:30 a 21:45.

### Texto en pantalla

**Disponible en la web**

Cuentas y acceso. Catálogo y carrito. Administración de productos.

**Próxima etapa**

Completar la compra web y la integración de pagos.

### Notas del expositor

“La versión actual reúne las capacidades que acabamos de recorrer: cuentas de clientes, acceso por rol, catálogo, carrito y administración de productos. La API incluye confirmación de carrito y registro de pedidos, pero falta completar la pantalla y el recorrido web de compra. La integración real con MercadoPago y el envío de consultas desde contacto también requieren trabajo. El formulario de contacto actual presenta una confirmación de demostración. Este alcance permite evaluar el catálogo y la administración en un piloto, y definir la prioridad de la siguiente etapa con necesidades concretas del comercio. Cada ampliación deberá conservar la trazabilidad entre documentación, código y resultado observable.”

**Apoyo:** distinguir capacidad web, capacidad de backend y trabajo pendiente. No mostrar botones o pantallas reservadas como operaciones terminadas. El registro manual de un pago con un método denominado MercadoPago no equivale a una pasarela integrada.

**Transición:** “Con este alcance definido, podemos presentar una propuesta concreta de uso”.

## Diapositiva 15: Una propuesta concreta para el comercio

**Tiempo:** 1:15. **Tramo:** 21:45 a 23:00.

### Texto en pantalla

Un catálogo consultable.

Una selección organizada.

Una administración centralizada.

**Propuesta: evaluar un piloto con el catálogo de la tienda.**

### Notas del expositor

“Proponemos comenzar con un piloto centrado en mantener el catálogo y permitir que los clientes consulten productos y organicen su selección. El comercio puede evaluar esas capacidades con sus necesidades reales y priorizar la siguiente etapa. Durante esta presentación mostramos los cinco requisitos concretos seleccionados, los cinco casos de uso y los criterios de calidad contemplados. También seguimos una operación desde la documentación hasta el código y su resultado en la página. TotalTech ofrece una base concreta para digitalizar la presentación de productos y su administración, con un recorrido definido para completar la venta online. El siguiente paso comercial es validar esa base con el catálogo de la tienda.”

**Cierre:** mantener la propuesta de piloto visible. Abrir preguntas después de completar la exposición. No introducir precios, plazos de entrega, porcentajes de ahorro o garantías de disponibilidad sin evidencia y definición previa.

## Preparación del recorrido y contingencia

### Datos y ventanas antes de exponer

1. Confirmar que el sitio y la API estén disponibles en el entorno de demostración. Las rutas de este documento son rutas de la aplicación, no confirman que los servicios estén activos.
2. Preparar un contexto para el cliente y otro para el administrador. Usar datos ficticios y mantener las sesiones separadas durante todo el recorrido.
3. Reservar un correo de cliente nuevo por ensayo, siguiendo el patrón `ana.demo.<numero>@example.com`. La contraseña se elige durante la preparación y no se incorpora a este documento. Confirmar nombre, apellido, teléfono y coincidencia de las contraseñas.
4. Verificar que Lenovo Yoga 7 2 en 1 exista, tenga al menos dos unidades y que su precio quede dentro del rango de la búsqueda preparada. El catálogo inicial define $1.450.000 y stock 5, pero el inicializador omite productos existentes y no garantiza esos valores en la base actual.
5. Empezar con el carrito del cliente vacío. Mantenerlo con una sola línea durante la comparación de una y dos unidades.
6. Confirmar la categoría Periféricos y el proveedor activo PixelWare Distribuidora Demo para el producto del ABM. Preparar una imagen permitida de hasta 5 MB.
7. Dejar disponibles el documento general, los casos de uso, la API vigente y los métodos de código enlazados. Mostrar sólo los extractos pertinentes durante la exposición.

### Grabación y ensayo pendientes

Preparar una grabación de respaldo con las mismas acciones de las diapositivas 7 a 11 y la adaptación móvil de la 12. Utilizar datos ficticios, mostrar cada resultado y conservar el orden del guion. Si el entorno falla al exponer, presentar esa grabación e identificarla como recorrido registrado. Un HTML documental, una captura o un diagrama por sí solos no reemplazan las operaciones de la tienda.

Ensayar con cronómetro sin contar las preguntas. Registrar tiempo de inicio y fin de cada diapositiva, incluyendo cambios de ventana. La meta es 23 minutos. Si un ensayo queda por debajo de 20, desarrollar los resultados y la trazabilidad de las operaciones dentro de sus bloques. Las esperas por fallos no forman parte del contenido previsto.

### Lista para comprobar durante el ensayo

- [ ] Hay 15 diapositivas, con los títulos y el orden de este documento.
- [ ] La exposición dura al menos 20 minutos sin preguntas y apunta a 23 minutos.
- [ ] La diapositiva 5 enumera cinco requisitos concretos y muestra los grupos originales 1, 2, 3 y 5.
- [ ] La diapositiva 6 enumera UC-01, UC-02, UC-08, UC-09 y UC-19, cada uno con actor y resultado.
- [ ] Registro muestra validación, creación de cliente y paso al acceso.
- [ ] Login muestra rechazo recuperable y acceso correcto. El contraste de roles utiliza sesiones independientes.
- [ ] Catálogo muestra búsqueda combinada, filtros, estado vacío y recuperación.
- [ ] Detalle muestra información del producto y carrito muestra agregar, cantidad, total y eliminación.
- [ ] Administración muestra alta, edición con imagen, cambio visible en el catálogo y eliminación del producto sin relaciones.
- [ ] La diapositiva 12 contiene los cinco RNF originales seleccionados, con mecanismos y límites de evidencia.
- [ ] La diapositiva 13 vincula los métodos reales con el resultado de la búsqueda.
- [ ] El cierre distingue las capacidades disponibles de la compra web y las integraciones pendientes.
- [ ] La grabación de respaldo fue preparada y se puede reproducir.

Esta lista corresponde a una preparación futura. Ninguna casilla se marca como cumplida por la sola existencia de este documento.

## Fuentes y límites de la revisión

- [Documentación del proyecto V2](https://docs.google.com/document/d/14_6aZhFY5ACk26mPNe4lj_6eVJhEZgfSD8BGV72ycTA/edit): situación del negocio, alcance y requisitos. Copia local: [Documentación - Grupo 1 -TotalTech.pdf][documento-general].
- [Casos de uso vigentes](https://docs.google.com/document/d/1rh62s6QJSOWW0H5KNVBXnn_lgfnHlPnFVXOgC5LrwbI/edit): identificadores de casos y actores. Copia local: [Casos de Uso- Totaltech.pdf][documento-casos].
- [API vigente del 01/10/2026](https://drive.google.com/file/d/1uB4R9esellhJg2zGNR4G1xXL_0wX-4_U/view): contratos actuales. Copias locales: [API.md][documento-api-md] y [Documentación de API.pdf][documento-api-pdf].
- Código actual del repositorio, modelos, controladores, endpoints, lógica, repositorios y pruebas existentes, inspeccionados por lectura. Las pruebas no se ejecutaron para preparar este contenido.
- Se consultaron también la documentación histórica, las entidades, el caso Finalizar compra de 2025, la planificación y las presentaciones anteriores. Ante diferencias de implementación, se utilizó el código actual y la API vigente.

Los dos mockups de Canva y el diagrama de Lucid enlazados desde el README no pudieron inspeccionarse en este entorno. No se usan como evidencia visual leída. Este trabajo produjo contenido documental: no ejecutó las demostraciones, no realizó el ensayo oral ni preparó una grabación.

## Enlaces locales para preparar la evidencia

Los enlaces apuntan al checkout utilizado en esta preparación. Si se mueve el repositorio, conservar los nombres de archivos y símbolos para localizarlos nuevamente.

[documento-general]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación - Grupo 1 -TotalTech.pdf>
[documento-casos]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Casos de Uso- Totaltech.pdf>
[documento-api-md]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/API.md>
[documento-api-pdf]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Documentacion/Documentación de API.pdf>
[registro-mvc]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/HomeController.cs:143>
[registro-api]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Endpoints/AuthEndpoints.cs:49>
[registro-logica]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/UsuariosLogica.cs:145>
[login-mvc]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/HomeController.cs:47>
[login-api]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Endpoints/AuthEndpoints.cs:15>
[login-logica]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/UsuariosLogica.cs:55>
[catalogo-mvc]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/ProductosController.cs:41>
[catalogo-servicio]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Services/ProductosApiService.cs:130>
[catalogo-api]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Endpoints/ProductosEndpoints.cs:14>
[catalogo-logica]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/ProductosLogica.cs:114>
[catalogo-repositorio]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Repositorios/ProductosRepositorio.cs:93>
[detalle-mvc]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/ProductosController.cs:77>
[carrito-mvc]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/CarritoController.cs:25>
[carrito-api]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Endpoints/CarritosEndpoints.cs:25>
[carrito-logica]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/CarritosLogica.cs:316>
[admin-crear]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/ProductosController.cs:110>
[admin-editar]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/ProductosController.cs:156>
[admin-eliminar]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Frontend/Controllers/ProductosController.cs:223>
[admin-api]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Endpoints/ProductosEndpoints.cs:42>
[admin-logica]: <C:/Users/facu_/Documents/GitHub/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Grupo-N-1---SistemaVentaDeProductosTecnologicos/Totaltech/Logica/ProductosLogica.cs:129>
