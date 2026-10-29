# Manifiesto propuesto de pruebas de escritura en Azure

Autorizado específicamente por el usuario y ejecutado el 28/10/2026. No se operó contra registros comerciales.

## Resultado del lote

| Identificador | Estado final |
|---|---|
| Cliente 15 | Baja lógica, Rol Cliente, sin privilegios; permanece para conservar la auditoría |
| Producto 12 | Eliminado después de verificar persistencia; precio 1 y stock 3 hasta su eliminación |
| Carrito 11 | Ítem removido por API; carrito vacío eliminado con guardas de pertenencia |
| Imagen b56332aadcf349aeaa51f65dbd0745e2.png | Eliminada del directorio persistente, ETag verificado; URL final 404 |
| Imagen b47c9858b79f4080817ef129e59f9aee.png | Eliminada del directorio persistente, ETag verificado; URL final 404 |
| Auditoría de usuario 15 | Conservada: 1 registro de baja |

PASS: registro MVC, duplicado rechazado, login incorrecto, login correcto y cookie Secure/HttpOnly/Lax, autorización Cliente/Admin, carrito API/MVC, dos uploads, CSRF, logout, revocación de cookie/JWT y bloqueo del usuario inactivo.

PASS: sesión, producto, carrito e imágenes conservados después de reinicio y redeploy del mismo ZIP frontend. Ambos App Services quedaron Running. No se reinició ni desplegó la API como operación del lote.

Estado inicial: 14 usuarios, 11 productos, 6 categorías, 10 carritos, 10 pedidos, 10 pagos y 0 auditorías. Estado final: 14 usuarios originales más el cliente inactivo 15; 11 productos, 6 categorías, 10 carritos, 10 pedidos y 10 pagos; 1 auditoría del usuario temporal. Comparación JSON de todos los productos comerciales contra la lectura inicial: idéntica.

La limpieza de producto/carrito usó una transacción SQL limitada a IDs 12/11, con cuenta 15 inactiva, carrito vacío, ausencia de pedidos del cliente y nombre/precio/stock/ImagenUrl exactos. No hubo conflictos referenciales ni se deshabilitaron restricciones. La baja del cliente se realizó mediante MVC para conservar la auditoría. No se borró el usuario para evitar perder esa trazabilidad.

Evidencia adicional sin secretos: publish/evidencia/TT-AZURE-20261028.json y publish/TT-AZURE-20261028-before.json / -cleanup.json. Contraseña, tokens y cookies se mantuvieron exclusivamente en memoria.

## Alcance autorizado y aplicado

Identificador del lote: TT-AZURE-20261028. Registrar IDs devueltos antes de cualquier limpieza.

| Operación | Alcance máximo | Efecto permitido propuesto |
|---|---|---|
| Registro | 1 cliente azure-demo-20261028@totaltech.invalid | Usuario cliente, sin privilegios; contraseña aleatoria privada, nunca en Git/logs |
| Registro duplicado | Mismo email | Debe rechazar sin crear otra fila |
| Login/logout | Esa cuenta | Cookie/JWT privados; probar contraseña errónea y CSRF |
| Producto de prueba | 1 producto TT-AZURE-20261028, precio 1, stock 3 | Categoría/proveedor existentes; no editar los 11 productos comerciales |
| Upload | Hasta 2 imágenes PNG válidas pequeñas y rechazos controlados | Asociarlas exclusivamente al producto dedicado |
| Carrito | Solo el usuario/producto dedicado | Agregar, cambiar cantidad y quitar; no comprar ni modificar stock comercial |
| Reinicio/redeploy frontend | Sin cambiar SQL | Verificar cookie, imagen y configuración persistentes |
| Limpieza | Solo IDs registrados del lote | Quitar detalle/carrito según API, eliminar producto si no tiene referencias y desactivar cliente con administrador autorizado; conservar auditoría |

Se requiere que el usuario introduzca en privado las credenciales del administrador existente. No crear/elevar un administrador ni restablecer contraseñas. Si no está disponible, las pruebas administrativas quedan BLOCKED y las públicas continúan.

Checkout/pagos/pedidos reales excluidos: CompraDemo=false y esquema de Pedidos pendiente. Se validan en LocalDB aislado.

Antes de empezar: registrar conteos, estado de respaldo y resultado HTTP público. Después: confirmar que datos comerciales y sus cantidades/stock no cambiaron y documentar IDs dedicados que deban permanecer por restricciones referenciales.
