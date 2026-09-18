using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.IntegrationTests.Infrastructure;
using Totaltech.Logica.DTOs;

namespace Totaltech.IntegrationTests.Endpoints;

public sealed class CatalogoEndpointsTests
{
    [Fact]
    public async Task Catalogo_AplicaFiltrosCombinadosYProyectaNombres()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        var ids = await SembrarAsync(factory);

        var result = await client.GetFromJsonAsync<CatalogoProductosResponse>(
            $"/productos/catalogo?texto={Uri.EscapeDataString("Portátil")}&idCategoria={ids.Categoria}&precioMin=100&precioMax=200&soloDisponibles=true");

        var item = Assert.Single(result!.Items);
        Assert.Equal("Portátil Alfa", item.Nombre);
        Assert.Equal("Computación prueba", item.CategoriaNombre);
        Assert.Equal("Proveedor prueba", item.ProveedorNombre);
    }

    [Fact]
    public async Task Catalogo_RangoEsInclusivoYDisponibilidadExcluyeStockCero()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        await SembrarAsync(factory);
        var result = await client.GetFromJsonAsync<CatalogoProductosResponse>(
            "/productos/catalogo?precioMin=150&precioMax=150&soloDisponibles=true");
        Assert.Contains(result!.Items, p => p.Nombre == "Portátil Alfa");
        Assert.DoesNotContain(result.Items, p => p.Stock == 0);
    }

    [Fact]
    public async Task Catalogo_PaginaDeterministaNoDuplicaItems()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        await SembrarAsync(factory);
        var primera = await client.GetFromJsonAsync<CatalogoProductosResponse>("/productos/catalogo?pagina=1&tamanoPagina=2");
        var segunda = await client.GetFromJsonAsync<CatalogoProductosResponse>("/productos/catalogo?pagina=2&tamanoPagina=2");
        Assert.Equal(2, primera!.Items.Count);
        Assert.Empty(primera.Items.Select(x => x.IdProducto).Intersect(segunda!.Items.Select(x => x.IdProducto)));
        Assert.Equal(2, primera.TamanoPagina);
    }

    [Theory]
    [InlineData("pagina=0")]
    [InlineData("tamanoPagina=49")]
    [InlineData("precioMin=-1")]
    [InlineData("precioMin=20&precioMax=10")]
    [InlineData("idCategoria=0")]
    public async Task Catalogo_ParametrosInvalidosDevuelvenBadRequest(string query)
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var response = await factory.CreateClient().GetAsync($"/productos/catalogo?{query}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Catalogo_CategoriaInexistenteDevuelvePaginaVacia()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        var result = await factory.CreateClient().GetFromJsonAsync<CatalogoProductosResponse>(
            "/productos/catalogo?idCategoria=2147483647");
        Assert.Empty(result!.Items);
        Assert.Equal(0, result.TotalItems);
    }

    private static async Task<(int Categoria, int Proveedor)> SembrarAsync(TotaltechWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TotaltechDbContext>();
        var categoria = new Categoria { Nombre = "Computación prueba", Descripcion = "Prueba" };
        var proveedor = new Proveedor { RazonSocial = "Proveedor prueba", Cuit = $"20-{Random.Shared.Next(10000000,99999999)}-1", EmailComercial = $"p{Guid.NewGuid():N}@test.local", TelefonoComercial = "123", CondicionIva = "RI", MonedaPreferida = "ARS", Activo = true };
        db.AddRange(categoria, proveedor);
        await db.SaveChangesAsync();
        db.Productos.AddRange(
            new Producto { Nombre = "Portátil Alfa", Descripcion = "Portátil liviana", Precio = 150, Stock = 3, IdCategoria = categoria.IdCategoria, IdProveedor = proveedor.IdProveedor },
            new Producto { Nombre = "Portátil Beta", Descripcion = "Portátil sin stock", Precio = 150, Stock = 0, IdCategoria = categoria.IdCategoria, IdProveedor = proveedor.IdProveedor },
            new Producto { Nombre = "Mouse Gamma", Descripcion = "Accesorio", Precio = 50, Stock = 4, IdCategoria = categoria.IdCategoria, IdProveedor = proveedor.IdProveedor });
        await db.SaveChangesAsync();
        return (categoria.IdCategoria, proveedor.IdProveedor);
    }
}
