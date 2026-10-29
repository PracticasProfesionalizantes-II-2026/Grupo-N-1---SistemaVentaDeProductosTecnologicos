// ============================================================================
// MÓDULO: INICIO Y AUTENTICACIÓN
// RESPONSABILIDAD: Servir las páginas generales y coordinar inicio/cierre de sesión.
// INTEGRACIÓN: Valida credenciales en la API y crea la cookie MVC con sus claims.
// SEGURIDAD: No autentica por sí mismo ni persiste contraseñas en el frontend.
// ============================================================================
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using Frontend.Models.ViewModels.Home;
using Frontend.Services;

namespace Frontend.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HomeController> _logger;
    private readonly ProductosApiService _productos;
    private readonly CategoriasApiService _categorias;

    public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger,
        ProductosApiService productos, CategoriasApiService categorias)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _productos = productos;
        _categorias = categorias;
    }

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var productos = ConsultarInicioAsync(() => _productos.ObtenerTodosAsync(cancellationToken),
            "No pudimos cargar los productos. Volvé a intentarlo en unos minutos.", cancellationToken);
        var categorias = ConsultarInicioAsync(() => _categorias.ObtenerTodosAsync(cancellationToken),
            "No pudimos cargar las categorías. Volvé a intentarlo en unos minutos.", cancellationToken);
        await Task.WhenAll(productos, categorias);
        var resultadoProductos = await productos;
        var resultadoCategorias = await categorias;
        return View(InicioViewModel.Desde(resultadoProductos.Datos, resultadoCategorias.Datos,
            resultadoProductos.Error, resultadoCategorias.Error));
    }

    private async Task<(List<T> Datos, string? Error)> ConsultarInicioAsync<T>(
        Func<Task<List<T>>> consultar, string mensaje, CancellationToken cancellationToken)
    {
        try
        {
            return (await consultar(), null);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException ||
            exception is TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(exception, "Consulta del inicio no disponible: {Mensaje}", mensaje);
            return ([], mensaje);
        }
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var esAdministrador = User.IsInRole("Admin");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData.Remove("AuthWelcome");
        TempData["AuthSuccess"] = "Sesión cerrada correctamente.";
        return RedirectToAction(esAdministrador ? nameof(Login) : nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login(string? email = null, string? returnUrl = null)
    {
        return View(new LoginViewModel { Email = email?.Trim() ?? string.Empty, ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        model.Email = model.Email.Trim().ToLowerInvariant();
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("TotaltechApi");
            var response = await client.PostAsJsonAsync("auth/login", new
            {
                Email = model.Email,
                Contrasena = model.Contrasena
            }, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return RedirectToAction(nameof(Register), new { email = model.Email, origen = "login" });
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError(nameof(model.Contrasena), "La contraseña es incorrecta.");
                return View(model);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ModelState.AddModelError(string.Empty, "Tu cuenta está desactivada. Contactá al administrador.");
                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, await LeerErrorApiAsync(response, cancellationToken));
                return View(model);
            }

            var usuario = await response.Content.ReadFromJsonAsync<AuthUserResponse>(cancellationToken);
            if (usuario is null || string.IsNullOrWhiteSpace(usuario.AccessToken))
            {
                ModelState.AddModelError(string.Empty, "No pudimos completar el inicio de sesión.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}".Trim()),
                new(ClaimTypes.Email, usuario.Email),
                new(ClaimTypes.Role, usuario.Rol == 1 ? "Admin" : "Cliente")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var propiedades = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = usuario.ExpiresAtUtc
            };
            propiedades.StoreTokens(
            [
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = usuario.AccessToken
                }
            ]);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                propiedades);

            TempData.Remove("AuthSuccess");
            TempData["AuthWelcome"] = $"¡Bienvenido, {usuario.Nombre}!";
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl);
            return usuario.Rol == 1 ? RedirectToAction("Index", "Administracion") : RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "No se pudo contactar la API durante el inicio de sesión.");
            ModelState.AddModelError(string.Empty, "El servicio de acceso no está disponible. Intentá nuevamente en unos minutos.");
            return View(model);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(exception, "La API agotó el tiempo de espera durante el inicio de sesión.");
            ModelState.AddModelError(string.Empty, "El servicio está demorando más de lo esperado. Volvé a intentarlo.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register(string? email = null, string? origen = null)
    {
        ViewData["RedirectedFromLogin"] = string.Equals(origen, "login", StringComparison.OrdinalIgnoreCase);
        return View(new RegisterViewModel { Email = email?.Trim() ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        model.Nombre = model.Nombre.Trim();
        model.Apellido = model.Apellido.Trim();
        model.Email = model.Email.Trim().ToLowerInvariant();
        model.Telefono = model.Telefono.Trim();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("TotaltechApi");
            var response = await client.PostAsJsonAsync("auth/registro", new
            {
                model.Nombre,
                model.Apellido,
                model.Email,
                model.Contrasena,
                model.Telefono,
                FechaRegistro = DateTime.UtcNow,
                Rol = 0
            }, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(nameof(model.Email), "Ya existe una cuenta asociada a este email.");
                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, await LeerErrorApiAsync(response, cancellationToken));
                return View(model);
            }

            TempData["AuthSuccess"] = "Tu cuenta fue creada. Ya podés iniciar sesión.";
            return RedirectToAction(nameof(Login), new { email = model.Email });
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "No se pudo contactar la API durante el registro.");
            ModelState.AddModelError(string.Empty, "El servicio de registro no está disponible. Intentá nuevamente en unos minutos.");
            return View(model);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(exception, "La API agotó el tiempo de espera durante el registro.");
            ModelState.AddModelError(string.Empty, "El servicio está demorando más de lo esperado. Volvé a intentarlo.");
            return View(model);
        }
    }

    private static async Task<string> LeerErrorApiAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var detalle = await response.Content.ReadAsStringAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(detalle)
            ? "No pudimos procesar la solicitud. Revisá los datos e intentá nuevamente."
            : detalle.Trim('"');
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private sealed class AuthUserResponse
    {
        public int IdUsuario { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public string Apellido { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public int Rol { get; init; }
        public string AccessToken { get; init; } = string.Empty;
        public DateTimeOffset ExpiresAtUtc { get; init; }
    }
}
