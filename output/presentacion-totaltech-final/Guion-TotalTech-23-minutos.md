# TotalTech · Guion de exposición y demostración

13 diapositivas · 23 minutos · preguntas aparte.

Los tiempos son objetivos de ensayo: 14 minutos de exposición y 9 de demostración. No son una medición de una exposición humana.

## 1. TotalTech

**0:00–0:30 · 0:30**

TotalTech permite al comercio mantener su catálogo actualizado y al cliente encontrar productos y organizar su selección desde una misma plataforma. Hoy queremos mostrar una propuesta concreta para una tienda de tecnología: información accesible para el cliente y control para el comercio. Primero explicaremos las necesidades y las decisiones que dieron forma al sistema; al final recorreremos la página.

**Respaldo:** README.md: integrantes; documentación general, propósito y alcance.

## 2. Por qué elegimos este proyecto

**0:30–1:30 · 1:00**

Elegimos este proyecto porque una tienda de tecnología necesita comunicar una oferta que cambia: modelos, precios, disponibilidad e imágenes. Cuando esos datos se mantienen en distintos lugares, una actualización puede llegar tarde al cliente. Pensemos en una persona que busca una notebook: necesita saber qué opciones hay y cuánto cuestan antes de consultar al comercio. El negocio, por su parte, necesita mantener esa información sin repetir el mismo trabajo en cada consulta. TotalTech nace de esa necesidad. No partimos de una cifra de ahorro o de una promesa de aumento de ventas; partimos de una tarea que se puede resolver y verificar en la página: consultar y mantener un catálogo. Ese es el valor que vamos a demostrar.

**Respaldo:** Documentacion/Documentación - Grupo 1 -TotalTech.pdf: contexto y requerimientos, p. 8.

## 3. Propósito del sistema

**1:30–2:15 · 0:45**

Nuestro propósito es que cliente y comercio trabajen sobre la misma información. Para el cliente significa encontrar productos, consultar sus características y organizar una selección. Para el comercio significa mantener productos, precios, imágenes y stock desde una administración centralizada. La historia de la demostración será sencilla: una persona busca una notebook y el administrador actualiza la oferta. Al terminar podremos observar ambos resultados. Esta versión cubre el catálogo y el carrito; la compra online completa pertenece a la próxima etapa.

**Respaldo:** Documentación general, p. 8; Frontend/Controllers/CheckoutController.cs: estructura reservada.

## 4. El problema y la oportunidad

**2:15–3:15 · 1:00**

Identificamos tres situaciones. Primero, la información puede quedar desactualizada si se mantiene en distintos lugares: la respuesta es un catálogo que el administrador puede editar. Segundo, encontrar una opción puede exigir recorrer toda la oferta: la búsqueda y los filtros permiten acotar por palabra, categoría, precio y disponibilidad. Tercero, elegir varios productos puede obligar a calcular cantidades e importes por separado: el carrito organiza esa selección y presenta sus totales. En cada fila hay una relación entre una necesidad del negocio y una capacidad observable. El beneficio es facilitar estas tareas; no afirmamos que el sistema haya producido un porcentaje de mejora porque esa medición no forma parte del proyecto.

**Respaldo:** Documentación general, sección 5.1, grupos 1, 2 y 3.

## 5. La solución y sus usuarios

**3:15–4:15 · 1:00**

La solución tiene dos perspectivas principales. El cliente crea su cuenta, inicia sesión, busca un producto, abre su ficha y organiza el carrito. El administrador mantiene la información que alimenta ese recorrido: nombre, descripción, precio, stock e imagen. Las responsabilidades son diferentes y los permisos también. El catálogo es público, pero el mantenimiento de productos requiere el rol administrativo. Un cambio realizado desde administración se refleja al consultar nuevamente el catálogo. Esa conexión es el centro de nuestra propuesta: el comercio actualiza una oferta que el cliente puede consultar. Durante la demostración usaremos sesiones separadas para que se vea con claridad qué puede hacer cada actor.

**Respaldo:** Frontend/Controllers/ProductosController.cs: Index público; Crear, Editar y Eliminar requieren Admin.

## 6. Cinco requerimientos funcionales

**4:15–5:30 · 1:15**

Seleccionamos cinco requerimientos concretos implementados. El primero es registrar usuarios y el segundo autenticar usuarios; ambos se desprenden del grupo documental Gestión de usuarios. El tercero es consultar y buscar productos, dentro de Catálogo. El cuarto es administrar el carrito: agregar, modificar cantidades, eliminar y consultar totales. El quinto es gestionar productos desde administración. Es importante leer la columna de origen: no estamos diciendo que cinco módulos documentales estén completos. Por ejemplo, el grupo Gestión de usuarios también prevé otras funciones, pero aquí acreditamos registro e inicio de sesión. Al final, cada fila tendrá una acción visible y un resultado en la página. Conservamos así la relación entre lo solicitado y lo que realmente se implementó.

**Respaldo:** Documentación general, p. 8, sección 5.1. Registro y autenticación derivan del mismo grupo 5.

## 7. Cinco casos de uso seleccionados

**5:30–6:45 · 1:15**

Los casos seleccionados conservan sus identificadores originales. UC-01 describe el registro y termina con una cuenta de cliente. UC-02 describe el inicio de sesión. UC-08 representa la búsqueda y aplicación de filtros. UC-09 permite consultar el detalle del producto. UC-19 corresponde al mantenimiento de productos por el administrador. En la demostración los primeros cuatro forman el recorrido del cliente y el último muestra el trabajo del comercio. El carrito también se demuestra para acreditar su requerimiento funcional, pero no lo llamamos UC-10: el documento define ese caso como selección para un checkout directo sin carrito. En UC-08 mostraremos los filtros implementados, sin incluir marca. En UC-19 mostraremos la eliminación disponible, sujeta a relaciones; no la presentaremos como desactivación lógica.

**Respaldo:** Documentacion/Casos de Uso- Totaltech.pdf: pp. 2–3 y notas de modelado p. 5.

## 8. Cinco criterios de calidad

**6:45–8:15 · 1:30**

Además de las funciones, contemplamos cinco criterios de calidad. Rendimiento: el catálogo usa paginación y consulta en paralelo categorías y productos. La documentación fija tres segundos como objetivo; una carga rápida en nuestra computadora no demuestra rendimiento de producción. Usabilidad: veremos etiquetas, validaciones y una búsqueda vacía que se puede limpiar. Compatibilidad: mostraremos escritorio y un ancho móvil; eso no sustituye una prueba en cada navegador previsto. Seguridad: hay contraseñas con hash, controles por rol y protección de formularios. Estos controles son evidencia parcial y no equivalen a certificar cifrado integral ni cumplimiento de privacidad. Mantenibilidad: la separación entre interfaz, API, lógica y persistencia permite localizar responsabilidades. Este último criterio se ve principalmente en el código. La idea es explicar qué decisión respalda cada requisito y hasta dónde llega la evidencia.

**Respaldo:** Documentación general, p. 9; ProductosController.Index; ProductosRepositorio.ObtenerCatalogoAsync; UsuariosLogica; ValidateAntiForgeryToken.

## 9. La documentación y su implementación

**8:15–10:15 · 2:00**

Esta matriz muestra la trazabilidad de los cinco casos. Registro conecta Gestión de usuarios con HomeController.Register y el formulario de registro. Acceso conecta ese mismo grupo con la autenticación y la pantalla de inicio de sesión. Búsqueda conecta Catálogo con ProductosController.Index y la página de productos. Detalle conecta el catálogo con ProductosController.Detalle y la ficha. ABM conecta Gestión de productos con las acciones de administración. Ampliemos UC-08: la necesidad es encontrar una notebook según criterios. El controlador recibe texto, categoría, precios y disponibilidad. Llama al servicio HTTP, que consulta GET /productos/catalogo. La API deriva la consulta a la lógica y al repositorio. El fragmento visible es real: Skip calcula el desplazamiento y Take limita la cantidad de productos de la página. Así conectamos el comportamiento solicitado con una decisión de implementación. El resultado vuelve a la vista Razor y se convierte en productos visibles. El carrito tiene su propia cadena: CarritoController, CarritosEndpoints, lógica y persistencia. En la demostración verificaremos esos resultados sin volver a interrumpir el recorrido con código.

**Respaldo:** Frontend/Controllers/HomeController.cs; Frontend/Controllers/ProductosController.cs; Frontend/Controllers/CarritoController.cs; Totaltech/Endpoints/AuthEndpoints.cs; Totaltech/Endpoints/ProductosEndpoints.cs; Totaltech/Endpoints/CarritosEndpoints.cs; Totaltech/Repositorios/ProductosRepositorio.cs, líneas 119–120.

## 10. Arquitectura del sistema

**10:15–11:30 · 1:15**

La aplicación separa presentación, reglas y persistencia. La vista Razor muestra información y formularios. El controlador MVC coordina la solicitud y utiliza un servicio HTTP para comunicarse con la API. Los endpoints reciben la operación; la lógica aplica las reglas y el repositorio accede a SQL Server mediante Entity Framework Core. Podemos seguir la búsqueda de una notebook de izquierda a derecha y luego la respuesta de vuelta hasta la pantalla. Esta separación ayuda a localizar dónde corregir o ampliar una función. No estamos afirmando que cada componente sea un microservicio ni que se despliegue de forma independiente: describimos las responsabilidades del proyecto real. La arquitectura respalda la mantenibilidad y el control de las operaciones en el servidor.

**Respaldo:** Frontend/Services/ProductosApiService.cs; Totaltech/Endpoints/ProductosEndpoints.cs; Totaltech/Logica/ProductosLogica.cs; Totaltech/Repositorios/ProductosRepositorio.cs.

## 11. Alcance actual y próxima etapa

**11:30–12:30 · 1:00**

La versión que presentamos permite usar cuentas, catálogo, detalle, carrito y administración de productos. La próxima etapa es completar la compra web e integrar el pago. Aunque hay operaciones de pedidos en backend, eso no significa que el checkout MVC esté terminado: su controlador está reservado. También quedan límites concretos: no mostraremos filtros por marca y la eliminación de productos está condicionada por sus relaciones. Para un piloto comercial hay que cuidar la actualización del catálogo, preparar al administrador y comprobar el entorno de ejecución. Presentamos una base funcional verificable y una evolución definida, sin confundir funciones previstas con capacidades disponibles.

**Respaldo:** Frontend/Controllers/CheckoutController.cs; documentación de casos de uso; matriz de trazabilidad existente.

## 12. La propuesta para el comercio

**12:30–13:30 · 1:00**

La propuesta reúne tres resultados. Primero, un catálogo consultable donde el cliente puede buscar y revisar información. Segundo, una selección organizada en un carrito con cantidades y totales. Tercero, una administración centralizada para mantener la oferta. Un piloto permitiría cargar productos del comercio, preparar a quien los administra y observar consultas y actualizaciones habituales. Sería el momento de recoger comentarios y medir tiempos reales de uso, en lugar de prometer beneficios que todavía no se midieron. La implementación actual es la base de esa evaluación. Ahora vamos a comprobar en la página la experiencia del cliente y el trabajo del administrador, siguiendo la misma historia de la notebook.

**Respaldo:** Propuesta de piloto: recomendación del equipo, sin resultados comerciales medidos.

## 13. TotalTech en funcionamiento

**13:30–14:00 · 0:30**

Vamos a recorrer el sistema como cliente: registro, acceso, búsqueda, detalle y carrito. Después pasaremos a la sesión administrativa para crear, editar y eliminar un producto de demostración. Cerraremos con la adaptación móvil y los permisos. Toda la demostración empieza ahora y dura nueve minutos. Al terminar, mantener el navegador abierto y cerrar oralmente: “Con TotalTech, el comercio mantiene su oferta y el cliente puede encontrar y organizar los productos que le interesan”.

**Respaldo:** Guion de demostración: ver sección final; usar datos ficticios y sesiones separadas.

## Demostración final

### 14:00–15:15 · Registro

**UC-01 / RF registrar.** Abrir /Home/Register. Enviar incompleto para mostrar una validación. Completar datos ficticios con correo único y una contraseña válida; crear la cuenta. Comprobar la confirmación.

### 15:15–16:00 · Acceso

**UC-02 / RF autenticar.** Abrir /Home/Login. Iniciar sesión con la cuenta recién creada. Mostrar la navegación disponible para cliente. Mantener separada la sesión administrativa.

### 16:00–17:15 · Búsqueda

**UC-08 / RF catálogo.** Abrir /Productos. Buscar notebook, elegir categoría, rango de precios y sólo disponibles según los datos del catálogo. Aplicar. Buscar un texto inexistente y recuperar resultados con Limpiar filtros.

### 17:15–18:00 · Detalle

**UC-09 / RF catálogo.** Abrir una notebook disponible. Señalar imagen, descripción, precio y stock. Elegir un producto con al menos dos unidades para el siguiente paso.

### 18:00–19:15 · Carrito

**RF carrito, sin nuevo identificador UC.** Agregar la notebook. Cambiar cantidad de 1 a 2 y actualizar. Comparar subtotal y total. Eliminar y mostrar carrito vacío. No continuar hacia checkout.

### 19:15–21:45 · Administración

**UC-19 / RF productos.** En la sesión Admin crear Producto de demostración TotalTech con categoría y proveedor válidos, precio y stock. Editar precio, descripción e imagen. Recargar catálogo público y comprobar el cambio. Eliminar únicamente ese producto, sin relaciones. Mostrar confirmación.

### 21:45–23:00 · Calidad y cierre

**RNF usabilidad, compatibilidad y seguridad.** Mostrar el catálogo a ancho móvil. En la sesión cliente comprobar que no puede acceder a una acción administrativa. Relacionar validaciones y controles con lo observado. Cerrar con la propuesta de piloto y la frase final, manteniendo el sistema abierto.

## Preparación y contingencia

- Iniciar API y frontend según README.md; verificar catálogo antes de exponer. No mostrar terminales con credenciales ni archivos de configuración sensibles.
- Preparar dos sesiones: cliente y administrador. Usar datos ficticios y un correo nuevo para el registro.
- Tener una notebook con al menos dos unidades de stock; preparar categoría, proveedor e imagen válidos para el ABM.
- El producto de demostración no debe vincularse con carritos ni pedidos. El sistema puede impedir borrar productos relacionados.
- Abrir previamente los documentos de requisitos (pp. 8–9), casos (pp. 2–3, 5) y el código citado. El PPT ya incluye la evidencia breve.
- Ensayar con cronómetro y mantener las preguntas fuera de los 23 minutos. No rellenar el mínimo con esperas por fallos.
- Registrar durante el ensayo los tiempos de carga con sus condiciones; no afirmar un SLA ni compatibilidad no probada.
- Si se utiliza una grabación de respaldo, anunciar que es una grabación del sistema. La grabación no sustituye una verificación del entorno el día de la exposición.

## Criterios para el ensayo

- [ ] 5 RF listados y demostrados.
- [ ] 5 casos con sus identificadores originales.
- [ ] 5 RNF explicados con evidencia y límites.
- [ ] Registro, acceso, búsqueda, detalle, carrito y ABM completados.
- [ ] Sin checkout, marca ni baja lógica presentados como implementados.
- [ ] Demostración completa al final; cierre sobre el navegador.
- [ ] Ensayo humano de 23 minutos realizado.
