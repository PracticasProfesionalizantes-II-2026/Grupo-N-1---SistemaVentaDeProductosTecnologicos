using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Totaltech.Datos;
using Totaltech.Endpoints;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Repositorios;
using Totaltech.Seguridad;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.Seccion))
    .ValidateDataAnnotations()
    .Validate(options => Encoding.UTF8.GetByteCount(options.SigningKey) >= 32,
        "Authentication:SigningKey debe tener al menos 32 bytes.")
    .ValidateOnStart();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.Seccion).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Falta configurar la sección Authentication.");
if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
    string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
    Encoding.UTF8.GetByteCount(jwtOptions.SigningKey) < 32)
{
    throw new InvalidOperationException(
        "Authentication requiere Issuer, Audience y SigningKey de al menos 32 bytes. " +
        "Configure la clave mediante Authentication__SigningKey o User Secrets.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy(Autorizacion.PoliticaAdministrador,
        policy => policy.RequireRole(nameof(RolUsuario.Administrador)));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta configurar la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContext<TotaltechDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IUsuariosRepositorio, UsuariosRepositorio>();
builder.Services.AddScoped<IDireccionesRepositorio, DireccionesRepositorio>();
builder.Services.AddScoped<IProveedoresRepositorio, ProveedoresRepositorio>();
builder.Services.AddScoped<IProductosRepositorio, ProductosRepositorio>();
builder.Services.AddScoped<ICategoriasRepositorio, CategoriasRepositorio>();
builder.Services.AddScoped<IPedidosRepositorio, PedidosRepositorio>();
builder.Services.AddScoped<IDetallePedidosRepositorio, DetallePedidosRepositorio>();
builder.Services.AddScoped<ICarritosRepositorio, CarritosRepositorio>();
builder.Services.AddScoped<IDetalleCarritosRepositorio, DetalleCarritosRepositorio>();
builder.Services.AddScoped<IPagosRepositorio, PagosRepositorio>();
builder.Services.AddScoped<IComprasRepositorio, ComprasRepositorio>();
builder.Services.AddScoped<IReportesRepositorio, ReportesRepositorio>();
builder.Services.AddScoped<IConsultasRepositorio, ConsultasRepositorio>();

builder.Services.AddScoped<IUsuariosLogica, UsuariosLogica>();
builder.Services.AddScoped<IDireccionesLogica, DireccionesLogica>();
builder.Services.AddScoped<IProveedoresLogica, ProveedoresLogica>();
builder.Services.AddScoped<IProductosLogica, ProductosLogica>();
builder.Services.AddScoped<ICategoriasLogica, CategoriasLogica>();
builder.Services.AddScoped<IPedidosLogica, PedidosLogica>();
builder.Services.AddScoped<IDetallePedidosLogica, DetallePedidosLogica>();
builder.Services.AddScoped<ICarritosLogica, CarritosLogica>();
builder.Services.AddScoped<IDetalleCarritosLogica, DetalleCarritosLogica>();
builder.Services.AddScoped<IPagosLogica, PagosLogica>();
builder.Services.AddScoped<IComprasLogica, ComprasLogica>();
builder.Services.AddScoped<IReportesLogica, ReportesLogica>();
builder.Services.AddScoped<IConsultasLogica, ConsultasLogica>();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var categoriasLogica = scope.ServiceProvider.GetRequiredService<ICategoriasLogica>();
    var categoriasCreadas = await CategoriasIniciales.InicializarAsync(categoriasLogica);

    if (categoriasCreadas.Count > 0)
    {
        app.Logger.LogInformation(
            "Se inicializaron las categorías canónicas: {Categorias}.",
            string.Join(", ", categoriasCreadas));
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "No se pudieron verificar o inicializar las categorías canónicas.");
}

try
{
    using var scope = app.Services.CreateScope();
    var usuariosLogica = scope.ServiceProvider.GetRequiredService<IUsuariosLogica>();
    await usuariosLogica.AsegurarAdministradorAsync(
        "Admin@admin.com",
        "Admin123456789");
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "No se pudo verificar o inicializar la cuenta administrativa.");
    throw;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapUsuariosEndpoints();
app.MapAuthEndpoints();
app.MapDireccionesEndpoints();
app.MapProveedoresEndpoints();
app.MapProductosEndpoints();
app.MapCategoriasEndpoints();
app.MapPedidosEndpoints();
app.MapDetallePedidosEndpoints();
app.MapCarritosEndpoints();
app.MapDetalleCarritosEndpoints();
app.MapPagosEndpoints();
app.MapComprasEndpoints();
app.MapReportesEndpoints();
app.MapConsultasEndpoints();

app.Run();
