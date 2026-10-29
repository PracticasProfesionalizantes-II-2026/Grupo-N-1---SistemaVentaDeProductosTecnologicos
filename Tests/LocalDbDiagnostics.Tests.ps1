#requires -Version 5.1
# Pruebas aisladas: no ejecutan el gestor, LocalDB ni conexiones SQL reales.
$ErrorActionPreference = 'Stop'
$rutaGestor = Join-Path (Split-Path $PSScriptRoot -Parent) 'totaltech.ps1'
$tokens = $null
$errores = $null
$ast = [Management.Automation.Language.Parser]::ParseFile($rutaGestor, [ref]$tokens, [ref]$errores)
if ($errores.Count) { throw ($errores | Out-String) }
foreach ($definicion in $ast.FindAll({ param($nodo) $nodo -is [Management.Automation.Language.FunctionDefinitionAst] }, $false)) {
    # Cargar solo funciones: nunca ejecutar el bloque principal de iniciar/detener.
    . ([scriptblock]::Create($definicion.Extent.Text))
}

$localAppDataAnterior = $env:LOCALAPPDATA
$baseTemporal = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$nombreTemporal = 'TotaltechLocalDbDiagnostics_' + [guid]::NewGuid().ToString('N')
$raizPruebas = Join-Path $baseTemporal $nombreTemporal
$script:casos = 0
New-Item -ItemType Directory -Path $raizPruebas | Out-Null

function Assert-Condicion([bool] $Condicion, [string] $Mensaje) {
    if (-not $Condicion) { throw $Mensaje }
}

function Test-Caso([string] $Nombre, [scriptblock] $Prueba) {
    & $Prueba
    $script:casos++
    Write-Host "PASS: $Nombre"
}

function Reset-Escenario {
    $env:LOCALAPPDATA = $localAppDataAnterior
    $script:raizTotaltech = Join-Path $raizPruebas ([guid]::NewGuid().ToString('N'))
    $script:consultas = 0
    $script:fallosConexion = 0
    $script:comandosSql = New-Object 'Collections.Generic.List[string]'
    $script:codigoNativo = 0
    $script:errorNativo = ''
    $script:faltaUtilidad = $false
    $script:fallaProceso = $false
    function script:Test-BaseTotaltech {
        $script:consultas++
        if ($script:consultas -le $script:fallosConexion) {
            throw (New-Object InvalidOperationException('Conexion simulada rechazada'))
        }
    }
    function script:Get-Command {
        param($Name, $CommandType, $ErrorAction)
        if ($script:faltaUtilidad) { throw 'SqlLocalDB.exe no instalado (simulado)' }
        [pscustomobject]@{ Source = 'SqlLocalDB-simulado.exe' }
    }
    function script:Invoke-UtilidadLocalDb {
        param($RutaUtilidad, $Argumentos)
        $script:comandosSql.Add($Argumentos)
        if ($script:fallaProceso) { throw 'No se pudo crear proceso (simulado)' }
        [pscustomobject]@{ ExitCode = $script:codigoNativo; Salida = 'Mensaje nativo simulado'; Error = $script:errorNativo }
    }
    function script:Start-Sleep { param($Milliseconds) }
}

function Get-LogPrueba {
    $log = @(Get-ChildItem (Join-Path $script:raizTotaltech 'Desarrollo/logs') -Filter 'SQL-*.log' |
        Where-Object { $_.Name -notmatch '\.(stdout|stderr)\.log$' })
    Assert-Condicion ($log.Count -eq 1) 'Debe existir un diagnostico consolidado por ejecucion.'
    Get-Content -LiteralPath $log[0].FullName -Raw
}

try {
    Test-Caso 'El proceso real conserva salida, stderr y codigo no cero' {
        $resultado = Invoke-UtilidadLocalDb $env:ComSpec '/d /c "echo salida-prueba & echo error-prueba 1>&2 & exit /b 37"'
        Assert-Condicion ($resultado.ExitCode -eq 37) 'Se perdio el codigo real del proceso.'
        Assert-Condicion ($resultado.Salida -match 'salida-prueba') 'Falta stdout.'
        Assert-Condicion ($resultado.Error -match 'error-prueba') 'Falta stderr.'
    }
    Test-Caso 'Un codigo cero no borra el mensaje de stderr' {
        $resultado = Invoke-UtilidadLocalDb $env:ComSpec '/d /c "echo advertencia-prueba 1>&2 & exit /b 0"'
        Assert-Condicion ($resultado.ExitCode -eq 0 -and $resultado.Error -match 'advertencia-prueba') 'No se conservaron ambos datos.'
    }
    Test-Caso 'El proceso conserva codigos negativos y su representacion hexadecimal' {
        $resultado = Invoke-UtilidadLocalDb $env:ComSpec '/d /c "exit /b -1"'
        Assert-Condicion ($resultado.ExitCode -eq -1 -and ('{0:X8}' -f $resultado.ExitCode) -eq 'FFFFFFFF') 'Se altero el codigo negativo.'
    }
    Test-Caso 'Los mensajes ANSI conservan las tildes tambien en PowerShell 7' {
        $powershellWindows = Join-Path $env:SystemRoot 'System32/WindowsPowerShell/v1.0/powershell.exe'
        $argumentos = '-NoProfile -NonInteractive -Command "$m=''informaci''+[char]243+''n''; $b=[Text.Encoding]::Default.GetBytes($m); [Console]::OpenStandardError().Write($b,0,$b.Length)"'
        $resultado = Invoke-UtilidadLocalDb $powershellWindows $argumentos
        Assert-Condicion ($resultado.Error -eq ('informaci' + [char]243 + 'n')) 'Se perdieron caracteres del mensaje nativo.'
    }
    Test-Caso 'El contexto informa las rutas sin modificar el entorno' {
        $env:LOCALAPPDATA = Join-Path $raizPruebas 'perfil-redirigido'
        $contexto = Get-ContextoLocalDb
        Assert-Condicion ($contexto.LocalAppDataEfectivo -eq $env:LOCALAPPDATA) 'No se informa la ruta efectiva.'
        Assert-Condicion $contexto.PosibleRedireccion 'No se detecto la diferencia del perfil.'
        Assert-Condicion ($env:LOCALAPPDATA -eq (Join-Path $raizPruebas 'perfil-redirigido')) 'El diagnostico modifico el entorno.'
    }
    Test-Caso 'Conexion disponible conserva el camino habitual sin comandos nativos' {
        Reset-Escenario
        Start-BaseTotaltech
        Assert-Condicion ($script:consultas -eq 1 -and $script:comandosSql.Count -eq 0) 'El camino disponible debe retornar inmediatamente.'
        Assert-Condicion ((Get-LogPrueba) -match '\[Disponibilidad\] Consulta de solo lectura confirmada') 'Falta evidencia de la consulta exitosa.'
    }
    Test-Caso 'Un perfil redirigido valido no bloquea la conexion' {
        Reset-Escenario
        $env:LOCALAPPDATA = Join-Path $raizPruebas 'redirigido-valido'
        Start-BaseTotaltech
        $log = Get-LogPrueba
        Assert-Condicion ($log -match 'PosibleRedireccion.:true' -and $log -match 'Puede ser una redireccion valida') 'La redireccion debe ser una senal, no un error.'
        Assert-Condicion ($script:consultas -eq 1 -and $script:comandosSql.Count -eq 0) 'Se bloqueo un contexto disponible.'
    }
    Test-Caso 'Un fallo transitorio se recupera por consulta incluso con codigo nativo no cero' {
        Reset-Escenario
        $script:fallosConexion = 1
        $script:codigoNativo = 37
        $script:errorNativo = 'fallo nativo simulado'
        Start-BaseTotaltech
        $log = Get-LogPrueba
        Assert-Condicion ($script:consultas -eq 2 -and $script:comandosSql.Count -eq 2) 'Debe conservarse el reintento y comprobar conexion real.'
        Assert-Condicion ($log -match 'ExitCode=37 \(0x00000025\)' -and $log -match 'stderr: fallo nativo simulado') 'Faltan codigo o mensaje nativo.'
    }
    Test-Caso 'Codigo cero con stderr no convierte una conexion fallida en exito' {
        Reset-Escenario
        $script:fallosConexion = 10
        $script:errorNativo = 'SQL Server process failed to start'
        $fallo = $null
        try { Start-BaseTotaltech } catch { $fallo = $_ }
        Assert-Condicion ($null -ne $fallo -and $fallo.Exception.Message -match '\[Disponibilidad\]') 'Falta el fallo de disponibilidad.'
        Assert-Condicion ($script:consultas -eq 3) 'Deben mantenerse las tres comprobaciones.'
        Assert-Condicion (@($script:comandosSql | Where-Object { $_ -eq 'start MSSQLLocalDB' }).Count -eq 2) 'No deben agregarse arranques extra.'
        $log = Get-LogPrueba
        foreach ($texto in '\[Contexto\]', '\[Conexion\]', '\[Instancia\]', '\[Disponibilidad\]', 'ExitCode=0', 'SQL Server process failed to start') {
            Assert-Condicion ($log -match $texto) "Falta diagnostico: $texto"
        }
    }
    Test-Caso 'La ausencia de utilidad se informa despues de comprobar conexion' {
        Reset-Escenario
        $script:fallosConexion = 10
        $script:faltaUtilidad = $true
        $fallo = $null
        try { Start-BaseTotaltech } catch { $fallo = $_ }
        Assert-Condicion ($null -ne $fallo -and $script:consultas -eq 1) 'Debe probarse conexion antes de depender de la utilidad.'
        Assert-Condicion ((Get-LogPrueba) -match '\[Instancia\] No se encontro SqlLocalDB.exe') 'Falta diagnostico de instalacion/utilidad.'
    }
    Test-Caso 'El fallo al crear un proceso queda registrado sin ocultar conexion' {
        Reset-Escenario
        $script:fallosConexion = 10
        $script:fallaProceso = $true
        try { Start-BaseTotaltech } catch { }
        $log = Get-LogPrueba
        Assert-Condicion ($log -match 'No se pudo ejecutar SqlLocalDB' -and $log -match '\[Conexion\]') 'Falta la causa del fallo de proceso o conexion.'
    }
    Test-Caso 'LOCALAPPDATA ausente no impide comprobar una conexion disponible' {
        Reset-Escenario
        $env:LOCALAPPDATA = $null
        Start-BaseTotaltech
        Assert-Condicion ($script:consultas -eq 1) 'Una ruta ausente no debe impedir la comprobacion.'
    }
    Test-Caso 'El gestor conserva componentes que ya estaban iniciados' {
        Reset-Escenario
        $script:componentesTotaltech = @(@{ Nombre = 'API' }, @{ Nombre = 'Frontend' })
        $script:esperas = 0
        function script:Assert-PuertosTotaltech { }
        function script:Get-ProcesosTotaltech { param($Componente) [pscustomobject]@{ ProcessId = 123 } }
        function script:Wait-ComponenteTotaltech { param($Componente, $Proceso) $script:esperas++ }
        function script:Show-EstadoTotaltech { }
        Start-ComponentesTotaltech
        Assert-Condicion ($script:consultas -eq 1 -and $script:esperas -eq 2) 'Cambio el flujo de componentes ya iniciados.'
    }
    Write-Host "Correctas: $script:casos pruebas aisladas. Sin conexiones SQL reales."
} finally {
    $env:LOCALAPPDATA = $localAppDataAnterior
    # Limitar la limpieza al directorio temporal exclusivo creado por esta prueba.
    $rutaLimpieza = [IO.Path]::GetFullPath($raizPruebas)
    if ((Split-Path $rutaLimpieza -Parent).TrimEnd([char[]]'\/') -ne $baseTemporal.TrimEnd([char[]]'\/') -or
        (Split-Path $rutaLimpieza -Leaf) -ne $nombreTemporal) {
        throw 'La ruta de limpieza no coincide con el directorio temporal de pruebas.'
    }
    Remove-Item -LiteralPath $rutaLimpieza -Recurse -Force
}
