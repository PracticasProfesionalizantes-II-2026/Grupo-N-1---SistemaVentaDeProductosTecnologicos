using Frontend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Totaltech.Datos;
using Totaltech.Logica;
using Totaltech.Repositorios;

namespace Totaltech.IntegrationTests.Persistencia;

public sealed class CatalogoDemostracionTests
{
    [Fact]
    public async Task CargaDosVecesSinDuplicarNiSobrescribirCambiosManuales()
    {
        await using var contexto = CrearContexto();
        var categoriasRepositorio = new CategoriasRepositorio(contexto);
        var proveedoresRepositorio = new ProveedoresRepositorio(contexto);
        var productosRepositorio = new ProductosRepositorio(contexto);
        var categoriasLogica = new CategoriasLogica(categoriasRepositorio);
        var proveedoresLogica = new ProveedoresLogica(proveedoresRepositorio);
        var productosLogica = new ProductosLogica(
            productosRepositorio,
            categoriasRepositorio,
            proveedoresRepositorio);

        await CategoriasIniciales.InicializarAsync(categoriasLogica);

        var primeraCarga = await CatalogoDemostracionIniciales.InicializarAsync(
            categoriasLogica,
            proveedoresLogica,
            productosLogica,
            NullLogger.Instance);

        Assert.Equal(3, primeraCarga.ProveedoresCreados);
        Assert.Equal(20, primeraCarga.ProductosCreados);

        var productoEditado = await contexto.Productos
            .SingleAsync(producto => producto.Nombre == "Notebook Lenovo Yoga 7 2 en 1");
        productoEditado.Stock = 99;
        var proveedorEditado = await contexto.Proveedores
            .SingleAsync(proveedor => proveedor.Cuit == "DEMO-30-00000001-0");
        proveedorEditado.RazonSocial = "Nombre modificado manualmente";
        await contexto.SaveChangesAsync();

        var segundaCarga = await CatalogoDemostracionIniciales.InicializarAsync(
            categoriasLogica,
            proveedoresLogica,
            productosLogica,
            NullLogger.Instance);

        Assert.Equal(0, segundaCarga.ProveedoresCreados);
        Assert.Equal(0, segundaCarga.ProductosCreados);
        Assert.Equal(3, await contexto.Proveedores.CountAsync());
        Assert.Equal(20, await contexto.Productos.CountAsync());
        Assert.Equal(99, productoEditado.Stock);
        Assert.Equal("Nombre modificado manualmente", proveedorEditado.RazonSocial);
        Assert.All(await contexto.Productos.ToListAsync(), producto =>
        {
            Assert.True(producto.Precio >= 0);
            Assert.True(producto.Stock >= 0);
            Assert.True(contexto.Categorias.Any(categoria => categoria.IdCategoria == producto.IdCategoria));
            Assert.True(contexto.Proveedores.Any(proveedor => proveedor.IdProveedor == producto.IdProveedor));
        });
        Assert.Equal(8, await ProductosDeCategoriaAsync(contexto, "Notebooks"));
        Assert.Equal(12, await ProductosDeCategoriaAsync(contexto, "Periféricos"));
        Assert.Single(await contexto.Productos.Where(producto => producto.Stock == 0).ToListAsync());
    }

    [Theory]
    [InlineData("Production", "Server=(localdb)\\MSSQLLocalDB;Database=TotaltechDev;Trusted_Connection=True")]
    [InlineData("Development", "Server=cancho.database.windows.net;Database=TotaltechDev;User Id=test;Password=test")]
    [InlineData("Development", "Server=(localdb)\\MSSQLLocalDB;Database=BaseCompartida;Trusted_Connection=True")]
    public void GuardiaRechazaEntornoServidorOBaseNoAutorizados(
        string entorno,
        string connectionString)
    {
        var environment = new TestHostEnvironment { EnvironmentName = entorno };

        Assert.Throws<InvalidOperationException>(() =>
            CatalogoDemostracionIniciales.ValidarDestino(environment, connectionString));
    }

    [Fact]
    public void GuardiaAceptaUnicamenteLaLocalDbDeDesarrolloAutorizada()
    {
        var environment = new TestHostEnvironment { EnvironmentName = Environments.Development };

        CatalogoDemostracionIniciales.ValidarDestino(
            environment,
            "Server=(localdb)\\MSSQLLocalDB;Database=TotaltechDev;Trusted_Connection=True");
    }

    [Fact]
    public void ResolverAsociaLosVeinteProductosAArchivosPublicosExistentes()
    {
        var nombres = new[]
        {
            "Notebook Lenovo Yoga 7 2 en 1",
            "Notebook ASUS TUF Gaming A16",
            "Notebook HP Victus Gaming 15",
            "Notebook ASUS ROG Strix",
            "Notebook HP OMEN 14",
            "Notebook Lenovo LOQ 15",
            "Notebook Acer Nitro V 15",
            "Notebook Samsung Book 4",
            "Auriculares HyperX Cloud Stinger",
            "Auriculares inalámbricos HyperX",
            "Auriculares gamer Redragon",
            "Auriculares gamer Razer",
            "Auriculares inalámbricos JBL",
            "Auriculares gamer SteelSeries",
            "Teclado inalámbrico Logitech TKL",
            "Teclado mecánico HyperX",
            "Teclado Logitech G213",
            "Teclado mecánico Redragon",
            "Teclado mecánico Razer",
            "Teclado mecánico ASUS"
        };
        var resolver = new ProductoImagenResolver();
        var raizRepositorio = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var rutas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var nombre in nombres)
        {
            var rutaPublica = resolver.Resolver(nombre);
            Assert.False(string.IsNullOrWhiteSpace(rutaPublica));
            Assert.True(rutas.Add(rutaPublica!));

            var rutaFisica = Path.Combine(
                raizRepositorio,
                "Frontend",
                "wwwroot",
                rutaPublica!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(rutaFisica), $"No existe la imagen pública {rutaPublica}.");
        }

        Assert.Equal(20, rutas.Count);
        Assert.Null(resolver.Resolver("Producto agregado manualmente"));
    }

    private static TotaltechDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<TotaltechDbContext>()
            .UseInMemoryDatabase($"catalogo-demo-{Guid.NewGuid():N}")
            .Options;
        return new TotaltechDbContext(options);
    }

    private static async Task<int> ProductosDeCategoriaAsync(
        TotaltechDbContext contexto,
        string nombreCategoria)
    {
        var idCategoria = await contexto.Categorias
            .Where(categoria => categoria.Nombre == nombreCategoria)
            .Select(categoria => categoria.IdCategoria)
            .SingleAsync();
        return await contexto.Productos.CountAsync(producto => producto.IdCategoria == idCategoria);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Totaltech.IntegrationTests";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
