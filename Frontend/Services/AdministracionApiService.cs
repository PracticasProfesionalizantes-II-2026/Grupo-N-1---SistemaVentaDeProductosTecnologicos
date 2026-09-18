using System.Net.Http.Json;
using Frontend.Models.Api.Responses;

namespace Frontend.Services;

public sealed record AdminDashboardMetricas(
    int? TotalProductos,
    int? TotalUsuarios,
    int? PedidosPendientes);

public sealed class AdministracionApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AdministracionApiService> _logger;

    public AdministracionApiService(
        IHttpClientFactory httpClientFactory,
        ILogger<AdministracionApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<AdminDashboardMetricas> ObtenerMetricasAsync(
        CancellationToken cancellationToken)
    {
        var productosTask = ObtenerTotalProductosSeguroAsync(cancellationToken);
        var usuariosTask = ObtenerTotalUsuariosSeguroAsync(cancellationToken);
        var pedidosTask = ObtenerPedidosPendientesSeguroAsync(cancellationToken);

        await Task.WhenAll(productosTask, usuariosTask, pedidosTask);

        return new AdminDashboardMetricas(
            await productosTask,
            await usuariosTask,
            await pedidosTask);
    }

    private async Task<int?> ObtenerTotalProductosSeguroAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var catalogo = await CrearCliente().GetFromJsonAsync<CatalogoProductosResponse>(
                "/productos/catalogo?pagina=1&tamanoPagina=1",
                cancellationToken);
            return catalogo?.TotalItems;
        }
        catch (Exception exception) when (EsFalloRecuperable(exception, cancellationToken))
        {
            _logger.LogWarning(exception, "No se pudo obtener el total de productos del panel administrativo.");
            return null;
        }
    }

    private async Task<int?> ObtenerTotalUsuariosSeguroAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarios = await CrearCliente().GetFromJsonAsync<List<UsuarioResumenResponse>>(
                "/usuarios/",
                cancellationToken);
            return usuarios?.Count;
        }
        catch (Exception exception) when (EsFalloRecuperable(exception, cancellationToken))
        {
            _logger.LogWarning(exception, "No se pudo obtener el total de usuarios del panel administrativo.");
            return null;
        }
    }

    private async Task<int?> ObtenerPedidosPendientesSeguroAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var pedidos = await CrearCliente().GetFromJsonAsync<List<PedidoResumenResponse>>(
                "/pedidos/estado/Pendiente",
                cancellationToken);
            return pedidos?.Count;
        }
        catch (Exception exception) when (EsFalloRecuperable(exception, cancellationToken))
        {
            _logger.LogWarning(exception, "No se pudo obtener la cantidad de pedidos pendientes del panel administrativo.");
            return null;
        }
    }

    private HttpClient CrearCliente()
    {
        return _httpClientFactory.CreateClient("TotaltechApi");
    }

    private static bool EsFalloRecuperable(
        Exception exception,
        CancellationToken cancellationToken)
    {
        return exception is HttpRequestException or NotSupportedException or System.Text.Json.JsonException ||
               exception is TaskCanceledException && !cancellationToken.IsCancellationRequested;
    }
}
