using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;

namespace Frontend.Filters;

// Excluir la ruta antes de autorización permite devolver 404 también a visitantes anónimos.
[AttributeUsage(AttributeTargets.Method)]
public sealed class CompraDemoHabilitadaAttribute : ActionMethodSelectorAttribute
{
    public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action) =>
        routeContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>()
            .GetValue<bool>("CompraDemo:Enabled");
}
