using System.Net;
using Frontend.Models.Api.Responses;

namespace Frontend.Services.Interfaces;

public interface ICarritosApiService
{
    Task<CarritoResumenResponse?> ObtenerActualAsync(CancellationToken cancellationToken = default);
    Task<CarritoApiResultado> AgregarAsync(int idProducto, int cantidad, CancellationToken cancellationToken = default);
    Task<CarritoApiResultado> ActualizarCantidadAsync(int idProducto, int cantidad, CancellationToken cancellationToken = default);
    Task<CarritoApiResultado> EliminarAsync(int idProducto, CancellationToken cancellationToken = default);
}

public sealed record CarritoApiResultado(CarritoResumenResponse? Resumen, HttpStatusCode Estado, string? Error)
{
    public bool Exitoso => (int)Estado is >= 200 and < 300;
}
