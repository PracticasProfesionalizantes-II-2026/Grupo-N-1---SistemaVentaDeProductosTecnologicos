using System.Runtime.CompilerServices;
using Scalar.AspNetCore;

namespace Totaltech.Configuracion;

internal static class DocumentacionApiExtensions
{
    // Mantiene la dependencia opcional de documentación fuera del JIT de
    // arranque cuando el entorno no utiliza la referencia interactiva.
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void MapDevelopmentApiReference(this WebApplication app) => app.MapScalarApiReference();
}
