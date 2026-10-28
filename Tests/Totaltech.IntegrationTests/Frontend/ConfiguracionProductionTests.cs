using System.Net;
using Frontend.Configuracion;
using Frontend.Controllers;
using Frontend.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Totaltech.IntegrationTests.Frontend;

public sealed class ConfiguracionProductionTests
{
    [Theory]
    [InlineData("http://localhost:5070", "Production")]
    [InlineData("relativa/api", "Development")]
    [InlineData("https://user:password@example.test", "Production")]
    [InlineData("https://example.test?token=privado", "Production")]
    public void RechazaDestinoInvalidoSinIncluirSuValorEnElError(string url, string ambiente)
    {
        var configuration = Config(url);
        var error = Assert.Throws<InvalidOperationException>(() =>
            ConfiguracionApi.Leer(configuration, new Entorno { EnvironmentName = ambiente }));
        Assert.DoesNotContain(url, error.Message);
    }

    [Fact]
    public void ConservaSegmentosDeApiYResuelveRutasRelativasConTimeoutConfigurado()
    {
        var api = ConfiguracionApi.Leer(Config("https://example.test/api", "60"), new Entorno());
        Assert.Equal("https://example.test/api/productos/catalogo", new Uri(api.BaseAddress, "productos/catalogo").AbsoluteUri);
        Assert.Equal(TimeSpan.FromSeconds(60), api.Timeout);
        Assert.Equal(TimeSpan.FromSeconds(10),
            ConfiguracionApi.Leer(Config("http://localhost:5070"), new Entorno { EnvironmentName = "Development" }).Timeout);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("301")]
    public void RechazaTimeoutFueraDeRango(string seconds) =>
        Assert.Throws<InvalidOperationException>(() => ConfiguracionApi.Leer(Config("https://example.test", seconds), new Entorno()));

    [Fact]
    public async Task UploadExternoSeSirveEnProductionYPersisteEntreHostsSinPublicarOtrosTipos()
    {
        var root = Path.Combine(Path.GetTempPath(), "totaltech-uploads-test-" + Guid.NewGuid().ToString("N"));
        var directory = Path.Combine(root, "persistent");
        try
        {
            var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aAfkAAAAASUVORK5CYII=");
            string url;
            await using (var factory = Factory(directory))
            {
                using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://frontend.test") });
                var storage = factory.Services.GetRequiredService<ProductoImagenStorage>();
                await using var stream = new MemoryStream(bytes);
                var file = new FormFile(stream, 0, bytes.Length, "ImagenArchivo", "imagen.png")
                    { Headers = new HeaderDictionary(), ContentType = "image/png" };
                var result = await storage.GuardarAsync(file);
                Assert.Null(result.Error);
                url = result.ImagenUrl!;
                Assert.Equal(bytes, await client.GetByteArrayAsync(url));
                var cookie = factory.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                    .Get(CookieAuthenticationDefaults.AuthenticationScheme);
                Assert.Equal(CookieSecurePolicy.Always, cookie.Cookie.SecurePolicy);
                Assert.True(cookie.Cookie.HttpOnly);
                Assert.Equal(SameSiteMode.Lax, cookie.Cookie.SameSite);
                await File.WriteAllTextAsync(Path.Combine(directory, "privado.txt"), "no publicar");
                Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/uploads/productos/privado.txt")).StatusCode);
            }
            await using (var factory = Factory(directory))
            {
                using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://frontend.test") });
                Assert.Equal(bytes, await client.GetByteArrayAsync(url));
                Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/uploads/productos/")).StatusCode);
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData("directorio-relativo")]
    [InlineData("%TOTALTECH_VARIABLE_INEXISTENTE%/productos")]
    public void RechazaDirectorioNoResuelto(string directory)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            { ["ProductoImagenes:Directorio"] = directory }).Build();
        Assert.Throws<InvalidOperationException>(() => new ProductoImagenStorage(new EntornoWeb(), configuration));
    }

    private static WebApplicationFactory<HomeController> Factory(string directory) =>
        new WebApplicationFactory<HomeController>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ApiBaseUrl", "https://api.invalid/");
            builder.UseSetting("ProductoImagenes:Directorio", directory);
        });

    private static IConfiguration Config(string url, string? seconds = null) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            { ["ApiBaseUrl"] = url, ["ApiTimeoutSeconds"] = seconds }).Build();

    private class Entorno : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Production";
        public string ApplicationName { get; set; } = "Frontend";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class EntornoWeb : Entorno, IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
