using Frontend.Models.Api.Responses;

namespace Frontend.Models.ViewModels.Carrito;

public sealed class CarritoViewModel
{
    public List<CarritoLineaResponse> Items { get; set; } = [];
    public int CantidadTotal { get; set; }
    public decimal Total { get; set; }
    public bool Vacio => Items.Count == 0;

    public static CarritoViewModel Desde(CarritoResumenResponse? resumen) => new()
    {
        Items = resumen?.Items ?? [],
        CantidadTotal = resumen?.CantidadTotal ?? 0,
        Total = resumen?.Total ?? 0
    };
}
