using Microsoft.Playwright;

namespace Totaltech.IntegrationTests.E2E;

[Collection("Catalogo E2E")]
[Trait("Category", "E2E")]
public sealed class UsuariosAdministracionE2ETests(CatalogoE2EFixture fixture)
{
    [Fact]
    public async Task RegistroEdicionBajaReactivacionYRevocacion_FuncionanDesdeNavegador()
    {
        var email = $"registro-{Guid.NewGuid():N}@test.local";
        const string clave = "Usuario123456";
        await using var clienteContext = await fixture.Browser.NewContextAsync();
        var cliente = await clienteContext.NewPageAsync();
        await cliente.GotoAsync($"{fixture.FrontendUrl}/Home/Register");
        await cliente.GetByLabel("Nombre", new() { Exact = true }).FillAsync("Cliente");
        await cliente.GetByLabel("Apellido", new() { Exact = true }).FillAsync("Prueba");
        await cliente.GetByLabel("Email", new() { Exact = true }).FillAsync(email);
        await cliente.GetByLabel("Teléfono", new() { Exact = true }).FillAsync("1122334455");
        await cliente.GetByLabel("Contraseña", new() { Exact = true }).FillAsync(clave);
        await cliente.GetByLabel("Confirmar contraseña", new() { Exact = true }).FillAsync(clave);
        await cliente.GetByRole(AriaRole.Button, new() { Name = "Crear mi cuenta", Exact = true }).ClickAsync();
        await Assertions.Expect(cliente).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Home/Login\\?email="));
        await LoginAsync(cliente, email, clave);
        var original = await fixture.ObtenerUsuarioAsync(email);

        var (adminEmail, adminClave) = await fixture.CrearAdministradorAsync();
        await using var adminContext = await fixture.Browser.NewContextAsync();
        var admin = await adminContext.NewPageAsync();
        await LoginAsync(admin, adminEmail, adminClave);
        await admin.GotoAsync($"{fixture.FrontendUrl}/Usuarios");
        var fila = admin.Locator("tbody tr").Filter(new() { HasText = email });
        await fila.GetByRole(AriaRole.Link, new() { Name = "Editar", Exact = true }).ClickAsync();
        await admin.GetByLabel("Nombre", new() { Exact = true }).FillAsync("Actualizado");
        await admin.GetByRole(AriaRole.Button, new() { Name = "Guardar cambios" }).ClickAsync();
        await Assertions.Expect(admin.GetByText("Usuario actualizado correctamente.", new() { Exact = true })).ToBeVisibleAsync();
        var actualizado = await fixture.ObtenerUsuarioAsync(email);
        Assert.Equal("Actualizado", actualizado.Nombre);
        Assert.Equal(original.Contrasena, actualizado.Contrasena);
        Assert.Equal(original.FechaRegistro, actualizado.FechaRegistro);
        await fila.GetByRole(AriaRole.Link, new() { Name = "Dar de baja", Exact = true }).ClickAsync();
        await admin.GetByRole(AriaRole.Link, new() { Name = "Cancelar", Exact = true }).ClickAsync();
        Assert.True((await fixture.ObtenerUsuarioAsync(email)).Activo);
        await fila.GetByRole(AriaRole.Link, new() { Name = "Dar de baja", Exact = true }).ClickAsync();
        await admin.GetByRole(AriaRole.Button, new() { Name = "Confirmar baja" }).ClickAsync();
        await cliente.GotoAsync($"{fixture.FrontendUrl}/Carrito");
        await Assertions.Expect(cliente).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Home/Login"));
        await LoginAsync(cliente, email, clave, aceptar: false);
        await Assertions.Expect(cliente.GetByText("Tu cuenta está desactivada. Contactá al administrador.", new() { Exact = true })).ToBeVisibleAsync();
        await fila.GetByRole(AriaRole.Link, new() { Name = "Reactivar", Exact = true }).ClickAsync();
        await admin.GetByRole(AriaRole.Button, new() { Name = "Confirmar reactivación" }).ClickAsync();
        await LoginAsync(cliente, email, clave);

        await fila.GetByRole(AriaRole.Link, new() { Name = "Editar", Exact = true }).ClickAsync();
        await admin.GetByLabel("Rol", new() { Exact = true }).SelectOptionAsync("1");
        await admin.GetByRole(AriaRole.Button, new() { Name = "Guardar cambios" }).ClickAsync();
        await cliente.GotoAsync($"{fixture.FrontendUrl}/Carrito");
        await Assertions.Expect(cliente).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Home/Login"));
        await LoginAsync(cliente, email, clave);
        await Assertions.Expect(cliente).ToHaveURLAsync($"{fixture.FrontendUrl}/Administracion");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OperacionPropia_CierraSesionConOtroAdministradorActivo(bool baja)
    {
        await fixture.CrearAdministradorAsync();
        var (email, clave) = await fixture.CrearAdministradorAsync();
        var usuario = await fixture.ObtenerUsuarioAsync(email);
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await LoginAsync(page, email, clave);
        await page.GotoAsync($"{fixture.FrontendUrl}/Usuarios/{(baja ? "DarBaja" : "Editar")}/{usuario.IdUsuario}");
        if (baja) await page.GetByRole(AriaRole.Button, new() { Name = "Confirmar baja" }).ClickAsync();
        else
        {
            await page.GetByLabel("Rol", new() { Exact = true }).SelectOptionAsync("0");
            await page.GetByRole(AriaRole.Button, new() { Name = "Guardar cambios" }).ClickAsync();
        }
        await Assertions.Expect(page).ToHaveURLAsync($"{fixture.FrontendUrl}/Home/Login");
        await page.GotoAsync($"{fixture.FrontendUrl}/Usuarios");
        await Assertions.Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Home/Login"));
    }

    [Fact]
    public async Task Pantallas_RespondenEnSeisAnchosYErroresPreservanFormulario()
    {
        var (email, clave) = await fixture.CrearAdministradorAsync();
        var (otroEmail, _) = await fixture.CrearClienteAsync();
        var otro = await fixture.ObtenerUsuarioAsync(otroEmail);
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await LoginAsync(page, email, clave);
        foreach (var ancho in new[] { 320, 375, 425, 768, 1024, 1280 })
        {
            await page.SetViewportSizeAsync(ancho, 900);
            var respuestaListado = await page.GotoAsync($"{fixture.FrontendUrl}/Usuarios");
            Assert.Equal(200, respuestaListado!.Status);
            Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"));
            if (ancho is 320 or 1280)
                await page.ScreenshotAsync(new() { Path = Path.Combine(Path.GetTempPath(), $"totaltech-usuarios-{ancho}.png"), FullPage = true });
            var respuestaEdicion = await page.GotoAsync($"{fixture.FrontendUrl}/Usuarios/Editar/{otro.IdUsuario}");
            Assert.Equal(200, respuestaEdicion!.Status);
            Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"));
            await page.GetByLabel("Nombre", new() { Exact = true }).FocusAsync();
            await Assertions.Expect(page.GetByLabel("Nombre", new() { Exact = true })).ToBeFocusedAsync();
        }
        await page.GetByLabel("Nombre", new() { Exact = true }).FillAsync("Cambio pendiente");
        await page.GetByLabel("Email", new() { Exact = true }).FillAsync(email);
        await page.GetByRole(AriaRole.Button, new() { Name = "Guardar cambios" }).ClickAsync();
        await Assertions.Expect(page.GetByText("Ya existe un usuario registrado con ese email.", new() { Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByLabel("Nombre", new() { Exact = true })).ToHaveValueAsync("Cambio pendiente");
        Assert.NotEqual("Cambio pendiente", (await fixture.ObtenerUsuarioAsync(otroEmail)).Nombre);
        // Una mutación sin el token antiforgery no puede llegar a la API.
        var cookies = await context.CookiesAsync();
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("Cookie", string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}")));
        using var respuesta = await http.PostAsync($"{fixture.FrontendUrl}/Usuarios/DarBaja/{otro.IdUsuario}", new FormUrlEncodedContent([]));
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.True((await fixture.ObtenerUsuarioAsync(otroEmail)).Activo);
    }

    private async Task LoginAsync(IPage page, string email, string clave, bool aceptar = true)
    {
        await page.GotoAsync($"{fixture.FrontendUrl}/Home/Login");
        await page.GetByLabel("Email", new() { Exact = true }).FillAsync(email);
        await page.GetByLabel("Contraseña", new() { Exact = true }).FillAsync(clave);
        await page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión", Exact = true }).ClickAsync();
        if (aceptar)
            await page.WaitForURLAsync(url => !url.Contains("/Home/Login"));
    }
}
