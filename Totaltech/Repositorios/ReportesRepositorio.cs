using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.Logica.DTOs;

namespace Totaltech.Repositorios
{
    public interface IReportesRepositorio
    {
        Task<List<Reporte>> ObtenerTodosAsync();
        Task<Reporte?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(Reporte reporte);
        Task ActualizarAsync(Reporte reporte);
        Task EliminarAsync(Reporte reporte);
        Task<ReporteVentasDto> ObtenerVentasAsync();
        Task<ReporteIngresosDto> ObtenerIngresosAsync();
        Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync();
    }

    public class ReportesRepositorio : IReportesRepositorio
    {
        private readonly TotaltechDbContext _context;

        public ReportesRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<Reporte>> ObtenerTodosAsync()
        {
            return await _context.Reportes.ToListAsync();
        }

        public async Task<Reporte?> ObtenerPorIdAsync(int id)
        {
            return await _context.Reportes.FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Reportes.AnyAsync(reporte => reporte.IdReporte == id);
        }

        public async Task CrearAsync(Reporte reporte)
        {
            _context.Reportes.Add(reporte);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Reporte reporte)
        {
            _context.Reportes.Update(reporte);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Reporte reporte)
        {
            _context.Reportes.Remove(reporte);
            await _context.SaveChangesAsync();
        }

        public async Task<ReporteVentasDto> ObtenerVentasAsync()
        {
            var estadosVenta = new[] { EstadoPedido.Pagado, EstadoPedido.Enviado, EstadoPedido.Entregado };
            var pedidosIds = await _context.Pedidos
                .Where(pedido => estadosVenta.Contains(pedido.Estado))
                .Select(pedido => pedido.IdPedido)
                .ToListAsync();

            var totalVentas = await _context.DetallePedidos
                .Where(detalle => pedidosIds.Contains(detalle.IdPedido))
                .SumAsync(detalle => (decimal?)detalle.Subtotal) ?? 0;

            return new ReporteVentasDto
            {
                CantidadPedidos = pedidosIds.Count,
                TotalVentas = totalVentas
            };
        }

        public async Task<ReporteIngresosDto> ObtenerIngresosAsync()
        {
            var pagosAprobados = _context.Pagos.Where(pago => pago.Estado == EstadoPago.Aprobado);

            return new ReporteIngresosDto
            {
                CantidadPagosAprobados = await pagosAprobados.CountAsync(),
                TotalIngresos = await pagosAprobados.SumAsync(pago => (decimal?)pago.Monto) ?? 0
            };
        }

        public async Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync()
        {
            var estadosVenta = new[] { EstadoPedido.Pagado, EstadoPedido.Enviado, EstadoPedido.Entregado };
            var pedidosIds = await _context.Pedidos
                .Where(pedido => estadosVenta.Contains(pedido.Estado))
                .Select(pedido => pedido.IdPedido)
                .ToListAsync();

            var ventas = await _context.DetallePedidos
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
            var productos = await _context.Productos
                .Where(producto => productosIds.Contains(producto.IdProducto))
                .ToDictionaryAsync(producto => producto.IdProducto, producto => producto.Nombre);

            return ventas
                .Select(venta => new ProductoMasVendidoDto
                {
                    IdProducto = venta.IdProducto,
                    Nombre = productos.TryGetValue(venta.IdProducto, out var nombre) ? nombre : string.Empty,
                    CantidadVendida = venta.CantidadVendida,
                    TotalVendido = venta.TotalVendido
                })
                .ToList();
        }
    }
}
