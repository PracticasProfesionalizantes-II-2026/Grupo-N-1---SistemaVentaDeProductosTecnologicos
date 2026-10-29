using System.Globalization;
using Frontend.Models.Api.Responses;

namespace Frontend.Models.ViewModels.Home;

public sealed class InicioViewModel
{
    public IReadOnlyList<ProductoResponse> Productos { get; init; } = [];
    public IReadOnlyList<ProductoResponse> Destacados { get; init; } = [];
    public IReadOnlyList<InicioCategoriaViewModel> Categorias { get; init; } = [];
    public string? Error { get; init; }
    public string? ErrorCategorias { get; init; }

    public static InicioViewModel Desde(IEnumerable<ProductoResponse> productos,
        IEnumerable<CategoriaResponse>? categorias = null, string? error = null, string? errorCategorias = null)
    {
        var ordenados = productos.OrderBy(p => p.Nombre,
            StringComparer.Create(CultureInfo.GetCultureInfo("es-AR"), ignoreCase: true))
            .ThenBy(p => p.IdProducto).ToList();
        var disponibles = ordenados.Where(p => p.Stock > 0).ToList();
        // Selección visual de demostración: diversidad de categorías, sin ranking de ventas.
        var destacados = disponibles.GroupBy(p => p.IdCategoria).Select(g => g.First()).Take(6).ToList();
        var seleccionados = destacados.Select(p => p.IdProducto).ToHashSet();
        destacados.AddRange(disponibles.Where(p => !seleccionados.Contains(p.IdProducto)).Take(6 - destacados.Count));
        var tarjetas = (categorias ?? []).OrderBy(c => c.Nombre,
            StringComparer.Create(CultureInfo.GetCultureInfo("es-AR"), ignoreCase: true))
            .ThenBy(c => c.IdCategoria).Select(InicioCategoriaViewModel.Desde).ToList();
        return new InicioViewModel
        {
            Productos = ordenados, Destacados = destacados, Categorias = tarjetas,
            Error = error, ErrorCategorias = errorCategorias
        };
    }
}
