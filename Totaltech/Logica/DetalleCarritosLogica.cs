using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IDetalleCarritosLogica : ILogica<DetalleCarrito>
    {
        Task<List<DetalleCarrito>> ObtenerPorCarritoAsync(int idCarrito);
    }

    public class DetalleCarritosLogica : Logica<DetalleCarrito>, IDetalleCarritosLogica
    {
        private readonly IDetalleCarritosRepositorio _repositorio;
        private readonly ICarritosRepositorio _carritosRepositorio;
        private readonly IProductosRepositorio _productosRepositorio;

        public DetalleCarritosLogica(IDetalleCarritosRepositorio repositorio, ICarritosRepositorio carritosRepositorio, IProductosRepositorio productosRepositorio) : base(repositorio)
        {
            _repositorio = repositorio;
            _carritosRepositorio = carritosRepositorio;
            _productosRepositorio = productosRepositorio;
        }

        public Task<List<DetalleCarrito>> ObtenerPorCarritoAsync(int idCarrito)
        {
            return _repositorio.ObtenerPorCarritoAsync(idCarrito);
        }

        public override async Task<ResultadoOperacion<DetalleCarrito>> CrearValidadoAsync(DetalleCarrito detalle)
        {
            var validacion = await ValidarDetalleAsync(detalle);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<DetalleCarrito>.BadRequest(validacion.Error ?? "El detalle de carrito no es valido.");
            }

            var existente = await _repositorio.ObtenerPorCarritoYProductoAsync(detalle.IdCarrito, detalle.IdProducto);
            if (existente is not null)
            {
                return ResultadoOperacion<DetalleCarrito>.Conflict("El producto ya existe en el carrito.");
            }

            detalle.Subtotal = detalle.PrecioUnitario * detalle.Cantidad;
            return await base.CrearValidadoAsync(detalle);
        }

        public override async Task<ResultadoOperacion<DetalleCarrito>> ActualizarValidadoAsync(int id, DetalleCarrito detalle)
        {
            var validacion = await ValidarDetalleAsync(detalle);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<DetalleCarrito>.BadRequest(validacion.Error ?? "El detalle de carrito no es valido.");
            }

            detalle.Subtotal = detalle.PrecioUnitario * detalle.Cantidad;
            return await base.ActualizarValidadoAsync(id, detalle);
        }

        private async Task<ResultadoOperacion> ValidarDetalleAsync(DetalleCarrito detalle)
        {
            if (detalle.Cantidad <= 0 || detalle.PrecioUnitario < 0)
            {
                return ResultadoOperacion.BadRequest("La cantidad debe ser mayor a cero y el precio no puede ser negativo.");
            }

            if (!await _carritosRepositorio.ExisteAsync(detalle.IdCarrito))
            {
                return ResultadoOperacion.BadRequest("El carrito indicado no existe.");
            }

            if (!await _productosRepositorio.ExisteAsync(detalle.IdProducto))
            {
                return ResultadoOperacion.BadRequest("El producto indicado no existe.");
            }

            return ResultadoOperacion.Ok();
        }
    }
}
