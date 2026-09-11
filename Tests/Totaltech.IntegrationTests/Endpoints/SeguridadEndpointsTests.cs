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

public sealed class SeguridadEndpointsTests
{
    private static readonly string[] NombresSensibles =
    [
        "contrasena",
        "password",
        "passwordHash",
        "hash",
        "salt"
    ];

    [Fact]
    public async Task RegistroConRolAdministrador_FuerzaClienteYHasheaPassword()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        factory.VerificarPersistenciaAislada();
        const string email = "registro@test.local";
        const string contrasena = "Registro123456";

        using var response = await RegistrarAsync(client, email, contrasena, RolUsuario.Administrador);
        var json = await LeerJsonAsync(response);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal((int)RolUsuario.Cliente, json.GetProperty("rol").GetInt32());
        VerificarAusenciaDeDatosSensibles(json);

        using var scope = factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<TotaltechDbContext>();
        var usuario = await contexto.Usuarios.SingleAsync(item => item.Email == email);
        Assert.Equal(RolUsuario.Cliente, usuario.Rol);
        Assert.NotEqual(contrasena, usuario.Contrasena);
    }

    [Fact]
    public async Task LoginClienteCorrecto_DevuelveTokenRolClienteYSinPassword()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        factory.VerificarPersistenciaAislada();
        const string email = "login-cliente@test.local";
        const string contrasena = "Cliente123456";
        using var registro = await RegistrarAsync(client, email, contrasena, RolUsuario.Cliente);
        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);

        using var response = await LoginAsync(client, email, contrasena);
        var json = await LeerJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal((int)RolUsuario.Cliente, json.GetProperty("rol").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("accessToken").GetString()));
        VerificarAusenciaDeDatosSensibles(json);
    }

    [Fact]
    public async Task LoginConPasswordIncorrecto_DevuelveUnauthorized()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        factory.VerificarPersistenciaAislada();
        const string email = "login-invalido@test.local";
        using var registro = await RegistrarAsync(client, email, "Correcta123456", RolUsuario.Cliente);
        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);

        using var response = await LoginAsync(client, email, "Incorrecta123456");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminCanonico_IniciaSesionYAccedeAEndpointAdministrativo()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        factory.VerificarPersistenciaAislada();

        var login = await AutenticarAsync(client, "Admin@admin.com", "Admin123456789");
        Assert.Equal(RolUsuario.Administrador, login.Rol);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);

        using var response = await client.PostAsJsonAsync("/categorias/", new
        {
            nombre = "Categoria de prueba Admin",
            descripcion = "Creada unicamente en EF Core InMemory."
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AnonimoEnEndpointAdministrativo_DevuelveUnauthorized()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        factory.VerificarPersistenciaAislada();

        using var response = await client.PostAsJsonAsync("/categorias/", new
        {
            nombre = "No autorizada",
            descripcion = "No debe crearse."
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ClienteEnEndpointAdministrativo_DevuelveForbidden()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        factory.VerificarPersistenciaAislada();
        var cliente = await CrearClienteAutenticadoAsync(client, "cliente-forbidden@test.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cliente.Token);

        using var response = await client.PostAsJsonAsync("/categorias/", new
        {
            nombre = "No autorizada",
            descripcion = "No debe crearse."
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PropietarioPuedeConsultarSuDireccion()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        factory.VerificarPersistenciaAislada();
        var propietario = await CrearClienteAutenticadoAsync(client, "propietario@test.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", propietario.Token);
        var idDireccion = await CrearDireccionAsync(client, propietario.IdUsuario);

        using var response = await client.GetAsync($"/direcciones/{idDireccion}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OtroClienteNoPuedeConsultarDireccionAjena()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        factory.VerificarPersistenciaAislada();
        var propietario = await CrearClienteAutenticadoAsync(client, "propietario-ajeno@test.local");
        var otroCliente = await CrearClienteAutenticadoAsync(client, "otro-cliente@test.local");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", propietario.Token);
        var idDireccion = await CrearDireccionAsync(client, propietario.IdUsuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otroCliente.Token);

        using var response = await client.GetAsync($"/direcciones/{idDireccion}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AltaDireccionConIdUsuarioAjeno_UsaLaIdentidadAutenticada()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        factory.VerificarPersistenciaAislada();
        var clienteA = await CrearClienteAutenticadoAsync(client, "cliente-a@test.local");
        var clienteB = await CrearClienteAutenticadoAsync(client, "cliente-b@test.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clienteA.Token);

        using var response = await client.PostAsJsonAsync("/direcciones/", CrearDireccion(clienteB.IdUsuario));
        var json = await LeerJsonAsync(response);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(clienteA.IdUsuario, json.GetProperty("idUsuario").GetInt32());

        var idDireccion = json.GetProperty("idDireccion").GetInt32();
        using var scope = factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<TotaltechDbContext>();
        var persistida = await contexto.Direcciones.FindAsync(idDireccion);
        Assert.NotNull(persistida);
        Assert.Equal(clienteA.IdUsuario, persistida.IdUsuario);
    }

    [Fact]
    public async Task RespuestasDeRegistroLoginYUsuario_NoExponenDatosSensibles()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        factory.VerificarPersistenciaAislada();
        const string email = "respuesta-segura@test.local";
        const string contrasena = "Respuesta123456";

        using var registro = await RegistrarAsync(client, email, contrasena, RolUsuario.Cliente);
        var jsonRegistro = await LeerJsonAsync(registro);
        VerificarAusenciaDeDatosSensibles(jsonRegistro);
        var idUsuario = jsonRegistro.GetProperty("idUsuario").GetInt32();

        using var loginResponse = await LoginAsync(client, email, contrasena);
        var jsonLogin = await LeerJsonAsync(loginResponse);
        VerificarAusenciaDeDatosSensibles(jsonLogin);
        var token = jsonLogin.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var usuarioResponse = await client.GetAsync($"/usuarios/{idUsuario}");
        var jsonUsuario = await LeerJsonAsync(usuarioResponse);
        Assert.Equal(HttpStatusCode.OK, usuarioResponse.StatusCode);
        VerificarAusenciaDeDatosSensibles(jsonUsuario);
    }

    [Fact]
    public async Task BootstrapAdministrador_EsIdempotenteEnHostAislado()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        factory.VerificarPersistenciaAislada();

        using var scope = factory.Services.CreateScope();
        var logica = scope.ServiceProvider.GetRequiredService<Totaltech.Logica.IUsuariosLogica>();
        await logica.AsegurarAdministradorAsync("Admin@admin.com", "Admin123456789");
        await logica.AsegurarAdministradorAsync("Admin@admin.com", "Admin123456789");

        var contexto = scope.ServiceProvider.GetRequiredService<TotaltechDbContext>();
        var administradores = await contexto.Usuarios
            .Where(usuario => usuario.Email == "Admin@admin.com")
            .ToListAsync();

        var administrador = Assert.Single(administradores);
        Assert.Equal(RolUsuario.Administrador, administrador.Rol);
        Assert.NotEqual("Admin123456789", administrador.Contrasena);
    }

    [Fact]
    public async Task RegistroIgnoraFechaEnviadaPorElCliente()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        var fechaAntes = DateTime.UtcNow.AddMinutes(-1);

        using var response = await client.PostAsJsonAsync("/auth/registro", new
        {
            nombre = "Cliente",
            apellido = "Prueba",
            email = "fecha-registro@test.local",
            contrasena = "Cliente123456",
            telefono = "1111111111",
            fechaRegistro = new DateTime(2000, 1, 1),
            rol = (int)RolUsuario.Cliente
        });
        var json = await LeerJsonAsync(response);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(json.GetProperty("fechaRegistro").GetDateTime() >= fechaAntes);
    }

    [Fact]
    public async Task ClienteNoControlaEstadoFechaNiPrecioDelCarrito()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        var cliente = await CrearClienteAutenticadoAsync(client, "carrito-autoritativo@test.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cliente.Token);

        int idProducto;
        using (var scope = factory.Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<TotaltechDbContext>();
            var producto = new Producto
            {
                Nombre = "Producto de prueba",
                Precio = 1250.50m,
                Stock = 10,
                IdCategoria = 1,
                IdProveedor = 1
            };
            contexto.Productos.Add(producto);
            await contexto.SaveChangesAsync();
            idProducto = producto.IdProducto;
        }

        using var alta = await client.PostAsJsonAsync("/carritos/", new
        {
            idUsuario = 99999,
            fechaCreacion = new DateTime(2000, 1, 1),
            estado = (int)EstadoCarrito.Confirmado
        });
        var carrito = await LeerJsonAsync(alta);
        Assert.Equal(HttpStatusCode.Created, alta.StatusCode);
        Assert.Equal(cliente.IdUsuario, carrito.GetProperty("idUsuario").GetInt32());
        Assert.Equal((int)EstadoCarrito.Activo, carrito.GetProperty("estado").GetInt32());
        Assert.True(carrito.GetProperty("fechaCreacion").GetDateTime() >= DateTime.UtcNow.AddMinutes(-1));
        var idCarrito = carrito.GetProperty("idCarrito").GetInt32();

        using var agregar = await client.PostAsJsonAsync($"/carritos/{idCarrito}/productos", new
        {
            idProducto,
            cantidad = 2,
            precioUnitario = 0.01m
        });
        var detalle = await LeerJsonAsync(agregar);
        Assert.Equal(HttpStatusCode.Created, agregar.StatusCode);
        Assert.Equal(1250.50m, detalle.GetProperty("precioUnitario").GetDecimal());
        Assert.Equal(2501.00m, detalle.GetProperty("subtotal").GetDecimal());

        using var edicion = await client.PutAsJsonAsync($"/carritos/{idCarrito}", new
        {
            idUsuario = cliente.IdUsuario,
            fechaCreacion = new DateTime(2000, 1, 1),
            estado = (int)EstadoCarrito.Cancelado
        });
        var carritoEditado = await LeerJsonAsync(edicion);
        Assert.Equal(HttpStatusCode.OK, edicion.StatusCode);
        Assert.Equal((int)EstadoCarrito.Activo, carritoEditado.GetProperty("estado").GetInt32());
        Assert.NotEqual(2000, carritoEditado.GetProperty("fechaCreacion").GetDateTime().Year);

        var idDireccion = await CrearDireccionAsync(client, cliente.IdUsuario);
        using var confirmacion = await client.PostAsJsonAsync($"/carritos/{idCarrito}/confirmar", new
        {
            idDireccion
        });
        Assert.Equal(HttpStatusCode.Created, confirmacion.StatusCode);

        using var verificacionScope = factory.Services.CreateScope();
        var contextoVerificacion = verificacionScope.ServiceProvider.GetRequiredService<TotaltechDbContext>();
        var detallePedido = await contextoVerificacion.DetallePedidos.SingleAsync();
        var productoPersistido = await contextoVerificacion.Productos.FindAsync(idProducto);
        Assert.Equal(1250.50m, detallePedido.PrecioUnitario);
        Assert.Equal(2501.00m, detallePedido.Subtotal);
        Assert.NotNull(productoPersistido);
        Assert.Equal(8, productoPersistido.Stock);
    }

    [Fact]
    public async Task ConsultaPublicaFuerzaIdentidadNulaEstadoPendienteYFechaActual()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/consultas/", new
        {
            idUsuario = 99999,
            nombre = "Visitante",
            email = "visitante@test.local",
            mensaje = "Consulta académica",
            fechaConsulta = new DateTime(2000, 1, 1),
            estado = (int)EstadoConsulta.Cerrada
        });
        var json = await LeerJsonAsync(response);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(JsonValueKind.Null, json.GetProperty("idUsuario").ValueKind);
        Assert.Equal((int)EstadoConsulta.Pendiente, json.GetProperty("estado").GetInt32());
        Assert.True(json.GetProperty("fechaConsulta").GetDateTime() >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task ClienteNoPuedeCrearPedidoDirectamente()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        var cliente = await CrearClienteAutenticadoAsync(client, "pedido-directo@test.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cliente.Token);

        using var response = await client.PostAsJsonAsync("/pedidos/", new
        {
            idUsuario = cliente.IdUsuario,
            fechaPedido = DateTime.UtcNow,
            estado = (int)EstadoPedido.Pagado,
            idDireccion = 1
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> RegistrarAsync(
        HttpClient client,
        string email,
        string contrasena,
        RolUsuario rol)
    {
        return await client.PostAsJsonAsync("/auth/registro", new
        {
            nombre = "Cliente",
            apellido = "Prueba",
            email,
            contrasena,
            telefono = "1111111111",
            fechaRegistro = DateTime.UtcNow,
            rol = (int)rol
        });
    }

    private static async Task<HttpResponseMessage> LoginAsync(
        HttpClient client,
        string email,
        string contrasena)
    {
        return await client.PostAsJsonAsync("/auth/login", new { email, contrasena });
    }

    private static async Task<UsuarioAutenticado> CrearClienteAutenticadoAsync(
        HttpClient client,
        string email)
    {
        const string contrasena = "Cliente123456";
        using var registro = await RegistrarAsync(client, email, contrasena, RolUsuario.Cliente);
        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);

        return await AutenticarAsync(client, email, contrasena);
    }

    private static async Task<UsuarioAutenticado> AutenticarAsync(
        HttpClient client,
        string email,
        string contrasena)
    {
        using var response = await LoginAsync(client, email, contrasena);
        var json = await LeerJsonAsync(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return new UsuarioAutenticado(
            json.GetProperty("idUsuario").GetInt32(),
            json.GetProperty("accessToken").GetString()!,
            (RolUsuario)json.GetProperty("rol").GetInt32());
    }

    private static async Task<int> CrearDireccionAsync(HttpClient client, int idUsuarioSolicitado)
    {
        using var response = await client.PostAsJsonAsync("/direcciones/", CrearDireccion(idUsuarioSolicitado));
        var json = await LeerJsonAsync(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return json.GetProperty("idDireccion").GetInt32();
    }

    private static object CrearDireccion(int idUsuario)
    {
        return new
        {
            idUsuario,
            calle = "Calle de Prueba",
            numero = "123",
            ciudad = "Buenos Aires",
            provincia = "Buenos Aires",
            codigoPostal = "1000",
            pais = "Argentina",
            tipo = 0
        };
    }

    private static async Task<JsonElement> LeerJsonAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.Clone();
    }

    private static void VerificarAusenciaDeDatosSensibles(JsonElement elemento)
    {
        if (elemento.ValueKind == JsonValueKind.Object)
        {
            foreach (var propiedad in elemento.EnumerateObject())
            {
                Assert.False(
                    NombresSensibles.Contains(propiedad.Name, StringComparer.OrdinalIgnoreCase),
                    $"La respuesta expuso la propiedad sensible '{propiedad.Name}'.");
                VerificarAusenciaDeDatosSensibles(propiedad.Value);
            }
        }
        else if (elemento.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in elemento.EnumerateArray())
            {
                VerificarAusenciaDeDatosSensibles(item);
            }
        }
    }

    private sealed record UsuarioAutenticado(int IdUsuario, string Token, RolUsuario Rol);
}
