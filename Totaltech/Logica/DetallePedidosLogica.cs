using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IDetallePedidosLogica : ILogica<DetallePedido>
    {
    }

    public class DetallePedidosLogica : Logica<DetallePedido>, IDetallePedidosLogica
    {
        private readonly IPedidosRepositorio _pedidosRepositorio;
        private readonly IProductosRepositorio _productosRepositorio;

        public DetallePedidosLogica(IDetallePedidosRepositorio repositorio, IPedidosRepositorio pedidosRepositorio, IProductosRepositorio productosRepositorio) : base(repositorio)
        {
            _pedidosRepositorio = pedidosRepositorio;
            _productosRepositorio = productosRepositorio;
        }

        public override async Task<ResultadoOperacion<DetallePedido>> CrearValidadoAsync(DetallePedido detalle)
        {
            var validacion = await ValidarDetalleAsync(detalle);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<DetallePedido>.BadRequest(validacion.Error ?? "El detalle de pedido no es valido.");
            }

            detalle.Subtotal = detalle.PrecioUnitario * detalle.Cantidad;
            return await base.CrearValidadoAsync(detalle);
        }

        public override async Task<ResultadoOperacion<DetallePedido>> ActualizarValidadoAsync(int id, DetallePedido detalle)
        {
            var validacion = await ValidarDetalleAsync(detalle);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<DetallePedido>.BadRequest(validacion.Error ?? "El detalle de pedido no es valido.");
            }

            detalle.Subtotal = detalle.PrecioUnitario * detalle.Cantidad;
            return await base.ActualizarValidadoAsync(id, detalle);
        }

        private async Task<ResultadoOperacion> ValidarDetalleAsync(DetallePedido detalle)
        {
            if (detalle.Cantidad <= 0 || detalle.PrecioUnitario < 0)
            {
                return ResultadoOperacion.BadRequest("La cantidad debe ser mayor a cero y el precio no puede ser negativo.");
            }

            if (!await _pedidosRepositorio.ExisteAsync(detalle.IdPedido))
            {
                return ResultadoOperacion.BadRequest("El pedido indicado no existe.");
            }

            if (!await _productosRepositorio.ExisteAsync(detalle.IdProducto))
            {
                return ResultadoOperacion.BadRequest("El producto indicado no existe.");
            }

            return ResultadoOperacion.Ok();
        }
    }
}
