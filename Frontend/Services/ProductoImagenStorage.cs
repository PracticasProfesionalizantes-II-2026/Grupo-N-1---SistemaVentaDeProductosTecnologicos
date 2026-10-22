namespace Frontend.Services;

public sealed class ProductoImagenStorage
{
    public const long MaximoBytes = 5 * 1024 * 1024;
    private const string PrefijoUrl = "/uploads/productos/";
    private readonly string _directorio;

    public ProductoImagenStorage(IWebHostEnvironment environment)
    {
        var webRoot = environment.WebRootPath
            ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        _directorio = Path.Combine(webRoot, "uploads", "productos");
    }

    public async Task<(string? ImagenUrl, string? Error)> GuardarAsync(
        IFormFile archivo,
        CancellationToken cancellationToken = default)
    {
        if (archivo.Length == 0)
            return (null, "Seleccioná una imagen que no esté vacía.");

        if (archivo.Length > MaximoBytes)
            return (null, "La imagen no puede superar los 5 MB.");

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var mimeEsperado = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => null
        };
        if (mimeEsperado is null || !string.Equals(archivo.ContentType, mimeEsperado, StringComparison.OrdinalIgnoreCase))
            return (null, "La imagen debe ser un archivo JPG, PNG o WebP válido.");

        // Se limita también la lectura real: el tamaño declarado por el cliente no es suficiente.
        using var contenido = new MemoryStream();
        await using (var origen = archivo.OpenReadStream())
        {
            var buffer = new byte[81920];
            while (true)
            {
                var limiteLectura = (int)Math.Min(buffer.Length, MaximoBytes + 1 - contenido.Length);
                var leidos = await origen.ReadAsync(buffer.AsMemory(0, limiteLectura), cancellationToken);
                if (leidos == 0)
                    break;

                await contenido.WriteAsync(buffer.AsMemory(0, leidos), cancellationToken);
                if (contenido.Length > MaximoBytes)
                    return (null, "La imagen no puede superar los 5 MB.");
            }
        }

        if (contenido.Length == 0)
            return (null, "Seleccioná una imagen que no esté vacía.");

        if (!TieneFirmaValida(contenido.GetBuffer().AsSpan(0, (int)contenido.Length), mimeEsperado))
            return (null, "El contenido del archivo no corresponde a una imagen JPG, PNG o WebP válida.");

        var extensionGuardada = extension == ".jpeg" ? ".jpg" : extension;
        var nombre = $"{Guid.NewGuid():N}{extensionGuardada}";
        var destino = Path.Combine(_directorio, nombre);
        Directory.CreateDirectory(_directorio);
        var archivoCreado = false;
        try
        {
            await using var archivoDestino = new FileStream(
                destino, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            archivoCreado = true;
            contenido.Position = 0;
            await contenido.CopyToAsync(archivoDestino, cancellationToken);
        }
        catch
        {
            // El archivo aún no se ha enviado a la API; es seguro limpiar una escritura incompleta.
            try { if (archivoCreado) File.Delete(destino); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            throw;
        }

        return (PrefijoUrl + nombre, null);
    }

    public void Eliminar(string? imagenUrl)
    {
        if (imagenUrl is null || !imagenUrl.StartsWith(PrefijoUrl, StringComparison.Ordinal))
            return;

        var nombre = imagenUrl[PrefijoUrl.Length..];
        if (nombre.Length is not (36 or 37) || !Guid.TryParseExact(nombre[..32], "N", out _))
            return;

        if (nombre[32..] is not (".jpg" or ".png" or ".webp"))
            return;

        File.Delete(Path.Combine(_directorio, nombre));
    }

    private static bool TieneFirmaValida(ReadOnlySpan<byte> contenido, string mime)
    {
        return mime switch
        {
            "image/jpeg" => contenido.Length >= 3
                && contenido[0] == 0xFF && contenido[1] == 0xD8 && contenido[2] == 0xFF,
            "image/png" => contenido.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            "image/webp" => contenido.Length >= 12
                && contenido[..4].SequenceEqual("RIFF"u8)
                && contenido.Slice(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }
}
