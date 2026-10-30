using Frontend.Models.ViewModels.Productos;

namespace Frontend.Services.Interfaces;

public sealed record CatalogoLineaExtraida(string Texto, string Origen);

public interface ICatalogoExtractorAgent
{
    Task<IReadOnlyList<ProductoImportacionFilaViewModel>> NormalizarAsync(
        IReadOnlyList<CatalogoLineaExtraida> lineas, CancellationToken cancellationToken = default);
}
