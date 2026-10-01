using System.Text;
using Microsoft.Data.SqlClient;
using Totaltech.Entidades;

namespace Totaltech.Logica;

public sealed record ResultadoCatalogoDemostracion(
    int ProveedoresCreados,
    int ProductosCreados,
    int ProveedoresOmitidos,
    int ProductosOmitidos);

public static class CatalogoDemostracionIniciales
{
    private const string ServidorPermitido = @"(localdb)\MSSQLLocalDB";
    private const string BasePermitida = "TotaltechDev";

    private static readonly ProveedorDemo[] Proveedores =
    [
        new(
            "tecnosur",
            "TecnoSur Distribuciones Demo",
            "DEMO-30-00000001-0",
            "ventas@tecnosur.example",
            "+54 11 0000-0001",
            "Calle Demostración",
            "1001",
            "Rafaela",
            "Santa Fe",
            "2300",
            "Argentina",
            30,
            4),
        new(
            "conecta",
            "Conecta Digital Demo",
            "DEMO-30-00000002-0",
            "ventas@conecta.example",
            "+54 11 0000-0002",
            "Avenida Muestra",
            "2002",
            "Rafaela",
            "Santa Fe",
            "2300",
            "Argentina",
            15,
            3),
        new(
            "pixelware",
            "PixelWare Distribuidora Demo",
            "DEMO-30-00000003-0",
            "ventas@pixelware.example",
            "+54 11 0000-0003",
            "Pasaje Académico",
            "3003",
            "Rafaela",
            "Santa Fe",
            "2300",
            "Argentina",
            21,
            5)
    ];

    private static readonly ProductoDemo[] Productos =
    [
        new("Notebook Lenovo Yoga 7 2 en 1", "Notebook convertible Lenovo para demostración académica.", 1450000m, 5, "Notebooks", "tecnosur"),
        new("Notebook ASUS TUF Gaming A16", "Notebook ASUS de línea gaming para demostración académica.", 1850000m, 10, "Notebooks", "tecnosur"),
        new("Notebook HP Victus Gaming 15", "Notebook HP de línea gaming con pantalla de 15 pulgadas.", 1600000m, 15, "Notebooks", "tecnosur"),
        new("Notebook ASUS ROG Strix", "Notebook ASUS ROG orientada a juegos.", 2300000m, 5, "Notebooks", "tecnosur"),
        new("Notebook HP OMEN 14", "Notebook compacta HP OMEN de 14 pulgadas.", 2100000m, 10, "Notebooks", "tecnosur"),
        new("Notebook Lenovo LOQ 15", "Notebook Lenovo LOQ con pantalla de 15 pulgadas.", 1750000m, 15, "Notebooks", "tecnosur"),
        new("Notebook Acer Nitro V 15", "Notebook Acer Nitro de 15 pulgadas para demostración.", 1550000m, 20, "Notebooks", "tecnosur"),
        new("Notebook Samsung Book 4", "Notebook Samsung Book con pantalla de 15 pulgadas.", 1200000m, 5, "Notebooks", "tecnosur"),
        new("Auriculares HyperX Cloud Stinger", "Auriculares gamer HyperX con micrófono integrado.", 85000m, 10, "Periféricos", "conecta"),
        new("Auriculares inalámbricos HyperX", "Auriculares inalámbricos HyperX con micrófono.", 160000m, 15, "Periféricos", "conecta"),
        new("Auriculares gamer Redragon", "Auriculares Redragon cableados con micrófono.", 120000m, 20, "Periféricos", "conecta"),
        new("Auriculares gamer Razer", "Auriculares Razer para juegos con micrófono.", 190000m, 10, "Periféricos", "conecta"),
        new("Auriculares inalámbricos JBL", "Auriculares inalámbricos JBL con micrófono.", 140000m, 0, "Periféricos", "conecta"),
        new("Auriculares gamer SteelSeries", "Auriculares SteelSeries para juegos con micrófono.", 210000m, 15, "Periféricos", "conecta"),
        new("Teclado inalámbrico Logitech TKL", "Teclado Logitech compacto sin teclado numérico.", 220000m, 5, "Periféricos", "pixelware"),
        new("Teclado mecánico HyperX", "Teclado mecánico HyperX de formato completo.", 180000m, 10, "Periféricos", "pixelware"),
        new("Teclado Logitech G213", "Teclado Logitech de formato completo con iluminación.", 95000m, 15, "Periféricos", "pixelware"),
        new("Teclado mecánico Redragon", "Teclado mecánico Redragon compacto con teclado numérico.", 140000m, 20, "Periféricos", "pixelware"),
        new("Teclado mecánico Razer", "Teclado mecánico Razer con iluminación.", 200000m, 10, "Periféricos", "pixelware"),
        new("Teclado mecánico ASUS", "Teclado mecánico ASUS para demostración académica.", 250000m, 5, "Periféricos", "pixelware")
    ];

    public static void ValidarDestino(IHostEnvironment environment, string connectionString)
    {
        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "La carga de demostración sólo puede ejecutarse en el entorno Development.");
        }

        var connectionBuilder = new SqlConnectionStringBuilder(connectionString);
        if (!string.Equals(connectionBuilder.DataSource, ServidorPermitido, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(connectionBuilder.InitialCatalog, BasePermitida, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"La carga de demostración sólo admite {ServidorPermitido} y la base {BasePermitida}.");
        }
    }

    public static async Task<ResultadoCatalogoDemostracion> InicializarAsync(
        ICategoriasLogica categoriasLogica,
        IProveedoresLogica proveedoresLogica,
        IProductosLogica productosLogica,
        ILogger logger)
    {
        var categorias = (await categoriasLogica.ObtenerTodosAsync())
            .ToDictionary(categoria => Normalizar(categoria.Nombre), StringComparer.Ordinal);

        foreach (var categoriaRequerida in new[] { "Notebooks", "Periféricos" })
        {
            if (!categorias.ContainsKey(Normalizar(categoriaRequerida)))
            {
                throw new InvalidOperationException(
                    $"La categoría requerida '{categoriaRequerida}' no existe.");
            }
        }

        var proveedoresExistentes = await proveedoresLogica.ObtenerTodosAsync();
        var proveedoresPorCuit = proveedoresExistentes
            .ToDictionary(proveedor => Normalizar(proveedor.Cuit), StringComparer.Ordinal);
        var emailsExistentes = proveedoresExistentes
            .Select(proveedor => Normalizar(proveedor.EmailComercial))
            .ToHashSet(StringComparer.Ordinal);
        var proveedoresFaltantes = Proveedores
            .Where(proveedor =>
                !proveedoresPorCuit.ContainsKey(Normalizar(proveedor.Cuit)) &&
                !emailsExistentes.Contains(Normalizar(proveedor.Email)))
            .ToList();

        var productosExistentes = await productosLogica.ObtenerTodosAsync();
        var nombresProductos = productosExistentes
            .Select(producto => Normalizar(producto.Nombre))
            .ToHashSet(StringComparer.Ordinal);
        var productosFaltantes = Productos
            .Where(producto => !nombresProductos.Contains(Normalizar(producto.Nombre)))
            .ToList();

        logger.LogInformation(
            "Vista previa del catálogo demo: {Proveedores} proveedores y {Productos} productos nuevos.",
            proveedoresFaltantes.Count,
            productosFaltantes.Count);

        foreach (var proveedor in proveedoresFaltantes)
        {
            var error = await proveedoresLogica.CrearAsync(proveedor.ToEntity());
            if (error is not null)
            {
                throw new InvalidOperationException(
                    $"No se pudo crear el proveedor demo '{proveedor.RazonSocial}': {error}");
            }
        }

        proveedoresExistentes = await proveedoresLogica.ObtenerTodosAsync();
        proveedoresPorCuit = proveedoresExistentes
            .ToDictionary(proveedor => Normalizar(proveedor.Cuit), StringComparer.Ordinal);

        var proveedoresPorClave = Proveedores.ToDictionary(
            proveedor => proveedor.Clave,
            proveedor => proveedoresPorCuit[Normalizar(proveedor.Cuit)],
            StringComparer.Ordinal);

        foreach (var producto in productosFaltantes)
        {
            var categoria = categorias[Normalizar(producto.Categoria)];
            var proveedor = proveedoresPorClave[producto.Proveedor];
            var error = await productosLogica.CrearAsync(producto.ToEntity(
                categoria.IdCategoria,
                proveedor.IdProveedor));

            if (error is not null)
            {
                throw new InvalidOperationException(
                    $"No se pudo crear el producto demo '{producto.Nombre}': {error}");
            }
        }

        var resultado = new ResultadoCatalogoDemostracion(
            proveedoresFaltantes.Count,
            productosFaltantes.Count,
            Proveedores.Length - proveedoresFaltantes.Count,
            Productos.Length - productosFaltantes.Count);

        logger.LogInformation(
            "Carga demo completada: {ProveedoresCreados} proveedores y {ProductosCreados} productos creados; " +
            "{ProveedoresOmitidos} proveedores y {ProductosOmitidos} productos ya existían.",
            resultado.ProveedoresCreados,
            resultado.ProductosCreados,
            resultado.ProveedoresOmitidos,
            resultado.ProductosOmitidos);

        return resultado;
    }

    private static string Normalizar(string valor)
    {
        return valor.Trim().Normalize(NormalizationForm.FormKC).ToUpperInvariant();
    }

    private sealed record ProveedorDemo(
        string Clave,
        string RazonSocial,
        string Cuit,
        string Email,
        string Telefono,
        string Calle,
        string Numero,
        string Ciudad,
        string Provincia,
        string CodigoPostal,
        string Pais,
        int PlazoPagoDias,
        int TiempoEntregaDias)
    {
        public Proveedor ToEntity() => new()
        {
            RazonSocial = RazonSocial,
            Cuit = Cuit,
            EmailComercial = Email,
            TelefonoComercial = Telefono,
            CondicionIva = "Responsable inscripto",
            PlazoPagoDias = PlazoPagoDias,
            TiempoEntregaDias = TiempoEntregaDias,
            MonedaPreferida = "Pesos argentinos",
            Activo = true,
            Direccion = new Direccion
            {
                Calle = Calle,
                Numero = Numero,
                Ciudad = Ciudad,
                Provincia = Provincia,
                CodigoPostal = CodigoPostal,
                Pais = Pais,
                Tipo = TipoDireccion.Fiscal
            }
        };
    }

    private sealed record ProductoDemo(
        string Nombre,
        string Descripcion,
        decimal Precio,
        int Stock,
        string Categoria,
        string Proveedor)
    {
        public Producto ToEntity(int idCategoria, int idProveedor) => new()
        {
            Nombre = Nombre,
            Descripcion = Descripcion,
            Precio = Precio,
            Stock = Stock,
            IdCategoria = idCategoria,
            IdProveedor = idProveedor
        };
    }
}
