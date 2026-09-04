using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace Frontend.Services;

public sealed class ApiBearerTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiBearerTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var contexto = _httpContextAccessor.HttpContext;
        if (contexto is not null)
        {
            var autenticacion = await contexto.AuthenticateAsync();
            var accessToken = autenticacion.Properties?.GetTokenValue("access_token");
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
