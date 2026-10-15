[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [ValidateSet('iniciar', 'detener', 'reiniciar', 'estado', 'comandos')]
    [string] $Accion = 'iniciar',
    [switch] $SinCompilar
)

$ErrorActionPreference = 'Stop'
$raizTotaltech = [IO.Path]::GetFullPath($PSScriptRoot)
foreach ($rutaRequerida in 'AGENTS.md', 'Totaltech/Totaltech.csproj', 'Frontend/Frontend.csproj') {
    if (-not (Test-Path -LiteralPath (Join-Path $raizTotaltech $rutaRequerida))) {
        throw 'Ejecutar el script de la raiz que contiene Frontend, Totaltech y AGENTS.md.'
    }
}
$componentesTotaltech = @(
    @{ Nombre = 'API'; Proyecto = 'Totaltech'; Puerto = 5070; Comprobacion = '/productos/catalogo?tamanoPagina=1' },
    @{ Nombre = 'Frontend'; Proyecto = 'Frontend'; Puerto = 5087; Comprobacion = '/Home/Login' }
)
foreach ($componente in $componentesTotaltech) {
    $componente.Carpeta = Join-Path $raizTotaltech $componente.Proyecto
    $componente.Csproj = Join-Path $componente.Carpeta ($componente.Proyecto + '.csproj')
    [xml] $proyectoXml = Get-Content -LiteralPath $componente.Csproj -Raw
    $frameworkTotaltech = $proyectoXml.SelectSingleNode('/Project/PropertyGroup/TargetFramework').InnerText
    $componente.Dll = Join-Path $componente.Carpeta ('bin/Debug/' + $frameworkTotaltech + '/' + $componente.Proyecto + '.dll')
    $componente.Url = 'http://localhost:' + $componente.Puerto
    $componente.PatronProceso = '(?i)^\s*(?:"[^"]*dotnet(?:\.exe)?"|[^\s"]*dotnet(?:\.exe)?)\s+(?:exec\s+)?"?' +
        [regex]::Escape($componente.Dll.Replace('/', '\')) + '"?(?:\s|$)'
}

function Test-ProcesoTotaltech($Proceso, $Componente) {
    return $null -ne $Proceso -and $null -ne $Proceso.CommandLine -and
        $Proceso.Name -eq 'dotnet.exe' -and
        $Proceso.CommandLine.Replace('/', '\') -match $Componente.PatronProceso
}

function Get-ProcesosTotaltech($Componente) {
    Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
        Where-Object { Test-ProcesoTotaltech $_ $Componente }
}

function Get-EscuchasTotaltech($Componente) {
    Get-NetTCPConnection -State Listen -LocalPort $Componente.Puerto -ErrorAction SilentlyContinue
}

function Assert-PuertosTotaltech([switch] $PermitirProcesosSinEscucha) {
    foreach ($componente in $componentesTotaltech) {
        $identificadores = @(Get-ProcesosTotaltech $componente | ForEach-Object { $_.ProcessId })
        foreach ($escucha in @(Get-EscuchasTotaltech $componente)) {
            if ($escucha.OwningProcess -notin $identificadores) {
                throw "Puerto $($componente.Puerto) ocupado por otro proceso (PID $($escucha.OwningProcess)). No se detuvo ese proceso. Usar .\totaltech.ps1 estado."
            }
        }
        if (-not $PermitirProcesosSinEscucha -and $identificadores.Count -gt 1) {
            throw "Hay varias instancias de $($componente.Nombre). Usar .\totaltech.ps1 detener antes de iniciar."
        }
        if (-not $PermitirProcesosSinEscucha -and $identificadores.Count -gt 0 -and @(Get-EscuchasTotaltech $componente).Count -eq 0) {
            throw "$($componente.Nombre) esta iniciandose o usa otro puerto. Consultar estado; detener antes de recompilar."
        }
    }
}

function Show-EstadoTotaltech {
    foreach ($componente in $componentesTotaltech) {
        $procesos = @(Get-ProcesosTotaltech $componente)
        $escuchas = @(Get-EscuchasTotaltech $componente)
        if ($escuchas.Count -eq 0) {
            $detalle = if ($procesos.Count) { 'proceso presente, sin escucha en el puerto esperado' } else { 'detenido; puerto libre' }
            Write-Host "$($componente.Nombre): $detalle ($($componente.Url))"
        } else {
            foreach ($identificador in @($escuchas.OwningProcess | Select-Object -Unique)) {
                $pertenece = $identificador -in @($procesos.ProcessId)
                $detalle = if ($pertenece) { 'este repositorio' } else { 'OTRO PROCESO: no se detendra automaticamente' }
                Write-Host "$($componente.Nombre): puerto $($componente.Puerto), PID $identificador, $detalle"
            }
        }
    }
}

function Stop-ComponentesTotaltech {
    foreach ($componente in @($componentesTotaltech[1], $componentesTotaltech[0])) {
        foreach ($proceso in @(Get-ProcesosTotaltech $componente)) {
            # Revalidar identidad y fecha evita detener otro proceso si Windows reutilizo el PID.
            $vigente = Get-CimInstance Win32_Process -Filter "ProcessId = $($proceso.ProcessId)"
            if ((Test-ProcesoTotaltech $vigente $componente) -and $vigente.CreationDate -eq $proceso.CreationDate) {
                $handle = Get-Process -Id $vigente.ProcessId -ErrorAction SilentlyContinue
                if ($handle) {
                    Stop-Process -Id $handle.Id -ErrorAction Stop
                    if (-not $handle.WaitForExit(10000)) { throw "No termino $($componente.Nombre)." }
                    Write-Host "$($componente.Nombre) detenido (PID $($handle.Id))."
                }
            }
        }
    }
}

function Wait-ComponenteTotaltech($Componente, $Proceso) {
    for ($intento = 0; $intento -lt 40; $intento++) {
        if ($Proceso -and $Proceso.HasExited) {
            throw "$($Componente.Nombre) termino al iniciar. Revisar Desarrollo/logs."
        }
        try {
            $respuesta = Invoke-WebRequest -Uri ($Componente.Url + $Componente.Comprobacion) -UseBasicParsing -TimeoutSec 2
            if ([int] $respuesta.StatusCode -eq 200) { return }
        } catch { }
        Start-Sleep -Milliseconds 500
    }
    throw "$($Componente.Nombre) no respondio correctamente. Revisar Desarrollo/logs."
}

function Start-ComponentesTotaltech {
    Assert-PuertosTotaltech
    $faltantes = @($componentesTotaltech | Where-Object { @(Get-ProcesosTotaltech $_).Count -eq 0 })
    if ($faltantes.Count -eq 0) {
        & sqllocaldb start MSSQLLocalDB
        if ($LASTEXITCODE -ne 0) { throw 'No inicio MSSQLLocalDB. Consultar Documentacion/DesarrolloLocal.md.' }
        foreach ($componente in $componentesTotaltech) { Wait-ComponenteTotaltech $componente $null }
        Write-Host 'TotalTech ya esta ejecutandose. Para aplicar cambios usar .\totaltech.ps1 reiniciar.'
        Show-EstadoTotaltech
        return
    }
    $dotnetTotaltech = (Get-Command dotnet -ErrorAction Stop).Source
    foreach ($componente in $faltantes) {
        if (-not $SinCompilar) {
            & $dotnetTotaltech build $componente.Csproj --nologo
            if ($LASTEXITCODE -ne 0) { throw "Fallo la compilacion de $($componente.Nombre)." }
        }
        if (-not (Test-Path -LiteralPath $componente.Dll)) {
            throw "Falta compilar $($componente.Nombre). Ejecutar iniciar sin -SinCompilar."
        }
    }
    # El perfil local usa una sola instancia; no adjunta archivos ni modifica el esquema.
    & sqllocaldb start MSSQLLocalDB
    if ($LASTEXITCODE -ne 0) { throw 'No inicio MSSQLLocalDB. Revisar su error.log; no borrar ni recrear la base.' }

    $rutaLogsTotaltech = Join-Path $raizTotaltech 'Desarrollo/logs'
    New-Item -ItemType Directory -Path $rutaLogsTotaltech -Force | Out-Null
    $iniciados = @()
    try {
        foreach ($componente in $componentesTotaltech) {
            if (@(Get-ProcesosTotaltech $componente).Count -gt 0) {
                Write-Host "$($componente.Nombre) ya estaba ejecutandose; se conserva."
                Wait-ComponenteTotaltech $componente $null
                continue
            }
            # Los cambios de entorno solo afectan este comando y sus hijos, no la configuracion de Windows.
            $variables = @{
                ASPNETCORE_ENVIRONMENT = 'Development'
                ASPNETCORE_URLS = $componente.Url
            }
            if ($componente.Nombre -eq 'API') {
                $variables.ConnectionStrings__DefaultConnection = 'Server=(localdb)\MSSQLLocalDB;Database=TotaltechDev;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True'
            } else {
                $variables.ApiBaseUrl = $componentesTotaltech[0].Url
            }
            $anteriores = @{}
            try {
                foreach ($clave in $variables.Keys) {
                    $anteriores[$clave] = [Environment]::GetEnvironmentVariable($clave, 'Process')
                    [Environment]::SetEnvironmentVariable($clave, $variables[$clave], 'Process')
                }
                $marca = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
                $argumentos = @(('"' + $componente.Dll + '"'), '--Database:ApplyMigrations=false', '--DemoData:Enabled=false', '--BootstrapAdmin:Enabled=false')
                $proceso = Start-Process -FilePath $dotnetTotaltech -ArgumentList $argumentos -WorkingDirectory $componente.Carpeta `
                    -WindowStyle Hidden -PassThru `
                    -RedirectStandardOutput (Join-Path $rutaLogsTotaltech "$($componente.Nombre)-$marca.stdout.log") `
                    -RedirectStandardError (Join-Path $rutaLogsTotaltech "$($componente.Nombre)-$marca.stderr.log")
                $identidad = Get-CimInstance Win32_Process -Filter "ProcessId = $($proceso.Id)"
                $iniciados += @{ Componente = $componente; Proceso = $proceso; Creacion = $identidad.CreationDate }
            } finally {
                foreach ($clave in $anteriores.Keys) { [Environment]::SetEnvironmentVariable($clave, $anteriores[$clave], 'Process') }
            }
            Wait-ComponenteTotaltech $componente $proceso
            Write-Host "$($componente.Nombre) disponible: $($componente.Url) (PID $($proceso.Id))."
        }
        Write-Host 'Ambas aplicaciones quedan en segundo plano. Detener con .\totaltech.ps1 detener.'
    } catch {
        # Un fallo parcial solo revierte procesos creados por esta ejecucion.
        foreach ($entrada in $iniciados) {
            $vigente = Get-CimInstance Win32_Process -Filter "ProcessId = $($entrada.Proceso.Id)"
            if ((Test-ProcesoTotaltech $vigente $entrada.Componente) -and $vigente.CreationDate -eq $entrada.Creacion) {
                Stop-Process -Id $vigente.ProcessId -ErrorAction SilentlyContinue
            }
        }
        throw
    }
}

try {
    if ($Accion -eq 'comandos') {
        Write-Host '.\totaltech.ps1 iniciar              Iniciar API y frontend; conserva instancias ya iniciadas.'
        Write-Host '.\totaltech.ps1 estado               Mostrar puertos, PID y pertenencia al repositorio.'
        Write-Host '.\totaltech.ps1 reiniciar            Detener, recompilar e iniciar ambas aplicaciones.'
        Write-Host '.\totaltech.ps1 detener              Liberar los puertos cerrando solo estas aplicaciones.'
        Write-Host '.\totaltech.ps1 iniciar -SinCompilar  Iniciar con las DLL ya compiladas.'
        Write-Host 'Guia completa: Documentacion/DesarrolloLocal.md'
        return
    }
    if ($Accion -eq 'estado') { Show-EstadoTotaltech; return }
    if (-not $PSCmdlet.ShouldProcess($raizTotaltech, $Accion)) { return }

    $hashTotaltech = [Security.Cryptography.SHA256]::Create()
    try { $identidadRaiz = [BitConverter]::ToString($hashTotaltech.ComputeHash([Text.Encoding]::UTF8.GetBytes($raizTotaltech.ToLowerInvariant()))).Replace('-', '') }
    finally { $hashTotaltech.Dispose() }
    $mutexTotaltech = New-Object Threading.Mutex($false, ('Local\Totaltech-' + $identidadRaiz.Substring(0, 24)))
    $tomado = $false
    try {
        try { $tomado = $mutexTotaltech.WaitOne(0) } catch [Threading.AbandonedMutexException] { $tomado = $true }
        if (-not $tomado) { throw 'Otra operacion de TotalTech esta en curso. Esperar y consultar estado.' }
        switch ($Accion) {
            'detener' { Stop-ComponentesTotaltech; Show-EstadoTotaltech }
            'reiniciar' { Assert-PuertosTotaltech -PermitirProcesosSinEscucha; Stop-ComponentesTotaltech; Start-ComponentesTotaltech }
            'iniciar' { Start-ComponentesTotaltech }
        }
    } finally {
        if ($tomado) { $mutexTotaltech.ReleaseMutex() }
        $mutexTotaltech.Dispose()
    }
} catch {
    Write-Error $_.Exception.Message -ErrorAction Continue
    exit 1
}
