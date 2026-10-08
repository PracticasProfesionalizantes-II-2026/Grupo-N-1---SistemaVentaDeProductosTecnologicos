# Gestión administrativa de usuarios — 08/10/2026

UC-24 implementado para modificación de datos y rol, baja lógica y reactivación desde /Usuarios. El alta continúa en /Home/Register y la autenticación en /Home/Login. No hay alta en el panel ni reset administrativo de contraseña. UC-32 contiene registro persistente de acciones, sin pantalla de consulta.

## Persistencia y despliegue

La migración GestionAdministrativaUsuarios agrega Activo=true y VersionSesion=1 a los usuarios existentes y crea AuditoriaUsuarios. No elimina usuarios ni modifica hashes o relaciones históricas. Aplicar primero en una base desechable y después únicamente en una base autorizada, antes de iniciar la nueva versión de API/MVC. Deshabilitar Database:ApplyMigrations durante verificaciones contra bases compartidas. Los JWT anteriores no tienen version_sesion y exigirán nuevo login.

El bootstrap crea cuentas inexistentes; no restaura contraseña, rol o estado de cuentas ya administradas. Los emails inactivos siguen reservados. Una baja o degradación propia es posible sólo si queda otro administrador activo; la sesión se cierra tras la operación.

Rollback: detener API/MVC antes de volver a la migración anterior y al código anterior. El Down elimina columnas de estado/versión y la tabla de auditoría: exportar esa información antes de un rollback autorizado; la versión anterior volvería a admitir cuentas inactivadas. No ejecutar automáticamente sobre datos reales.

## Pruebas reproducibles

- dotnet build Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln
- dotnet test Tests/Totaltech.UnitTests/Totaltech.UnitTests.csproj
- dotnet test Tests/Totaltech.IntegrationTests/Totaltech.IntegrationTests.csproj --filter "Category!=SqlServer&Category!=E2E&Category!=UI"
- dotnet test Tests/Totaltech.IntegrationTests/Totaltech.IntegrationTests.csproj --filter "Category=SqlServer"
- dotnet test Tests/Totaltech.IntegrationTests/Totaltech.IntegrationTests.csproj --filter "FullyQualifiedName~UsuariosAdministracionE2ETests"

SQL Server y E2E usan exclusivamente LocalDB y bases TotaltechTests_<GUID>, con guardas y limpieza. E2E requiere Chromium de Playwright. La referencia vigente de contratos es API.md; los PDF históricos no contienen este cambio.

## Validación realizada

08/10/2026, sobre el árbol de trabajo de Rama--Facu (sin commit):
- Restore de la solución correcto.
- Build de la solución: 0 errores, 0 advertencias.
- Suite completa: 17 pruebas unitarias y 143 de integración, todas correctas, sin omisiones (160 en total).
- Incluye SQL Server real en bases desechables, migración incremental con datos anteriores, atomicidad, concurrencia, rehash obsoleto y recorridos de navegador.
- Usuarios verificados a 320, 375, 425, 768, 1024 y 1280 px; revisión visual de capturas móvil/escritorio.
- git diff --check correcto.
- No se aplicó la migración a una base compartida ni se realizaron commit o push.
