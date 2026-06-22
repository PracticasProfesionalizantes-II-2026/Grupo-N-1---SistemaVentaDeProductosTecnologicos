using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Totaltech.Repositorios
{
    public interface IReportesRepositorio : IRepositorio<Reporte>
    {
        Task<ReporteVentasDto> ObtenerVentasAsync();
        Task<ReporteIngresosDto> ObtenerIngresosAsync();
        Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync();
    }

    public class ReportesRepositorio : Repositorio<Reporte>, IReportesRepositorio
    {
        public ReportesRepositorio(TotaltechDbContext context) : base(context)
        {
        }

        public async Task<ReporteVentasDto> ObtenerVentasAsync()
        {
            var estadosVenta = new[] { EstadoPedido.Pagado, EstadoPedido.Enviado, EstadoPedido.Entregado };
            var pedidosVendidos = Context.Pedidos.Where(pedido => estadosVenta.Contains(pedido.Estado));
            var pedidosIds = await pedidosVendidos.Select(pedido => pedido.IdPedido).ToListAsync();
            var cantidadPedidos = pedidosIds.Count;
            var totalVentas = await Context.DetallePedidos
                .Where(detalle => pedidosIds.Contains(detalle.IdPedido))
                .SumAsync(detalle => (decimal?)detalle.Subtotal) ?? 0;

            return new ReporteVentasDto(cantidadPedidos, totalVentas);
        }

        public async Task<ReporteIngresosDto> ObtenerIngresosAsync()
        {
            var pagosAprobados = Context.Pagos.Where(pago => pago.Estado == EstadoPago.Aprobado);
            var cantidadPagos = await pagosAprobados.CountAsync();
            var totalIngresos = await pagosAprobados.SumAsync(pago => (decimal?)pago.Monto) ?? 0;

            return new ReporteIngresosDto(cantidadPagos, totalIngresos);
        }

        public async Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync()
        {
            var estadosVenta = new[] { EstadoPedido.Pagado, EstadoPedido.Enviado, EstadoPedido.Entregado };
            var pedidosIds = await Context.Pedidos
                .Where(pedido => estadosVenta.Contains(pedido.Estado))
                .Select(pedido => pedido.IdPedido)
                .ToListAsync();

            // Solo estos estados cuentan como venta real para reportes comerciales.
            var ventas = await Context.DetallePedidos
                .Where(detalle => pedidosIds.Contains(detalle.IdPedido))
                .GroupBy(detalle => detalle.IdProducto)
                .Select(grupo => new
                {
                    IdProducto = grupo.Key,
                    CantidadVendida = grupo.Sum(detalle => detalle.Cantidad),
                    TotalVendido = grupo.Sum(detalle => detalle.Subtotal)
                })
                .OrderByDescending(producto => producto.CantidadVendida)
                .Take(10)
                .ToListAsync();

            var productosIds = ventas.Select(venta => venta.IdProducto).ToList();
            var productos = await Context.Productos
                .Where(producto => productosIds.Contains(producto.IdProducto))
                .ToDictionaryAsync(producto => producto.IdProducto, producto => producto.Nombre);

            return ventas
                .Select(venta => new ProductoMasVendidoDto(
                    venta.IdProducto,
                    productos.TryGetValue(venta.IdProducto, out var nombre) ? nombre : string.Empty,
                    venta.CantidadVendida,
                    venta.TotalVendido))
                .ToList();
        }
    }
}
