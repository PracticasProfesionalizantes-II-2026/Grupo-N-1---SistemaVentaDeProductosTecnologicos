using Microsoft.EntityFrameworkCore;
using Prometheus;
using Totaltech.Datos;

namespace Totaltech.Observabilidad;

public sealed class SqlDependencyWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly Gauge.Child _disponible;
    private readonly ILogger<SqlDependencyWorker> _logger;

    public SqlDependencyWorker(IServiceScopeFactory scopes, IMetricFactory factory, ILogger<SqlDependencyWorker> logger)
    {
        _scopes = scopes;
        _logger = logger;
        _disponible = factory.CreateGauge("totaltech_dependency_up",
            "Última comprobación de disponibilidad de la dependencia (1 disponible, 0 no disponible).",
            ["dependency"]).WithLabels("sqlserver");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // La primera conexión tampoco debe bloquear el arranque del servidor HTTP.
        await Task.Yield();
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            do
            {
                await ComprobarAsync(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    private async Task ComprobarAsync(CancellationToken stoppingToken)
    {
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        limite.CancelAfter(TimeSpan.FromSeconds(2));
        try
        {
            await using var scope = _scopes.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<TotaltechDbContext>();
            _disponible.Set(await context.Database.CanConnectAsync(limite.Token) ? 1 : 0);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
        catch (Exception)
        {
            _disponible.Set(0);
            _logger.LogWarning("La comprobación periódica de SQL Server no pudo completarse.");
        }
    }
}
