namespace Totaltech.Validaciones;

public static class ProductoImagenUrl
{
    public static bool EsValida(string? ruta)
    {
        if (ruta is null) return true;
        if (ruta.Length is 0 or > 500) return false;

        const string subidas = "/uploads/productos/";
        const string originales = "/images/categorias/";
        if (!ruta.StartsWith(subidas, StringComparison.Ordinal) &&
            !ruta.StartsWith(originales, StringComparison.Ordinal)) return false;

        var segmentos = ruta[1..].Split('/');
        if (segmentos.Any(segmento => segmento.Length == 0 || segmento is "." or ".." ||
            segmento != segmento.Trim() ||
            segmento.Any(caracter => !char.IsLetterOrDigit(caracter) && caracter is not ('-' or '_' or '.' or ' '))))
            return false;

        var extension = Path.GetExtension(segmentos[^1]);
        if (extension is not (".jpg" or ".png" or ".webp")) return false;

        // Las imágenes originales contienen nombres legibles; las subidas usan identificadores generados.
        return !ruta.StartsWith(subidas, StringComparison.Ordinal) ||
            (segmentos.Length == 3 && Guid.TryParseExact(Path.GetFileNameWithoutExtension(segmentos[^1]), "N", out _));
    }
}
