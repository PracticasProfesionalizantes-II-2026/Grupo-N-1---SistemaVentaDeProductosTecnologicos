namespace Frontend.Configuracion;

public sealed record ConfiguracionApi(Uri BaseAddress, TimeSpan Timeout)
{
    public static ConfiguracionApi Leer(IConfiguration configuration, IHostEnvironment environment)
    {
        var valor = configuration["ApiBaseUrl"];
        if (!Uri.TryCreate(valor, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https") || !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) ||
            environment.IsProduction() && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "ApiBaseUrl debe ser una URI HTTP absoluta, sin credenciales, query ni fragmento; Production requiere HTTPS.");
        }

        var segundos = configuration.GetValue<int?>("ApiTimeoutSeconds") ?? 10;
        if (segundos is < 1 or > 300)
            throw new InvalidOperationException("ApiTimeoutSeconds debe estar entre 1 y 300 segundos.");

        return new ConfiguracionApi(new Uri(uri.AbsoluteUri.TrimEnd('/') + "/"), TimeSpan.FromSeconds(segundos));
    }
}
