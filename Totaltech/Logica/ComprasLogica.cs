using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IComprasLogica : ILogica<Compra>
    {
    }

    public class ComprasLogica : Logica<Compra>, IComprasLogica
    {
        private readonly IProveedoresRepositorio _proveedoresRepositorio;

        public ComprasLogica(IComprasRepositorio repositorio, IProveedoresRepositorio proveedoresRepositorio) : base(repositorio)
        {
            _proveedoresRepositorio = proveedoresRepositorio;
        }

        public override async Task<ResultadoOperacion<Compra>> CrearValidadoAsync(Compra compra)
        {
            var validacion = await ValidarCompraAsync(compra);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Compra>.BadRequest(validacion.Error ?? "La compra no es valida.");
            }

            if (compra.FechaCompra == default)
            {
                compra.FechaCompra = DateTime.Now;
            }

            return await base.CrearValidadoAsync(compra);
        }

        public override async Task<ResultadoOperacion<Compra>> ActualizarValidadoAsync(int id, Compra compra)
        {
            var validacion = await ValidarCompraAsync(compra);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Compra>.BadRequest(validacion.Error ?? "La compra no es valida.");
            }

            if (compra.FechaCompra == default)
            {
                compra.FechaCompra = DateTime.Now;
            }

            return await base.ActualizarValidadoAsync(id, compra);
        }

        private async Task<ResultadoOperacion> ValidarCompraAsync(Compra compra)
        {
            if (compra.Total < 0)
            {
                return ResultadoOperacion.BadRequest("El total de la compra no puede ser negativo.");
            }

            if (!await _proveedoresRepositorio.ExisteAsync(compra.IdProveedor))
            {
                return ResultadoOperacion.BadRequest("El proveedor indicado no existe.");
            }

            return ResultadoOperacion.Ok();
        }
    }
}
