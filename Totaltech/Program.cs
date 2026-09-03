using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Totaltech.Datos;
using Totaltech.Endpoints;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Repositorios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    try
    {
        using var scope = app.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IUsuariosRepositorio>();
        var logica = scope.ServiceProvider.GetRequiredService<IUsuariosLogica>();
        var adminExistente = await repositorio.ObtenerPorEmailAsync("Admin@admin.com");
        if (adminExistente is null)
        {
            await logica.CrearAsync(new Usuario
            {
                Nombre = "Administrador",
                Apellido = "TotalTech",
                Email = "Admin@admin.com",
                Contrasena = "Admin123456789",
                Telefono = "1122334455",
                FechaRegistro = DateTime.UtcNow,
                Rol = RolUsuario.Administrador
            });
        }
        else if (adminExistente.Rol != RolUsuario.Administrador)
        {
            adminExistente.Rol = RolUsuario.Administrador;
            await repositorio.ActualizarAsync(adminExistente);
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "No se pudo verificar o inicializar la cuenta administrativa de desarrollo.");
    }
}

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
