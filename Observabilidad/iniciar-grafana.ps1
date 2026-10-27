[CmdletBinding()]
param(
    [string] $GrafanaHome = (Join-Path $env:ProgramFiles 'GrafanaLabs/grafana'),
    [ValidateRange(1024, 65535)]
    [int] $Port = 3000
)

$ErrorActionPreference = 'Stop'
$installation = (Resolve-Path -LiteralPath $GrafanaHome).Path
$executable = Join-Path $installation 'bin/grafana.exe'
$configuration = Join-Path $PSScriptRoot 'Grafana/grafana.ini'
$provisioning = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot 'Grafana/provisioning')).Path
$dashboards = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot 'Grafana/dashboards')).Path
$runtime = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../Desarrollo/logs/grafana'))

foreach ($file in @($executable, $configuration, (Join-Path $installation 'conf/defaults.ini'))) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        throw "Archivo requerido no encontrado: $file. Consultar Documentacion/Agente-Diagnostico-Observabilidad.md."
    }
}

$previousDashboardPath = $env:TOTALTECH_GRAFANA_DASHBOARDS_PATH
try {
    $env:TOTALTECH_GRAFANA_DASHBOARDS_PATH = $dashboards
    foreach ($directory in @('data', 'log', 'plugins')) {
        New-Item -ItemType Directory -Path (Join-Path $runtime $directory) -Force | Out-Null
    }
    Write-Host "Grafana: http://127.0.0.1:$Port. Detener con Ctrl+C."
    Write-Host 'Datasource y dashboard se cargan desde Observabilidad/Grafana/.'
    & $executable server "--homepath=$installation" "--config=$configuration" `
        "cfg:server.http_addr=127.0.0.1" "cfg:server.http_port=$Port" "cfg:server.root_url=http://127.0.0.1:$Port/" `
        "cfg:default.paths.data=$runtime/data" "cfg:default.paths.logs=$runtime/log" `
        "cfg:default.paths.plugins=$runtime/plugins" "cfg:default.paths.provisioning=$provisioning"
    if ($LASTEXITCODE -ne 0) { throw "Grafana terminó con código $LASTEXITCODE." }
}
finally {
    $env:TOTALTECH_GRAFANA_DASHBOARDS_PATH = $previousDashboardPath
}
