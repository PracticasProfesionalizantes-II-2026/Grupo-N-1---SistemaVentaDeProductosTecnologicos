namespace Frontend.Models.ViewModels.Administracion;

public sealed class AdminDashboardViewModel
{
    public int? TotalProductos { get; init; }
    public int? TotalUsuarios { get; init; }
    public int? PedidosPendientes { get; init; }

    public bool TieneMetricasNoDisponibles =>
        TotalProductos is null || TotalUsuarios is null || PedidosPendientes is null;
}
