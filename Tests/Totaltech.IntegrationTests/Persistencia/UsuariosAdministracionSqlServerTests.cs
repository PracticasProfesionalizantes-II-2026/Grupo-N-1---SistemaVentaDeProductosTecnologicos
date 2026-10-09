using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Prometheus;
using System.Text;
using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.IntegrationTests.Infrastructure;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Observabilidad;
using Totaltech.Repositorios;

namespace Totaltech.IntegrationTests.Persistencia;

[Trait("Category", "SqlServer")]
public sealed class UsuariosAdministracionSqlServerTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OperacionesConcurrentes_PreservanUnAdministradorActivo(bool degradar)
    {
        var database = new SqlServerTestDatabase();
        var (registry, metricas) = CrearMetricas();
        try
        {
            await database.InitializeAsync();
            int primero, segundo;
            await using (var db = database.CreateContext())
            {
                var a = Admin("a"); var b = Admin("b");
                db.Usuarios.AddRange(a, b);
                await db.SaveChangesAsync();
                primero = a.IdUsuario; segundo = b.IdUsuario;
            }
            var inicio = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            async Task<ResultadoUsuario> CambiarAsync(int id)
            {
                await using var db = database.CreateContext();
                var logica = new UsuariosLogica(new UsuariosRepositorio(db), metricas);
                await inicio.Task;
                return degradar
                    ? await logica.ActualizarAsync(id, new()
                    { Nombre = "Admin", Apellido = "Prueba", Email = id == primero ? "a@test.local" : "b@test.local",
                      Telefono = "111", Rol = RolUsuario.Cliente }, id, true)
                    : await logica.CambiarEstadoAsync(id, false, id);
            }
            var tareas = new[] { CambiarAsync(primero), CambiarAsync(segundo) };
            inicio.SetResult();
            var resultados = await Task.WhenAll(tareas);
            Assert.Single(resultados, r => r.Estado == EstadoOperacionUsuario.Exito);
            Assert.Single(resultados, r => r.Estado == EstadoOperacionUsuario.Conflicto);
            await using var verificacion = database.CreateContext();
            Assert.Equal(1, await verificacion.Usuarios.CountAsync(u => u.Activo && u.Rol == RolUsuario.Administrador));
            Assert.Equal(1, await verificacion.AuditoriaUsuarios.CountAsync());
            var texto = await ExportarAsync(registry);
            var operacion = degradar ? "update" : "deactivate";
            Assert.Contains($"totaltech_user_operations_total{{operation=\"{operacion}\",result=\"success\"}} 1", texto);
            Assert.Contains($"totaltech_user_operations_total{{operation=\"{operacion}\",result=\"conflict\"}} 1", texto);
        }
        finally { await database.DisposeAsync(); }
    }

    [Fact]
    public async Task MigracionIncremental_ActivaUsuariosExistentesSinPerderDatos()
    {
        var database = new SqlServerTestDatabase();
        try
        {
            await database.InitializeAsync();
            await using var db = database.CreateContext();
            var migrador = db.GetService<IMigrator>();
            await migrador.MigrateAsync("20260929214228_AgregarImagenProducto");
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Usuarios (Nombre, Apellido, Email, Contrasena, Telefono, FechaRegistro, Rol) " +
                "VALUES (N'Anterior', N'Prueba', N'anterior@test.local', N'hash-historico', N'111', '2026-01-01', 0)");
            await migrador.MigrateAsync();
            var usuario = await db.Usuarios.SingleAsync();
            Assert.True(usuario.Activo);
            Assert.Equal(1, usuario.VersionSesion);
            Assert.Equal("hash-historico", usuario.Contrasena);
            Assert.Equal(new DateTime(2026, 1, 1), usuario.FechaRegistro);
            Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        }
        finally { await database.DisposeAsync(); }
    }

    [Fact]
    public async Task Baja_PreservaPedidoDireccionCarritoYAuditoria()
    {
        var database = new SqlServerTestDatabase();
        try
        {
            await database.InitializeAsync();
            await using var db = database.CreateContext();
            var admin = Admin("admin");
            var cliente = Admin("cliente"); cliente.Rol = RolUsuario.Cliente;
            var direccion = new Direccion { Usuario = cliente, Calle = "Histórica" };
            var carrito = new Carrito { Usuario = cliente, FechaCreacion = DateTime.UtcNow };
            var pedido = new Pedido
            {
                Usuario = cliente, Carrito = carrito, Direccion = direccion,
                FechaPedido = DateTime.UtcNow, Total = 10, DireccionCalle = "Histórica"
            };
            db.Usuarios.Add(admin);
            db.Pedidos.Add(pedido);
            await db.SaveChangesAsync();
            var resultado = await new UsuariosLogica(new UsuariosRepositorio(db))
                .CambiarEstadoAsync(cliente.IdUsuario, false, admin.IdUsuario);
            Assert.Equal(EstadoOperacionUsuario.Exito, resultado.Estado);
            db.ChangeTracker.Clear();
            Assert.False((await db.Usuarios.FindAsync(cliente.IdUsuario))!.Activo);
            Assert.Equal(cliente.IdUsuario, (await db.Pedidos.SingleAsync()).IdUsuario);
            Assert.Equal(cliente.IdUsuario, (await db.Carritos.SingleAsync()).IdUsuario);
            Assert.Equal(cliente.IdUsuario, (await db.Direcciones.SingleAsync()).IdUsuario);
            Assert.Equal("Baja", (await db.AuditoriaUsuarios.SingleAsync()).Accion);
        }
        finally { await database.DisposeAsync(); }
    }

    [Fact]
    public async Task FalloDespuesDeGuardar_RevierteUsuarioYAuditoria()
    {
        var database = new SqlServerTestDatabase();
        var (registry, metricas) = CrearMetricas();
        try
        {
            await database.InitializeAsync();
            int id;
            await using (var db = database.CreateContext())
            {
                var usuario = Admin("rollback"); usuario.Rol = RolUsuario.Cliente;
                db.Usuarios.Add(usuario); await db.SaveChangesAsync(); id = usuario.IdUsuario;
            }
            var options = new DbContextOptionsBuilder<TotaltechDbContext>()
                .UseSqlServer(database.ConnectionString, sql => sql.EnableRetryOnFailure())
                .AddInterceptors(new FalloDespuesDeGuardar()).Options;
            await using (var db = new TotaltechDbContext(options))
            {
                var logica = new UsuariosLogica(new UsuariosRepositorio(db), metricas);
                await Assert.ThrowsAsync<InvalidOperationException>(() => logica.CambiarEstadoAsync(id, false, id));
            }
            await using var verificacion = database.CreateContext();
            Assert.True((await verificacion.Usuarios.SingleAsync()).Activo);
            Assert.Empty(await verificacion.AuditoriaUsuarios.ToListAsync());
            var texto = await ExportarAsync(registry);
            Assert.Contains("totaltech_user_operations_total{operation=\"deactivate\",result=\"error\"} 1", texto);
            Assert.DoesNotContain("result=\"success\"", texto);
        }
        finally { await database.DisposeAsync(); }
    }

    [Fact]
    public async Task RehashObsoleto_NoReactivaUnaCuentaDesactivada()
    {
        var database = new SqlServerTestDatabase();
        try
        {
            await database.InitializeAsync();
            await using var contextoAnterior = database.CreateContext();
            var usuario = Admin("rehash"); usuario.Rol = RolUsuario.Cliente;
            contextoAnterior.Usuarios.Add(usuario);
            await contextoAnterior.SaveChangesAsync();
            await using (var otroContexto = database.CreateContext())
            {
                var logica = new UsuariosLogica(new UsuariosRepositorio(otroContexto));
                await logica.CambiarEstadoAsync(usuario.IdUsuario, false, usuario.IdUsuario);
            }
            usuario.Contrasena = "hash-actualizado";
            await new UsuariosRepositorio(contextoAnterior).ActualizarContrasenaAsync(usuario);
            await using var verificacion = database.CreateContext();
            var actual = await verificacion.Usuarios.SingleAsync();
            Assert.False(actual.Activo);
            Assert.Equal(2, actual.VersionSesion);
            Assert.Equal("hash-actualizado", actual.Contrasena);
        }
        finally { await database.DisposeAsync(); }
    }

    private static Usuario Admin(string nombre) => new()
    {
        Nombre = "Admin", Apellido = "Prueba", Email = $"{nombre}@test.local",
        Contrasena = "hash-prueba", Telefono = "111", FechaRegistro = DateTime.UtcNow, Rol = RolUsuario.Administrador
    };

    private static (CollectorRegistry Registry, MetricasNegocio Metricas) CrearMetricas()
    {
        var registry = Metrics.NewCustomRegistry();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Observability:Enabled"] = "true" }).Build();
        return (registry, new MetricasNegocio(Metrics.WithCustomRegistry(registry), config));
    }

    private static async Task<string> ExportarAsync(CollectorRegistry registry)
    {
        await using var stream = new MemoryStream();
        await registry.CollectAndExportAsTextAsync(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }
    private sealed class FalloDespuesDeGuardar : SaveChangesInterceptor
    {
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Fallo simulado antes del commit.");
    }
}
