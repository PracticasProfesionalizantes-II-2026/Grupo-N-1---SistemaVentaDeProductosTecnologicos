param(
    [Parameter(Mandatory)][uri]$FrontendUrl,
    [Parameter(Mandatory)][uri]$ApiUrl,
    [ValidateRange(1, 20)][int]$Attempts = 12,
    [ValidateRange(1, 30)][int]$DelaySeconds = 10
)
$ErrorActionPreference = 'Stop'
if ($FrontendUrl.Scheme -ne 'https' -or $ApiUrl.Scheme -ne 'https') {
    throw 'La validación pública requiere HTTPS.'
}
$front = $FrontendUrl.AbsoluteUri.TrimEnd('/')
$api = $ApiUrl.AbsoluteUri.TrimEnd('/')
for ($attempt = 1; $attempt -le $Attempts; $attempt++) {
    try {
        $categories = Invoke-RestMethod "$api/categorias/" -TimeoutSec 30
        $catalog = Invoke-RestMethod "$api/productos/catalogo" -TimeoutSec 30
        if ($categories.Count -eq 0 -or $catalog.totalItems -lt 1 -or @($catalog.items).Count -eq 0) {
            throw 'La API no devolvió categorías y productos reales.'
        }
        $homeResponse = Invoke-WebRequest "$front/" -TimeoutSec 30
        $products = Invoke-WebRequest "$front/Productos" -TimeoutSec 30
        $css = Invoke-WebRequest "$front/css/site.css" -TimeoutSec 30
        if ($homeResponse.StatusCode -ne 200 -or $products.StatusCode -ne 200 -or $css.StatusCode -ne 200) {
            throw 'Estado HTTP inesperado en el frontend.'
        }
        if ($homeResponse.Content -notmatch 'data-inicio-productos' -or
            -not [Net.WebUtility]::HtmlDecode($homeResponse.Content).Contains($catalog.items[0].nombre) -or
            -not [Net.WebUtility]::HtmlDecode($products.Content).Contains($catalog.items[0].nombre)) {
            throw 'El HTML MVC no contiene los productos de la API.'
        }
        [pscustomobject]@{
            Utc = [DateTime]::UtcNow.ToString('o')
            Frontend = $front
            Categories = $categories.Count
            Products = $catalog.totalItems
            Home = 200
            Catalog = 200
            Css = 200
        } | ConvertTo-Json -Compress
        exit 0
    }
    catch {
        if ($attempt -eq $Attempts) { throw 'No se pudo verificar el flujo público dentro del plazo. Revisar eventlog.xml y logs de ambas aplicaciones.' }
        Write-Host "Esperando aplicaciones y SQL: intento $attempt de $Attempts."
        Start-Sleep -Seconds $DelaySeconds
    }
}
