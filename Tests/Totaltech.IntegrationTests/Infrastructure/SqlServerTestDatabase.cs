using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;

namespace Totaltech.IntegrationTests.Infrastructure;

public sealed class SqlServerTestDatabase : IAsyncLifetime
{
    internal const string LocalDbDataSource = @"(localdb)\MSSQLLocalDB";
    internal const string DatabasePrefix = "TotaltechTests_";

    public SqlServerTestDatabase()
    {
        DatabaseName = $"{DatabasePrefix}{Guid.NewGuid():N}";
        ConnectionString = new SqlConnectionStringBuilder
        {
            DataSource = LocalDbDataSource,
            InitialCatalog = DatabaseName,
            IntegratedSecurity = true,
            MultipleActiveResultSets = true,
            TrustServerCertificate = true,
            ConnectTimeout = 30
        }.ConnectionString;
    }

    public string DatabaseName { get; }

    public string ConnectionString { get; }

    public async Task InitializeAsync()
    {
        ValidateConnectionString(ConnectionString);
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        ValidateConnectionString(ConnectionString);
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }

    public TotaltechDbContext CreateContext()
    {
        ValidateConnectionString(ConnectionString);

        var options = new DbContextOptionsBuilder<TotaltechDbContext>()
            .UseSqlServer(
                ConnectionString,
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure())
            .Options;

        return new TotaltechDbContext(options);
    }

    internal static void ValidateConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("La cadena de pruebas SQL no puede estar vacía.");
        }

        var builder = new SqlConnectionStringBuilder(connectionString);
        var isLocalDb = string.Equals(
            builder.DataSource,
            LocalDbDataSource,
            StringComparison.OrdinalIgnoreCase);
        var isDisposableDatabase = builder.InitialCatalog.StartsWith(
            DatabasePrefix,
            StringComparison.Ordinal);

        if (!isLocalDb || !isDisposableDatabase)
        {
            throw new InvalidOperationException(
                "Las pruebas relacionales sólo pueden usar MSSQLLocalDB con una base TotaltechTests_<GUID>.");
        }
    }
}
