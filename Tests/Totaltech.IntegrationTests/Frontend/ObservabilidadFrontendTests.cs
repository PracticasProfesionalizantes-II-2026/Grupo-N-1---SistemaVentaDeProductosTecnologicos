using System.Net;
using System.Security.Claims;
using Frontend.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Totaltech.IntegrationTests.Frontend;

public sealed class ObservabilidadFrontendTests
{
    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    [InlineData("::ffff:127.0.0.1")]
    public async Task ExtraeSinHttpsNiCookieSesionNiApiDisponible(string remote)
    {
        var api = new ApiInalcanzable();
        await using var factory = Factory(true, api);
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("http://frontend.test"), AllowAutoRedirect = false });
        var options = factory.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var properties = new AuthenticationProperties();
        properties.StoreTokens([new() { Name = "access_token", Value = "token-privado" }]);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "456"), new Claim(ClaimTypes.Name, "Nombre privado"),
             new Claim(ClaimTypes.Email, "privado@test.local"), new Claim(ClaimTypes.Role, "Admin")],
            CookieAuthenticationDefaults.AuthenticationScheme)), properties, CookieAuthenticationDefaults.AuthenticationScheme);
        using var request = Request(HttpMethod.Get, "/metrics", remote);
        request.Headers.Add("Cookie", $"Totaltech.Auth={options.TicketDataFormat.Protect(ticket)}");
        request.Headers.Add("Authorization", "Bearer jwt-invalido");

        using var response = await client.SendAsync(request);
        var metrics = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/plain", response.Content.Headers.ContentType?.ToString());
        Assert.Contains("# HELP", metrics);
        Assert.Null(response.Headers.Location);
        Assert.False(response.Headers.Contains("Set-Cookie"));
        Assert.Equal(0, api.Calls);
        Assert.DoesNotContain("privado", metrics);
        Assert.DoesNotContain("jwt-invalido", metrics);
        Assert.DoesNotContain("route=\"/metrics\"", metrics);
    }

    [Fact]
    public async Task CookieInvalidaTampocoProduceValidacionNiBorradoEnExtraccion()
    {
        var api = new ApiInalcanzable();
        await using var factory = Factory(true, api);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var request = Request(HttpMethod.Get, "/metrics");
        request.Headers.Add("Cookie", "Totaltech.Auth=cookie-invalida");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("Set-Cookie"));
        Assert.Equal(0, api.Calls);
    }

    [Theory]
    [InlineData(true, "192.0.2.10", "GET", 403)]
    [InlineData(true, "2001:db8::10", "GET", 403)]
    [InlineData(true, "::ffff:192.0.2.10", "GET", 403)]
    [InlineData(true, "127.0.0.1", "POST", 405)]
    [InlineData(true, "127.0.0.1", "HEAD", 405)]
    [InlineData(false, "127.0.0.1", "GET", 404)]
    public async Task ProtegeMetodoOrigenYConfiguracion(bool enabled, string remote, string method, int status)
    {
        var api = new ApiInalcanzable();
        await using var factory = Factory(enabled, api);
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("http://frontend.test"), AllowAutoRedirect = false });
        using var request = Request(new HttpMethod(method), "/metrics", remote);
        request.Headers.Add("X-Forwarded-For", "127.0.0.1");
        request.Headers.Add("Forwarded", "for=127.0.0.1");

        using var response = await client.SendAsync(request);

        Assert.Equal(status, (int)response.StatusCode);
        Assert.Null(response.Headers.Location);
        Assert.Equal(0, api.Calls);
    }

    [Fact]
    public async Task RegistraErrorUnaSolaVezConRutaOriginalYPreservaReejecucionDeError()
    {
        await using var factory = Factory(true, new ApiInalcanzable());
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://frontend.test"), AllowAutoRedirect = false });
        using var failure = await client.SendAsync(Request(HttpMethod.Get, "/observability-test/throw"));
        Assert.Equal(HttpStatusCode.InternalServerError, failure.StatusCode);
        Assert.Contains("Error", await failure.Content.ReadAsStringAsync());
        using var scrape = await client.SendAsync(Request(HttpMethod.Get, "/metrics"));
        var metrics = await scrape.Content.ReadAsStringAsync();

        var requestSample = Assert.Single(metrics.Split('\n'), line =>
            line.StartsWith("totaltech_http_requests_total{") && line.Contains("code=\"500\""));
        Assert.EndsWith(" 1", requestSample.Trim());
        Assert.Contains("ObservabilityTest/Throw", requestSample);
        Assert.DoesNotContain("Home/Error", requestSample);
    }

    [Fact]
    public async Task HostsSeparadosNoCompartenObservaciones()
    {
        await using var first = Factory(true, new ApiInalcanzable());
        await using var second = Factory(true, new ApiInalcanzable());
        using var firstClient = first.CreateClient(new() { BaseAddress = new Uri("https://frontend.test") });
        using var secondClient = second.CreateClient(new() { BaseAddress = new Uri("https://frontend.test") });
        using var stylesheet = await firstClient.SendAsync(Request(HttpMethod.Get, "/Frontend.styles.css"));
        Assert.Equal(HttpStatusCode.OK, stylesheet.StatusCode);
        Assert.Equal("text/css", stylesheet.Content.Headers.ContentType?.MediaType);
        var fingerprintStylesheet = first.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>().Select(endpoint => endpoint.RoutePattern.RawText?.TrimStart('/'))
            .First(route => route is not null && route.StartsWith("Frontend.", StringComparison.Ordinal)
                && route.EndsWith(".styles.css", StringComparison.Ordinal) && route != "Frontend.styles.css")!;
        using var fingerprintResponse = await firstClient.SendAsync(Request(HttpMethod.Get, $"/{fingerprintStylesheet}"));
        Assert.Equal(HttpStatusCode.OK, fingerprintResponse.StatusCode);
        Assert.Equal("text/css", fingerprintResponse.Content.Headers.ContentType?.MediaType);
        using var page = await firstClient.SendAsync(Request(HttpMethod.Get, "/Home/Login"));
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        using var firstScrape = await firstClient.SendAsync(Request(HttpMethod.Get, "/metrics"));
        using var secondScrape = await secondClient.SendAsync(Request(HttpMethod.Get, "/metrics"));

        var metrics = await firstScrape.Content.ReadAsStringAsync();
        Assert.Contains("Home/Login", metrics);
        Assert.DoesNotContain("Frontend.styles.css", metrics);
        Assert.DoesNotContain(fingerprintStylesheet, metrics);
        Assert.DoesNotContain("Home/Login", await secondScrape.Content.ReadAsStringAsync());
    }

    private static WebApplicationFactory<HomeController> Factory(bool enabled, ApiInalcanzable api) =>
        new WebApplicationFactory<HomeController>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseStaticWebAssets();
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?> { ["Observability:Enabled"] = enabled.ToString() }));
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IHttpClientFactory>(api);
                services.AddTransient<IStartupFilter, DireccionRemotaPrueba>();
                services.AddControllersWithViews().AddApplicationPart(typeof(ObservabilityTestController).Assembly);
            });
        });

    private static HttpRequestMessage Request(HttpMethod method, string path, string remote = "127.0.0.1")
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-Test-Remote", remote);
        return request;
    }

    private sealed class ApiInalcanzable : IHttpClientFactory
    {
        public int Calls { get; private set; }
        public HttpClient CreateClient(string name)
        {
            Calls++;
            throw new HttpRequestException("La API de prueba está indisponible.");
        }
    }

    private sealed class DireccionRemotaPrueba : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(async (context, continuation) =>
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse(context.Request.Headers["X-Test-Remote"].FirstOrDefault() ?? "127.0.0.1");
                await continuation();
            });
            next(app);
        };
    }
}

[AllowAnonymous]
[Route("observability-test")]
public sealed class ObservabilityTestController : Controller
{
    [HttpGet("throw")]
    public IActionResult Throw() => throw new InvalidOperationException("Fallo deliberado para verificar instrumentación.");
}
