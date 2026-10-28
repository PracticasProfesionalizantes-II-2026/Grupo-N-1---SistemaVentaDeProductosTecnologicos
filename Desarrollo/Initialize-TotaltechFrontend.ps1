[CmdletBinding(SupportsShouldProcess)]
param()
$ErrorActionPreference = 'Stop'
$subscription = 'c86846c1-d545-4505-a3cf-089a86c1636e'
$group = 'Servidores'
$app = 'TotaltechFrontend'
$planId = "/subscriptions/$subscription/resourceGroups/$group/providers/Microsoft.Web/serverfarms/ASP-Servidores-a321"
$appId = "/subscriptions/$subscription/resourceGroups/$group/providers/Microsoft.Web/sites/$app"
$principalId = 'df2e4f8d-d08f-4169-8d81-755433575ace'

function Invoke-Azure {
    param([string[]]$Arguments)
    & az @Arguments
    if ($LASTEXITCODE -ne 0) { throw 'Falló una operación Azure. Revisar el último error; no se continuará con las siguientes.' }
}

$account = Invoke-Azure @('account', 'show', '-o', 'json') | ConvertFrom-Json
if ($account.id -ne $subscription) { throw 'Seleccionar la suscripción Azure for Students autorizada antes de ejecutar.' }
$plan = Invoke-Azure @('appservice', 'plan', 'show', '--ids', $planId, '-o', 'json') | ConvertFrom-Json
if ($plan.sku.name -ne 'F1' -or $plan.reserved -eq $true) { throw 'Se requiere el plan F1 Windows existente; no se cambiará de SKU.' }
$sites = @(Invoke-Azure @('webapp', 'list', '-g', $group, '-o', 'json') | ConvertFrom-Json)
$existing = @($sites | Where-Object name -eq $app)
if ($existing.Count -gt 0 -and $existing[0].serverFarmId -ne $planId) { throw 'El nombre pertenece a otra configuración; detener.' }
if ($existing.Count -eq 0) {
    $usage = Invoke-Azure @('rest', '--method', 'get', '--url', ("https://management.azure.com" + $planId + "/usages?api-version=2024-04-01"), '-o', 'json') | ConvertFrom-Json
    $siteQuota = $usage.value | Where-Object { $_.name.value -eq 'NumberOfSites' }
    if ($null -eq $siteQuota -or $siteQuota.currentValue -ge $siteQuota.limit) { throw 'No se confirmó cuota libre para otra Web App.' }
    if ($PSCmdlet.ShouldProcess($appId, 'Crear Web App Windows .NET 10 en el plan F1 existente')) {
        Invoke-Azure @('webapp', 'create', '-g', $group, '-n', $app, '--plan', $planId, '--runtime', 'dotnet:10', '--query', '{name:name,host:defaultHostName}', '-o', 'json')
    }
}

if ($PSCmdlet.ShouldProcess($appId, 'Configurar HTTPS, TLS, runtime y variables no sensibles')) {
    Invoke-Azure @('webapp', 'update', '-g', $group, '-n', $app, '--https-only', 'true', '--query', '{name:name,httpsOnly:httpsOnly}', '-o', 'json')
    Invoke-Azure @('webapp', 'config', 'set', '-g', $group, '-n', $app, '--net-framework-version', 'v10.0', '--min-tls-version', '1.2', '--ftps-state', 'Disabled', '--always-on', 'false', '--query', '{runtime:netFrameworkVersion,minTls:minTlsVersion,scmTls:scmMinTlsVersion}', '-o', 'json')
    # El archivo evita que az.cmd expanda %HOME% usando el entorno de la máquina local.
    $settingsFile = Join-Path $env:TEMP ("totaltech-frontend-settings-" + [Guid]::NewGuid().ToString('N') + ".json")
    try {
        @{
            ASPNETCORE_ENVIRONMENT = 'Production'
            ApiBaseUrl = 'https://totaltechapp-ghd2etambcgcfgcb.brazilsouth-01.azurewebsites.net/'
            ApiTimeoutSeconds = '60'
            Observability__Enabled = 'false'
            CompraDemo__Enabled = 'false'
            ProductoImagenes__Directorio = '%HOME%\data\Totaltech\productos'
            WEBSITE_RUN_FROM_PACKAGE = '1'
        } | ConvertTo-Json | Set-Content -LiteralPath $settingsFile -Encoding utf8
        Invoke-Azure @('webapp', 'config', 'appsettings', 'set', '-g', $group, '-n', $app,
            '--settings', ("@" + $settingsFile), '--query', '[].name', '-o', 'json')
    }
    finally { if (Test-Path -LiteralPath $settingsFile) { Remove-Item -LiteralPath $settingsFile } }
    foreach ($policy in @('scm', 'ftp')) {
        Invoke-Azure @('resource', 'update', '--ids', "$appId/basicPublishingCredentialsPolicies/$policy", '--api-version', '2024-04-01', '--set', 'properties.allow=false', '--query', 'properties.allow', '-o', 'json')
    }
}

if ($PSCmdlet.ShouldProcess($appId, 'Asignar Website Contributor a la identidad OIDC existente, solo en esta Web App')) {
    Invoke-Azure @('role', 'assignment', 'create', '--assignee-object-id', $principalId,
        '--assignee-principal-type', 'ServicePrincipal', '--role', 'Website Contributor', '--scope', $appId,
        '--query', '{scope:scope,role:roleDefinitionId}', '-o', 'json')
}
if (-not $WhatIfPreference) {
    Invoke-Azure @('webapp', 'show', '-g', $group, '-n', $app, '--query', '{name:name,host:defaultHostName,httpsOnly:httpsOnly,plan:serverFarmId}', '-o', 'json')
}
