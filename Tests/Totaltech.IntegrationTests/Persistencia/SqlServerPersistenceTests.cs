using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.IntegrationTests.Infrastructure;

namespace Totaltech.IntegrationTests.Persistencia;

public sealed class SqlServerPersistenceTests : IClassFixture<SqlServerTestDatabase>
{
    private readonly SqlServerTestDatabase _database;

    public SqlServerPersistenceTests(SqlServerTestDatabase database)
    {
        _database = database;
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public void GuardiaRechazaAzureYBaseCompartida()
    {
        Assert.Throws<InvalidOperationException>(() =>
            SqlServerTestDatabase.ValidateConnectionString(
                "Server=tcp:cancho.database.windows.net,1433;Database=TotaltechTests_Test;Integrated Security=true;"));

        Assert.Throws<InvalidOperationException>(() =>
            SqlServerTestDatabase.ValidateConnectionString(
                @"Server=(localdb)\MSSQLLocalDB;Database=TotatechDB;Integrated Security=true;"));
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public async Task MigracionesSeAplicanEnBaseDesechable()
    {
        await using var context = _database.CreateContext();

        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", context.Database.ProviderName);
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        Assert.NotEmpty(await context.Database.GetAppliedMigrationsAsync());
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public async Task IndiceUnicoDeEmailSeAplicaEnSqlServer()
    {
        const string email = "sql-constraint@test.local";

        await using (var firstContext = _database.CreateContext())
        {
            firstContext.Usuarios.Add(CreateUser(email));
            await firstContext.SaveChangesAsync();
        }

        await using var secondContext = _database.CreateContext();
        secondContext.Usuarios.Add(CreateUser(email));

        await Assert.ThrowsAsync<DbUpdateException>(() => secondContext.SaveChangesAsync());
    }

    private static Usuario CreateUser(string email) => new()
    {
        Nombre = "Prueba",
        Apellido = "SQL",
        Email = email,
        Contrasena = "hash-de-prueba-no-reversible",
        Telefono = "0000000000",
        FechaRegistro = DateTime.UtcNow,
        Rol = RolUsuario.Cliente
    };
}
