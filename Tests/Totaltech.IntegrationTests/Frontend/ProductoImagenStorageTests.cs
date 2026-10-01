using Frontend.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Totaltech.IntegrationTests.Frontend;

public sealed class ProductoImagenStorageTests : IDisposable
{
    private readonly string _raiz = Path.Combine(Path.GetTempPath(), $"totaltech-imagenes-{Guid.NewGuid():N}");

    [Theory]
    [InlineData("foto.jpg", "image/jpeg", ".jpg")]
    [InlineData("foto.JPEG", "image/jpeg", ".jpg")]
    [InlineData("foto.png", "image/png", ".png")]
    [InlineData("foto.webp", "image/webp", ".webp")]
    public async Task GuardaFormatosPermitidosConNombreGenerado(string nombre, string mime, string extension)
    {
        var bytes = ContenidoImagen(mime);
        var storage = CrearStorage();
        var resultado = await storage.GuardarAsync(CrearArchivo(nombre, mime, bytes));

        Assert.Null(resultado.Error);
        Assert.Matches($"^/uploads/productos/[a-f0-9]{{32}}\\{extension}$", resultado.ImagenUrl!);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(RutaArchivo(resultado.ImagenUrl!)));
    }

    [Theory]
    [InlineData("foto.svg", "image/svg+xml")]
    [InlineData("foto.jpg", "image/png")]
    [InlineData("foto.png", "image/jpeg")]
    [InlineData("foto.webp", "image/jpeg")]
    [InlineData("foto.jpg", "application/octet-stream")]
    [InlineData("foto.jpg", "")]
    public async Task RechazaExtensionOMimeIncorrectosSinEscribir(string nombre, string mime)
    {
        var resultado = await CrearStorage().GuardarAsync(CrearArchivo(nombre, mime, ContenidoImagen("image/jpeg")));

        Assert.Null(resultado.ImagenUrl);
        Assert.NotNull(resultado.Error);
        Assert.False(Directory.Exists(_raiz));
    }

    [Theory]
    [InlineData("foto.jpg", "image/jpeg")]
    [InlineData("foto.png", "image/png")]
    [InlineData("foto.webp", "image/webp")]
    public async Task RechazaArchivoConExtensionYMimetipoDeImagenPeroFirmaFalsa(string nombre, string mime)
    {
        var resultado = await CrearStorage().GuardarAsync(CrearArchivo(nombre, mime, "<script>alert(1)</script>"u8.ToArray()));

        Assert.Null(resultado.ImagenUrl);
        Assert.Contains("contenido", resultado.Error);
        Assert.False(Directory.Exists(_raiz));
    }

    [Fact]
    public async Task RechazaFirmaDeOtroFormato()
    {
        var resultado = await CrearStorage().GuardarAsync(CrearArchivo("foto.webp", "image/webp", ContenidoImagen("image/png")));

        Assert.Null(resultado.ImagenUrl);
        Assert.NotNull(resultado.Error);
        Assert.False(Directory.Exists(_raiz));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public async Task RechazaArchivoVacioInclusoConLongitudDeclaradaMayor(long longitudDeclarada)
    {
        var archivo = new ArchivoConLongitudDeclarada("foto.jpg", "image/jpeg", [], longitudDeclarada);
        var resultado = await CrearStorage().GuardarAsync(archivo);

        Assert.Null(resultado.ImagenUrl);
        Assert.Contains("vacía", resultado.Error);
        Assert.False(Directory.Exists(_raiz));
    }

    [Theory]
    [InlineData(ProductoImagenStorage.MaximoBytes + 1)]
    [InlineData(1)]
    public async Task RechazaExcesoDeTamanoDeclaradoOReal(long longitudDeclarada)
    {
        var bytes = new byte[ProductoImagenStorage.MaximoBytes + 1];
        ContenidoImagen("image/jpeg").CopyTo(bytes, 0);
        var archivo = new ArchivoConLongitudDeclarada("foto.jpg", "image/jpeg", bytes, longitudDeclarada);
        var resultado = await CrearStorage().GuardarAsync(archivo);

        Assert.Null(resultado.ImagenUrl);
        Assert.Contains("5 MB", resultado.Error);
        Assert.False(Directory.Exists(_raiz));
    }

    [Fact]
    public async Task AceptaExactamenteElLimite()
    {
        var bytes = new byte[ProductoImagenStorage.MaximoBytes];
        ContenidoImagen("image/jpeg").CopyTo(bytes, 0);
        var resultado = await CrearStorage().GuardarAsync(CrearArchivo("foto.jpg", "image/jpeg", bytes));

        Assert.Null(resultado.Error);
        Assert.Equal(ProductoImagenStorage.MaximoBytes, new FileInfo(RutaArchivo(resultado.ImagenUrl!)).Length);
    }

    [Fact]
    public async Task ConservaArchivosEntreInstanciasYNoReemplazaImagenesAnteriores()
    {
        var bytes = ContenidoImagen("image/png");
        var primera = await CrearStorage().GuardarAsync(CrearArchivo("foto.png", "image/png", bytes));
        var segundaInstancia = CrearStorage();
        var segunda = await segundaInstancia.GuardarAsync(CrearArchivo("foto.png", "image/png", bytes));

        Assert.NotEqual(primera.ImagenUrl, segunda.ImagenUrl);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(RutaArchivo(primera.ImagenUrl!)));
        Assert.Equal(bytes, await File.ReadAllBytesAsync(RutaArchivo(segunda.ImagenUrl!)));

        segundaInstancia.Eliminar(segunda.ImagenUrl);
        Assert.False(File.Exists(RutaArchivo(segunda.ImagenUrl!)));
        Assert.True(File.Exists(RutaArchivo(primera.ImagenUrl!)));
    }

    [Fact]
    public async Task EliminarIgnoraRutasExternasManipuladasEImagenesOriginales()
    {
        var storage = CrearStorage();
        var guardada = await storage.GuardarAsync(CrearArchivo("foto.png", "image/png", ContenidoImagen("image/png")));
        var archivoProtegido = Path.Combine(_raiz, "original.png");
        await File.WriteAllTextAsync(archivoProtegido, "conservar");
        var nombreGenerado = Path.GetFileName(guardada.ImagenUrl!);
        string?[] urls =
        [
            null, "", "/images/categorias/original.png", archivoProtegido,
            "/uploads/productos/../../original.png", "/uploads/productos/..\\..\\original.png",
            "/uploads/productos/%2e%2e/%2e%2e/original.png", "/uploads/productos/original.png",
            $"/uploads/productos/../productos/{nombreGenerado}",
            $"/uploads/productos/{nombreGenerado}?version=1", $"https://example.com{guardada.ImagenUrl}"
        ];

        foreach (var url in urls)
            storage.Eliminar(url);

        Assert.True(File.Exists(archivoProtegido));
        Assert.True(File.Exists(RutaArchivo(guardada.ImagenUrl!)));
    }

    [Fact]
    public async Task PropagaCancelacionSinGuardarArchivo()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CrearStorage().GuardarAsync(
            CrearArchivo("foto.png", "image/png", ContenidoImagen("image/png")), cancellation.Token));
        Assert.False(Directory.Exists(_raiz));
    }

    private ProductoImagenStorage CrearStorage() => new(new EntornoWeb
    {
        ContentRootPath = _raiz,
        WebRootPath = Path.Combine(_raiz, "wwwroot")
    });

    private string RutaArchivo(string imagenUrl) => Path.Combine(_raiz, "wwwroot", "uploads", "productos", Path.GetFileName(imagenUrl));

    private static IFormFile CrearArchivo(string nombre, string mime, byte[] bytes) =>
        new ArchivoConLongitudDeclarada(nombre, mime, bytes, bytes.Length);

    private static byte[] ContenidoImagen(string mime) => mime switch
    {
        "image/jpeg" => [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x02, 0xFF, 0xD9],
        "image/png" => Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aAfkAAAAASUVORK5CYII="),
        "image/webp" => Convert.FromBase64String("UklGRiIAAABXRUJQVlA4IBYAAAAwAQCdASoBAAEADsD+JaQAA3AAAAAA"),
        _ => throw new ArgumentException("Formato de prueba desconocido.", nameof(mime))
    };

    public void Dispose()
    {
        if (Directory.Exists(_raiz))
            Directory.Delete(_raiz, recursive: true);
    }

    private sealed class EntornoWeb : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "Frontend";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class ArchivoConLongitudDeclarada(string nombre, string mime, byte[] bytes, long longitud) : IFormFile
    {
        public string ContentType => mime;
        public string ContentDisposition => string.Empty;
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public long Length => longitud;
        public string Name => "ImagenArchivo";
        public string FileName => nombre;
        public Stream OpenReadStream() => new MemoryStream(bytes, writable: false);
        public void CopyTo(Stream target) => target.Write(bytes);
        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) => target.WriteAsync(bytes, cancellationToken).AsTask();
    }
}
