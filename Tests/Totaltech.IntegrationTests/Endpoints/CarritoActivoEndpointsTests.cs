using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.IntegrationTests.Infrastructure;
using Totaltech.Logica.DTOs;

namespace Totaltech.IntegrationTests.Endpoints;

public sealed class CarritoActivoEndpointsTests
{
    [Fact]
    public async Task CarritoActivo_ConsolidaActualizaYEliminaConTotalesDelServidor()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        var producto = await SembrarProductoAsync(factory, 10, 125.50m);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await CrearTokenAsync(client, "carrito-activo@test.local"));

        using var primera = await client.PostAsJsonAsync("/carritos/actual/productos", new { idProducto = producto, cantidad = 1, precioUnitario = 1m });
        using var segunda = await client.PostAsJsonAsync("/carritos/actual/productos", new { idProducto = producto, cantidad = 1, precioUnitario = 1m });
        var resumen = await segunda.Content.ReadFromJsonAsync<CarritoResumenResponse>();
        Assert.Equal(HttpStatusCode.OK, primera.StatusCode);
        Assert.Equal(HttpStatusCode.OK, segunda.StatusCode);
        Assert.Single(resumen!.Items);
        Assert.Equal(2, resumen.CantidadTotal);
        Assert.Equal(125.50m, resumen.Items[0].PrecioUnitario);
        Assert.Equal(251m, resumen.Total);

        using var actualizar = await client.PatchAsJsonAsync($"/carritos/actual/productos/{producto}", new { cantidad = 3 });
        var actualizado = await actualizar.Content.ReadFromJsonAsync<CarritoResumenResponse>();
        Assert.Equal(HttpStatusCode.OK, actualizar.StatusCode);
        Assert.Equal(3, actualizado!.CantidadTotal);
        Assert.Equal(376.50m, actualizado.Total);

        using var invalido = await client.PatchAsJsonAsync($"/carritos/actual/productos/{producto}", new { cantidad = 0 });
        using var despues = await client.GetAsync("/carritos/actual");
        var persistido = await despues.Content.ReadFromJsonAsync<CarritoResumenResponse>();
        Assert.Equal(HttpStatusCode.BadRequest, invalido.StatusCode);
        Assert.Equal(3, persistido!.CantidadTotal);

        using var eliminar = await client.DeleteAsync($"/carritos/actual/productos/{producto}");
        var vacio = await eliminar.Content.ReadFromJsonAsync<CarritoResumenResponse>();
        Assert.Equal(HttpStatusCode.OK, eliminar.StatusCode);
        Assert.Empty(vacio!.Items);
        Assert.Equal(0, vacio.CantidadTotal);
        Assert.Equal(0m, vacio.Total);
    }

    [Fact]
    public async Task CarritoActivo_RechazaSinStockYNoPermiteAccesoEntreUsuarios()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        var producto = await SembrarProductoAsync(factory, 1, 50m);
        var tokenA = await CrearTokenAsync(client, "carrito-a@test.local");
        var tokenB = await CrearTokenAsync(client, "carrito-b@test.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        using var agregado = await client.PostAsJsonAsync("/carritos/actual/productos", new { idProducto = producto, cantidad = 1 });
        using var exceso = await client.PostAsJsonAsync("/carritos/actual/productos", new { idProducto = producto, cantidad = 1 });
        Assert.Equal(HttpStatusCode.Conflict, exceso.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        using var resumenB = await client.GetAsync("/carritos/actual");
        using var modificarA = await client.PatchAsJsonAsync($"/carritos/actual/productos/{producto}", new { cantidad = 1 });
        var carritoB = await resumenB.Content.ReadFromJsonAsync<CarritoResumenResponse>();
        Assert.Empty(carritoB!.Items);
        Assert.Equal(HttpStatusCode.BadRequest, modificarA.StatusCode);
    }

    private static async Task<int> SembrarProductoAsync(TotaltechWebApplicationFactory factory, int stock, decimal precio)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TotaltechDbContext>();
        var categoria = new Categoria { Nombre = $"Cat {Guid.NewGuid():N}", Descripcion = "Prueba" };
        var proveedor = new Proveedor { RazonSocial = "Proveedor", Cuit = $"30-{Random.Shared.Next(10000000, 99999999)}-1", EmailComercial = $"p{Guid.NewGuid():N}@test.local", TelefonoComercial = "123", CondicionIva = "RI", MonedaPreferida = "ARS" };
        db.AddRange(categoria, proveedor); await db.SaveChangesAsync();
        var producto = new Producto { Nombre = "Producto carrito", Descripcion = "Prueba", Precio = precio, Stock = stock, IdCategoria = categoria.IdCategoria, IdProveedor = proveedor.IdProveedor };
        db.Productos.Add(producto); await db.SaveChangesAsync();
        return producto.IdProducto;
    }

    private static async Task<string> CrearTokenAsync(HttpClient client, string email)
    {
        const string contrasena = "Cliente123456";
        using var registro = await client.PostAsJsonAsync("/auth/registro", new { nombre = "Cliente", apellido = "Prueba", email, contrasena, telefono = "1111111111", fechaRegistro = DateTime.UtcNow, rol = 0 });
        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);
        using var login = await client.PostAsJsonAsync("/auth/login", new { email, contrasena });
        var respuesta = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        return respuesta!.AccessToken;
    }
}
