using System.Security.Claims;
using Frontend.Controllers;
using Frontend.Models.Api.Responses;
using Frontend.Models.ViewModels.Checkout;
using Frontend.Models.ViewModels.Pedidos;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Totaltech.IntegrationTests.Frontend;

public sealed class CompraDemoControllerTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public async Task FlagDesactivadoOAusenteBloqueaTodasLasPantallasSinConsultarApi(bool? enabled)
    {
        var api = new CarritoSimulado();
        var config = Config(enabled);
        var checkout = Contexto(new CheckoutController(api, config));
        var pedidos = Contexto(new PedidosController(config));
        Assert.IsType<NotFoundResult>(await checkout.Index(default));
        Assert.IsType<NotFoundResult>(checkout.Confirmacion("DEMO-prueba"));
        Assert.IsType<NotFoundResult>(pedidos.Index());
        Assert.IsType<NotFoundResult>(pedidos.Detalle("DEMO-prueba"));
        Assert.Equal(0, api.Lecturas);
    }

    [Fact]
    public async Task CheckoutUsaImportesDeApiSinMutaciones()
    {
        var api = new CarritoSimulado();
        var controller = Contexto(new CheckoutController(api, Config(true)));
        var view = Assert.IsType<ViewResult>(await controller.Index(default));
        var model = Assert.IsType<CheckoutViewModel>(view.Model);
        Assert.True(model.PuedeSimular);
        Assert.Equal(7, model.IdUsuario);
        Assert.Equal(123.45m, model.Carrito!.Total);
        Assert.Equal(42m, model.Carrito.Items[0].Subtotal);
        Assert.Equal(1, api.Lecturas);
    }

    [Theory]
    [InlineData("vacio")]
    [InlineData("null")]
    [InlineData("red")]
    [InlineData("timeout")]
    public async Task CarritoVacioOFalloNoPermiteSimular(string modo)
    {
        var api = new CarritoSimulado { Modo = modo };
        var view = Assert.IsType<ViewResult>(await Contexto(new CheckoutController(api, Config(true))).Index(default));
        var model = Assert.IsType<CheckoutViewModel>(view.Model);
        Assert.False(model.PuedeSimular);
        if (modo != "vacio") Assert.NotNull(model.Error);
    }

    [Fact]
    public async Task CancelacionSolicitadaSePropaga()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var controller = Contexto(new CheckoutController(new CarritoSimulado(), Config(true)));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => controller.Index(cancellation.Token));
    }

    [Fact]
    public async Task SesionSinIdentificadorSeRechazaYControllersRequierenAutenticacion()
    {
        var api = new CarritoSimulado();
        var checkout = Contexto(new CheckoutController(api, Config(true)), "invalido");
        var pedidos = Contexto(new PedidosController(Config(true)), "invalido");
        Assert.IsType<ChallengeResult>(await checkout.Index(default));
        Assert.IsType<ChallengeResult>(checkout.Confirmacion(null));
        Assert.IsType<ChallengeResult>(pedidos.Index());
        Assert.IsType<ChallengeResult>(pedidos.Detalle(null));
        Assert.Equal(0, api.Lecturas);
        Assert.NotNull(Attribute.GetCustomAttribute(typeof(CheckoutController), typeof(AuthorizeAttribute)));
        Assert.NotNull(Attribute.GetCustomAttribute(typeof(PedidosController), typeof(AuthorizeAttribute)));
    }

    [Fact]
    public void HistorialYDetalleUtilizanUsuarioDeSesion()
    {
        var pedidos = Contexto(new PedidosController(Config(true)));
        Assert.Equal(7, Assert.IsType<PedidoListadoViewModel>(Assert.IsType<ViewResult>(pedidos.Index()).Model).IdUsuario);
        var detalle = Assert.IsType<PedidoDetalleViewModel>(Assert.IsType<ViewResult>(pedidos.Detalle("DEMO-x")).Model);
        Assert.Equal(7, detalle.IdUsuario);
        Assert.Equal("DEMO-x", detalle.IdPedido);
        var checkout = Contexto(new CheckoutController(new CarritoSimulado(), Config(true)));
        Assert.Equal(7, Assert.IsType<ConfirmacionCompraViewModel>(Assert.IsType<ViewResult>(checkout.Confirmacion("DEMO-x")).Model).IdUsuario);
    }

    private static IConfiguration Config(bool? enabled) => new ConfigurationBuilder().AddInMemoryCollection(
        enabled.HasValue ? new Dictionary<string, string?> { ["CompraDemo:Enabled"] = enabled.Value.ToString() } : []).Build();

    private static T Contexto<T>(T controller, string id = "7") where T : Controller
    {
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
        { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id)], "Tests")) } };
        return controller;
    }

    private sealed class CarritoSimulado : ICarritosApiService
    {
        public string Modo { get; init; } = "normal";
        public int Lecturas { get; private set; }
        public Task<CarritoResumenResponse?> ObtenerActualAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Lecturas++;
            if (Modo == "red") throw new HttpRequestException();
            if (Modo == "timeout") throw new TaskCanceledException();
            return Task.FromResult<CarritoResumenResponse?>(Modo switch
            {
                "null" => null,
                "vacio" => new(),
                _ => new() { Total = 123.45m, CantidadTotal = 2, Items = [new() { IdProducto = 1, Nombre = "Producto", Cantidad = 2, PrecioUnitario = 21m, Subtotal = 42m }] }
            });
        }
        public Task<CarritoApiResultado> AgregarAsync(int idProducto, int cantidad, CancellationToken cancellationToken = default) => throw new InvalidOperationException("La demo no debe modificar el carrito.");
        public Task<CarritoApiResultado> ActualizarCantidadAsync(int idProducto, int cantidad, CancellationToken cancellationToken = default) => throw new InvalidOperationException("La demo no debe modificar el carrito.");
        public Task<CarritoApiResultado> EliminarAsync(int idProducto, CancellationToken cancellationToken = default) => throw new InvalidOperationException("La demo no debe modificar el carrito.");
    }
}
