using System.Globalization;
using System.Text;
using Frontend.Models.Api.Responses;

namespace Frontend.Models.ViewModels.Home;

public sealed class InicioCategoriaViewModel
{
    public int IdCategoria { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? ImagenUrl { get; init; }

    public static InicioCategoriaViewModel Desde(CategoriaResponse categoria)
    {
        // La ilustración es decorativa: el identificador y el nombre provienen de la API.
        var nombre = new string(categoria.Nombre.Trim().Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray())
            .ToUpperInvariant();
        var imagen = nombre switch
        {
            "CELULARES" => "celulares.jpg",
            "PERIFERICOS" => "perifericos.jpg",
            "NOTEBOOKS" => "notebooks.jpg",
            "MONITORES" => "monitores.jpg",
            "GABINETES" => "gabinetes.png",
            "ALMACENAMIENTO" => "almacenamiento.png",
            "PLACAS DE VIDEO" => "placas-de-video.png",
            _ => null
        };
        return new InicioCategoriaViewModel
        {
            IdCategoria = categoria.IdCategoria, Nombre = categoria.Nombre,
            ImagenUrl = imagen is null ? null : "~/images/categorias/" + imagen
        };
    }
}
