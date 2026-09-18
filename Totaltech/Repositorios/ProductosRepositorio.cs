using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.Logica.DTOs;

namespace Totaltech.Repositorios
{
    public interface IProductosRepositorio
    {
        Task<List<Producto>> ObtenerTodosAsync();
        Task<Producto?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(Producto producto);
        Task ActualizarAsync(Producto producto);
        Task EliminarAsync(Producto producto);
        Task<List<Producto>> BuscarAsync(string? texto);
        Task<List<Producto>> ObtenerPorCategoriaAsync(int idCategoria);
        Task<List<Producto>> ObtenerDisponiblesAsync();
        Task<bool> DescontarStockAsync(int idProducto, int cantidad);
        Task<CatalogoProductosResponse> ObtenerCatalogoAsync(
            FiltroCatalogoProductos filtro,
            CancellationToken cancellationToken = default);
    }

    public class ProductosRepositorio : IProductosRepositorio
    {
        private readonly TotaltechDbContext _context;

        public ProductosRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<Producto>> ObtenerTodosAsync()
        {
            return await _context.Productos.ToListAsync();
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Productos.FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Productos.AnyAsync(producto => producto.IdProducto == id);
        }

        public async Task CrearAsync(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Producto producto)
        {
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Producto producto)
        {
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Producto>> BuscarAsync(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return await _context.Productos.ToListAsync();
            }

            return await _context.Productos
                .Where(producto => producto.Nombre.Contains(texto) || producto.Descripcion.Contains(texto))
                .ToListAsync();
        }

        public async Task<List<Producto>> ObtenerPorCategoriaAsync(int idCategoria)
        {
            return await _context.Productos
                .Where(producto => producto.IdCategoria == idCategoria)
                .ToListAsync();
        }

        public async Task<List<Producto>> ObtenerDisponiblesAsync()
        {
            return await _context.Productos
                .Where(producto => producto.Stock > 0)
                .ToListAsync();
        }

        public async Task<CatalogoProductosResponse> ObtenerCatalogoAsync(
            FiltroCatalogoProductos filtro,
            CancellationToken cancellationToken = default)
        {
            var consulta = _context.Productos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Texto))
            {
                consulta = consulta.Where(producto =>
                    producto.Nombre.Contains(filtro.Texto) ||
                    producto.Descripcion.Contains(filtro.Texto));
            }

            if (filtro.IdCategoria.HasValue)
                consulta = consulta.Where(producto => producto.IdCategoria == filtro.IdCategoria.Value);
            if (filtro.PrecioMin.HasValue)
                consulta = consulta.Where(producto => producto.Precio >= filtro.PrecioMin.Value);
            if (filtro.PrecioMax.HasValue)
                consulta = consulta.Where(producto => producto.Precio <= filtro.PrecioMax.Value);
            if (filtro.SoloDisponibles)
                consulta = consulta.Where(producto => producto.Stock > 0);

            var totalItems = await consulta.CountAsync(cancellationToken);
            var items = await consulta
                .OrderBy(producto => producto.Nombre)
                .ThenBy(producto => producto.IdProducto)
                .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
                .Take(filtro.TamanoPagina)
                .Select(producto => new ProductoCatalogoResponse
                {
                    IdProducto = producto.IdProducto,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Stock = producto.Stock,
                    IdCategoria = producto.IdCategoria,
                    CategoriaNombre = producto.Categoria!.Nombre,
                    IdProveedor = producto.IdProveedor,
                    ProveedorNombre = producto.Proveedor!.RazonSocial
                })
                .ToListAsync(cancellationToken);

            return new CatalogoProductosResponse
            {
                Items = items,
                Pagina = filtro.Pagina,
                TamanoPagina = filtro.TamanoPagina,
                TotalItems = totalItems,
                TotalPaginas = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)filtro.TamanoPagina)
            };
        }

        public async Task<bool> DescontarStockAsync(int idProducto, int cantidad)
        {
            if (cantidad <= 0)
            {
                return false;
            }

            if (_context.Database.IsRelational())
            {
                var filasAfectadas = await _context.Productos
                    .Where(producto => producto.IdProducto == idProducto && producto.Stock >= cantidad)
                    .ExecuteUpdateAsync(actualizacion => actualizacion
                        .SetProperty(producto => producto.Stock, producto => producto.Stock - cantidad));

                return filasAfectadas == 1;
            }

            var producto = await _context.Productos.FindAsync(idProducto);
            if (producto is null || producto.Stock < cantidad)
            {
                return false;
            }

            producto.Stock -= cantidad;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
