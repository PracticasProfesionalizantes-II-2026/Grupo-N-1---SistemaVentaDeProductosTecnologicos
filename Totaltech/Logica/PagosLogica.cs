using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IPagosLogica : ILogica<Pago>
    {
        Task<ResultadoOperacion<Pago>> ActualizarEstadoAsync(int id, EstadoPago estado);
        Task<List<Pago>> ObtenerPorPedidoAsync(int idPedido);
        Task<ResultadoOperacion<Pago>> CrearParaPedidoAsync(int idPedido, Pago pago);
    }

    public class PagosLogica : Logica<Pago>, IPagosLogica
    {
        private readonly IPagosRepositorio _repositorio;
        private readonly IPedidosRepositorio _pedidosRepositorio;

        public PagosLogica(IPagosRepositorio repositorio, IPedidosRepositorio pedidosRepositorio) : base(repositorio)
        {
            _repositorio = repositorio;
            _pedidosRepositorio = pedidosRepositorio;
        }

        public async Task<ResultadoOperacion<Pago>> ActualizarEstadoAsync(int id, EstadoPago estado)
        {
            var pago = await _repositorio.ObtenerPorIdAsync(id);

            if (pago is null)
            {
                return ResultadoOperacion<Pago>.NotFound("El pago indicado no existe.");
            }

            pago.Estado = estado;
            await _repositorio.ActualizarAsync(pago);

            // Un pago aprobado confirma el estado comercial del pedido.
            await MarcarPedidoPagadoSiCorrespondeAsync(pago);
            return ResultadoOperacion<Pago>.Ok(pago);
        }

        public Task<List<Pago>> ObtenerPorPedidoAsync(int idPedido)
        {
            return _repositorio.ObtenerPorPedidoAsync(idPedido);
        }

        public async Task<ResultadoOperacion<Pago>> CrearParaPedidoAsync(int idPedido, Pago pago)
        {
            pago.IdPedido = idPedido;
            return await CrearValidadoAsync(pago);
        }

        public override async Task<ResultadoOperacion<Pago>> CrearValidadoAsync(Pago pago)
        {
            var validacion = await ValidarPagoAsync(pago);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Pago>.BadRequest(validacion.Error ?? "El pago no es valido.");
            }

            if (pago.FechaPago == default)
            {
                pago.FechaPago = DateTime.Now;
            }

            await _repositorio.CrearAsync(pago);
            await MarcarPedidoPagadoSiCorrespondeAsync(pago);
            return ResultadoOperacion<Pago>.Ok(pago);
        }

        public override async Task<ResultadoOperacion<Pago>> ActualizarValidadoAsync(int id, Pago pago)
        {
            var validacion = await ValidarPagoAsync(pago);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Pago>.BadRequest(validacion.Error ?? "El pago no es valido.");
            }

            if (!await _repositorio.ExisteAsync(id))
            {
                return ResultadoOperacion<Pago>.NotFound("El pago indicado no existe.");
            }

            if (pago.FechaPago == default)
            {
                pago.FechaPago = DateTime.Now;
            }

            await _repositorio.ActualizarAsync(pago);
            await MarcarPedidoPagadoSiCorrespondeAsync(pago);
            return ResultadoOperacion<Pago>.Ok(pago);
        }

        private async Task<ResultadoOperacion> ValidarPagoAsync(Pago pago)
        {
            if (pago.Monto <= 0)
            {
                return ResultadoOperacion.BadRequest("El monto del pago debe ser mayor a cero.");
            }

            if (!await _pedidosRepositorio.ExisteAsync(pago.IdPedido))
            {
                return ResultadoOperacion.BadRequest("El pedido indicado no existe.");
            }

            return ResultadoOperacion.Ok();
        }

        private async Task MarcarPedidoPagadoSiCorrespondeAsync(Pago pago)
        {
            if (pago.Estado != EstadoPago.Aprobado)
            {
                return;
            }

            var pedido = await _pedidosRepositorio.ObtenerPorIdAsync(pago.IdPedido);

            if (pedido is null || pedido.Estado == EstadoPedido.Cancelado)
            {
                return;
            }

            pedido.Estado = EstadoPedido.Pagado;
            await _pedidosRepositorio.ActualizarAsync(pedido);
        }
    }
}
