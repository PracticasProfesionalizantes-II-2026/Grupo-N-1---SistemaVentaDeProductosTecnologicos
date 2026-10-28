# Comparación de esquema Azure SQL — 28/10/2026

Fuente esperada: script CREATE generado desde el DbContext actual, sin conexión a Azure. Fuente real: sys.columns, índices, FK, restricciones e historial de TotatechDB. Lecturas únicamente.

| Entidad/tabla | Columnas esperadas | Diferencias | Impacto |
|---|---:|---|---|
| AuditoriaUsuarios | 10 | Falta IdAuditoriaUsuario; Falta IdActor; Falta IdUsuario; Falta Accion; Falta FechaUtc; Falta CamposModificados; Falta RolAnterior; Falta RolNuevo; Falta ActivoAnterior; Falta ActivoNuevo | Bloquea autenticación/gestión de usuarios |
| Carritos | 4 | Ninguna | Sin diferencia de columnas/tipos/nullability |
| Categorias | 3 | Ninguna | Sin diferencia de columnas/tipos/nullability |
| Compras | 5 | Ninguna | Sin diferencia de columnas/tipos/nullability |
| Consultas | 7 | Ninguna | Sin diferencia de columnas/tipos/nullability |
| DetalleCarritos | 6 | Ninguna | Sin diferencia de columnas/tipos/nullability |
| DetallePedidos | 6 | Ninguna | Sin diferencia de columnas/tipos/nullability |
| Direcciones | 9 | Ninguna | Sin diferencia de columnas/tipos/nullability |
| Pagos | 6 | Ninguna | Sin diferencia de columnas/tipos/nullability |
| Pedidos | 13 | Falta IdCarrito; Falta Total; Falta DireccionCalle; Falta DireccionNumero; Falta DireccionCiudad; Falta DireccionProvincia; Falta DireccionCodigoPostal; Falta DireccionPais | Revisar antes de escrituras |
| Productos | 8 | Ninguna | Sin diferencia de columnas/tipos/nullability |
| Proveedores | 11 | Ninguna | Sin diferencia de columnas/tipos/nullability |
| Reportes | 5 | Ninguna | Sin diferencia de columnas/tipos/nullability |
| Usuarios | 10 | Falta Activo; Falta VersionSesion | Bloquea autenticación/gestión de usuarios |


## Índices, relaciones e historial

- Claves foráneas: 15. Deshabilitadas/no confiables: 0.
- Índices: 30. Constraints CHECK: 0.
- La inspección completa de metadatos queda en publish/azure-schema.json, ignorado por Git. No se exportaron registros ni credenciales.
- Historial EF: cinco migraciones de junio; la inicial actual y las de septiembre/octubre no figuran.
- Productos.ImagenUrl ya existe con nvarchar(500) NULL.
- No registrar una baseline ficticia ni activar migraciones automáticas. Comparar por separado columnas nuevas, defaults, índices y constraints antes de reconciliar el historial.
