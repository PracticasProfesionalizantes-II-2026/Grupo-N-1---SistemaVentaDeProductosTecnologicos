using Frontend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Totaltech.Datos;
using Totaltech.IntegrationTests.Infrastructure;
using Totaltech.Logica;
using Totaltech.Repositorios;

namespace Totaltech.IntegrationTests.Persistencia;

public sealed class CatalogoDemostracionTests
{
    [Fact]
    public async Task CargaDosVecesSinDuplicarNiSobrescribirCambiosManuales()
    {
        await using var contexto = CrearContexto();
        await VerificarCargaIdempotenteAsync(contexto);
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public async Task CargaDosVecesEnSqlAisladoSinDuplicarNiSobrescribirCambiosManuales()
    {
        var database = new SqlServerTestDatabase();
        try
        {
            await database.InitializeAsync();
            await using var contexto = database.CreateContext();
            await VerificarCargaIdempotenteAsync(contexto);
        }
        finally
        {
            await database.DisposeAsync();
        }
    }

    private static async Task VerificarCargaIdempotenteAsync(TotaltechDbContext contexto)
    {
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
        Assert.Equal(38, primeraCarga.ProductosCreados);

        var celularesEsperados = new (string Nombre, decimal Precio)[]
        {
            ("Celular Honor Magic 7 Lite 5G 12 GB 512 GB Titanium Black", 650000m),
            ("Celular iPhone 16 128 GB Black", 1400000m),
            ("Celular Samsung A57 5G", 750000m),
            ("Celular Samsung S26 Ultra 12 GB 256 GB Cobalt Violet", 2300000m),
            ("Celular Xiaomi 17 12 GB 512 GB Black", 1600000m),
            ("Celular Xiaomi Redmi Note 14 Pro 8 GB 256 GB Midnight Black con cargador", 550000m)
        };
        foreach (var esperado in celularesEsperados)
        {
            var celular = await contexto.Productos.Include(producto => producto.Categoria)
                .Include(producto => producto.Proveedor)
                .SingleAsync(producto => producto.Nombre == esperado.Nombre);
            Assert.Equal(esperado.Precio, celular.Precio);
            Assert.Equal(10, celular.Stock);
            Assert.Equal("Celulares", celular.Categoria!.Nombre);
            Assert.Equal("Conecta Digital Demo", celular.Proveedor!.RazonSocial);
            Assert.Equal("DEMO-30-00000002-0", celular.Proveedor.Cuit);
            Assert.NotNull(new ProductoImagenResolver().Resolver(celular.Nombre));
        }

        var almacenamientoEsperado = new (string Nombre, decimal Precio, string Imagen)[]
        {
            ("SSD Kingston NV3 1 TB (SNV3S/1000G)", 95000m, "snv3s-1000g.png"),
            ("SSD Samsung 870 EVO 500 GB (MZ-77E500B)", 85000m, "mz-77e500b.png"),
            ("Disco rígido Western Digital Blue 1 TB (WD10EZEX)", 80000m, "wd10ezex.png"),
            ("Disco externo Seagate Expansion Portable 2 TB (STKM2000400)", 130000m, "stkm2000400.png"),
            ("Pendrive SanDisk Ultra Flair 128 GB (SDCZ73-128G-G46)", 18000m, "sdcz73-128g-g46.png"),
            ("SanDisk Ultra microSDXC 128 GB (SDSQUAB-128G-GN6MA)", 20000m, "sdsquab-128g-gn6ma.png")
        };
        foreach (var esperado in almacenamientoEsperado)
        {
            var unidad = await contexto.Productos.Include(producto => producto.Categoria)
                .Include(producto => producto.Proveedor)
                .SingleAsync(producto => producto.Nombre == esperado.Nombre);
            Assert.Equal(esperado.Precio, unidad.Precio);
            Assert.Equal(10, unidad.Stock);
            Assert.Equal("Almacenamiento", unidad.Categoria!.Nombre);
            Assert.Equal("PixelWare Distribuidora Demo", unidad.Proveedor!.RazonSocial);
            Assert.Equal("DEMO-30-00000003-0", unidad.Proveedor.Cuit);
            Assert.Equal($"/images/categorias/Almacenamiento/{esperado.Imagen}",
                new ProductoImagenResolver().Resolver(unidad.Nombre));
            Assert.False(string.IsNullOrWhiteSpace(unidad.Descripcion));
            Assert.DoesNotContain("http", unidad.Descripcion);
            Assert.DoesNotContain("&#", unidad.Descripcion);
        }

        var placasEsperadas = new (string Nombre, decimal Precio, string Descripcion, string Imagen)[]
        {
            ("MSI GeForce RTX 4060 VENTUS 2X BLACK OC", 550000m,
                "8 GB GDDR6 · PCIe 4.0 · 128 bits. Placa gráfica con doble ventilador TORX Fan 4.0, arquitectura NVIDIA Ada Lovelace, ray tracing y DLSS 3. Diseñada para videojuegos en Full HD, edición multimedia y tareas de creación de contenido.",
                "msi-geforce-rtx-4060-ventus-2x-black-oc.png"),
            ("Gigabyte GeForce RTX 5060 Ti WINDFORCE OC", 850000m,
                "16 GB GDDR7 · PCIe 5.0 · 128 bits. Tarjeta gráfica con 4.608 núcleos CUDA, sistema de refrigeración WINDFORCE y compatibilidad con DLSS 4. Ideal para videojuegos modernos, aplicaciones de diseño 3D y creación de contenido.",
                "gigabyte-geforce-rtx-5060-ti-windforce-oc.png"),
            ("ASUS PRIME GeForce RTX 5070", 1200000m,
                "12 GB GDDR7 · PCIe 5.0 · 192 bits. Placa gráfica con 6.144 núcleos CUDA, sistema de triple ventilador y soporte para ray tracing y DLSS 4. Orientada a videojuegos exigentes, renderizado 3D y creación de contenido de alta resolución.",
                "asus-prime-geforce-rtx-5070.png"),
            ("XFX Speedster SWFT 210 Radeon RX 7600 Core Edition", 450000m,
                "8 GB GDDR6 · PCIe 4.0 · 128 bits. Placa de video con arquitectura AMD RDNA 3, 2.048 procesadores stream y refrigeración de doble ventilador. Adecuada para videojuegos en 1080p, streaming y tareas multimedia, con soporte para AMD FSR.",
                "xfx-speedster-swft-210-radeon-rx-7600-core-edition.png"),
            ("Sapphire PULSE Radeon RX 9060 XT OC", 700000m,
                "16 GB GDDR6 · PCIe 5.0 · 128 bits. Tarjeta gráfica con arquitectura AMD RDNA 4, 2.048 procesadores stream y refrigeración Dual-X. Sus 16 GB de memoria permiten ejecutar videojuegos exigentes en Full HD y 1440p, además de aplicaciones gráficas compatibles con AMD FSR 4.",
                "sapphire-pulse-radeon-rx-9060-xt-oc.png"),
            ("Intel Arc B580 Limited Edition", 500000m,
                "12 GB GDDR6 · PCIe 4.0 · 192 bits. Placa gráfica basada en arquitectura Intel Xe2, con 20 núcleos Xe, ray tracing y tecnología Intel XeSS 2. Orientada a videojuegos en 1080p y 1440p, creación de contenido y edición de video con aceleración AV1.",
                "intel-arc-b580-limited-edition.png")
        };
        foreach (var esperado in placasEsperadas)
        {
            var placa = await contexto.Productos.Include(producto => producto.Categoria)
                .Include(producto => producto.Proveedor)
                .SingleAsync(producto => producto.Nombre == esperado.Nombre);
            Assert.Equal(esperado.Precio, placa.Precio);
            Assert.Equal(10, placa.Stock);
            Assert.Equal(esperado.Descripcion, placa.Descripcion);
            Assert.Equal("Placas de Video", placa.Categoria!.Nombre);
            Assert.Equal("PixelWare Distribuidora Demo", placa.Proveedor!.RazonSocial);
            Assert.Equal("DEMO-30-00000003-0", placa.Proveedor.Cuit);
            Assert.Equal($"/images/categorias/Placas de Video/{esperado.Imagen}",
                new ProductoImagenResolver().Resolver(placa.Nombre));
        }

        var placaEditada = await contexto.Productos
            .SingleAsync(producto => producto.Nombre == "Intel Arc B580 Limited Edition");
        placaEditada.Precio = 510000m;
        placaEditada.Stock = 2;
        placaEditada.Descripcion = "Placa editada manualmente";
        placaEditada.ImagenUrl = "/uploads/productos/cccccccccccccccccccccccccccccccc.png";

        var unidadEditada = await contexto.Productos
            .SingleAsync(producto => producto.Nombre == "SSD Kingston NV3 1 TB (SNV3S/1000G)");
        Assert.Contains("6.000 MB/s de lectura y 4.000 MB/s de escritura", unidadEditada.Descripcion);
        unidadEditada.Precio = 99000m;
        unidadEditada.Stock = 4;
        unidadEditada.Descripcion = "Almacenamiento editado manualmente";
        unidadEditada.ImagenUrl = "/uploads/productos/bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb.png";

        var celularEditado = await contexto.Productos
            .SingleAsync(producto => producto.Nombre == "Celular Samsung A57 5G");
        Assert.Equal("Celular Samsung A57 con conectividad 5G.", celularEditado.Descripcion);
        celularEditado.Precio = 800000m;
        celularEditado.Stock = 3;
        celularEditado.Descripcion = "Descripción modificada manualmente";
        celularEditado.ImagenUrl = "/uploads/productos/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.png";

        var productoEditado = await contexto.Productos
            .SingleAsync(producto => producto.Nombre == "Notebook Lenovo Yoga 7 2 en 1");
        productoEditado.Stock = 99;
        var proveedorEditado = await contexto.Proveedores
            .SingleAsync(proveedor => proveedor.Cuit == "DEMO-30-00000001-0");
        proveedorEditado.RazonSocial = "Nombre modificado manualmente";
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();

        var segundaCarga = await CatalogoDemostracionIniciales.InicializarAsync(
            categoriasLogica,
            proveedoresLogica,
            productosLogica,
            NullLogger.Instance);

        Assert.Equal(0, segundaCarga.ProveedoresCreados);
        Assert.Equal(0, segundaCarga.ProductosCreados);
        Assert.Equal(3, await contexto.Proveedores.CountAsync());
        Assert.Equal(38, await contexto.Productos.CountAsync());
        productoEditado = await contexto.Productos.SingleAsync(producto => producto.IdProducto == productoEditado.IdProducto);
        unidadEditada = await contexto.Productos.SingleAsync(producto => producto.IdProducto == unidadEditada.IdProducto);
        celularEditado = await contexto.Productos.SingleAsync(producto => producto.IdProducto == celularEditado.IdProducto);
        placaEditada = await contexto.Productos.SingleAsync(producto => producto.IdProducto == placaEditada.IdProducto);
        proveedorEditado = await contexto.Proveedores.SingleAsync(proveedor => proveedor.IdProveedor == proveedorEditado.IdProveedor);
        Assert.Equal(510000m, placaEditada.Precio);
        Assert.Equal(2, placaEditada.Stock);
        Assert.Equal("Placa editada manualmente", placaEditada.Descripcion);
        Assert.Equal("/uploads/productos/cccccccccccccccccccccccccccccccc.png", placaEditada.ImagenUrl);
        Assert.Equal(99, productoEditado.Stock);
        Assert.Equal(99000m, unidadEditada.Precio);
        Assert.Equal(4, unidadEditada.Stock);
        Assert.Equal("Almacenamiento editado manualmente", unidadEditada.Descripcion);
        Assert.Equal("/uploads/productos/bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb.png", unidadEditada.ImagenUrl);
        Assert.Equal(800000m, celularEditado.Precio);
        Assert.Equal(3, celularEditado.Stock);
        Assert.Equal("Descripción modificada manualmente", celularEditado.Descripcion);
        Assert.Equal("/uploads/productos/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.png", celularEditado.ImagenUrl);
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
        Assert.Equal(6, await ProductosDeCategoriaAsync(contexto, "Celulares"));
        Assert.Equal(6, await ProductosDeCategoriaAsync(contexto, "Almacenamiento"));
        Assert.Equal(6, await ProductosDeCategoriaAsync(contexto, "Placas de Video"));
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
    public void ResolverAsociaLosTreintaYOchoProductosAArchivosPublicosExistentes()
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
            "Teclado mecánico ASUS",
            "Celular Honor Magic 7 Lite 5G 12 GB 512 GB Titanium Black",
            "Celular iPhone 16 128 GB Black",
            "Celular Samsung A57 5G",
            "Celular Samsung S26 Ultra 12 GB 256 GB Cobalt Violet",
            "Celular Xiaomi 17 12 GB 512 GB Black",
            "Celular Xiaomi Redmi Note 14 Pro 8 GB 256 GB Midnight Black con cargador",
            "SSD Kingston NV3 1 TB (SNV3S/1000G)",
            "SSD Samsung 870 EVO 500 GB (MZ-77E500B)",
            "Disco rígido Western Digital Blue 1 TB (WD10EZEX)",
            "Disco externo Seagate Expansion Portable 2 TB (STKM2000400)",
            "Pendrive SanDisk Ultra Flair 128 GB (SDCZ73-128G-G46)",
            "SanDisk Ultra microSDXC 128 GB (SDSQUAB-128G-GN6MA)",
            "MSI GeForce RTX 4060 VENTUS 2X BLACK OC",
            "Gigabyte GeForce RTX 5060 Ti WINDFORCE OC",
            "ASUS PRIME GeForce RTX 5070",
            "XFX Speedster SWFT 210 Radeon RX 7600 Core Edition",
            "Sapphire PULSE Radeon RX 9060 XT OC",
            "Intel Arc B580 Limited Edition"
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

        Assert.Equal(38, rutas.Count);
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
