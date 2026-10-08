using Frontend.Models.Api.Requests;
using Frontend.Models.Api.Responses;
namespace Frontend.Services.Interfaces;

public interface IUsuariosApiService
{
    Task<List<UsuarioResponse>> ObtenerTodosAsync(CancellationToken cancellationToken);
    Task<UsuarioResponse?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken);
    Task<HttpResponseMessage> ActualizarAsync(int id, UsuarioActualizacionRequest request, CancellationToken cancellationToken);
    Task<HttpResponseMessage> DarBajaAsync(int id, CancellationToken cancellationToken);
    Task<HttpResponseMessage> ReactivarAsync(int id, CancellationToken cancellationToken);
}
