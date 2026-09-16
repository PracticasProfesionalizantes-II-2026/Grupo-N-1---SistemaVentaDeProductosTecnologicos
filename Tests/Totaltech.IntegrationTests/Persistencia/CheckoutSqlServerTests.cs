using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.IntegrationTests.Infrastructure;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.IntegrationTests.Persistencia;

public sealed class CheckoutSqlServerTests : IClassFixture<SqlServerTestDatabase>
{
    private readonly SqlServerTestDatabase _database;

    public CheckoutSqlServerTests(SqlServerTestDatabase database)
    {
        _database = database;
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public async Task RespuestaPerdidaRepiteElMismoPedidoYSnapshot()
    {
        var datos = await CrearEscenarioAsync(stock: 5, cantidad: 2, precio: 50m);

        await using (var context = _database.CreateContext())
        {
            var resultado = await CrearCarritosLogica(context).ConfirmarAsync(
                datos.IdCarrito,
                new ConfirmarCarritoDto { IdDireccion = datos.IdDireccion });

            Assert.Equal(EstadoConfirmacionCarrito.Creado, resultado.Estado);
            Assert.Equal(100m, resultado.Pedido!.Total);
            Assert.Equal("Calle original", resultado.Pedido.DireccionCalle);
        }

        await using (var context = _database.CreateContext())
        {
            var direccion = await context.Direcciones.FindAsync(datos.IdDireccion);
            var producto = await context.Productos.FindAsync(datos.IdProducto);
            direccion!.Calle = "Calle modificada";
            producto!.Precio = 999m;
            await context.SaveChangesAsync();
        }

        await using (var context = _database.CreateContext())
        {
            var resultado = await CrearCarritosLogica(context).ConfirmarAsync(
                datos.IdCarrito,
                new ConfirmarCarritoDto { IdDireccion = datos.IdDireccion });

            Assert.Equal(EstadoConfirmacionCarrito.Repetido, resultado.Estado);
            Assert.Equal(100m, resultado.Pedido!.Total);
            Assert.Equal("Calle original", resultado.Pedido.DireccionCalle);
            Assert.Single(await context.Pedidos.Where(pedido => pedido.IdCarrito == datos.IdCarrito).ToListAsync());
            Assert.Equal(3, (await context.Productos.FindAsync(datos.IdProducto))!.Stock);
        }
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public async Task ReintentoConOtraDireccionDevuelveConflicto()
    {
        var datos = await CrearEscenarioAsync();
        int otraDireccion;

        await using (var context = _database.CreateContext())
        {
            var direccion = CrearDireccion(datos.IdUsuario, "Otra calle");
            context.Direcciones.Add(direccion);
            await context.SaveChangesAsync();
            otraDireccion = direccion.IdDireccion;
        }

        await using (var context = _database.CreateContext())
        {
            Assert.Equal(
                EstadoConfirmacionCarrito.Creado,
                (await CrearCarritosLogica(context).ConfirmarAsync(
                    datos.IdCarrito,
                    new ConfirmarCarritoDto { IdDireccion = datos.IdDireccion })).Estado);
        }

        await using (var context = _database.CreateContext())
        {
            Assert.Equal(
                EstadoConfirmacionCarrito.Conflicto,
                (await CrearCarritosLogica(context).ConfirmarAsync(
                    datos.IdCarrito,
                    new ConfirmarCarritoDto { IdDireccion = otraDireccion })).Estado);
        }
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public async Task DosConfirmacionesConcurrentesCreanUnSoloPedido()
    {
        var datos = await CrearEscenarioAsync(stock: 4, cantidad: 1);
        await using var context1 = _database.CreateContext();
        await using var context2 = _database.CreateContext();

        var resultados = await Task.WhenAll(
            CrearCarritosLogica(context1).ConfirmarAsync(
                datos.IdCarrito,
                new ConfirmarCarritoDto { IdDireccion = datos.IdDireccion }),
            CrearCarritosLogica(context2).ConfirmarAsync(
                datos.IdCarrito,
                new ConfirmarCarritoDto { IdDireccion = datos.IdDireccion }));

        Assert.Contains(resultados, resultado => resultado.Estado == EstadoConfirmacionCarrito.Creado);
        Assert.All(resultados, resultado => Assert.Contains(
            resultado.Estado,
            new[] { EstadoConfirmacionCarrito.Creado, EstadoConfirmacionCarrito.Repetido }));

        await using var verificacion = _database.CreateContext();
        Assert.Equal(1, await verificacion.Pedidos.CountAsync(pedido => pedido.IdCarrito == datos.IdCarrito));
        Assert.Equal(3, (await verificacion.Productos.FindAsync(datos.IdProducto))!.Stock);
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public async Task UltimaUnidadDeStockSoloPermiteUnCheckout()
    {
        var datos = await CrearEscenarioAsync(stock: 1, cantidad: 1);
        int segundoCarrito;

        await using (var context = _database.CreateContext())
        {
            var carrito = new Carrito
            {
                IdUsuario = datos.IdUsuario,
                FechaCreacion = DateTime.UtcNow,
                Estado = EstadoCarrito.Activo
            };
            context.Carritos.Add(carrito);
            await context.SaveChangesAsync();
            context.DetalleCarritos.Add(new DetalleCarrito
            {
                IdCarrito = carrito.IdCarrito,
                IdProducto = datos.IdProducto,
                Cantidad = 1,
                PrecioUnitario = 50m,
                Subtotal = 50m
            });
            await context.SaveChangesAsync();
            segundoCarrito = carrito.IdCarrito;
        }

        await using var context1 = _database.CreateContext();
        await using var context2 = _database.CreateContext();
        var resultados = await Task.WhenAll(
            CrearCarritosLogica(context1).ConfirmarAsync(
                datos.IdCarrito,
                new ConfirmarCarritoDto { IdDireccion = datos.IdDireccion }),
            CrearCarritosLogica(context2).ConfirmarAsync(
                segundoCarrito,
                new ConfirmarCarritoDto { IdDireccion = datos.IdDireccion }));

        Assert.Single(resultados, resultado => resultado.Estado == EstadoConfirmacionCarrito.Creado);
        Assert.Single(resultados, resultado => resultado.Estado == EstadoConfirmacionCarrito.Conflicto);

        await using var verificacion = _database.CreateContext();
        Assert.Equal(0, (await verificacion.Productos.FindAsync(datos.IdProducto))!.Stock);
        Assert.Equal(1, await verificacion.Pedidos.CountAsync(pedido =>
            pedido.IdCarrito == datos.IdCarrito || pedido.IdCarrito == segundoCarrito));
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public async Task FallaIntermediaRevierteStockPedidoYCarrito()
    {
        var datos = await CrearEscenarioAsync(stock: 5, cantidad: 1);
        int segundoProducto;

        await using (var context = _database.CreateContext())
        {
            var original = await context.Productos.FindAsync(datos.IdProducto);
            var producto = new Producto
            {
                Nombre = $"Segundo-{Guid.NewGuid():N}",
                Descripcion = "Prueba",
                Precio = 30m,
                Stock = 5,
                IdCategoria = original!.IdCategoria,
                IdProveedor = original.IdProveedor
            };
            context.Productos.Add(producto);
            await context.SaveChangesAsync();
            context.DetalleCarritos.Add(new DetalleCarrito
            {
                IdCarrito = datos.IdCarrito,
                IdProducto = producto.IdProducto,
                Cantidad = 1,
                PrecioUnitario = producto.Precio,
                Subtotal = producto.Precio
            });
            await context.SaveChangesAsync();
            segundoProducto = producto.IdProducto;
        }

        await using (var context = _database.CreateContext())
        {
            var repositorio = new FallarSegundoDescuentoRepositorio(new ProductosRepositorio(context));
            var resultado = await CrearCarritosLogica(context, repositorio).ConfirmarAsync(
                datos.IdCarrito,
                new ConfirmarCarritoDto { IdDireccion = datos.IdDireccion });
            Assert.Equal(EstadoConfirmacionCarrito.Conflicto, resultado.Estado);
        }

        await using var verificacion = _database.CreateContext();
        Assert.Equal(5, (await verificacion.Productos.FindAsync(datos.IdProducto))!.Stock);
        Assert.Equal(5, (await verificacion.Productos.FindAsync(segundoProducto))!.Stock);
        Assert.False(await verificacion.Pedidos.AnyAsync(pedido => pedido.IdCarrito == datos.IdCarrito));
        Assert.Equal(EstadoCarrito.Activo, (await verificacion.Carritos.FindAsync(datos.IdCarrito))!.Estado);
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public async Task CancelacionReintegraStockUnaSolaVez()
    {
        var datos = await CrearEscenarioAsync(stock: 5, cantidad: 2);
        int idPedido;

        await using (var context = _database.CreateContext())
        {
            var confirmado = await CrearCarritosLogica(context).ConfirmarAsync(
                datos.IdCarrito,
                new ConfirmarCarritoDto { IdDireccion = datos.IdDireccion });
            idPedido = confirmado.Pedido!.IdPedido;
        }

        await using (var context = _database.CreateContext())
        {
            var logica = CrearPedidosLogica(context);
            Assert.Equal(
                EstadoOperacionDominio.Exitoso,
                (await logica.ActualizarEstadoAsync(idPedido, EstadoPedido.Cancelado)).Estado);
            Assert.Equal(
                EstadoOperacionDominio.Exitoso,
                (await logica.ActualizarEstadoAsync(idPedido, EstadoPedido.Cancelado)).Estado);
        }

        await using var verificacion = _database.CreateContext();
        Assert.Equal(5, (await verificacion.Productos.FindAsync(datos.IdProducto))!.Stock);
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public async Task ModificarLineasYConfirmarNuncaDejaEscriturasFueraDelPedido()
    {
        var datos = await CrearEscenarioAsync(stock: 5, cantidad: 1);
        int segundoProducto;

        await using (var context = _database.CreateContext())
        {
            var original = await context.Productos.FindAsync(datos.IdProducto);
            var producto = new Producto
            {
                Nombre = $"Concurrente-{Guid.NewGuid():N}",
                Descripcion = "Prueba",
                Precio = 20m,
                Stock = 5,
                IdCategoria = original!.IdCategoria,
                IdProveedor = original.IdProveedor
            };
            context.Productos.Add(producto);
            await context.SaveChangesAsync();
            segundoProducto = producto.IdProducto;
        }

        await using var checkoutContext = _database.CreateContext();
        await using var lineasContext = _database.CreateContext();
        var confirmar = CrearCarritosLogica(checkoutContext).ConfirmarAsync(
            datos.IdCarrito,
            new ConfirmarCarritoDto { IdDireccion = datos.IdDireccion });
        var agregar = CrearCarritosLogica(lineasContext).AgregarProductoAsync(
            datos.IdCarrito,
            new AgregarProductoCarritoDto { IdProducto = segundoProducto, Cantidad = 1 });

        await Task.WhenAll(confirmar, agregar);
        var resultadoConfirmacion = await confirmar;
        var resultadoLineas = await agregar;
        Assert.Equal(EstadoConfirmacionCarrito.Creado, resultadoConfirmacion.Estado);

        await using var verificacion = _database.CreateContext();
        var pedido = await verificacion.Pedidos.SingleAsync(
            candidato => candidato.IdCarrito == datos.IdCarrito);
        var existeEnCarrito = await verificacion.DetalleCarritos.AnyAsync(
            detalle => detalle.IdCarrito == datos.IdCarrito && detalle.IdProducto == segundoProducto);
        var existeEnPedido = await verificacion.DetallePedidos.AnyAsync(
            detalle => detalle.IdPedido == pedido.IdPedido && detalle.IdProducto == segundoProducto);

        if (resultadoLineas.Error is null)
        {
            Assert.True(existeEnCarrito);
            Assert.True(existeEnPedido);
        }
        else
        {
            Assert.False(existeEnCarrito);
            Assert.False(existeEnPedido);
        }
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public async Task PagosParcialExactoYSobrepagoRespetanElTotal()
    {
        var datos = await CrearEscenarioAsync(stock: 10, cantidad: 2, precio: 50m);
        int idPedido;

        await using (var context = _database.CreateContext())
        {
            idPedido = (await CrearCarritosLogica(context).ConfirmarAsync(
                datos.IdCarrito,
                new ConfirmarCarritoDto { IdDireccion = datos.IdDireccion })).Pedido!.IdPedido;
        }

        await using (var context = _database.CreateContext())
        {
            var logica = CrearPagosLogica(context);
            var parcial = new Pago { IdPedido = idPedido, MetodoPago = MetodoPago.Efectivo, Monto = 40m };
            Assert.Null(await logica.CrearParaPedidoAsync(idPedido, parcial));
            Assert.Equal(
                EstadoOperacionDominio.Exitoso,
                (await logica.ActualizarEstadoAsync(parcial.IdPago, EstadoPago.Aprobado)).Estado);
            Assert.Equal(EstadoPedido.Pendiente, (await context.Pedidos.FindAsync(idPedido))!.Estado);

            var sobrepago = new Pago { IdPedido = idPedido, MetodoPago = MetodoPago.Efectivo, Monto = 70m };
            Assert.Null(await logica.CrearParaPedidoAsync(idPedido, sobrepago));
            Assert.Equal(
                EstadoOperacionDominio.Conflicto,
                (await logica.ActualizarEstadoAsync(sobrepago.IdPago, EstadoPago.Aprobado)).Estado);
            Assert.Equal(EstadoPago.Pendiente, (await context.Pagos.FindAsync(sobrepago.IdPago))!.Estado);

            var saldo = new Pago { IdPedido = idPedido, MetodoPago = MetodoPago.Efectivo, Monto = 60m };
            Assert.Null(await logica.CrearParaPedidoAsync(idPedido, saldo));
            Assert.Equal(
                EstadoOperacionDominio.Exitoso,
                (await logica.ActualizarEstadoAsync(saldo.IdPago, EstadoPago.Aprobado)).Estado);
            Assert.Equal(EstadoPedido.Pagado, (await context.Pedidos.FindAsync(idPedido))!.Estado);
        }
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public void SqlServerUsaEstrategiaConReintentos()
    {
        using var context = _database.CreateContext();
        Assert.True(context.Database.CreateExecutionStrategy().RetriesOnFailure);
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public async Task MigracionCompletaPersisteTotalYSnapshotDePedido()
    {
        var nombreBase = $"{SqlServerTestDatabase.DatabasePrefix}{Guid.NewGuid():N}";
        var cadena = new SqlConnectionStringBuilder
        {
            DataSource = SqlServerTestDatabase.LocalDbDataSource,
            InitialCatalog = nombreBase,
            IntegratedSecurity = true,
            TrustServerCertificate = true
        }.ConnectionString;
        SqlServerTestDatabase.ValidateConnectionString(cadena);

        var opciones = new DbContextOptionsBuilder<TotaltechDbContext>()
            .UseSqlServer(cadena, sql => sql.EnableRetryOnFailure())
            .Options;

        try
        {
            const int idPedidoHistorico = 700001;
            await using (var context = new TotaltechDbContext(opciones))
            {
                await context.Database.MigrateAsync();

                var sufijo = Guid.NewGuid().ToString("N");
                var usuario = new Usuario
                {
                    Nombre = "Historico",
                    Apellido = "Migracion",
                    Email = $"historico-{sufijo}@test.local",
                    Contrasena = "hash-prueba",
                    Telefono = "0000000000",
                    FechaRegistro = DateTime.UtcNow
                };
                var categoria = new Categoria { Nombre = $"Historica-{sufijo}", Descripcion = "Prueba" };
                var proveedor = new Proveedor
                {
                    RazonSocial = $"Historico-{sufijo}",
                    Cuit = sufijo[..11],
                    EmailComercial = $"historico-proveedor-{sufijo}@test.local",
                    TelefonoComercial = "0000000000",
                    CondicionIva = "Prueba",
                    MonedaPreferida = "ARS"
                };
                context.AddRange(usuario, categoria, proveedor);
                await context.SaveChangesAsync();

                var direccion = CrearDireccion(usuario.IdUsuario, "Calle historica");
                var producto = new Producto
                {
                    Nombre = $"Historico-{sufijo}",
                    Descripcion = "Prueba",
                    Precio = 25m,
                    Stock = 10,
                    IdCategoria = categoria.IdCategoria,
                    IdProveedor = proveedor.IdProveedor
                };
                context.AddRange(direccion, producto);
                await context.SaveChangesAsync();

                var fecha = DateTime.UtcNow;
                await context.Database.ExecuteSqlInterpolatedAsync($$"""
                    SET IDENTITY_INSERT Pedidos ON;
                    INSERT INTO Pedidos (
                        IdPedido, IdUsuario, IdCarrito, FechaPedido, Estado, IdDireccion, Total,
                        DireccionCalle, DireccionNumero, DireccionCiudad, DireccionProvincia,
                        DireccionCodigoPostal, DireccionPais)
                    VALUES (
                        {{idPedidoHistorico}}, {{usuario.IdUsuario}}, NULL, {{fecha}},
                        {{(int)EstadoPedido.Pendiente}}, {{direccion.IdDireccion}}, 50,
                        N'Calle historica', N'123', N'Ciudad', N'Provincia', N'1000', N'Argentina');
                    SET IDENTITY_INSERT Pedidos OFF;

                    INSERT INTO DetallePedidos (IdPedido, IdProducto, Cantidad, PrecioUnitario, Subtotal)
                    VALUES ({{idPedidoHistorico}}, {{producto.IdProducto}}, 2, 25, 50);
                    """);
            }

            await using (var context = new TotaltechDbContext(opciones))
            {
                await context.Database.MigrateAsync();
                var pedido = await context.Pedidos.SingleAsync(
                    candidato => candidato.IdPedido == idPedidoHistorico);

                Assert.Null(pedido.IdCarrito);
                Assert.Equal(50m, pedido.Total);
                Assert.Equal("Calle historica", pedido.DireccionCalle);
                Assert.Equal("Argentina", pedido.DireccionPais);
            }
        }
        finally
        {
            await using var context = new TotaltechDbContext(opciones);
            await context.Database.EnsureDeletedAsync();
        }
    }

    private async Task<DatosCheckout> CrearEscenarioAsync(
        int stock = 5,
        int cantidad = 1,
        decimal precio = 50m)
    {
        await using var context = _database.CreateContext();
        var sufijo = Guid.NewGuid().ToString("N");
        var usuario = new Usuario
        {
            Nombre = "Usuario",
            Apellido = "Checkout",
            Email = $"checkout-{sufijo}@test.local",
            Contrasena = "hash-prueba",
            Telefono = "0000000000",
            FechaRegistro = DateTime.UtcNow
        };
        var categoria = new Categoria { Nombre = $"Categoria-{sufijo}", Descripcion = "Prueba" };
        var proveedor = new Proveedor
        {
            RazonSocial = $"Proveedor-{sufijo}",
            Cuit = sufijo[..11],
            EmailComercial = $"proveedor-{sufijo}@test.local",
            TelefonoComercial = "0000000000",
            CondicionIva = "Prueba",
            MonedaPreferida = "ARS"
        };
        context.AddRange(usuario, categoria, proveedor);
        await context.SaveChangesAsync();

        var direccion = CrearDireccion(usuario.IdUsuario, "Calle original");
        var producto = new Producto
        {
            Nombre = $"Producto-{sufijo}",
            Descripcion = "Prueba",
            Precio = precio,
            Stock = stock,
            IdCategoria = categoria.IdCategoria,
            IdProveedor = proveedor.IdProveedor
        };
        var carrito = new Carrito
        {
            IdUsuario = usuario.IdUsuario,
            FechaCreacion = DateTime.UtcNow,
            Estado = EstadoCarrito.Activo
        };
        context.AddRange(direccion, producto, carrito);
        await context.SaveChangesAsync();

        context.DetalleCarritos.Add(new DetalleCarrito
        {
            IdCarrito = carrito.IdCarrito,
            IdProducto = producto.IdProducto,
            Cantidad = cantidad,
            PrecioUnitario = precio,
            Subtotal = precio * cantidad
        });
        await context.SaveChangesAsync();

        return new(usuario.IdUsuario, direccion.IdDireccion, producto.IdProducto, carrito.IdCarrito);
    }

    private static Direccion CrearDireccion(int idUsuario, string calle) => new()
    {
        IdUsuario = idUsuario,
        Calle = calle,
        Numero = "123",
        Ciudad = "Buenos Aires",
        Provincia = "Buenos Aires",
        CodigoPostal = "1000",
        Pais = "Argentina"
    };

    private static CarritosLogica CrearCarritosLogica(
        TotaltechDbContext context,
        IProductosRepositorio? productosRepositorio = null) => new(
        context,
        new CarritosRepositorio(context),
        productosRepositorio ?? new ProductosRepositorio(context),
        new UsuariosRepositorio(context));

    private static PedidosLogica CrearPedidosLogica(TotaltechDbContext context) => new(
        context,
        new PedidosRepositorio(context));

    private static PagosLogica CrearPagosLogica(TotaltechDbContext context) => new(
        context,
        new PagosRepositorio(context));

    private sealed record DatosCheckout(
        int IdUsuario,
        int IdDireccion,
        int IdProducto,
        int IdCarrito);

    private sealed class FallarSegundoDescuentoRepositorio : IProductosRepositorio
    {
        private readonly IProductosRepositorio _inner;
        private int _descuentos;

        public FallarSegundoDescuentoRepositorio(IProductosRepositorio inner)
        {
            _inner = inner;
        }

        public Task<List<Producto>> ObtenerTodosAsync() => _inner.ObtenerTodosAsync();
        public Task<Producto?> ObtenerPorIdAsync(int id) => _inner.ObtenerPorIdAsync(id);
        public Task<bool> ExisteAsync(int id) => _inner.ExisteAsync(id);
        public Task CrearAsync(Producto producto) => _inner.CrearAsync(producto);
        public Task ActualizarAsync(Producto producto) => _inner.ActualizarAsync(producto);
        public Task EliminarAsync(Producto producto) => _inner.EliminarAsync(producto);
        public Task<List<Producto>> BuscarAsync(string? texto) => _inner.BuscarAsync(texto);
        public Task<List<Producto>> ObtenerPorCategoriaAsync(int idCategoria) =>
            _inner.ObtenerPorCategoriaAsync(idCategoria);
        public Task<List<Producto>> ObtenerDisponiblesAsync() => _inner.ObtenerDisponiblesAsync();

        public Task<bool> DescontarStockAsync(int idProducto, int cantidad)
        {
            _descuentos++;
            return _descuentos == 2
                ? Task.FromResult(false)
                : _inner.DescontarStockAsync(idProducto, cantidad);
        }
    }
}
