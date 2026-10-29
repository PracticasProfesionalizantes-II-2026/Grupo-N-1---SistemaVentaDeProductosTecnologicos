using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.IntegrationTests.Infrastructure;

namespace Totaltech.IntegrationTests.Persistencia;

[Trait("Category", "SqlServer")]
public sealed class UsuariosCompatibilidadScriptTests : IAsyncLifetime
{
    private readonly SqlServerTestDatabase _database = new();
    public Task InitializeAsync() => _database.InitializeAsync();
    public Task DisposeAsync() => _database.DisposeAsync();

    [Fact]
    public async Task ReparaEsquemaHistoricoConservaUsuarioYEsIdempotente()
    {
        await using var db = _database.CreateContext();
        var usuario = new Usuario
        {
            Nombre = "Prueba", Apellido = "Aislada", Email = "compatibilidad@example.test",
            Contrasena = "solo-dato-de-prueba-no-credencial", Telefono = "1111111111",
            FechaRegistro = DateTime.UtcNow, Rol = RolUsuario.Cliente
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        await db.Database.ExecuteSqlRawAsync("""
            DROP TABLE dbo.AuditoriaUsuarios;
            DECLARE @drop nvarchar(max) = (
                SELECT STRING_AGG(CONVERT(nvarchar(max),
                    N'ALTER TABLE dbo.Usuarios DROP CONSTRAINT '+QUOTENAME(d.name)+N';'), N' ')
                FROM sys.default_constraints d
                JOIN sys.columns c ON c.object_id=d.parent_object_id AND c.column_id=d.parent_column_id
                WHERE d.parent_object_id=OBJECT_ID(N'dbo.Usuarios') AND c.name IN (N'Activo',N'VersionSesion')
            );
            EXEC sys.sp_executesql @drop;
            ALTER TABLE dbo.Usuarios DROP COLUMN Activo,VersionSesion;
            """);

        var raiz = new DirectoryInfo(AppContext.BaseDirectory);
        while (raiz is not null && !File.Exists(Path.Combine(raiz.FullName,
            "Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln"))) raiz = raiz.Parent;
        Assert.NotNull(raiz);
        var script = await File.ReadAllTextAsync(Path.Combine(raiz.FullName,
            "Documentacion", "SQL", "Totaltech-Usuarios-compatibilidad.sql"));
        SqlServerTestDatabase.ValidateConnectionString(_database.ConnectionString);
        script = script.Replace("DB_NAME() <> N'TotatechDB'",
            $"DB_NAME() <> N'{_database.DatabaseName}'", StringComparison.Ordinal);
        await db.Database.ExecuteSqlRawAsync(script);
        await db.Database.ExecuteSqlRawAsync(script);
        db.ChangeTracker.Clear();
        var conservado = await db.Usuarios.SingleAsync();
        Assert.Equal(usuario.IdUsuario, conservado.IdUsuario);
        Assert.Equal(usuario.Contrasena, conservado.Contrasena);
        Assert.Equal(RolUsuario.Cliente, conservado.Rol);
        Assert.True(conservado.Activo);
        Assert.Equal(1, conservado.VersionSesion);
        Assert.Empty(await db.AuditoriaUsuarios.ToListAsync());
    }
}
