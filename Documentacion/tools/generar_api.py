"""Genera la referencia editorial desde contratos inspeccionados del repositorio.

Ejecutar desde la raíz interna con Python y reportlab disponibles.
No inicia aplicaciones, no lee secretos y no modifica la base de datos.
"""
from pathlib import Path
from html import escape
import json
import re
import subprocess

from reportlab.lib import colors
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak,
    Preformatted, KeepTogether,
)
from reportlab.platypus.tableofcontents import TableOfContents

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'output/pdf/Documentacion-de-API.pdf'
SOURCE = ROOT / 'Documentacion/API.md'
MODULES = [
    ('Auth', 'Autenticación', 'Identidad, registro público y emisión de tokens JWT.'),
    ('Productos', 'Productos', 'Catálogo público y mantenimiento administrativo de productos.'),
    ('Categorias', 'Categorías', 'Clasificación pública del catálogo; escritura administrativa.'),
    ('Carritos', 'Carritos', 'Carrito activo del usuario autenticado y confirmación de compra.'),
    ('Pedidos', 'Pedidos', 'Consulta de pedidos y operaciones de estado y pago.'),
    ('Pagos', 'Pagos', 'Consulta y transición controlada de pagos registrados.'),
    ('DetallePedidos', 'Detalles de pedidos', 'Lectura de líneas históricas generadas al confirmar un carrito.'),
    ('DetalleCarritos', 'Detalles de carritos', 'Lectura de líneas; las modificaciones se realizan mediante /carritos.'),
    ('Usuarios', 'Usuarios', 'Administración y edición del perfil propio sin exponer contraseñas.'),
    ('Direcciones', 'Direcciones', 'Direcciones de usuarios y control de propiedad.'),
    ('Proveedores', 'Proveedores', 'Información comercial y dirección fiscal del proveedor.'),
    ('Compras', 'Compras a proveedores', 'Registros de compras a proveedores; recurso distinto de los pedidos de clientes.'),
    ('Reportes', 'Reportes', 'Registros de reportes y agregados de ventas e ingresos.'),
    ('Consultas', 'Consultas', 'Contacto público y gestión administrativa de consultas.'),
]

def endpoints():
    result = {}
    status_map = {'Ok':200, 'Created':201, 'NoContent':204, 'BadRequest':400,
                  'Unauthorized':401, 'NotFound':404, 'Conflict':409}
    for key, _, _ in MODULES:
        path = ROOT / f'Totaltech/Endpoints/{key}Endpoints.cs'
        text = path.read_text(encoding='utf-8-sig')
        prefix = re.search(r'MapGroup\("([^"]+)"', text).group(1)
        matches = list(re.finditer(r'group\.Map(Get|Post|Put|Patch|Delete)\("([^"]+)"', text))
        rows = []
        admin_group = 'PoliticaAdministrador' in text[:matches[0].start()]
        for i, m in enumerate(matches):
            block = text[m.start():matches[i+1].start() if i+1 < len(matches) else len(text)]
            # Solo el cuerpo de la ruta; excluir métodos auxiliares posteriores.
            block = block.split('\n        private ')[0].split('\n    private ')[0]
            access = ('Público' if '.AllowAnonymous()' in block else
                      'Admin' if admin_group or 'PoliticaAdministrador' in block else 'JWT')
            request = re.search(r'\b(\w+(?:Request|Dto)) request\b', block)
            statuses = sorted({status_map[x] for x in re.findall(r'Results\.(\w+)\(', block) if x in status_map})
            if key == 'Usuarios':
                statuses = ([200] if m.group(1) == 'Get' and m.group(2) == '/' else
                            [200, 404] if m.group(1) == 'Get' else
                            [204, 404, 409] if m.group(1) == 'Delete' else
                            [200, 404, 409] if 'reactivar' in m.group(2) else
                            [200, 400, 404, 409] if m.group(1) == 'Put' else [201, 400, 409])
            if key == 'Auth' and m.group(2) == '/login':
                statuses = [200, 401, 403, 404]
            route = re.sub(r':int', '', prefix + ('' if m.group(2) == '/' else m.group(2)))
            rows.append(dict(method=m.group(1).upper(), route=route, access=access,
                             request=request.group(1) if request else '-',
                             statuses='/'.join(map(str, statuses))))
        result[key] = rows
    return result

NOTES = {
 'Auth': [
  'POST /auth/login devuelve LoginResponse: datos de UsuarioResponse, accessToken y expiresAtUtc. Email inexistente: 404 con codigo=usuario_no_registrado y mensaje; contraseña incorrecta: 401 sin cuerpo específico. Cuenta inactiva con credenciales válidas: 403 con codigo=usuario_inactivo y mensaje.',
  'POST /auth/registro devuelve UsuarioResponse y Location=/usuarios/{idUsuario}. El servidor fuerza rol=0 (Cliente) y fechaRegistro UTC. Nombre, apellido, email válido y teléfono son obligatorios; contraseña de al menos 8 caracteres; email duplicado: 409.',
  'POST /auth/recuperar-contrasena recibe email y devuelve {mensaje}. La implementación solo consulta si existe el usuario; no registra una solicitud persistente, no envía correo y no emite un token de recuperación.',
 ],
 'Productos': [
  'GET /productos/catalogo devuelve CatalogoProductosResponse con items de ProductoCatalogoResponse. Es la ruta paginada. Las otras rutas GET devuelven Producto o listas de Producto sin paginación.',
  'GET /productos/buscar recibe texto opcional. GET /productos/categoria/{idCategoria} filtra por categoría; /disponibles filtra stock positivo.',
  'POST y PUT devuelven Producto; POST incluye Location. nombre obligatorio, precio y stock >= 0, categoría y proveedor existentes. PATCH /{id}/stock recibe el stock absoluto y devuelve 204.',
  'imagenUrl es nullable. En PUT, omitirla o enviar null conserva la imagen vigente. La API recibe una ruta JSON, no un archivo multipart. Solo admite /images/categorias/... o /uploads/productos/{GUID de 32 caracteres}.jpg|png|webp, hasta 500 caracteres; no admite URLs externas ni segmentos de traversal.',
  'La carga de archivos ocurre en Frontend mediante ProductoImagenStorage (JPG, PNG o WebP, hasta 5 MB), con almacenamiento en wwwroot/uploads/productos. No existe una ruta de subida de imágenes en esta API.',
 ],
 'Categorias': ['GET devuelve Categoria o listas; POST y PUT devuelven Categoria. nombre es obligatorio. DELETE exitoso no devuelve cuerpo.'],
 'Carritos': [
  'Las cuatro rutas /actual trabajan siempre sobre el ID del JWT y devuelven CarritoResumenResponse. GET sin carrito activo devuelve idCarrito=null, items=[], cantidadTotal=0 y total=0; no crea uno por consultar.',
  'POST /actual/productos suma la cantidad a una línea existente o crea la línea y el carrito cuando corresponde. PATCH fija la cantidad absoluta (> 0); no elimina con cero. DELETE devuelve el resumen actualizado (200), o 404 si la línea no existe.',
  'El precioUnitario del AgregarProductoCarritoDto se ignora: el servidor consulta el precio vigente del producto. Stock insuficiente o conflicto concurrente en POST/PATCH /actual: 409. Entrada inválida: 400.',
  'Las rutas CRUD devuelven Carrito o listas. GET / y /usuario/{idUsuario} limitan clientes a sus propios carritos; el administrador puede acceder a los de otros. GET/PUT/DELETE por ID requieren propiedad o rol administrador.',
  'En POST /carritos, para clientes el servidor fija idUsuario, fechaCreacion UTC y estado=Activo. En PUT conserva esos tres valores para clientes. En administradores se utilizan los valores del request.',
  'POST /{idCarrito}/productos devuelve DetalleCarrito y 201; DELETE /{idCarrito}/productos/{idProducto} devuelve 204. Ambos requieren carrito propio o acceso administrador. Usar las rutas /actual para modificar cantidades desde el catálogo.',
  'POST /{idCarrito}/confirmar recibe únicamente idDireccion. Devuelve PedidoResponse: 201 al crear; 200 al repetir con la misma dirección; 409 si ya se confirmó con otra dirección, estado incompatible o stock insuficiente; 404 si faltan recursos; 400 para dirección inválida o carrito vacío.',
  'La confirmación exige dirección perteneciente al usuario del carrito; calcula precios y total, descuenta stock y guarda una copia de la dirección y líneas del pedido en una transacción relacional. No registra ni aprueba un pago.',
 ],
 'Pedidos': [
  'GET / y /usuario/{idUsuario} devuelven listas de PedidoResponse filtradas por propiedad para clientes. GET /{id} devuelve PedidoResponse. El administrador puede acceder a todos; /estado/{estado} es exclusivamente administrativo.',
  'PedidoResponse.detalles se devuelve vacío por el mapper actual en las rutas de pedido y confirmación. Obtener las líneas reales mediante GET /pedidos/{id}/detalles, que devuelve DetallePedidoResponse[]. La dirección es una copia histórica, no una navegación EF.',
  'PATCH /{id}/estado recibe ActualizarEstadoPedidoRequest. Transiciones: Pendiente -> Cancelado (restituye stock), Pagado -> Enviado, Enviado -> Entregado. Repetir el mismo estado es exitoso. Pendiente -> Pagado ocurre por aprobación de pagos, no por este PATCH.',
  'POST /{idPedido}/pagos es administrativo y devuelve PagoResponse (201). Solo admite pedidos pendientes, monto > 0 y metodoPago válido; estado inicial Pendiente y fecha UTC si no se envía. GET /{idPedido}/pagos devuelve PagoResponse[] con control de propiedad.',
  'No existen POST /pedidos, PUT /pedidos/{id} ni DELETE /pedidos/{id}. Los pedidos se crean al confirmar el carrito.',
 ],
 'Pagos': [
  'GET / devuelve PagoResponse[] y requiere administrador; GET /{id} devuelve PagoResponse y requiere propiedad del pedido o administrador.',
  'PATCH /{id}/estado es administrativo y devuelve PagoResponse (200). Un pago Pendiente puede pasar a Aprobado, Rechazado o Cancelado. Repetir el mismo estado es exitoso; otras transiciones: 409.',
  'Aprobar exige pedido Pendiente. La suma de pagos aprobados no puede superar el total; al igualarlo, el pedido pasa a Pagado. No se implementa una pasarela ni un webhook: MetodoPago identifica el método del registro.',
  'No existen POST /pagos, PUT /pagos/{id} ni DELETE /pagos/{id}; crear mediante POST /pedidos/{idPedido}/pagos.',
 ],
 'DetallePedidos': [
  'GET / devuelve DetallePedidoResponse[] solo para administrador. GET /{id} devuelve DetallePedidoResponse si el pedido es propio o el usuario es administrador.',
  'DetallePedidoResponse no incluye idPedido ni propiedades de navegación. No existen POST, PUT ni DELETE para este recurso. Las líneas se generan al confirmar el carrito; precio y subtotal quedan fijados.',
 ],
 'DetalleCarritos': [
  'Las respuestas son DetalleCarrito o listas. GET / devuelve todas las líneas al administrador y solo líneas de carritos propios al cliente. GET por ID o por carrito exige propiedad o administrador.',
  'No existen POST, PUT ni DELETE /detallecarritos. Agregar, cambiar cantidad y eliminar productos mediante /carritos/actual/productos o las rutas por carrito.',
 ],
 'Usuarios': [
  'GET / y POST / requieren administrador. GET y PUT /{id} permiten propietario o administrador. DELETE /{id} y POST /{id}/reactivar son administrativos. Todas las respuestas usan UsuarioResponse con activo, sin contraseña, hash ni versionSesion. El listado incluye activos e inactivos ordenados por idUsuario.',
  'PUT recibe UsuarioActualizacionRequest: nombre, apellido, email, telefono y rol. Conserva contraseña, fechaRegistro y estado. El cliente no puede cambiar su rol; el administrador sí. Email duplicado: 409; campos inválidos: 400.',
  'DELETE realiza baja lógica (204), preserva relaciones e historial. POST /{id}/reactivar devuelve el usuario (200). Repetir el estado actual es exitoso y no duplica auditoría. El email de una cuenta inactiva sigue reservado.',
  'Baja y degradación del último administrador activo devuelven 409. La validación y auditoría se guardan en una transacción serializable SQL Server. El administrador puede operar sobre su cuenta si queda otro administrador activo; la UI cierra su sesión.',
  'El cambio de rol o estado incrementa la versión de sesión: JWT anteriores quedan inválidos, incluso después de reactivar. La API verifica estado y versión en cada solicitud autenticada.',
  'AuditoriaUsuarios registra actor, usuario, acción, fecha UTC y nombres de campos modificados. Valores anteriores/nuevos sólo para rol y activo. No existe pantalla o endpoint de bitácora ni reset administrativo de contraseña.',
 ],
 'Direcciones': [
  'GET / devuelve Direccion[]: las propias para clientes y todas para administradores. GET/PUT/DELETE por ID exigen propietario o administrador. POST y PUT devuelven Direccion.',
  'numero es string, no integer. calle y numero obligatorios; tipo debe ser válido y el usuario indicado debe existir. Para clientes, POST y PUT reemplazan idUsuario por el ID del JWT.',
 ],
 'Proveedores': [
  'Todas las operaciones requieren administrador. GET devuelve Proveedor o listas; POST/PUT devuelven Proveedor. El contrato actual usa razonSocial, cuit, emailComercial, telefonoComercial, condicionIva, direccion anidada, plazos, monedaPreferida y activo.',
  'Obligatorios: razón social, CUIT (máximo 20 caracteres), email comercial, condición IVA, moneda y dirección con calle/numero. Plazos no negativos. La dirección fiscal no tiene usuario asociado. No se documenta validación de dígito verificador del CUIT ni 409 para duplicados porque la lógica no los implementa.',
 ],
 'Compras': [
  'Todas las operaciones requieren administrador. GET devuelve Compra o listas; POST/PUT devuelven Compra. idProveedor debe existir, total >= 0 y estado válido. La lógica asigna fecha actual si se omite.',
  'El recurso registra datos generales de compras a proveedores. No representa el checkout del cliente ni agrega automáticamente líneas de reposición o stock.',
 ],
 'Reportes': [
  'CRUD administrativo: devuelve Reporte o listas, recibe ReporteRequest. tipoReporte válido, usuario existente y fechaInicio <= fechaFin cuando ambas estén informadas.',
  'GET /ventas devuelve ReporteVentasDto: cantidadPedidos y totalVentas. Incluye pedidos Pagado, Enviado o Entregado y suma subtotales de sus detalles.',
  'GET /ingresos devuelve ReporteIngresosDto: cantidadPagosAprobados y totalIngresos. Solo cuenta pagos Aprobado.',
  'GET /productos-mas-vendidos devuelve ProductoMasVendidoDto[], hasta 10 productos ordenados por cantidadVendida en pedidos Pagado, Enviado o Entregado.',
  'Los tres agregados no reciben filtros de fechas. Los registros CRUD de Reporte no aplican automáticamente su período a estos agregados.',
 ],
 'Consultas': [
  'POST es público y devuelve Consulta (201). El servidor fija estado=Pendiente y fechaConsulta UTC; idUsuario se toma del JWT si está autenticado o queda null para visitantes. Los valores equivalentes del request no controlan esos campos.',
  'nombre, email válido y mensaje son obligatorios. GET /, GET /{id}, PUT y DELETE son administrativos. GET /usuario/{idUsuario} permite propietario o administrador y devuelve Consulta[].',
  'El recurso almacena consultas y su estado; no implementa por sí mismo envío de respuestas por email.',
 ],
}

def contracts():
    found = {}
    for path in sorted((ROOT / 'Totaltech/Logica/DTOs').glob('*.cs')):
        text = path.read_text(encoding='utf-8-sig')
        matches = list(re.finditer(r'public (?:sealed )?class (\w+)(?:\s*:\s*(\w+))?', text))
        for i, m in enumerate(matches):
            block = text[m.end():matches[i+1].start() if i+1 < len(matches) else len(text)]
            props = re.findall(r'public ([\w?<>]+) (\w+)\s*\{\s*get;', block)
            found[m.group(1)] = (m.group(2), props, path.relative_to(ROOT).as_posix())
    for name in ['Producto', 'Categoria', 'Carrito', 'DetalleCarrito', 'Direccion', 'Proveedor', 'Compra', 'Reporte', 'Consulta']:
        path = ROOT / f'Totaltech/Entidades/{name}.cs'
        props = re.findall(r'public ([\w?<>]+) (\w+)\s*\{\s*get;', path.read_text(encoding='utf-8-sig'))
        found[name] = (None, props, path.relative_to(ROOT).as_posix())
    return found

def json_type(typ):
    nullable = typ.endswith('?')
    t = typ.rstrip('?')
    basic = {'string':'string', 'int':'integer', 'decimal':'number', 'bool':'boolean', 'DateTime':'string (fecha ISO 8601)'}
    value = basic.get(t, 'integer ('+t+')' if t.startswith(('Estado', 'Tipo', 'Rol', 'Metodo')) else t)
    if '<' in t:
        value = 'array de ' + t[t.index('<')+1:-1]
    return value + (' | null' if nullable else '')

def build_markdown(rows, schemas):
    count = sum(map(len, rows.values()))
    commit = subprocess.check_output(['git','rev-parse','--short','HEAD'], cwd=ROOT, text=True).strip()
    lines = [
      '# TotalTech | Documentación de API', '',
      f'Referencia técnica actualizada al 1 de octubre de 2026. Grupo 1: Daiana Chinellato y Facundo Sola. Rama inspeccionada: Rama--Facu. Commit de referencia: {commit}.', '',
      f'{count} operaciones HTTP · 14 módulos · ASP.NET Core 10 · SQL Server · JWT', '',
      '## 01. Alcance y guía de lectura', '',
      'Esta edición adapta el PDF histórico de 54 páginas al código actual del repositorio interno. La fuente de verdad son los endpoints, DTO, entidades, lógica y repositorios inspeccionados. Los ejemplos son ilustrativos: los ID, importes y fechas no representan datos de una base real.', '',
      'Validación realizada: revisión estática de contratos y cobertura de rutas, generación del PDF y revisión visual. No se ejecutaron llamadas HTTP ni pruebas contra una base de datos. La presencia de un endpoint no demuestra que exista una pantalla MVC completa para ese flujo.', '',
      'El PDF original se conserva como referencia histórica. Esta edición y su fuente editable API.md constituyen la referencia vigente. El generador vuelve a extraer rutas y esquemas; las notas y ejemplos se deben revisar cuando cambie la lógica de negocio.', '',
      '## 02. Conexión y convenciones', '',
      '| Elemento | Valor actual |', '| --- | --- |',
      '| Backend HTTP local | http://localhost:5070 |',
      '| Backend HTTPS local | https://localhost:7038 |',
      '| Rutas de recursos | Sin prefijo /api ni versión en la URL |',
      '| OpenAPI en Development | /openapi/v1.json |',
      '| Scalar en Development | /scalar/v1 (ruta predeterminada de MapScalarApiReference) |',
      '| Cuerpos JSON | Content-Type: application/json; nombres camelCase |',
      '| Identificadores | integer; parámetros de ruta con restricción int |',
      '| Enumeraciones JSON | Valores numéricos; no hay JsonStringEnumConverter configurado |',
      '| Importes | JSON number; no enviar números monetarios como strings |',
      '| Fechas | ISO 8601; los flujos de autenticación/confirmación generan UTC |', '',
      'Los puertos pertenecen a Properties/launchSettings.json y pueden cambiar por configuración del entorno. No hay un host de producción documentado. OpenAPI y Scalar se mapean solo en Development y, al no tener AllowAnonymous explícito, quedan sujetos a la política de autenticación predeterminada.', '',
      'El backend necesita ConnectionStrings:DefaultConnection y Authentication:Issuer, Audience y SigningKey. La clave debe tener al menos 32 bytes UTF-8; usar variables de entorno o User Secrets. Este documento no contiene credenciales. Database:ApplyMigrations controla las migraciones de inicio (por defecto habilitadas en Development); DemoData y BootstrapAdmin son opciones separadas. No se aplicaron migraciones durante esta revisión.', '',
      '## 03. Seguridad y respuestas HTTP', '',
      'Iniciar sesión mediante POST /auth/login y enviar Authorization: Bearer <accessToken> en rutas protegidas. expiresAtUtc indica el vencimiento. El token usa HS256; se verifican firma, emisor, audiencia, vigencia, usuario activo, rol vigente y versión de sesión. Tokens anteriores sin version_sesion requieren nuevo login. ExpirationMinutes tiene valor por defecto 480 y rango 1-1440. No hay endpoint de refresh ni logout en la API.', '',
      '| Etiqueta | Requisito |', '| --- | --- |',
      '| Público | AllowAnonymous; no requiere JWT |',
      '| JWT | Usuario autenticado; verificar propiedad según las notas del módulo |',
      '| Admin | JWT con rol Administrador (no Admin) |', '',
      'Las tablas de rutas muestran los estados explícitos de cada handler. Además, las rutas protegidas pueden devolver 401 por token ausente/inválido, las administrativas 403 por rol insuficiente y el binding puede devolver 400 por JSON o tipos inválidos. Un recurso ajeno suele responder 404 para no revelar su existencia.', '',
      '| Estado | Significado y cuerpo |', '| --- | --- |',
      '| 200 | Lectura, actualización o repetición exitosa; JSON según recurso |',
      '| 201 | Creación; JSON y cabecera Location |',
      '| 204 | Éxito sin cuerpo; no intentar parsear JSON |',
      '| 400 | Validación o binding inválido |',
      '| 401 / 403 | Autenticación / autorización |',
      '| 404 | Recurso inexistente o inaccesible; login usa un objeto específico |',
      '| 409 | Relaciones que impiden eliminar, duplicado de email o conflicto de dominio |', '',
      'No existe un formato único de error: muchos handlers devuelven un string JSON; el catálogo usa ErrorCatalogoResponse; login inexistente devuelve {codigo, mensaje}; algunos errores y 204 no tienen cuerpo. Las listas vacías responden 200 con [], no 404. Los fallos inesperados de infraestructura no tienen contrato de error uniforme declarado.', '',
      '## 04. Catálogo paginado', '',
      '| Query | Tipo y regla |', '| --- | --- |',
      '| texto | string opcional; trim; hasta 100 caracteres |',
      '| idCategoria | integer opcional > 0 |',
      '| precioMin / precioMax | number opcional >= 0; mínimo <= máximo |',
      '| soloDisponibles | boolean; por defecto false |',
      '| pagina | integer >= 1; por defecto 1 |',
      '| tamanoPagina | integer de 1 a 48; por defecto 12 |', '',
      'Ejemplo: GET /productos/catalogo?texto=notebook&soloDisponibles=true&pagina=1&tamanoPagina=12. Error de filtro: 400 con codigo=catalogo_parametros_invalidos y mensaje. Respuesta: items, pagina, tamanoPagina, totalItems y totalPaginas.', '',
      '## 05. Referencia de operaciones', '',
      'Los nombres de body remiten al diccionario de contratos. El guion indica que no se recibe cuerpo JSON. Los parámetros entre llaves pertenecen a la ruta; los query se explican en las notas. GET devuelve una colección en las rutas de listado/filtro y un objeto en las rutas por ID.', '',
    ]
    for key, label, intro in MODULES:
        lines += [f'### {label}', '', intro, '',
                  '| Método | Ruta | Acceso | Body | HTTP |', '| --- | --- | --- | --- | --- |']
        for row in rows[key]:
            lines.append('| ' + ' | '.join(row[k] for k in ['method','route','access','request','statuses']) + ' |')
        lines += [''] + ['- '+n for n in NOTES[key]] + ['', f'Fuente: Totaltech/Endpoints/{key}Endpoints.cs y lógica/repositorios del módulo.', '']
    lines += ['## 06. Ejemplos de integración', '',
      'Los requests siguientes son JSON válido. Sustituir ID por recursos existentes. Las respuestas muestran valores ilustrativos y no son resultados obtenidos por HTTP.', '',
      '### Registro público', '', 'POST /auth/registro', '', '```json',
      json.dumps(dict(nombre='Ana',apellido='Pérez',email='ana@example.com',contrasena='<contraseña de al menos 8 caracteres>',telefono='3493000000'), ensure_ascii=False, indent=2), '```', '',
      'Respuesta 201: UsuarioResponse (idUsuario, nombre, apellido, email, telefono, fechaRegistro, rol=0, activo=true); el registro no emite un token. Ejecutar login a continuación.', '',
      '### Inicio de sesión', '', 'POST /auth/login', '', '```json',
      json.dumps(dict(email='ana@example.com',contrasena='<contraseña elegida>'), ensure_ascii=False,indent=2), '```', '',
      'Respuesta 200 ilustrativa:', '', '```json',
      json.dumps(dict(idUsuario=7,nombre='Ana',apellido='Pérez',email='ana@example.com',telefono='3493000000',fechaRegistro='2026-10-01T12:00:00Z',rol=0,activo=True,accessToken='<JWT emitido por el servidor>',expiresAtUtc='2026-10-01T20:00:00Z'),ensure_ascii=False,indent=2), '```', '',
      '### Producto y stock (administrador)', '', 'POST /productos o PUT /productos/{id}', '', '```json',
      json.dumps(dict(nombre='Notebook de ejemplo',descripcion='Equipo para oficina',imagenUrl=None,precio=850000,stock=10,idCategoria=1,idProveedor=1),ensure_ascii=False,indent=2),'```', '',
      'PATCH /productos/{id}/stock recibe {"stock": 8} y devuelve 204. En PUT, imagenUrl=null conserva la imagen existente.', '',
      '### Dirección de envío', '', 'POST /direcciones (JWT del cliente)', '', '```json',
      json.dumps(dict(calle='Av. Independencia',numero='123',ciudad='Sunchales',provincia='Santa Fe',codigoPostal='2322',pais='Argentina',tipo=0),ensure_ascii=False,indent=2),'```', '',
      'Respuesta 201: Direccion con idDireccion e idUsuario asignado desde el JWT; usar idDireccion en la confirmación.', '',
      '### Agregar al carrito activo', '', 'POST /carritos/actual/productos (JWT del cliente)', '', '```json',
      '{"idProducto": 1, "cantidad": 2}', '```', '',
      'Respuesta 200 ilustrativa:', '', '```json',
      json.dumps(dict(idCarrito=3,items=[dict(idProducto=1,nombre='Notebook de ejemplo',descripcion='Equipo para oficina',precioUnitario=850000,cantidad=2,subtotal=1700000)],cantidadTotal=2,total=1700000),ensure_ascii=False,indent=2),'```', '',
      'PATCH /carritos/actual/productos/1 recibe {"cantidad": 3}. DELETE en la misma ruta elimina la línea y devuelve el resumen actualizado.', '',
      '### Confirmación y consulta de líneas', '', 'POST /carritos/3/confirmar', '', '```json', '{"idDireccion": 5}', '```', '',
      'Respuesta 201 ilustrativa:', '', '```json',
      json.dumps(dict(idPedido=12,idCarrito=3,idUsuario=7,fechaPedido='2026-10-01T12:30:00Z',estado=0,total=1700000,idDireccion=5,direccion=dict(calle='Av. Independencia',numero='123',ciudad='Sunchales',provincia='Santa Fe',codigoPostal='2322',pais='Argentina'),detalles=[]),ensure_ascii=False,indent=2),'```', '',
      'GET /pedidos/12/detalles devuelve las líneas; ejemplo: [{"idDetallePedido": 20, "idProducto": 1, "cantidad": 2, "precioUnitario": 850000, "subtotal": 1700000}]. Repetir la confirmación con dirección 5 devuelve 200 con el mismo pedido.', '',
      '### Registro y aprobación de pago', '', 'POST /pedidos/12/pagos (JWT administrador)', '', '```json', '{"metodoPago": 2, "monto": 1700000}', '```', '',
      'Respuesta 201: PagoResponse con estado=0. PATCH /pagos/{idPago}/estado recibe {"estado": 1} y devuelve el pago aprobado. Si la suma aprobada alcanza el total, el pedido queda Pagado. Luego PATCH /pedidos/12/estado con {"estado": 2} permite enviarlo y con {"estado": 3} entregarlo.', '',
      '### Consulta pública', '', 'POST /consultas', '', '```json',
      json.dumps(dict(nombre='Visitante',email='visitante@example.com',mensaje='Quisiera información sobre envíos.'),ensure_ascii=False,indent=2),'```', '',
      'Respuesta 201: Consulta con idConsulta, idUsuario=null si no hay identidad autenticada, fechaConsulta UTC y estado=0.', '',
      '## 07. Diccionario de contratos', '',
      'Los campos listados se extraen de las propiedades públicas del código. Nullable indica capacidad del tipo de aceptar null; no sustituye la validación de negocio. Los DTO de entrada no contienen clave primaria. En PUT enviar todos los campos que se desean conservar, salvo las excepciones expresamente indicadas.', '',
      'Los contratos de respuesta dedicados son preferibles para integración. Varios recursos todavía serializan entidades EF: sus propiedades de navegación pueden aparecer como objetos o null según la consulta; no asumir que siempre son null ni que están siempre cargadas.', '',
    ]
    used = {r['request'] for group in rows.values() for r in group} - {'-'}
    response_names = ['UsuarioResponse','LoginResponse','ProductoCatalogoResponse','CatalogoProductosResponse','ErrorCatalogoResponse','LineaCarritoResponse','CarritoResumenResponse','DireccionPedidoResponse','DetallePedidoResponse','PedidoResponse','PagoResponse','ReporteVentasDto','ReporteIngresosDto','ProductoMasVendidoDto']
    entity_names = ['Producto','Categoria','Carrito','DetalleCarrito','Direccion','Proveedor','Compra','Reporte','Consulta']
    # Incluir contratos anidados, aunque no sean parámetros directos del handler.
    nested = {'DireccionProveedorRequest'}
    for name in sorted(used | nested) + response_names + entity_names:
        parent, props, src = schemas[name]
        if parent:
            props = schemas[parent][1] + props
        lines += [f'### {name}', '', '| Campo JSON | Tipo |', '| --- | --- |']
        for typ, prop in props:
            lines.append(f'| {prop[0].lower()+prop[1:]} | {json_type(typ).replace(" | ", " o ")} |')
        lines += ['', f'Fuente: {src}', '']
    lines += ['## 08. Enumeraciones y estados', '', '| Tipo | Valores JSON |', '| --- | --- |']
    for path in sorted((ROOT/'Totaltech/Entidades').glob('*.cs')):
        for name, body in re.findall(r'public enum (\w+)\s*\{([^}]+)\}',path.read_text(encoding='utf-8-sig')):
            values = re.findall(r'(\w+)\s*=\s*(\d+)',body)
            lines.append('| '+name+' | '+ '; '.join(f'{num}={label}' for label,num in values)+' |')
    lines += ['',
      'En rutas como GET /pedidos/estado/{estado}, usar el valor numérico (por ejemplo /pedidos/estado/1). Los estados de pedidos y pagos no se pueden cambiar libremente: respetar las transiciones de sus módulos.', '',
      '## 09. Flujo de compra y diferencias con el PDF histórico', '',
      '1. Registrar usuario y obtener JWT mediante login.',
      '2. Consultar /productos/catalogo y agregar productos al carrito /actual.',
      '3. Crear o elegir una dirección propia mediante /direcciones.',
      '4. Confirmar /carritos/{idCarrito}/confirmar; conservar el idPedido y consultar sus detalles.',
      '5. Un administrador registra y aprueba el pago desde /pedidos/{idPedido}/pagos y /pagos/{idPago}/estado.',
      '6. Un administrador cambia el pedido Pagado a Enviado y luego Entregado.', '',
      '| Documento histórico | Estado actual verificado |', '| --- | --- |',
      '| Login sin JWT; email inexistente 401 | JWT con vencimiento; email inexistente 404 |',
      '| rol, estados y métodos como strings | Enumeraciones numéricas en JSON |',
      '| id genérico, categoriaID, proveedorID, usuarioID | idUsuario, idProducto, idCategoria, idProveedor y claves específicas |',
      '| Proveedor con nombreEmpresa y contacto | Razón social, CUIT, condiciones y dirección fiscal anidada |',
      '| CRUD libre de pedidos, pagos y detalles | Creación desde carrito/pedido y cambios de estado controlados |',
      '| DELETE con mensaje o 200 | Normalmente 204; eliminar línea de /actual devuelve resumen 200 |',
      '| Catálogo sin paginación e imágenes | /productos/catalogo paginado e imagenUrl local |',
      '| Sin permisos detallados | Público, JWT, administrador y propiedad por recurso |',
      '| Recuperación descrita como solicitud registrada | Mensaje genérico; sin persistencia, envío ni reset implementados |', '',
      '## 10. Trazabilidad y mantenimiento', '',
      'Fuentes principales: Totaltech/Program.cs; Totaltech/Endpoints/*.cs; Totaltech/Seguridad/*.cs; Totaltech/Logica/DTOs/*.cs; Totaltech/Logica/*Logica.cs; Totaltech/Repositorios/*Repositorio.cs; Totaltech/Entidades/*.cs; Totaltech/Validaciones/ProductoImagenUrl.cs; Frontend/Services/ProductoImagenStorage.cs y controladores de producto.', '',
      'El alcance documental no modifica endpoints, permisos, dependencias, reglas de negocio ni esquema. El frontend contiene módulos todavía reservados; no se asegura un checkout MVC completo solo porque las operaciones backend estén disponibles.', '',
      'Para regenerar desde la raíz interna: python Documentacion/tools/generar_api.py. Requiere reportlab en el intérprete. Produce Documentacion/API.md y output/pdf/Documentacion-de-API.pdf. Revisar las notas y ejemplos junto al código antes de regenerar. El PDF contiene marcadores de navegación por sección.', '',
    ]
    return '\n'.join(lines)

NAVY = colors.HexColor('#102638')
TEAL = colors.HexColor('#007F86')
PALE = colors.HexColor('#EDF5F6')
INK = colors.HexColor('#263746')
GRAY = colors.HexColor('#627383')

class ReferenceDoc(SimpleDocTemplate):
    def afterFlowable(self, flowable):
        if isinstance(flowable, Paragraph) and flowable.style.name == 'Section':
            title = flowable.getPlainText()
            key = 'section-'+str(self.page)
            self.canv.bookmarkPage(key)
            self.canv.addOutlineEntry(title, key, 0, False)
            if title != 'Contenido':
                self.notify('TOCEntry', (0, title, self.page, key))

def render_pdf(markdown, route_count):
    regular = Path('C:/Windows/Fonts/arial.ttf')
    bold = Path('C:/Windows/Fonts/arialbd.ttf')
    mono = Path('C:/Windows/Fonts/consola.ttf')
    for name, path in [('BodyFont',regular),('BodyBold',bold),('CodeFont',mono)]:
        pdfmetrics.registerFont(TTFont(name,str(path)))
    styles = {
      'body': ParagraphStyle('Body',fontName='BodyFont',fontSize=9,leading=13.5,textColor=INK,spaceAfter=8),
      'h2': ParagraphStyle('Section',fontName='BodyBold',fontSize=20,leading=25,textColor=NAVY,spaceAfter=16),
      'h3': ParagraphStyle('Subsection',fontName='BodyBold',fontSize=12,leading=16,textColor=TEAL,spaceBefore=13,spaceAfter=8,keepWithNext=True),
      'cell': ParagraphStyle('Cell',fontName='BodyFont',fontSize=7.4,leading=10.5,textColor=INK,wordWrap='CJK'),
      'head': ParagraphStyle('HeadCell',fontName='BodyBold',fontSize=7.5,leading=10,textColor=colors.white),
      'code': ParagraphStyle('Code',fontName='CodeFont',fontSize=8,leading=10.5,textColor=NAVY,backColor=PALE,borderPadding=10,spaceBefore=4,spaceAfter=12),
    }
    def para(text, style='body'):
        return Paragraph(escape(text),styles[style])
    story = [Spacer(1,38), Paragraph('TOTALTECH',ParagraphStyle('Brand',fontName='BodyBold',fontSize=14,textColor=TEAL,leading=20)),
             Spacer(1,28), Paragraph('Documentación<br/>de API',ParagraphStyle('CoverTitle',fontName='BodyBold',fontSize=38,leading=44,textColor=NAVY)),
             Spacer(1,20), Paragraph('Referencia técnica del sistema<br/>de venta de productos tecnológicos',ParagraphStyle('Subtitle',fontName='BodyFont',fontSize=14,leading=21,textColor=GRAY)),
             Spacer(1,34)]
    stats = Table([[str(route_count),'14','JWT'],['OPERACIONES HTTP','MÓDULOS','AUTENTICACIÓN']],colWidths=[166,166,166])
    stats.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,-1),PALE),('FONTNAME',(0,0),(-1,0),'BodyBold'),('FONTSIZE',(0,0),(-1,0),24),('TEXTCOLOR',(0,0),(-1,0),TEAL),('FONTNAME',(0,1),(-1,1),'BodyFont'),('FONTSIZE',(0,1),(-1,1),7.5),('TEXTCOLOR',(0,1),(-1,1),GRAY),('LEFTPADDING',(0,0),(-1,-1),14),('TOPPADDING',(0,0),(-1,0),18),('BOTTOMPADDING',(0,0),(-1,1),14)]))
    story += [stats,Spacer(1,45),para('Grupo 1 · Daiana Chinellato · Facundo Sola'),para('Edición del 1 de octubre de 2026'),para('ASP.NET Core 10 / Entity Framework Core / SQL Server'),Spacer(1,16),para('Contratos contrastados con el código actual. Ejemplos ilustrativos y fuente editable incluidas.'),PageBreak(),para('Contenido','h2')]
    toc = TableOfContents()
    toc.levelStyles = [ParagraphStyle('TOC', fontName='BodyFont', fontSize=10,
                                     leading=17, spaceBefore=9, textColor=NAVY)]
    story.append(toc)
    story += [Spacer(1,15),para('Lectura sugerida: conexión y seguridad para comenzar; referencia de operaciones para integrar; diccionario para revisar campos y tipos; diferencias para migrar desde la documentación anterior.')]
    lines = markdown.splitlines()
    pending_heading = None
    pending_code_heading = None
    pending_code_label = None
    # La portada ya muestra los metadatos; el contenido empieza por el alcance.
    i = next(i for i,line in enumerate(lines) if line.startswith('## '))
    while i < len(lines):
        line = lines[i]
        if line.startswith('# ') or not line.strip():
            i += 1; continue
        if line.startswith('## '):
            story += [PageBreak(),para(line[3:],'h2')]
        elif line.startswith('### '):
            heading = para(line[4:],'h3')
            if i+2 < len(lines) and lines[i+2].startswith('|'):
                pending_heading = heading
            elif i+4 < len(lines) and lines[i+4].startswith('```'):
                pending_code_heading = heading
            else:
                story.append(heading)
        elif line.startswith('```'):
            code=[]; i+=1
            while i<len(lines) and not lines[i].startswith('```'):
                code.append(lines[i]); i+=1
            # Ajustar líneas largas sin romper el contenido de la fuente editable.
            import textwrap
            wrapped=[]
            for item in code:
                wrapped += textwrap.wrap(item,width=91,replace_whitespace=False,drop_whitespace=False) or ['']
            block = [Preformatted('\n'.join(wrapped),styles['code'])]
            if pending_code_label is not None:
                block.insert(0,pending_code_label)
                pending_code_label = None
            if pending_code_heading is not None:
                block.insert(0,pending_code_heading)
                pending_code_heading = None
            story.append(KeepTogether(block))
        elif line.startswith('|'):
            data=[]
            while i<len(lines) and lines[i].startswith('|'):
                cells=[c.strip() for c in lines[i].strip('|').split('|')]
                if not all(re.fullmatch(r'[- :]+',c) for c in cells): data.append(cells)
                i+=1
            n=len(data[0])
            widths = [49,175,49,152,73] if n==5 else [155,343]
            table=Table([[para(c,'head' if idx==0 else 'cell') for c in row] for idx,row in enumerate(data)],colWidths=widths,repeatRows=1,hAlign='LEFT',splitByRow=0)
            table.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,0),NAVY),('ROWBACKGROUNDS',(0,1),(-1,-1),[colors.white,PALE]),('VALIGN',(0,0),(-1,-1),'TOP'),('LEFTPADDING',(0,0),(-1,-1),7),('RIGHTPADDING',(0,0),(-1,-1),7),('TOPPADDING',(0,0),(-1,-1),7),('BOTTOMPADDING',(0,0),(-1,-1),7),('LINEBELOW',(0,0),(-1,0),1,TEAL)]))
            if i+1 < len(lines) and lines[i+1].startswith('Fuente:'):
                block = [table,Spacer(1,10),para(lines[i+1])]
                if pending_heading is not None:
                    block.insert(0,pending_heading)
                    pending_heading = None
                story.append(KeepTogether(block))
                i += 2
            else:
                story += [table,Spacer(1,10)]
            continue
        elif line.startswith('- '):
            story.append(Paragraph('• '+escape(line[2:]),styles['body']))
        else:
            paragraph = para(line)
            if i+2 < len(lines) and lines[i+2].startswith('```'):
                pending_code_label = paragraph
            elif i+2 < len(lines) and lines[i+2].startswith('|'):
                paragraph.keepWithNext = True
                story.append(paragraph)
            else:
                story.append(paragraph)
        i+=1
    OUT.parent.mkdir(parents=True,exist_ok=True)
    doc=ReferenceDoc(str(OUT),pagesize=A4,rightMargin=48,leftMargin=49,topMargin=65,bottomMargin=55,title='TotalTech - Documentación de API',author='Grupo 1 - Daiana Chinellato y Facundo Sola')
    def furniture(canvas,doc):
        w,h=A4
        canvas.saveState()
        if doc.page>1:
            canvas.setStrokeColor(TEAL);canvas.setLineWidth(1.2);canvas.line(49,h-39,w-48,h-39)
            canvas.setFont('BodyBold',8);canvas.setFillColor(NAVY);canvas.drawString(49,h-29,'TOTALTECH / REFERENCIA DE API')
            canvas.setFont('BodyFont',7.5);canvas.setFillColor(GRAY);canvas.drawRightString(w-48,h-29,'01 OCT 2026')
        canvas.setStrokeColor(colors.HexColor('#D4DFE4'));canvas.setLineWidth(.5);canvas.line(49,40,w-48,40)
        canvas.setFont('BodyFont',7);canvas.setFillColor(GRAY);canvas.drawString(49,27,'Grupo 1 · Documentación técnica')
        canvas.drawRightString(w-48,27,f'{doc.page:02d}')
        canvas.restoreState()
    doc.multiBuild(story,onFirstPage=furniture,onLaterPages=furniture)

if __name__=='__main__':
    rows=endpoints()
    schemas=contracts()
    md=build_markdown(rows,schemas)
    SOURCE.write_text(md+'\n',encoding='utf-8')
    render_pdf(md,sum(map(len,rows.values())))
    print(f'Operaciones: {sum(map(len,rows.values()))}; módulos: {len(rows)}')
    print(f'Fuente: {SOURCE}\nPDF: {OUT}')
