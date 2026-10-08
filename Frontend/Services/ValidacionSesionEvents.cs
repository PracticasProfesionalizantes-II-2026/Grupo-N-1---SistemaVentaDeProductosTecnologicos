using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Frontend.Models.Api.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Frontend.Services;

public sealed class ValidacionSesionEvents(IHttpClientFactory factory) : CookieAuthenticationEvents
{
    public const string ServicioNoDisponible = "Totaltech.SesionNoDisponible";

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var token = context.Properties.GetTokenValue("access_token");
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(token) || !int.TryParse(id, out var idUsuario))
        {
            await CerrarAsync(context);
            return;
        }
        try
        {
            // Este cliente no autentica el HttpContext: evita recursión del handler Bearer.
            var client = factory.CreateClient("TotaltechSessionApi");
            using var request = new HttpRequestMessage(HttpMethod.Get, $"usuarios/{idUsuario}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var response = await client.SendAsync(request, context.HttpContext.RequestAborted);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.NotFound)
            {
                await CerrarAsync(context);
                return;
            }
            response.EnsureSuccessStatusCode();
            var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>(context.HttpContext.RequestAborted);
            if (usuario is null || !usuario.Activo || usuario.IdUsuario != idUsuario || usuario.Rol is < 0 or > 1)
            {
                await CerrarAsync(context);
                return;
            }
            var nombre = $"{usuario.Nombre} {usuario.Apellido}".Trim();
            var rol = usuario.Rol == 1 ? "Admin" : "Cliente";
            if (context.Principal!.Identity?.Name != nombre ||
                context.Principal.FindFirstValue(ClaimTypes.Email) != usuario.Email ||
                context.Principal.FindFirstValue(ClaimTypes.Role) != rol)
            {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, id!),
                    new Claim(ClaimTypes.Name, nombre),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, rol)
                }, CookieAuthenticationDefaults.AuthenticationScheme);
                context.ReplacePrincipal(new ClaimsPrincipal(identity));
                context.ShouldRenew = true;
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException ||
                                   ex is TaskCanceledException && !context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            // No borrar una sesión por una caída de API; bloquear los recorridos protegidos.
            context.HttpContext.Items[ServicioNoDisponible] = true;
            context.RejectPrincipal();
        }
    }

    private static async Task CerrarAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
