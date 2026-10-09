using System.Text;
using Microsoft.Extensions.Configuration;
using Prometheus;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Observabilidad;
using Totaltech.Repositorios;
using Totaltech.UnitTests.Support;

namespace Totaltech.UnitTests.Observabilidad;

public sealed class MetricasNegocioTests
{
    [Fact]
    public async Task BajaRepetidaCuentaDosOperacionesYUnaAuditoria()
    {
        var (registry, metricas) = CrearMetricas();
        var repo = new FakeUsuariosRepositorio();
        var usuario = new Usuario
        {
            Nombre = "Cliente", Apellido = "Prueba", Email = "cliente@test.local",
            Contrasena = "hash", Telefono = "123", Rol = RolUsuario.Cliente
        };
        await repo.CrearAsync(usuario);
        var logica = new UsuariosLogica(repo, metricas);

        await logica.CambiarEstadoAsync(usuario.IdUsuario, false, usuario.IdUsuario);
        await logica.CambiarEstadoAsync(usuario.IdUsuario, false, usuario.IdUsuario);

        Assert.Single(repo.Auditorias);
        Assert.Contains("totaltech_user_operations_total{operation=\"deactivate\",result=\"success\"} 2", await ExportarAsync(registry));
    }

    [Fact]
    public async Task ErrorYCancelacionSeCuentanSinAlterarLaExcepcion()
    {
        var (registry, metricas) = CrearMetricas();
        var error = new InvalidOperationException("Prueba");
        var cancelacion = new OperationCanceledException();
        Assert.Same(error, await Assert.ThrowsAsync<InvalidOperationException>(() =>
            metricas.MedirUsuarioAsync("update", () => Task.FromException<ResultadoUsuario>(error))));
        Assert.Same(cancelacion, await Assert.ThrowsAsync<OperationCanceledException>(() =>
            metricas.MedirCheckoutAsync(() => Task.FromException<ConfirmarCarritoResultado>(cancelacion))));

        var texto = await ExportarAsync(registry);
        Assert.Contains("totaltech_user_operations_total{operation=\"update\",result=\"error\"} 1", texto);
        Assert.Contains("totaltech_checkout_operations_total{result=\"cancelled\"} 1", texto);
        Assert.Contains("totaltech_checkout_duration_seconds_count{result=\"cancelled\"} 1", texto);
    }

    [Fact]
    public async Task DosIntentosTransaccionalesCuentanSoloElResultadoExterior()
    {
        var (registry, metricas) = CrearMetricas();
        var repo = new DosIntentosRepositorio();
        var usuario = new Usuario
        {
            Nombre = "Cliente", Apellido = "Prueba", Email = "cliente@test.local",
            Contrasena = "hash", Telefono = "123", Rol = RolUsuario.Cliente
        };
        await repo.CrearAsync(usuario);
        var resultado = await new UsuariosLogica(repo, metricas).ActualizarAsync(usuario.IdUsuario,
            new UsuarioActualizacionRequest
            {
                Nombre = usuario.Nombre, Apellido = usuario.Apellido, Email = usuario.Email,
                Telefono = usuario.Telefono, Rol = usuario.Rol
            }, usuario.IdUsuario, false);

        Assert.Equal(EstadoOperacionUsuario.Exito, resultado.Estado);
        Assert.Equal(2, repo.Intentos);
        Assert.Contains("totaltech_user_operations_total{operation=\"update\",result=\"success\"} 1", await ExportarAsync(registry));
    }

    [Fact]
    public async Task DeshabilitadaPreservaElResultadoSinRegistrarFamilias()
    {
        var (registry, metricas) = CrearMetricas(enabled: false);
        var esperado = new ConfirmarCarritoResultado(EstadoConfirmacionCarrito.Creado);
        Assert.Same(esperado, await metricas.MedirCheckoutAsync(() => Task.FromResult(esperado)));
        metricas.RegistrarAutenticacion("login", "success");
        Assert.DoesNotContain("totaltech_", await ExportarAsync(registry));
    }

    private static (CollectorRegistry Registry, MetricasNegocio Metricas) CrearMetricas(bool enabled = true)
    {
        var registry = Metrics.NewCustomRegistry();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Observability:Enabled"] = enabled.ToString() }).Build();
        return (registry, new MetricasNegocio(Metrics.WithCustomRegistry(registry), config));
    }

    private static async Task<string> ExportarAsync(CollectorRegistry registry)
    {
        await using var stream = new MemoryStream();
        await registry.CollectAndExportAsTextAsync(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private sealed class DosIntentosRepositorio : IUsuariosRepositorio
    {
        private readonly FakeUsuariosRepositorio _inner = new();
        public int Intentos { get; private set; }
        public async Task<T> EjecutarTransaccionAsync<T>(Func<Task<T>> operacion)
        {
            Intentos++;
            await operacion();
            Intentos++;
            return await operacion();
        }
        public Task<List<Usuario>> ObtenerTodosAsync() => _inner.ObtenerTodosAsync();
        public Task<Usuario?> ObtenerPorIdAsync(int id) => _inner.ObtenerPorIdAsync(id);
        public Task<bool> ExisteAsync(int id) => _inner.ExisteAsync(id);
        public Task CrearAsync(Usuario usuario) => _inner.CrearAsync(usuario);
        public Task ActualizarContrasenaAsync(Usuario usuario) => _inner.ActualizarContrasenaAsync(usuario);
        public Task<int> ContarAdministradoresActivosAsync() => _inner.ContarAdministradoresActivosAsync();
        public Task GuardarCambioAsync(Usuario usuario, AuditoriaUsuario auditoria) => _inner.GuardarCambioAsync(usuario, auditoria);
        public Task<Usuario?> ObtenerPorEmailAsync(string email) => _inner.ObtenerPorEmailAsync(email);
    }
}
