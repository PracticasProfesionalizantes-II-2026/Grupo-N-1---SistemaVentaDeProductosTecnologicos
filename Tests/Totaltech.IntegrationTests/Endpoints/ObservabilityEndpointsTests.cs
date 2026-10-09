using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Prometheus;
using Totaltech.Datos;
using Totaltech.IntegrationTests.Infrastructure;
using Totaltech.Logica;

namespace Totaltech.IntegrationTests.Endpoints;

public sealed class ObservabilityEndpointsTests
{
    [Theory]
    [InlineData("127.0.0.1", 200)]
    [InlineData("::1", 200)]
    [InlineData("::ffff:127.0.0.1", 200)]
    [InlineData("192.0.2.10", 403)]
    public async Task Scrape_UsaOrigenRealSinJwt(string remoteIp, int expected)
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var request = new HttpRequestMessage(HttpMethod.Get, "/metrics");
        request.Headers.Add("X-Test-Remote-IP", remoteIp);
        request.Headers.Add("X-Forwarded-For", "127.0.0.1");
        request.Headers.Add("Authorization", "Bearer token-que-no-es-una-sesion");
        using var response = await client.SendAsync(request);
        Assert.Equal(expected, (int)response.StatusCode);
        Assert.Null(response.Headers.Location);
        if (expected == 200)
        {
            Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
            var text = await response.Content.ReadAsStringAsync();
            Assert.Contains("process_cpu_seconds_total", text);
            Assert.Contains("totaltech_threadpool_pending_work_items", text);
            Assert.DoesNotContain("token-que-no-es-una-sesion", text);
            Assert.DoesNotContain("totaltech_http_requests_total{", text);
        }
    }

    [Fact]
    public async Task DeshabilitadoYMetodoIncorrecto_NoHeredanJwt()
    {
        await using var disabled = CreateFactory(enabled: false);
        using var disabledClient = disabled.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await disabledClient.GetAsync("/metrics")).StatusCode);
        await using var enabled = CreateFactory();
        using var client = enabled.CreateClient();
        using var response = await client.PostAsync("/metrics", null);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        Assert.Contains("GET", response.Content.Headers.Allow);
    }

    [Fact]
    public async Task RutasYMetodos_NoIncluyenIdsConsultasNiMultiplicanSeries()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await client.GetAsync("/productos/98771?texto=busqueda-privada");
        await client.GetAsync("/productos/98772?email=privado@example.invalid");
        await client.SendAsync(new(new HttpMethod("VERBO-PRIVADO"), "/ruta-inexistente-98771"));
        await client.SendAsync(new(new HttpMethod("VERBO-DIFERENTE"), "/ruta-inexistente-98772"));
        await client.GetAsync("/css/no-existe.css");
        var text = await ExportAsync(factory);
        Assert.Contains("http_method=\"OTHER\",route=\"_unmatched\"", text);
        Assert.Contains("route=\"/productos/{id:int}\"", text);
        foreach (var forbidden in new[] { "98771", "98772", "busqueda-privada", "privado@example", "VERBO-PRIVADO", "VERBO-DIFERENTE", "no-existe.css" })
            Assert.DoesNotContain(forbidden, text);
    }

    [Fact]
    public async Task ExcepcionNoControlada_Cuenta500UnaVezYNoExponeSuMensaje()
    {
        await using var factory = CreateFactory(brokenProducts: true);
        using var client = factory.CreateClient();
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetAsync("/productos/98771"));
        var text = await ExportAsync(factory);
        Assert.Contains("totaltech_http_requests_total{http_method=\"GET\",route=\"/productos/{id:int}\",code=\"500\"} 1", text);
        Assert.Contains("totaltech_http_requests_in_progress{http_method=\"GET\",route=\"/productos/{id:int}\"} 0", text);
        Assert.DoesNotContain("mensaje-interno-privado", text);
    }

    [Fact]
    public async Task RegistroYLogin_ClasificanResultadosSinDatosPersonales()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        using var malformed = await client.PostAsync("/auth/registro", new StringContent("{", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);
        using var missing = await client.PostAsJsonAsync("/auth/login", new { Email = "no-existe@example.invalid", Contrasena = "ClaveInvalida123" });
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        using var incorrect = await client.PostAsJsonAsync("/auth/login", new { Email = "Admin@admin.com", Contrasena = "ClaveInvalida123" });
        Assert.Equal(HttpStatusCode.Unauthorized, incorrect.StatusCode);
        using var success = await client.PostAsJsonAsync("/auth/login", new { Email = "Admin@admin.com", Contrasena = "Admin123456789" });
        Assert.Equal(HttpStatusCode.OK, success.StatusCode);
        var text = await ExportAsync(factory);
        Assert.Contains("totaltech_auth_operations_total{operation=\"register\",result=\"invalid\"} 1", text);
        Assert.Contains("totaltech_auth_operations_total{operation=\"login\",result=\"not_found\"} 1", text);
        Assert.Contains("totaltech_auth_operations_total{operation=\"login\",result=\"invalid_credentials\"} 1", text);
        Assert.Contains("totaltech_auth_operations_total{operation=\"login\",result=\"success\"} 1", text);
        Assert.DoesNotContain("Admin@admin.com", text);
        Assert.DoesNotContain("Admin123456789", text);
        Assert.DoesNotContain("trace_id", text);
    }

    [Fact]
    public async Task JsonInvalidoConExcepcion_Cuenta400YEntradaInvalida()
    {
        await using var factory = CreateFactory().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true)));
        using var client = factory.CreateClient();
        await Assert.ThrowsAsync<BadHttpRequestException>(() => client.PostAsync("/auth/registro",
            new StringContent("{", Encoding.UTF8, "application/json")));
        var text = await ExportAsync(factory);
        Assert.Contains("totaltech_http_requests_total{http_method=\"POST\",route=\"/auth/registro\",code=\"400\"} 1", text);
        Assert.Contains("totaltech_auth_operations_total{operation=\"register\",result=\"invalid\"} 1", text);
        Assert.DoesNotContain("code=\"500\"", text);
    }

    [Fact]
    public async Task Registros_QuedanSeparadosEntreHosts()
    {
        await using var first = CreateFactory();
        await using var second = CreateFactory();
        using var client = first.CreateClient();
        await client.GetAsync("/productos/98771");
        Assert.Contains("totaltech_http_requests_total{", await ExportAsync(first));
        Assert.DoesNotContain("totaltech_http_requests_total{", await ExportAsync(second));
    }

    private static WebApplicationFactory<TotaltechDbContext> CreateFactory(bool enabled = true, bool brokenProducts = false) =>
        new TotaltechWebApplicationFactory().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Observability:Enabled", enabled.ToString());
            builder.ConfigureTestServices(services =>
            {
                services.AddTransient<IStartupFilter, RemoteIpFilter>();
                if (brokenProducts)
                {
                    services.RemoveAll<IProductosLogica>();
                    services.AddScoped(_ => DispatchProxy.Create<IProductosLogica, FailingProducts>());
                }
            });
        });

    private static async Task<string> ExportAsync(WebApplicationFactory<TotaltechDbContext> factory)
    {
        using var stream = new MemoryStream();
        await factory.Services.GetRequiredService<CollectorRegistry>().CollectAndExportAsTextAsync(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private sealed class RemoteIpFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(async (context, following) =>
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse(context.Request.Headers["X-Test-Remote-IP"].FirstOrDefault() ?? "127.0.0.1");
                await following(context);
            });
            next(app);
        };
    }

    public class FailingProducts : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            throw new InvalidOperationException("mensaje-interno-privado");
    }
}
