# Desarrollo local y comandos utiles

## Causa y solucion

El error `address already in use` significa que otra instancia sigue escuchando en el mismo puerto. API usa **5070** y frontend **5087**. Repetir `dotnet run` o recompilar no libera el puerto; una aplicacion abierta tambien puede bloquear la copia de su DLL durante la compilacion.

Usar un solo modo de inicio. El recomendado es `totaltech.ps1`, ubicado en la raiz que contiene `AGENTS.md`, `Frontend/` y `Totaltech/` (hay un repositorio exterior con un nombre similar).

El gestor:

- Comprueba los puertos antes de compilar y distingue los procesos de esta copia de otros procesos.
- Inicia LocalDB y comprueba que el catalogo responda antes de iniciar el frontend.
- Ejecuta ambas DLL en segundo plano, sin abrir terminales adicionales. Una segunda llamada a `iniciar` conserva los procesos ya disponibles.
- Serializa iniciar/detener/reiniciar para evitar dos arranques simultaneos.
- Detiene solamente las DLL Debug de esta copia, revalidando PID, ruta y fecha de inicio. No termina todos los procesos `dotnet` del equipo.
- Si el inicio falla parcialmente, cierra solo los procesos nuevos de esa ejecucion.
- No aplica migraciones, no adjunta archivos MDF, no cambia secretos y no carga datos demo. La base `TotaltechDev` debe existir y tener el esquema requerido.

El gestor rechaza los puertos ocupados por aplicaciones ajenas antes de compilar o conectar a la base, con un aviso y codigo 1. Este control esta en el comando de desarrollo; no cambia los programas ni su comportamiento en Production. Un `dotnet run` manual sigue requiriendo un puerto libre.

## Uso cotidiano

Ejecutar desde la raiz indicada:

| Comando | Uso |
|---|---|
| `.\totaltech.ps1 iniciar` | Compilar e iniciar API y frontend. |
| `.\totaltech.ps1 estado` | Ver puertos, PID y si pertenecen a este repositorio. |
| `.\totaltech.ps1 reiniciar` | Detener, recompilar e iniciar despues de cambiar codigo. |
| `.\totaltech.ps1 detener` | Cerrar estas aplicaciones y liberar sus puertos. |
| `.\totaltech.ps1 iniciar -SinCompilar` | Iniciar con las DLL ya compiladas. |
| `.\totaltech.ps1 detener -WhatIf` | Mostrar la operacion sin ejecutarla. |
| `.\totaltech.ps1 comandos` | Mostrar la ayuda breve. |

Despues de `iniciar`, abrir [TotalTech](http://localhost:5087). El catalogo API se puede consultar en [5070](http://localhost:5070/productos/catalogo).

Requisitos: Windows PowerShell 5.1 o PowerShell 7, SDK .NET 10, SQL Server LocalDB con la instancia `MSSQLLocalDB`, base existente `TotaltechDev` y la configuracion Authentication de desarrollo mediante User Secrets o variables de entorno. El gestor fija la conexion local solo en el entorno de sus procesos hijos, sin mostrar ni modificar claves JWT.

Si el puerto pertenece a otra aplicacion, el gestor informa su PID y se detiene. Cerrar esa aplicacion desde su terminal o herramienta original; no cambiar de puerto al azar ni terminar procesos desconocidos. Para procesos Release, `dotnet watch` u otras herramientas, detenerlos desde la herramienta que los inicio antes de usar el gestor.

## Logs y diagnostico

```powershell
# Estado de las aplicaciones de esta copia
.\totaltech.ps1 estado

# Todos los listeners de los puertos usados
Get-NetTCPConnection -State Listen |
    Where-Object { $_.LocalPort -in 5070,5087 } |
    Select-Object LocalAddress,LocalPort,OwningProcess

# Logs mas recientes del gestor
Get-ChildItem .\Desarrollo\logs -File |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 6 Name,LastWriteTime,Length

# Leer un archivo concreto de los mostrados arriba (reemplazar FECHA)
Get-Content .\Desarrollo\logs\API-FECHA.stdout.log -Tail 80

# Verificar HTTP sin autenticar
Invoke-WebRequest http://localhost:5070/productos/catalogo -UseBasicParsing
Invoke-WebRequest http://localhost:5087/Home/Login -UseBasicParsing

# Instancias SQL y estado de la instancia usada
sqllocaldb info
sqllocaldb info MSSQLLocalDB
sqllocaldb start MSSQLLocalDB

# Registro del motor si SQL no inicia
Get-Content "$env:LOCALAPPDATA\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB\error.log" -Tail 80
```

Los nombres de logs incluyen fecha y hora. Los archivos quedan ignorados por Git. No publicar logs que contengan informacion de usuarios o configuracion privada.

Si SQL falla, comprobar que no este activa otra instancia usando los mismos archivos de `TotaltechDev`. No borrar ni volver a adjuntar la base para resolver un bloqueo. `detener` no cierra LocalDB, porque puede estar siendo utilizada por otras herramientas. No usar `taskkill /IM dotnet.exe /F`: cerraria aplicaciones ajenas.

Si un log muestra `0x800711C7` o `Una directiva de Control de aplicaciones bloqueo este archivo`, es un bloqueo de Windows sobre una DLL o ejecutable, distinto del conflicto de puertos. Consultar los eventos antes de cambiar la configuracion; el gestor no modifica ni desactiva esa proteccion. Microsoft explica el control de archivos sin firma o reputacion en [Smart App Control](https://learn.microsoft.com/windows/apps/develop/smart-app-control/overview).

```powershell
Get-WinEvent -FilterHashtable @{LogName='Microsoft-Windows-CodeIntegrity/Operational';Id=3033,3077} -MaxEvents 5 |
    Select-Object TimeCreated,Id,Message
```

## Compilar y probar

Detener primero las aplicaciones para evitar DLL bloqueadas:

```powershell
.\totaltech.ps1 detener
dotnet restore .\Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln
dotnet build .\Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln --no-restore
dotnet test .\Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln --no-build
.\totaltech.ps1 iniciar -SinCompilar
```

Las pruebas SQL existentes utilizan bases temporales `TotaltechTests_*`, separadas de `TotaltechDev`.

## Alternativa: dos terminales

Si se prefiere el control directo, ejecutar primero `totaltech.ps1 detener` y no volver a usar `iniciar` mientras esten abiertas estas ejecuciones.

Terminal API, desde la raiz:

```powershell
sqllocaldb start MSSQLLocalDB
dotnet run --project .\Totaltech\Totaltech.csproj --launch-profile TotaltechLocalDb -- --Database:ApplyMigrations=false
```

Terminal frontend, desde la misma raiz:

```powershell
dotnet run --project .\Frontend\Frontend.csproj --launch-profile http
```

Detener cada uno con **Ctrl+C** en su terminal. Antes de recompilar o abrirlos con Visual Studio, verificar `totaltech.ps1 estado`. El nombre de perfil `TotaltechLocalDb` selecciona la instancia **MSSQLLocalDB**, no una instancia llamada `TotaltechLocalDb`.
