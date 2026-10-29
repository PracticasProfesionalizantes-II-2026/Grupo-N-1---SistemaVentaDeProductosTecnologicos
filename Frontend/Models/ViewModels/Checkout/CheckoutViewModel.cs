using Frontend.Models.Api.Responses;

namespace Frontend.Models.ViewModels.Checkout;

public sealed class CheckoutViewModel
{
    public int IdUsuario { get; init; }
    public CarritoResumenResponse? Carrito { get; set; }
    public string? Error { get; set; }
    public bool PuedeSimular => Error is null && Carrito?.Items.Count > 0;
}
