using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IDireccionesLogica : ILogica<Direccion>
    {
    }

    public class DireccionesLogica : Logica<Direccion>, IDireccionesLogica
    {
        private readonly IUsuariosRepositorio _usuariosRepositorio;

        public DireccionesLogica(IDireccionesRepositorio repositorio, IUsuariosRepositorio usuariosRepositorio) : base(repositorio)
        {
            _usuariosRepositorio = usuariosRepositorio;
        }

        public override async Task<ResultadoOperacion<Direccion>> CrearValidadoAsync(Direccion direccion)
        {
            var validacion = await ValidarDireccionAsync(direccion);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Direccion>.BadRequest(validacion.Error ?? "La direccion no es valida.");
            }

            return await base.CrearValidadoAsync(direccion);
        }

        public override async Task<ResultadoOperacion<Direccion>> ActualizarValidadoAsync(int id, Direccion direccion)
        {
            var validacion = await ValidarDireccionAsync(direccion);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Direccion>.BadRequest(validacion.Error ?? "La direccion no es valida.");
            }

            return await base.ActualizarValidadoAsync(id, direccion);
        }

        private async Task<ResultadoOperacion> ValidarDireccionAsync(Direccion direccion)
        {
            if (string.IsNullOrWhiteSpace(direccion.Calle) || string.IsNullOrWhiteSpace(direccion.Numero))
            {
                return ResultadoOperacion.BadRequest("La calle y el numero son obligatorios.");
            }

            if (direccion.IdUsuario.HasValue && !await _usuariosRepositorio.ExisteAsync(direccion.IdUsuario.Value))
            {
                return ResultadoOperacion.BadRequest("El usuario indicado no existe.");
            }

            return ResultadoOperacion.Ok();
        }
    }
}
