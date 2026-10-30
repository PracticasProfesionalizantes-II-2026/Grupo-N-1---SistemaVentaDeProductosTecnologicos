using System.Text.RegularExpressions;
using Frontend.Models.ViewModels.Productos;
using Frontend.Services.Interfaces;

namespace Frontend.Services;

public sealed class CatalogoExtractorHeuristico : ICatalogoExtractorAgent
{
    public Task<IReadOnlyList<ProductoImportacionFilaViewModel>> NormalizarAsync(
        IReadOnlyList<CatalogoLineaExtraida> lineas, CancellationToken cancellationToken = default)
    {
        var filas = new List<ProductoImportacionFilaViewModel>();
        foreach (var linea in lineas)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var texto = linea.Texto.Trim();
            if (string.IsNullOrWhiteSpace(texto)) continue;
            var partes = Regex.Split(texto, @"\s*[;|\t]\s*|\s{2,}");
            var rotulados = Regex.Matches(texto,
                @"(?:^|[;|])\s*(Nombre|Descripci[oó]n|Precio|Stock|Categor[ií]a(?:Nombre)?)\s*:\s*([^;|]*)",
                RegexOptions.IgnoreCase);
            ProductoImportacionFilaViewModel? fila = null;
            if (rotulados.Count > 0)
            {
                var campos = rotulados.GroupBy(m => CatalogoImportacionNormalizador.Clave(m.Groups[1].Value))
                    .ToDictionary(g => g.Key, g => g.First().Groups[2].Value.Trim());
                fila = new()
                {
                    Nombre = campos.GetValueOrDefault("NOMBRE", ""),
                    Descripcion = campos.GetValueOrDefault("DESCRIPCION", ""),
                    Precio = campos.GetValueOrDefault("PRECIO", ""),
                    Stock = campos.GetValueOrDefault("STOCK", ""),
                    CategoriaNombre = campos.GetValueOrDefault("CATEGORIA", campos.GetValueOrDefault("CATEGORIANOMBRE", ""))
                };
            }
            else if (partes.Length is >= 3 and <= 5)
            {
                if (CatalogoImportacionNormalizador.Clave(partes[0]) == "NOMBRE"
                    && partes.Any(p => CatalogoImportacionNormalizador.Clave(p) == "PRECIO")) continue;
                fila = partes.Length >= 4
                    ? new() { Nombre = partes[0], Descripcion = partes[1], Precio = partes[2], Stock = partes[3],
                        CategoriaNombre = partes.Length == 5 ? partes[4] : "" }
                    : new() { Nombre = partes[0], Precio = partes[1], Stock = partes[2] };
            }
            else
            {
                // Exigir una marca monetaria evita interpretar memoria/modelo como precio o stock.
                var match = Regex.Match(texto, @"^(?<nombre>.+?)\s+(?:ARS\s*|\$\s*)(?<precio>[\d.,]+)\s+(?<stock>\d+)\s*$",
                    RegexOptions.IgnoreCase);
                if (match.Success) fila = new() { Nombre = match.Groups["nombre"].Value,
                    Precio = match.Groups["precio"].Value, Stock = match.Groups["stock"].Value };
            }
            if (fila is null) continue;
            fila.Nombre = CatalogoImportacionNormalizador.Espacios(fila.Nombre);
            fila.Descripcion = CatalogoImportacionNormalizador.Espacios(fila.Descripcion);
            fila.Precio = fila.Precio.Trim();
            fila.Stock = fila.Stock.Trim();
            fila.CategoriaNombre = CatalogoImportacionNormalizador.Espacios(fila.CategoriaNombre);
            if (fila.CategoriaNombre.Length == 0)
                fila.CategoriaNombre = CatalogoImportacionNormalizador.SugerirCategoria(fila.Nombre);
            fila.Origen = linea.Origen;
            filas.Add(fila);
        }
        return Task.FromResult<IReadOnlyList<ProductoImportacionFilaViewModel>>(filas);
    }
}
