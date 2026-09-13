using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Totaltech.Datos;

namespace Totaltech.IntegrationTests.Infrastructure;

internal sealed class TotaltechWebApplicationFactory : WebApplicationFactory<TotaltechDbContext>
{
    private const string ProveedorInMemory = "Microsoft.EntityFrameworkCore.InMemory";
    private readonly string _nombreBase = $"totaltech-tests-{Guid.NewGuid():N}";
    private readonly InMemoryDatabaseRoot _raizBase = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Authentication:Issuer", "Totaltech.IntegrationTests");
        builder.UseSetting("Authentication:Audience", "Totaltech.IntegrationTests.Client");
        builder.UseSetting(
            "Authentication:SigningKey",
            "totaltech-integration-tests-signing-key-2026");
        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            "Server=(localdb)\\MSSQLLocalDB;Database=TotaltechTests_InMemoryHost;Trusted_Connection=True;TrustServerCertificate=True");
        builder.UseSetting("BootstrapAdmin:Enabled", "true");
        builder.UseSetting("BootstrapAdmin:Email", "Admin@admin.com");
        builder.UseSetting("BootstrapAdmin:Password", "Admin123456789");

        builder.ConfigureServices(services =>
        {
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
            services.RemoveAll<TotaltechDbContext>();
            services.RemoveAll<DbContextOptions<TotaltechDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<TotaltechDbContext>>();

            services.AddDbContext<TotaltechDbContext>(options =>
                options
                    .UseInMemoryDatabase(_nombreBase, _raizBase)
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        });
    }

    public void VerificarPersistenciaAislada()
    {
        using var scope = Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<TotaltechDbContext>();

        Assert.Equal(ProveedorInMemory, contexto.Database.ProviderName);
        Assert.DoesNotContain("SqlServer", contexto.Database.ProviderName ?? string.Empty);
    }
}
