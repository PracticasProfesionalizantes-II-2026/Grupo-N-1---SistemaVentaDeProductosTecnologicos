using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Frontend.Models.Api.Responses;
using Frontend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Totaltech.IntegrationTests.Frontend;

public sealed class ValidacionSesionTests
{
    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    public async Task SesionInvalida_EliminaCookie(int estado)
    {
        using var servicios = Servicios();
        var context = Contexto(servicios);
        using var client = new HttpClient(new Respuesta((HttpStatusCode)estado))
        { BaseAddress = new Uri("https://api.test") };
        await new ValidacionSesionEvents(new Factory(client)).ValidatePrincipal(context);
        Assert.Null(context.Principal);
        Assert.False(context.HttpContext.Items.ContainsKey(ValidacionSesionEvents.ServicioNoDisponible));
        Assert.Contains("expires=", context.Response.Headers.SetCookie.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApiNoDisponible_BloqueaPrincipalPeroConservaCookie()
    {
        using var servicios = Servicios();
        var context = Contexto(servicios);
        using var client = new HttpClient(new Respuesta(HttpStatusCode.ServiceUnavailable))
        { BaseAddress = new Uri("https://api.test") };
        await new ValidacionSesionEvents(new Factory(client)).ValidatePrincipal(context);
        Assert.Null(context.Principal);
        Assert.True(context.HttpContext.Items.ContainsKey(ValidacionSesionEvents.ServicioNoDisponible));
        Assert.False(context.Response.Headers.ContainsKey("Set-Cookie"));
        Assert.Equal("token-prueba", context.Properties.GetTokenValue("access_token"));
    }

    [Fact]
    public async Task SesionVigente_ActualizaClaimsSinRecursionYConservaToken()
    {
        using var servicios = Servicios();
        var context = Contexto(servicios);
        var handler = new Respuesta(HttpStatusCode.OK);
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.test") };
        await new ValidacionSesionEvents(new Factory(client)).ValidatePrincipal(context);
        Assert.Equal("Bearer", handler.Autorizacion?.Scheme);
        Assert.Equal("token-prueba", handler.Autorizacion?.Parameter);
        Assert.Equal("Actualizado Prueba", context.Principal?.Identity?.Name);
        Assert.True(context.Principal?.IsInRole("Admin"));
        Assert.True(context.ShouldRenew);
        Assert.Equal("token-prueba", context.Properties.GetTokenValue("access_token"));
    }

    private static ServiceProvider Servicios()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection().UseEphemeralDataProtectionProvider();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
        return services.BuildServiceProvider();
    }
    private static CookieValidatePrincipalContext Contexto(IServiceProvider services)
    {
        var http = new DefaultHttpContext { RequestServices = services };
        var properties = new AuthenticationProperties();
        properties.StoreTokens([new() { Name = "access_token", Value = "token-prueba" }]);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "7"), new Claim(ClaimTypes.Name, "Anterior"),
             new Claim(ClaimTypes.Role, "Admin")], "Cookies"));
        var scheme = new AuthenticationScheme("Cookies", null, typeof(CookieAuthenticationHandler));
        return new CookieValidatePrincipalContext(http, scheme, new CookieAuthenticationOptions(),
            new AuthenticationTicket(principal, properties, "Cookies"));
    }
    private sealed class Factory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            Assert.Equal("TotaltechSessionApi", name);
            return client;
        }
    }
    private sealed class Respuesta(HttpStatusCode estado) : HttpMessageHandler
    {
        public AuthenticationHeaderValue? Autorizacion { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Autorizacion = request.Headers.Authorization;
            return Task.FromResult(new HttpResponseMessage(estado)
            {
                Content = JsonContent.Create(new UsuarioResponse
                { IdUsuario = 7, Nombre = "Actualizado", Apellido = "Prueba", Email = "actual@test.local", Rol = 1, Activo = true })
            });
        }
    }
}
