[CmdletBinding()]
param(
    [string] $DistributionPath = (Join-Path $PSScriptRoot '../ControlProyecto/prometheus-3.15.0.windows-amd64/prometheus-3.15.0.windows-amd64'),
    [string] $DataPath = (Join-Path $env:LOCALAPPDATA 'Totaltech/Prometheus/data')
)

$ErrorActionPreference = 'Stop'
$distribution = (Resolve-Path -LiteralPath $DistributionPath).Path
$prometheus = Join-Path $distribution 'prometheus.exe'
$promtool = Join-Path $distribution 'promtool.exe'
$configuration = Join-Path $PSScriptRoot 'prometheus.yml'
$rules = Join-Path $PSScriptRoot 'alertas.yml'

foreach ($file in @($prometheus, $promtool, $configuration, $rules)) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        throw "Archivo requerido no encontrado: $file. Consultar Documentacion/Observabilidad.md."
    }
}

& $promtool check config $configuration
if ($LASTEXITCODE -ne 0) { throw 'La configuración de Prometheus no es válida.' }
& $promtool check rules $rules
if ($LASTEXITCODE -ne 0) { throw 'Las reglas de Prometheus no son válidas.' }

New-Item -ItemType Directory -Path $DataPath -Force | Out-Null
Write-Host 'Prometheus: http://127.0.0.1:9090. Detener con Ctrl+C.'
Write-Host 'Iniciar API y Frontend por separado con el perfil TotalTech completo.'

# Ejecución en primer plano: no inicia aplicaciones ni aplica migraciones.
& $prometheus "--config.file=$configuration" '--web.listen-address=127.0.0.1:9090' `
    "--storage.tsdb.path=$DataPath" '--storage.tsdb.retention.time=15d' '--storage.tsdb.retention.size=1GB'
if ($LASTEXITCODE -ne 0) { throw "Prometheus terminó con código $LASTEXITCODE." }
