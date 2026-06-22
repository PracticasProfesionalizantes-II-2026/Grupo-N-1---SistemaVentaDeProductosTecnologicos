using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IProveedoresLogica : ILogica<Proveedor>
    {
    }

    public class ProveedoresLogica : Logica<Proveedor>, IProveedoresLogica
    {
        private readonly IDireccionesRepositorio _direccionesRepositorio;

        public ProveedoresLogica(IProveedoresRepositorio repositorio, IDireccionesRepositorio direccionesRepositorio) : base(repositorio)
        {
            _direccionesRepositorio = direccionesRepositorio;
        }

        public override async Task<ResultadoOperacion<Proveedor>> CrearValidadoAsync(Proveedor proveedor)
        {
            var validacion = await ValidarProveedorAsync(proveedor);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Proveedor>.BadRequest(validacion.Error ?? "El proveedor no es valido.");
            }

            return await base.CrearValidadoAsync(proveedor);
        }

        public override async Task<ResultadoOperacion<Proveedor>> ActualizarValidadoAsync(int id, Proveedor proveedor)
        {
            var validacion = await ValidarProveedorAsync(proveedor);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Proveedor>.BadRequest(validacion.Error ?? "El proveedor no es valido.");
            }

            return await base.ActualizarValidadoAsync(id, proveedor);
        }

        private async Task<ResultadoOperacion> ValidarProveedorAsync(Proveedor proveedor)
        {
            if (string.IsNullOrWhiteSpace(proveedor.RazonSocial) || string.IsNullOrWhiteSpace(proveedor.Cuit))
            {
                return ResultadoOperacion.BadRequest("La razon social y el CUIT son obligatorios.");
            }

            if (proveedor.PlazoPagoDias < 0 || proveedor.TiempoEntregaDias < 0)
            {
                return ResultadoOperacion.BadRequest("Los plazos no pueden ser negativos.");
            }

            if (proveedor.IdDireccion.HasValue && !await _direccionesRepositorio.ExisteAsync(proveedor.IdDireccion.Value))
            {
                return ResultadoOperacion.BadRequest("La direccion indicada no existe.");
            }

            return ResultadoOperacion.Ok();
        }
    }
}
