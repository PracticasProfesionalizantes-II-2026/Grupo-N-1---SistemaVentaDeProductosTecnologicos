using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace Totaltech.Observabilidad;

// Compilado en ambos hosts: cada contenedor DI conserva su propio registro.
internal static class ObservabilityExtensions
{
    private static readonly object ObservationKey = new();

    public static IServiceCollection AddTotaltechObservability(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ => new ObservabilitySettings(configuration.GetValue<bool>("Observability:Enabled")));
        services.AddSingleton(_ => Metrics.NewCustomRegistry());
        services.AddSingleton<IMetricFactory>(provider =>
        {
            var factory = Metrics.WithCustomRegistry(provider.GetRequiredService<CollectorRegistry>());
            factory.ExemplarBehavior = new ExemplarBehavior { DefaultExemplarProvider = (_, _) => Exemplar.None };
            return factory;
        });
        services.AddSingleton<HttpMetricInstruments>();
        services.AddSingleton<RuntimeMetricRegistration>();
        return services;
    }

    public static IApplicationBuilder UseTotaltechMetricsEndpoint(this IApplicationBuilder app)
    {
        var enabled = app.ApplicationServices.GetRequiredService<ObservabilitySettings>().Enabled;
        var registry = app.ApplicationServices.GetRequiredService<CollectorRegistry>();
        app.ApplicationServices.GetRequiredService<RuntimeMetricRegistration>();
        return app.MapWhen(context => context.Request.Path.Equals(new PathString("/metrics")), branch =>
            branch.Run(async context =>
            {
                if (!enabled)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
                var remote = context.Connection.RemoteIpAddress;
                if (remote?.IsIPv4MappedToIPv6 == true) remote = remote.MapToIPv4();
                if (remote is null || !IPAddress.IsLoopback(remote))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
                if (!HttpMethods.IsGet(context.Request.Method))
                {
                    context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                    context.Response.Headers.Allow = "GET";
                    return;
                }
                context.Response.ContentType = "text/plain; version=0.0.4; charset=utf-8";
                context.Response.Headers.CacheControl = "no-store";
                await registry.CollectAndExportAsTextAsync(context.Response.Body, context.RequestAborted);
            }));
    }

    public static IApplicationBuilder UseTotaltechHttpMetrics(this IApplicationBuilder app)
    {
        if (!app.ApplicationServices.GetRequiredService<ObservabilitySettings>().Enabled) return app;
        var instruments = app.ApplicationServices.GetRequiredService<HttpMetricInstruments>();
        return app.Use(async (context, next) =>
        {
            if (IsStatic(context.Request.Path) || context.Items.ContainsKey(ObservationKey))
            {
                await next(context);
                return;
            }
            var observation = new HttpObservation(instruments, NormalizeMethod(context.Request.Method));
            context.Items[ObservationKey] = observation;
            var started = Stopwatch.GetTimestamp();
            string? exceptionalCode = null;
            try
            {
                await next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                exceptionalCode = "cancelled";
                throw;
            }
            catch (BadHttpRequestException ex)
            {
                exceptionalCode = ex.StatusCode.ToString();
                throw;
            }
            catch
            {
                // Kestrel genera el 500 fuera del pipeline cuando la excepción escapa.
                exceptionalCode = context.Response.HasStarted ? context.Response.StatusCode.ToString() : "500";
                throw;
            }
            finally
            {
                observation.CaptureRoute(context);
                observation.Finish(exceptionalCode ?? context.Response.StatusCode.ToString(), Stopwatch.GetElapsedTime(started).TotalSeconds);
            }
        });
    }

    public static IApplicationBuilder CaptureTotaltechMetricRoute(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            if (context.Items.TryGetValue(ObservationKey, out var value) && value is HttpObservation observation)
                observation.CaptureRoute(context);
            await next(context);
        });

    private static bool IsStatic(PathString path) =>
        path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/uploads", StringComparison.OrdinalIgnoreCase) ||
        IsScopedCss(path.Value) ||
        path.Equals(new PathString("/favicon.ico"));

    private static bool IsScopedCss(string? path) =>
        path is not null && path.StartsWith("/Frontend.", StringComparison.OrdinalIgnoreCase) &&
        path.IndexOf('/', 1) < 0 &&
        (path.EndsWith(".styles.css", StringComparison.OrdinalIgnoreCase) ||
         path.EndsWith(".styles.css.gz", StringComparison.OrdinalIgnoreCase) ||
         path.EndsWith(".styles.css.br", StringComparison.OrdinalIgnoreCase));

    private static string NormalizeMethod(string method) => method.ToUpperInvariant() switch
    {
        "GET" => "GET", "POST" => "POST", "PUT" => "PUT", "PATCH" => "PATCH",
        "DELETE" => "DELETE", "HEAD" => "HEAD", "OPTIONS" => "OPTIONS", _ => "OTHER"
    };

    private sealed class HttpObservation(HttpMetricInstruments instruments, string method)
    {
        private string? _route;

        public void CaptureRoute(HttpContext context)
        {
            if (_route is not null) return;
            var endpoint = context.GetEndpoint();
            var action = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
            _route = action is not null ? $"{action.ControllerName}/{action.ActionName}"
                : (endpoint as RouteEndpoint)?.RoutePattern.RawText ?? "_unmatched";
            instruments.InProgress.WithLabels(method, _route).Inc();
        }

        public void Finish(string code, double seconds)
        {
            instruments.InProgress.WithLabels(method, _route!).Dec();
            instruments.Requests.WithLabels(method, _route!, code).Inc();
            instruments.Duration.WithLabels(method, _route!, code).Observe(seconds);
        }
    }
}

internal sealed record ObservabilitySettings(bool Enabled);

internal sealed class HttpMetricInstruments
{
    public Counter Requests { get; }
    public Histogram Duration { get; }
    public Gauge InProgress { get; }

    public HttpMetricInstruments(IMetricFactory factory)
    {
        Requests = factory.CreateCounter("totaltech_http_requests_total", "Solicitudes HTTP funcionales terminadas.", ["http_method", "route", "code"]);
        Duration = factory.CreateHistogram("totaltech_http_request_duration_seconds", "Duración completa de solicitudes HTTP funcionales.", new HistogramConfiguration
        {
            LabelNames = ["http_method", "route", "code"],
            Buckets = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10]
        });
        InProgress = factory.CreateGauge("totaltech_http_requests_in_progress", "Solicitudes HTTP funcionales en ejecución.", ["http_method", "route"]);
    }
}

internal sealed class RuntimeMetricRegistration
{
    public RuntimeMetricRegistration(CollectorRegistry registry, IMetricFactory factory, ObservabilitySettings settings)
    {
        if (!settings.Enabled) return;
        DotNetStats.Register(registry);
        var threads = factory.CreateGauge("totaltech_threadpool_threads", "Hilos del ThreadPool.");
        var pending = factory.CreateGauge("totaltech_threadpool_pending_work_items", "Trabajo pendiente del ThreadPool.");
        registry.AddBeforeCollectCallback(() =>
        {
            threads.Set(ThreadPool.ThreadCount);
            pending.Set(ThreadPool.PendingWorkItemCount);
        });
    }
}
