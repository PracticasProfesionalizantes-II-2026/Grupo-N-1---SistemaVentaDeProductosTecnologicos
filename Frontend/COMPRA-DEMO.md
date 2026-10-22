# Compra ficticia

La simulación se habilita con `CompraDemo:Enabled`. El valor general es `false`; Development lo activa explícitamente. Para desactivarla en cualquier entorno usar `CompraDemo__Enabled=false` y reiniciar el Frontend. También se puede ajustar la configuración del entorno.

Con una cuenta autenticada y productos en el carrito, elegir **Simular compra**, seleccionar aprobación, rechazo o demora y pulsar **Simular pago**. Los plazos son 2, 2 y 6 segundos. Una aprobación muestra confirmación y un seguimiento que se avanza manualmente.

El flujo sólo lee el carrito mediante el servicio existente. No confirma carritos, no descuenta stock y no registra pedidos o pagos en la API. El carrito conserva sus productos.

Los pedidos se guardan bajo `Totaltech.CompraDemo.v1.usuario.<id>` en localStorage. El intento de esta pestaña usa la misma clave con sufijo `.intento` en sessionStorage, conserva la copia original y evita duplicar pedidos al reintentar o recargar. **Nueva simulación** inicia otro intento. Los datos locales son ficticios y editables; no tienen validez comercial ni se sincronizan con otro navegador. Nunca guardar datos personales ni de tarjeta.

**Mis pedidos demo** permite consultar y borrar sólo el historial demo del usuario actual, con confirmación. Un historial corrupto no se borra automáticamente. Si no se puede persistir, no se declara la compra completada.

## Recuperación

Desactivar el flag oculta los accesos y bloquea las pantallas demo sin tocar el carrito. Ante una regresión grave atribuible a estos cambios, detener el trabajo y restaurar únicamente los archivos de esta tarea desde su copia anterior; conservar cambios locales ajenos. No usar reset o clean destructivos.

La copia local de recuperación y los manifiestos de archivos originales/nuevos están en `Frontend/obj/CompraDemoBackup-20261013/`, ignorados por Git. Los originales corresponden a HEAD: se verificó que estos archivos no tenían cambios locales al comenzar. Conservar esa carpeta hasta aceptar el cambio; una limpieza de artefactos podría eliminarla.

Las pruebas E2E usan la fixture existente con una base desechable `TotaltechTests_<GUID>`. No ejecutar pruebas contra la base de la tienda.
