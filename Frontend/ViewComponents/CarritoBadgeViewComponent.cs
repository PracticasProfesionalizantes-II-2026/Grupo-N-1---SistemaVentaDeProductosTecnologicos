using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.ViewComponents;

public sealed class CarritoBadgeViewComponent : ViewComponent
{
    private readonly ICarritosApiService _carritos;
    public CarritoBadgeViewComponent(ICarritosApiService carritos) => _carritos = carritos;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        try { return View((await _carritos.ObtenerActualAsync())?.CantidadTotal ?? 0); }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return View(0); }
    }
}
