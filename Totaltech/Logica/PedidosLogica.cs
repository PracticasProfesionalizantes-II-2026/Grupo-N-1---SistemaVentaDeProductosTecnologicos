using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IPedidosLogica
    {
        Task<List<Pedido>> ObtenerTodosAsync();
        Task<Pedido?> ObtenerPorIdAsync(int id);
        Task<string?> CrearAsync(Pedido pedido);
        Task<string?> ActualizarAsync(Pedido pedido);
        Task<bool> EliminarAsync(int id);
        Task<List<Pedido>> ObtenerPorUsuarioAsync(int idUsuario);
        Task<List<Pedido>> ObtenerPorEstadoAsync(EstadoPedido estado);
        Task<bool> ActualizarEstadoAsync(int id, EstadoPedido estado);
    }

    public class PedidosLogica : IPedidosLogica
    {
        private readonly IPedidosRepositorio _repositorio;
        private readonly IUsuariosRepositorio _usuariosRepositorio;
        private readonly IDireccionesRepositorio _direccionesRepositorio;

        public PedidosLogica(
            IPedidosRepositorio repositorio,
            IUsuariosRepositorio usuariosRepositorio,
            IDireccionesRepositorio direccionesRepositorio)
        {
            _repositorio = repositorio;
            _usuariosRepositorio = usuariosRepositorio;
            _direccionesRepositorio = direccionesRepositorio;
        }

        public Task<List<Pedido>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<Pedido?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<string?> CrearAsync(Pedido pedido)
        {
            var error = await ValidarPedidoAsync(pedido);
            if (error is not null)
            {
                return error;
            }

            if (pedido.FechaPedido == default)
            {
                pedido.FechaPedido = DateTime.Now;
            }

            await _repositorio.CrearAsync(pedido);
            return null;
        }

        public async Task<string?> ActualizarAsync(Pedido pedido)
        {
            var error = await ValidarPedidoAsync(pedido);
            if (error is not null)
            {
                return error;
            }

            if (pedido.FechaPedido == default)
            {
                pedido.FechaPedido = DateTime.Now;
            }

            await _repositorio.ActualizarAsync(pedido);
            return null;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var pedido = await _repositorio.ObtenerPorIdAsync(id);
            if (pedido is null)
            {
                return false;
            }

            await _repositorio.EliminarAsync(pedido);
            return true;
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

        private async Task<string?> ValidarPedidoAsync(Pedido pedido)
        {
            if (pedido.IdUsuario.HasValue && !await _usuariosRepositorio.ExisteAsync(pedido.IdUsuario.Value))
            {
                return "El usuario indicado no existe.";
            }

            if (!await _direccionesRepositorio.ExisteAsync(pedido.IdDireccion))
            {
                return "La direccion indicada no existe.";
            }

            return null;
        }
    }
}
