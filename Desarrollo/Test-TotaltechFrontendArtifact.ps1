[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PublishPath,
    [string]$SourceRoot = (Join-Path $PSScriptRoot '..')
)

$ErrorActionPreference = 'Stop'
$published = (Resolve-Path -LiteralPath $PublishPath).Path
$source = (Resolve-Path -LiteralPath $SourceRoot).Path
foreach ($file in @('Frontend.dll', 'Frontend.deps.json', 'Frontend.runtimeconfig.json', 'web.config', 'wwwroot/css/site.css')) {
    if (-not (Test-Path -LiteralPath (Join-Path $published $file) -PathType Leaf)) {
        throw "Falta $file en el artefacto."
    }
}
if (Test-Path -LiteralPath (Join-Path $published 'Totaltech.dll')) { throw 'El paquete contiene el backend.' }
[xml]$config = Get-Content -LiteralPath (Join-Path $published 'web.config') -Raw
if ($config.configuration.location.'system.webServer'.aspNetCore.arguments -ne '.\Frontend.dll') {
    throw 'web.config apunta a otra aplicación.'
}

$files = @(Get-ChildItem -LiteralPath $published -Recurse -File)
foreach ($file in $files) {
    $relative = [IO.Path]::GetRelativePath($published, $file.FullName).Replace('\', '/')
    if ($relative -match '(?i)(^|/)uploads/') {
        throw "El artefacto incluye contenido de uploads: $relative"
    }
    if ($file.Name -like '*staticwebassets*.json') {
        # También rechaza endpoints y variantes comprimidas referidos por manifiestos.
        $manifest = (Get-Content -LiteralPath $file.FullName -Raw).Replace('\', '/')
        if ($manifest -match '(?i)uploads/+productos') {
            throw "El manifiesto $relative incluye referencias a uploads de productos."
        }
    }
}

$imageRoot = Join-Path $source 'Frontend/wwwroot/images'
$images = @(Get-ChildItem -LiteralPath $imageRoot -Recurse -File |
    Where-Object { $_.Extension -in @('.jpg', '.jpeg', '.png', '.webp') })
if ($images.Count -eq 0) { throw 'No se encontraron imágenes estáticas en el código fuente.' }
foreach ($image in $images) {
    $relative = [IO.Path]::GetRelativePath($imageRoot, $image.FullName)
    $target = Join-Path (Join-Path $published 'wwwroot/images') $relative
    if (-not (Test-Path -LiteralPath $target -PathType Leaf)) { throw "Falta imagen estática: $relative" }
    if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne
        (Get-FileHash -LiteralPath $image.FullName -Algorithm SHA256).Hash) {
        throw "La imagen publicada difiere del original: $relative"
    }
}
[pscustomobject]@{ Status = 'PASS'; StaticImages = $images.Count; Uploads = 0; BackendIncluded = $false }
