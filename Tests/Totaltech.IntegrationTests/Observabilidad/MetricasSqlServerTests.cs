using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Prometheus;
using Totaltech.Datos;
using Totaltech.IntegrationTests.Infrastructure;
using Totaltech.Observabilidad;

namespace Totaltech.IntegrationTests.Observabilidad;

[Trait("Category", "SqlServer")]
public sealed class MetricasSqlServerTests(SqlServerTestDatabase database) : IClassFixture<SqlServerTestDatabase>
{
    [Fact]
    public async Task ReaderScalarNonqueryYExecuteUpdateMidenSyncYAsyncSinSqlEnEtiquetas()
    {
        var (registry, interceptor) = CrearInterceptor();
        await using var context = database.CreateContext(interceptor);
        _ = context.Usuarios.Count();
        _ = await context.Usuarios.CountAsync();

        var (command, parameters) = CrearScalar(context, "SELECT 1");
        Assert.Equal(1, command.ExecuteScalar(parameters));
        Assert.Equal(1, await command.ExecuteScalarAsync(parameters));

        context.Database.ExecuteSqlRaw("SELECT 1");
        await context.Database.ExecuteSqlRawAsync("SELECT 1");
        context.Usuarios.Where(u => u.IdUsuario == -1).ExecuteUpdate(update => update.SetProperty(u => u.Activo, false));
        await context.Usuarios.Where(u => u.IdUsuario == -1).ExecuteUpdateAsync(update => update.SetProperty(u => u.Activo, false));

        var texto = await ExportarAsync(registry);
        Assert.Contains("totaltech_db_commands_total{execution=\"reader\",result=\"success\"} 2", texto);
        Assert.Contains("totaltech_db_commands_total{execution=\"scalar\",result=\"success\"} 2", texto);
        Assert.Contains("totaltech_db_commands_total{execution=\"nonquery\",result=\"success\"} 4", texto);
        Assert.Contains("totaltech_db_command_duration_seconds_count{execution=\"nonquery\",result=\"success\"} 4", texto);
        Assert.DoesNotContain("SELECT", texto);
        Assert.DoesNotContain("Usuarios", texto);
        Assert.DoesNotContain("-1", texto);
    }

    [Fact]
    public async Task FalloSqlSyncYAsyncEsErrorYSePropaga()
    {
        var (registry, interceptor) = CrearInterceptor();
        await using var context = database.CreateContext(interceptor);
        var (command, parameters) = CrearScalar(context, ";THROW 50001, 'Fallo controlado de prueba', 1;");
        Assert.Throws<SqlException>(() => command.ExecuteScalar(parameters));
        await Assert.ThrowsAsync<SqlException>(() => command.ExecuteScalarAsync(parameters));
        Assert.Contains("totaltech_db_commands_total{execution=\"scalar\",result=\"error\"} 2", await ExportarAsync(registry));
    }

    [Fact]
    public async Task CancelacionDeComandoEnCursoSePropagaYCuentaUnaVez()
    {
        var (registry, interceptor) = CrearInterceptor();
        await using var context = database.CreateContext(interceptor);
        var (command, parameters) = CrearScalar(context, "WAITFOR DELAY '00:00:05'; SELECT 1;");
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
        var error = await Record.ExceptionAsync(() => command.ExecuteScalarAsync(parameters, cancellation.Token));
        // SqlClient también puede expresar la cancelación de un comando en curso
        // como SqlException; el interceptor debe conservar la excepción original.
        Assert.True(error is OperationCanceledException or SqlException);
        Assert.True(cancellation.IsCancellationRequested);
        var texto = await ExportarAsync(registry);
        Assert.Contains("totaltech_db_commands_total{execution=\"scalar\",result=\"cancelled\"} 1", texto);
        Assert.DoesNotContain("result=\"success\"", texto);
        Assert.DoesNotContain("result=\"error\"", texto);
        Assert.DoesNotContain("WAITFOR", texto);
    }

    private static (IRelationalCommand Command, RelationalCommandParameterObject Parameters) CrearScalar(
        TotaltechDbContext context, string sql)
    {
        var builder = context.GetService<IRelationalCommandBuilderFactory>().Create();
        builder.Append(sql);
        return (builder.Build(), new RelationalCommandParameterObject(
            context.GetService<IRelationalConnection>(), null, null, context,
            context.GetService<IRelationalCommandDiagnosticsLogger>()));
    }

    private static (CollectorRegistry Registry, MetricasSqlInterceptor Interceptor) CrearInterceptor()
    {
        var registry = Metrics.NewCustomRegistry();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Observability:Enabled"] = "true" }).Build();
        return (registry, new MetricasSqlInterceptor(Metrics.WithCustomRegistry(registry), config));
    }

    private static async Task<string> ExportarAsync(CollectorRegistry registry)
    {
        await using var stream = new MemoryStream();
        await registry.CollectAndExportAsTextAsync(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
