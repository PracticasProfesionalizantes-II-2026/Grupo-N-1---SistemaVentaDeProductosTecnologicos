using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IPedidosLogica : ILogica<Pedido>
    {
        Task<List<Pedido>> ObtenerPorUsuarioAsync(int idUsuario);
        Task<List<Pedido>> ObtenerPorEstadoAsync(EstadoPedido estado);
        Task<bool> ActualizarEstadoAsync(int id, EstadoPedido estado);
    }

    public class PedidosLogica : Logica<Pedido>, IPedidosLogica
    {
        private readonly IPedidosRepositorio _repositorio;
        private readonly IUsuariosRepositorio _usuariosRepositorio;
        private readonly IDireccionesRepositorio _direccionesRepositorio;

        public PedidosLogica(IPedidosRepositorio repositorio, IUsuariosRepositorio usuariosRepositorio, IDireccionesRepositorio direccionesRepositorio) : base(repositorio)
        {
            _repositorio = repositorio;
            _usuariosRepositorio = usuariosRepositorio;
            _direccionesRepositorio = direccionesRepositorio;
        }

        public Task<List<Pedido>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return _repositorio.ObtenerPorUsuarioAsync(idUsuario);
        }

        public Task<List<Pedido>> ObtenerPorEstadoAsync(EstadoPedido estado)
        {
            return _repositorio.ObtenerPorEstadoAsync(estado);
        }

        public async Task<bool> ActualizarEstadoAsync(int id, EstadoPedido estado)
        {
            var pedido = await _repositorio.ObtenerPorIdAsync(id);

            if (pedido is null)
            {
                return false;
            }

            pedido.Estado = estado;
            await _repositorio.ActualizarAsync(pedido);
            return true;
        }

        public override async Task<ResultadoOperacion<Pedido>> CrearValidadoAsync(Pedido pedido)
        {
            var validacion = await ValidarPedidoAsync(pedido);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Pedido>.BadRequest(validacion.Error ?? "El pedido no es valido.");
            }

            if (pedido.FechaPedido == default)
            {
                pedido.FechaPedido = DateTime.Now;
            }

            return await base.CrearValidadoAsync(pedido);
        }

        public override async Task<ResultadoOperacion<Pedido>> ActualizarValidadoAsync(int id, Pedido pedido)
        {
            var validacion = await ValidarPedidoAsync(pedido);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Pedido>.BadRequest(validacion.Error ?? "El pedido no es valido.");
            }

            if (pedido.FechaPedido == default)
            {
                pedido.FechaPedido = DateTime.Now;
            }

            return await base.ActualizarValidadoAsync(id, pedido);
        }

        private async Task<ResultadoOperacion> ValidarPedidoAsync(Pedido pedido)
        {
            if (pedido.IdUsuario.HasValue && !await _usuariosRepositorio.ExisteAsync(pedido.IdUsuario.Value))
            {
                return ResultadoOperacion.BadRequest("El usuario indicado no existe.");
            }

            if (!await _direccionesRepositorio.ExisteAsync(pedido.IdDireccion))
            {
                return ResultadoOperacion.BadRequest("La direccion indicada no existe.");
            }

            return ResultadoOperacion.Ok();
        }
    }
}
