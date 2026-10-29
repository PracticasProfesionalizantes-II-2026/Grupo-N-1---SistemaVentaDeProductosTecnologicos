using System.Net;
using Frontend.Models.Api.Requests;
using Frontend.Models.Api.Responses;
using Frontend.Services.Interfaces;
namespace Frontend.Services;

public sealed class UsuariosApiService(IHttpClientFactory factory) : IUsuariosApiService
{
    private HttpClient Cliente() => factory.CreateClient("TotaltechApi");
    public async Task<List<UsuarioResponse>> ObtenerTodosAsync(CancellationToken ct) =>
        await Cliente().GetFromJsonAsync<List<UsuarioResponse>>("usuarios/", ct) ?? [];
    public async Task<UsuarioResponse?> ObtenerPorIdAsync(int id, CancellationToken ct)
    {
        using var response = await Cliente().GetAsync($"usuarios/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UsuarioResponse>(ct);
    }
    public Task<HttpResponseMessage> ActualizarAsync(int id, UsuarioActualizacionRequest request, CancellationToken ct) =>
        Cliente().PutAsJsonAsync($"usuarios/{id}", request, ct);
    public Task<HttpResponseMessage> DarBajaAsync(int id, CancellationToken ct) => Cliente().DeleteAsync($"usuarios/{id}", ct);
    public Task<HttpResponseMessage> ReactivarAsync(int id, CancellationToken ct) =>
        Cliente().PostAsync($"usuarios/{id}/reactivar", null, ct);
}
