# Manifiesto propuesto de pruebas de escritura en Azure

Pendiente de autorización específica. No ejecutar contra registros comerciales.

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
