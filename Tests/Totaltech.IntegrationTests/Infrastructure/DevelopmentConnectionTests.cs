using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Totaltech.Datos;

namespace Totaltech.IntegrationTests.Infrastructure;

public sealed class DevelopmentConnectionTests
{
    [Theory]
    [InlineData("http")]
    [InlineData("https")]
    [InlineData("TotaltechLocalDb")]
    public void PerfilesLocalesSeleccionanMssqlLocalDbSinMigracionesAutomaticas(string perfil)
    {
        var raiz = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        using var documento = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(raiz, "Totaltech", "Properties", "launchSettings.json")));
        var variables = documento.RootElement.GetProperty("profiles").GetProperty(perfil)
            .GetProperty("environmentVariables");
        var conexion = new SqlConnectionStringBuilder(
            variables.GetProperty("ConnectionStrings__DefaultConnection").GetString());

        Assert.Equal("Development", variables.GetProperty("ASPNETCORE_ENVIRONMENT").GetString());
        Assert.Equal(@"(localdb)\MSSQLLocalDB", conexion.DataSource);
        Assert.Equal("TotaltechDev", conexion.InitialCatalog);
        Assert.True(conexion.IntegratedSecurity);
        Assert.Empty(conexion.AttachDBFilename);
        Assert.Equal("false", variables.GetProperty("Database__ApplyMigrations").GetString());
    }

    [Theory]
    [InlineData(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TotaltechDev;Integrated Security=True;TrustServerCertificate=True")]
    [InlineData(@"Data Source=(localdb)\TotaltechLocalDb;Initial Catalog=TotaltechDev;Integrated Security=True;TrustServerCertificate=True")]
    [InlineData(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CatalogoExplicito;AttachDBFilename=C:\ruta-configurada\Explicito.mdf;Integrated Security=True;TrustServerCertificate=True")]
    public async Task Development_ConservaLaConexionConfiguradaSinInferirUnArchivo(string connectionString)
    {
        await using var factory = new DevelopmentFactory(connectionString);
        using var client = factory.CreateClient();

        var expected = new SqlConnectionStringBuilder(connectionString);
        var actual = new SqlConnectionStringBuilder(factory.ConfiguredSqlConnection);
        Assert.Equal(expected.DataSource, actual.DataSource);
        Assert.Equal(expected.InitialCatalog, actual.InitialCatalog);
        Assert.Equal(expected.AttachDBFilename, actual.AttachDBFilename);
        Assert.True(expected.EquivalentTo(actual), "La conexión efectiva debe conservar todos los parámetros configurados.");
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TotaltechDbContext>();
        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", context.Database.ProviderName);
        Assert.False(context.Database.IsRelational());
    }

    private sealed class DevelopmentFactory(string connectionString) : WebApplicationFactory<TotaltechDbContext>
    {
        public string? ConfiguredSqlConnection { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
            builder.UseSetting("Authentication:Issuer", "DevelopmentConnectionTests");
            builder.UseSetting("Authentication:Audience", "DevelopmentConnectionTests.Client");
            builder.UseSetting("Authentication:SigningKey", "development-connection-tests-signing-key-2026");
            builder.UseSetting("Database:ApplyMigrations", "false");
            builder.UseSetting("DemoData:Enabled", "false");
            builder.UseSetting("BootstrapAdmin:Enabled", "false");
            builder.UseSetting("ApiReference:Enabled", "false");
            builder.UseSetting("Observability:Enabled", "false");
            builder.ConfigureServices(services =>
            {
                // Inspeccionar opciones SQL no abre una conexión. El arranque
                // posterior utiliza exclusivamente esta base InMemory aislada.
                using (var provider = services.BuildServiceProvider())
                using (var scope = provider.CreateScope())
                {
                    var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<TotaltechDbContext>>();
                    ConfiguredSqlConnection = options.Extensions.OfType<RelationalOptionsExtension>()
                        .Single().ConnectionString;
                }

                services.RemoveAll<TotaltechDbContext>();
                services.RemoveAll<DbContextOptions<TotaltechDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<TotaltechDbContext>>();
                var databaseName = $"development-connection-tests-{Guid.NewGuid():N}";
                services.AddDbContext<TotaltechDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));
            });
        }
    }
}
