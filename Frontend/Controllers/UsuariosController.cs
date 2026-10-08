using System.Net;
using System.Security.Claims;
using Frontend.Models.Api.Requests;
using Frontend.Models.Api.Responses;
using Frontend.Models.ViewModels.Usuarios;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

[Authorize(Roles = "Admin")]
[AutoValidateAntiforgeryToken]
public sealed class UsuariosController(IUsuariosApiService usuarios) : Controller
{
    private const string ErrorServicio = "El servicio de usuarios no está disponible. Volvé a intentarlo.";

    [HttpGet]
    public Task<IActionResult> Index(CancellationToken ct) => EjecutarAsync(async () =>
        View(new UsuariosListadoViewModel { Usuarios = await usuarios.ObtenerTodosAsync(ct) }),
        () => View(new UsuariosListadoViewModel { Error = ErrorServicio }), ct);

    [HttpGet]
    public Task<IActionResult> Editar(int id, CancellationToken ct) => EjecutarAsync(async () =>
    {
        var usuario = await usuarios.ObtenerPorIdAsync(id, ct);
        return usuario is null ? NotFound() : View(new UsuarioEdicionViewModel
        {
            IdUsuario = id, Nombre = usuario.Nombre, Apellido = usuario.Apellido,
            Email = usuario.Email, Telefono = usuario.Telefono, Rol = usuario.Rol
        });
    }, ErrorTemporal, ct);

    [HttpPost]
    public async Task<IActionResult> Editar(int id, UsuarioEdicionViewModel model, CancellationToken ct)
    {
        model.IdUsuario = id;
        if (!ModelState.IsValid) return View(model);
        return await EjecutarAsync(async () =>
        {
            using var response = await usuarios.ActualizarAsync(id,
                new UsuarioActualizacionRequest(model.Nombre, model.Apellido, model.Email, model.Telefono, model.Rol), ct);
            if (!response.IsSuccessStatusCode) return await ErrorMutacionAsync(response, () => View(model), ct);
            var actualizado = await response.Content.ReadFromJsonAsync<UsuarioResponse>(ct);
            if (EsPropio(id) && actualizado?.Rol != 1) return await CerrarSesionAsync();
            TempData["Mensaje"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }, () =>
        {
            ModelState.AddModelError(string.Empty, ErrorServicio);
            return View(model);
        }, ct);
    }

    [HttpGet]
    public Task<IActionResult> DarBaja(int id, CancellationToken ct) => ConfirmarAsync(id, false, ct);
    [HttpGet]
    public Task<IActionResult> Reactivar(int id, CancellationToken ct) => ConfirmarAsync(id, true, ct);
    [HttpPost, ActionName(nameof(DarBaja))]
    public Task<IActionResult> DarBajaConfirmada(int id, CancellationToken ct) => CambiarEstadoAsync(id, false, ct);
    [HttpPost, ActionName(nameof(Reactivar))]
    public Task<IActionResult> ReactivarConfirmada(int id, CancellationToken ct) => CambiarEstadoAsync(id, true, ct);

    private Task<IActionResult> ConfirmarAsync(int id, bool activar, CancellationToken ct) => EjecutarAsync(async () =>
    {
        var usuario = await usuarios.ObtenerPorIdAsync(id, ct);
        return usuario is null ? NotFound() : View("ConfirmarEstado", new UsuarioEstadoViewModel(usuario, activar));
    }, ErrorTemporal, ct);

    private Task<IActionResult> CambiarEstadoAsync(int id, bool activar, CancellationToken ct) => EjecutarAsync(async () =>
    {
        using var response = activar
            ? await usuarios.ReactivarAsync(id, ct) : await usuarios.DarBajaAsync(id, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                return await CerrarSesionAsync();
            if (response.StatusCode == HttpStatusCode.NotFound) return NotFound();
            var usuario = await usuarios.ObtenerPorIdAsync(id, ct);
            if (usuario is null) return NotFound();
            return await ErrorMutacionAsync(response,
                () => View("ConfirmarEstado", new UsuarioEstadoViewModel(usuario, activar)), ct);
        }
        if (!activar && EsPropio(id)) return await CerrarSesionAsync();
        TempData["Mensaje"] = activar ? "Usuario reactivado correctamente." : "Usuario dado de baja correctamente.";
        return RedirectToAction(nameof(Index));
    }, ErrorTemporal, ct);

    private async Task<IActionResult> ErrorMutacionAsync(HttpResponseMessage response, Func<IActionResult> vista, CancellationToken ct)
    {
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            return await CerrarSesionAsync();
        if (response.StatusCode == HttpStatusCode.NotFound) return NotFound();
        var mensaje = response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict
            ? await response.Content.ReadFromJsonAsync<string>(ct) ?? ErrorServicio : ErrorServicio;
        ModelState.AddModelError(string.Empty, mensaje);
        return vista();
    }

    private async Task<IActionResult> EjecutarAsync(Func<Task<IActionResult>> operacion, Func<IActionResult> error, CancellationToken ct)
    {
        try { return await operacion(); }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        { return await CerrarSesionAsync(); }
        catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException ||
                                   ex is TaskCanceledException && !ct.IsCancellationRequested)
        { return error(); }
    }

    private bool EsPropio(int id) => User.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString();
    private IActionResult ErrorTemporal() => StatusCode(503, ErrorServicio);
    private async Task<IActionResult> CerrarSesionAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["AuthSuccess"] = "Tu sesión finalizó. Iniciá sesión nuevamente para continuar.";
        return RedirectToAction("Login", "Home");
    }
}
