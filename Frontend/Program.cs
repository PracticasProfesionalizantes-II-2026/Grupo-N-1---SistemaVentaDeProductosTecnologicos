using Frontend.Services;
using Frontend.Services.Interfaces;
using Frontend.Observabilidad;
using Microsoft.AspNetCore.Authentication.Cookies;
using Prometheus;
using Totaltech.Observabilidad;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTotaltechObservability(builder.Configuration);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ApiBearerTokenHandler>();

builder.Services.AddHttpClient("TotaltechApi", client =>
{
    var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
        ?? throw new InvalidOperationException("Falta configurar ApiBaseUrl.");

    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
}).AddHttpMessageHandler(services => new ApiClientMetricsHandler(
    services.GetRequiredService<IMetricFactory>(), "TotaltechApi", builder.Configuration.GetValue<bool>("Observability:Enabled")))
  .AddHttpMessageHandler<ApiBearerTokenHandler>();

builder.Services.AddScoped<CategoriasApiService>();
builder.Services.AddScoped<ProductosApiService>();
builder.Services.AddScoped<ProveedoresApiService>();
builder.Services.AddScoped<AdministracionApiService>();
builder.Services.AddScoped<IUsuariosApiService, UsuariosApiService>();
builder.Services.AddScoped<ValidacionSesionEvents>();
builder.Services.AddHttpClient("TotaltechSessionApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]
        ?? throw new InvalidOperationException("Falta configurar ApiBaseUrl."));
    client.Timeout = TimeSpan.FromSeconds(10);
}).AddHttpMessageHandler(services => new ApiClientMetricsHandler(
    services.GetRequiredService<IMetricFactory>(), "TotaltechSessionApi", builder.Configuration.GetValue<bool>("Observability:Enabled")));
builder.Services.AddScoped<ICarritosApiService, CarritosApiService>();
builder.Services.AddSingleton<ProductoImagenResolver>();
builder.Services.AddSingleton<ProductoImagenStorage>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.EventsType = typeof(ValidacionSesionEvents);
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/Login";
        options.Cookie.Name = "Totaltech.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
    });

var app = builder.Build();

app.UseTotaltechMetricsEndpoint();
app.UseTotaltechHttpMetrics();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.CaptureTotaltechMetricRoute();

app.UseAuthentication();
app.Use(async (context, next) =>
{
    var protegido = context.GetEndpoint()?.Metadata.GetOrderedMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>().Count > 0;
    var anonimo = context.GetEndpoint()?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() is not null;
    if (context.Items.ContainsKey(ValidacionSesionEvents.ServicioNoDisponible) && protegido && !anonimo)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync("No pudimos verificar tu sesión. El servicio no está disponible. Volvé a intentarlo en unos minutos.");
        return;
    }
    await next();
});
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
