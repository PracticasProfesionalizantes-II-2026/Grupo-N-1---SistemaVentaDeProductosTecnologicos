using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Frontend.Observabilidad;
using Frontend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace Totaltech.IntegrationTests.Frontend;

public sealed class ApiClientMetricsHandlerTests
{
    [Theory]
    [InlineData("TotaltechApi", 101, "http_1xx")]
    [InlineData("TotaltechApi", 200, "http_2xx")]
    [InlineData("TotaltechSessionApi", 302, "http_3xx")]
    [InlineData("TotaltechSessionApi", 401, "http_4xx")]
    [InlineData("TotaltechApi", 503, "http_5xx")]
    public async Task CabecerasSeMidenSinConsumirCuerpoNiAlterarBearer(string client, int status, string result)
    {
        var registry = Metrics.NewCustomRegistry();
        var content = new ContenidoSinLeer();
        var inner = new Respuesta((request, cancellation) =>
        {
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("token-privado", request.Headers.Authorization?.Parameter);
            return new HttpResponseMessage((HttpStatusCode)status) { Content = content };
        });
        using var handler = new ApiClientMetricsHandler(Metrics.WithCustomRegistry(registry), client, true)
        { InnerHandler = inner };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/usuarios/456?email=privado@test.local");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token-privado");

        using var response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Equal(status, (int)response.StatusCode);
        Assert.False(content.Leido);
        var metrics = await Exportar(registry);
        Assert.Contains($"totaltech_api_client_requests_total{{client=\"{client}\",method=\"GET\",result=\"{result}\"}} 1", metrics);
        Assert.Contains($"totaltech_api_client_request_duration_seconds_count{{client=\"{client}\",method=\"GET\",result=\"{result}\"}} 1", metrics);
        Assert.DoesNotContain("privado", metrics);
        Assert.DoesNotContain("456", metrics.Split('\n').Where(line => line.Contains("{client="))
            .Select(line => line[..line.IndexOf('}')]).Aggregate(string.Empty, (all, label) => all + label));
        Assert.DoesNotContain("api.test", metrics);
    }

    [Theory]
    [InlineData("transport_error")]
    [InlineData("cancelled")]
    [InlineData("error")]
    public async Task FalloConservaExcepcionCancelacionYClasificaUnaSolaObservacion(string result)
    {
        var registry = Metrics.NewCustomRegistry();
        using var cancellation = new CancellationTokenSource();
        Exception failure = result switch
        {
            "transport_error" => new HttpRequestException("Fallo de transporte de prueba."),
            "cancelled" => new TaskCanceledException("Timeout de prueba."),
            _ => new InvalidOperationException("Error inesperado de prueba.")
        };
        using var handler = new ApiClientMetricsHandler(Metrics.WithCustomRegistry(registry), "TotaltechApi", true)
        {
            InnerHandler = new Respuesta((request, token) =>
            {
                Assert.Equal(cancellation.Token, token);
                throw failure;
            })
        };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(new HttpMethod("CUSTOM"), "https://api.test/anything");

        var thrown = await Record.ExceptionAsync(() => invoker.SendAsync(request, cancellation.Token));

        Assert.Same(failure, thrown);
        var metrics = await Exportar(registry);
        Assert.Contains($"totaltech_api_client_requests_total{{client=\"TotaltechApi\",method=\"OTHER\",result=\"{result}\"}} 1", metrics);
        Assert.Contains($"totaltech_api_client_request_duration_seconds_count{{client=\"TotaltechApi\",method=\"OTHER\",result=\"{result}\"}} 1", metrics);
        Assert.DoesNotContain("CUSTOM", metrics);
    }

    [Fact]
    public async Task DeshabilitadoNoObservaYLlamadasDeOtrosHostsNoContaminanElRegistro()
    {
        var primero = Metrics.NewCustomRegistry();
        var segundo = Metrics.NewCustomRegistry();
        using var enabled = new HttpMessageInvoker(new ApiClientMetricsHandler(Metrics.WithCustomRegistry(primero), "TotaltechApi", true)
        { InnerHandler = new Respuesta((_, _) => new(HttpStatusCode.OK)) });
        using var disabled = new HttpMessageInvoker(new ApiClientMetricsHandler(Metrics.WithCustomRegistry(segundo), "TotaltechApi", false)
        { InnerHandler = new Respuesta((_, _) => new(HttpStatusCode.OK)) });
        using var request1 = new HttpRequestMessage(HttpMethod.Post, "https://api.test");
        using var request2 = new HttpRequestMessage(HttpMethod.Post, "https://api.test");
        using var response1 = await enabled.SendAsync(request1, CancellationToken.None);
        using var response2 = await disabled.SendAsync(request2, CancellationToken.None);

        Assert.Contains("totaltech_api_client_requests_total{client=\"TotaltechApi\",method=\"POST\",result=\"http_2xx\"} 1", await Exportar(primero));
        Assert.DoesNotContain("{client=", await Exportar(segundo));
    }

    [Fact]
    public async Task ManejadorExteriorPreservaPropagacionBearerExistenteSinAutenticacionAdicional()
    {
        var registry = Metrics.NewCustomRegistry();
        var auth = new AutenticacionPrueba();
        using var services = new ServiceCollection().AddSingleton<IAuthenticationService>(auth).BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        var bearer = new ApiBearerTokenHandler(new Accesor(context))
        {
            InnerHandler = new Respuesta((request, _) =>
            {
                Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
                Assert.Equal("token-sesion", request.Headers.Authorization?.Parameter);
                return new(HttpStatusCode.OK);
            })
        };
        using var invoker = new HttpMessageInvoker(new ApiClientMetricsHandler(Metrics.WithCustomRegistry(registry), "TotaltechApi", true)
        { InnerHandler = bearer });
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/usuarios/7");

        using var response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Equal(1, auth.Calls);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("totaltech_api_client_requests_total{client=\"TotaltechApi\",method=\"GET\",result=\"http_2xx\"} 1", await Exportar(registry));
    }

    [Theory]
    [InlineData("get", "GET")]
    [InlineData("METODO-PRIVADO", "OTHER")]
    public async Task NormalizaMetodosConEtiquetasAcotadas(string sent, string expected)
    {
        var registry = Metrics.NewCustomRegistry();
        using var invoker = new HttpMessageInvoker(new ApiClientMetricsHandler(Metrics.WithCustomRegistry(registry), "TotaltechApi", true)
        { InnerHandler = new Respuesta((_, _) => new(HttpStatusCode.OK)) });
        using var request = new HttpRequestMessage(new HttpMethod(sent), "https://api.test");

        using var response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Contains($"totaltech_api_client_requests_total{{client=\"TotaltechApi\",method=\"{expected}\",result=\"http_2xx\"}} 1", await Exportar(registry));
    }

    private static async Task<string> Exportar(CollectorRegistry registry)
    {
        using var stream = new MemoryStream();
        await registry.CollectAndExportAsTextAsync(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private sealed class Respuesta(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request, cancellationToken));
    }

    private sealed class ContenidoSinLeer : HttpContent
    {
        public bool Leido { get; private set; }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            Leido = true;
            throw new InvalidOperationException("La instrumentación no debe leer el cuerpo.");
        }
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
    }

    private sealed class Accesor(HttpContext context) : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = context;
    }

    private sealed class AutenticacionPrueba : IAuthenticationService
    {
        public int Calls { get; private set; }
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
        {
            Calls++;
            var properties = new AuthenticationProperties();
            properties.StoreTokens([new() { Name = "access_token", Value = "token-sesion" }]);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new(), properties, "Cookies")));
        }
        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => throw new NotSupportedException();
        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => throw new NotSupportedException();
        public Task SignInAsync(HttpContext context, string? scheme, System.Security.Claims.ClaimsPrincipal principal, AuthenticationProperties? properties) => throw new NotSupportedException();
        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => throw new NotSupportedException();
    }
}
