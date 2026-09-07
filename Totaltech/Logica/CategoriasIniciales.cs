using System.Text;
using Totaltech.Entidades;

namespace Totaltech.Logica;

public static class CategoriasIniciales
{
    private static readonly (string Nombre, string Descripcion)[] CategoriasCanonicas =
    [
        ("Notebooks", "Equipos portátiles y computadoras notebook."),
        ("Celulares", "Teléfonos celulares y dispositivos móviles."),
        ("Almacenamiento", "Dispositivos y unidades para almacenamiento de datos."),
        ("Gabinetes", "Gabinetes y chasis para computadoras de escritorio."),
        ("Periféricos", "Periféricos y accesorios tecnológicos de entrada, salida y conectividad."),
        ("Placas de Video", "Tarjetas gráficas y placas de video para computadoras.")
    ];

    public static async Task<IReadOnlyList<string>> InicializarAsync(
        ICategoriasLogica categoriasLogica)
    {
        var categoriasExistentes = await categoriasLogica.ObtenerTodosAsync();
        var nombresExistentes = categoriasExistentes
            .Select(categoria => NormalizarNombre(categoria.Nombre))
            .ToHashSet(StringComparer.Ordinal);
        var categoriasCreadas = new List<string>();

        foreach (var categoriaCanonica in CategoriasCanonicas)
        {
            if (!nombresExistentes.Add(NormalizarNombre(categoriaCanonica.Nombre)))
            {
                continue;
            }

            var error = await categoriasLogica.CrearAsync(new Categoria
            {
                Nombre = categoriaCanonica.Nombre,
                Descripcion = categoriaCanonica.Descripcion
            });

            if (error is not null)
            {
                throw new InvalidOperationException(
                    $"No se pudo crear la categoría '{categoriaCanonica.Nombre}': {error}");
            }

            categoriasCreadas.Add(categoriaCanonica.Nombre);
        }

        return categoriasCreadas;
    }

    private static string NormalizarNombre(string nombre)
    {
        return nombre
            .Trim()
            .Normalize(NormalizationForm.FormKC)
            .ToUpperInvariant();
    }
}
