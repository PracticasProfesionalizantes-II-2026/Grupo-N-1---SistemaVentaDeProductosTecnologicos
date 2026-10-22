using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.IntegrationTests.Infrastructure;

namespace Totaltech.IntegrationTests.Endpoints;

public sealed class UsuariosAdministracionEndpointsTests
{
    private const string Clave = "Correcta123456";

    [Fact]
    public async Task BajaReactivacion_ConservanRelacionesAuditanYNoRevivenTokens()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var admin = factory.CreateClient();
        await LoginAsync(admin, "Admin@admin.com", "Admin123456789");
        using var cliente = factory.CreateClient();
        var id = await RegistrarAsync(cliente);
        var email = await EmailAsync(factory, id);
        var token = await LoginAsync(cliente, email, Clave);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TotaltechDbContext>();
            db.Carritos.Add(new() { IdUsuario = id, FechaCreacion = DateTime.UtcNow });
            db.Direcciones.Add(new() { IdUsuario = id, Calle = "Histórica", Numero = "1" });
            await db.SaveChangesAsync();
        }
        using var baja = await admin.DeleteAsync($"/usuarios/{id}");
        using var repetida = await admin.DeleteAsync($"/usuarios/{id}");
        Assert.Equal(HttpStatusCode.NoContent, baja.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, repetida.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync($"/usuarios/{id}")).StatusCode);
        using var loginInactivo = await cliente.PostAsJsonAsync("/auth/login", new { email, contrasena = Clave });
        Assert.Equal(HttpStatusCode.Forbidden, loginInactivo.StatusCode);
        using var claveIncorrecta = await cliente.PostAsJsonAsync("/auth/login", new { email, contrasena = "Incorrecta" });
        Assert.Equal(HttpStatusCode.Unauthorized, claveIncorrecta.StatusCode);
        using var registroDuplicado = await cliente.PostAsJsonAsync("/auth/registro", new
        { nombre = "Otro", apellido = "Otro", email, telefono = "111", contrasena = Clave });
        Assert.Equal(HttpStatusCode.Conflict, registroDuplicado.StatusCode);
        using var reactivar = await admin.PostAsync($"/usuarios/{id}/reactivar", null);
        using var reactivarRepetida = await admin.PostAsync($"/usuarios/{id}/reactivar", null);
        Assert.Equal(HttpStatusCode.OK, reactivar.StatusCode);
        Assert.Equal(HttpStatusCode.OK, reactivarRepetida.StatusCode);
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync($"/usuarios/{id}")).StatusCode);
        await LoginAsync(cliente, email, Clave);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync($"/usuarios/{id}")).StatusCode);
        using var verificacion = factory.Services.CreateScope();
        var contexto = verificacion.ServiceProvider.GetRequiredService<TotaltechDbContext>();
        Assert.True((await contexto.Usuarios.FindAsync(id))!.Activo);
        Assert.True(await contexto.Carritos.AnyAsync(c => c.IdUsuario == id));
        Assert.True(await contexto.Direcciones.AnyAsync(d => d.IdUsuario == id));
        Assert.Equal(2, await contexto.AuditoriaUsuarios.CountAsync(a => a.IdUsuario == id));
    }

    [Fact]
    public async Task Edicion_SinPasswordNiFechaYConRevocacionAlCambiarRol()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var admin = factory.CreateClient();
        await LoginAsync(admin, "Admin@admin.com", "Admin123456789");
        using var cliente = factory.CreateClient();
        var id = await RegistrarAsync(cliente);
        var email = await EmailAsync(factory, id);
        await LoginAsync(cliente, email, Clave);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TotaltechDbContext>();
        var original = await db.Usuarios.AsNoTracking().SingleAsync(u => u.IdUsuario == id);
        using var cambio = await admin.PutAsJsonAsync($"/usuarios/{id}", new
        {
            nombre = "Editado", apellido = "Apellido", email, telefono = "222", rol = 1,
            contrasena = "ClaveInyectada123", fechaRegistro = DateTime.UnixEpoch, activo = false
        });
        Assert.Equal(HttpStatusCode.OK, cambio.StatusCode);
        var json = await cambio.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("activo").GetBoolean());
        Assert.False(json.TryGetProperty("contrasena", out _));
        Assert.False(json.TryGetProperty("versionSesion", out _));
        var actual = await db.Usuarios.AsNoTracking().SingleAsync(u => u.IdUsuario == id);
        Assert.Equal(original.Contrasena, actual.Contrasena);
        Assert.Equal(original.FechaRegistro, actual.FechaRegistro);
        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/usuarios/")).StatusCode);
        await LoginAsync(cliente, email, Clave);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/usuarios/")).StatusCode);
        var auditoria = await db.AuditoriaUsuarios.SingleAsync(a => a.IdUsuario == id);
        Assert.Equal(RolUsuario.Cliente, auditoria.RolAnterior);
        Assert.Equal(RolUsuario.Administrador, auditoria.RolNuevo);
        Assert.DoesNotContain(email, auditoria.CamposModificados);
    }

    [Fact]
    public async Task PermisosYUltimoAdministrador_SeValidanEnServidor()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var anonimo = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonimo.GetAsync("/usuarios/")).StatusCode);
        using var cliente = factory.CreateClient();
        var id = await RegistrarAsync(cliente);
        var email = await EmailAsync(factory, id);
        await LoginAsync(cliente, email, Clave);
        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.DeleteAsync($"/usuarios/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.PostAsync($"/usuarios/{id}/reactivar", null)).StatusCode);
        using var ajeno = await cliente.PutAsJsonAsync("/usuarios/1", new
        { nombre = "Ataque", apellido = "Ataque", email, telefono = "111", rol = 1 });
        Assert.Equal(HttpStatusCode.NotFound, ajeno.StatusCode);
        using var propio = await cliente.PutAsJsonAsync($"/usuarios/{id}", new
        { nombre = "Propio", apellido = "Apellido", email, telefono = "111", rol = 1 });
        Assert.Equal(HttpStatusCode.OK, propio.StatusCode);
        Assert.Equal(0, (await propio.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("rol").GetInt32());
        using var admin = factory.CreateClient();
        await LoginAsync(admin, "Admin@admin.com", "Admin123456789");
        var adminUsuario = (await admin.GetFromJsonAsync<JsonElement[]>("/usuarios/"))!.Single(u => u.GetProperty("rol").GetInt32() == 1);
        var adminId = adminUsuario.GetProperty("idUsuario").GetInt32();
        Assert.Equal(HttpStatusCode.Conflict, (await admin.DeleteAsync($"/usuarios/{adminId}")).StatusCode);
        using var degradar = await admin.PutAsJsonAsync($"/usuarios/{adminId}", new
        { nombre = "Admin", apellido = "Admin", email = "Admin@admin.com", telefono = "111", rol = 0 });
        Assert.Equal(HttpStatusCode.Conflict, degradar.StatusCode);
    }

    private static async Task<int> RegistrarAsync(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync("/auth/registro", new
        { nombre = "Usuario", apellido = "Apellido", email = $"{Guid.NewGuid():N}@test.local", telefono = "111", contrasena = Clave });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("idUsuario").GetInt32();
    }
    private static async Task<string> EmailAsync(TotaltechWebApplicationFactory factory, int id)
    {
        using var scope = factory.Services.CreateScope();
        return (await scope.ServiceProvider.GetRequiredService<TotaltechDbContext>().Usuarios.FindAsync(id))!.Email;
    }
    private static async Task<string> LoginAsync(HttpClient client, string email, string contrasena)
    {
        using var response = await client.PostAsJsonAsync("/auth/login", new { email, contrasena });
        response.EnsureSuccessStatusCode();
        var token = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }
}
