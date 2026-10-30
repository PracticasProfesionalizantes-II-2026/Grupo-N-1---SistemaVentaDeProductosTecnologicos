using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Frontend.Models.Api.Requests;
using Frontend.Models.Api.Responses;
using Frontend.Models.ViewModels.Productos;
using Frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

public partial class ProductosController
{
    private readonly CatalogoArchivoExtractor _catalogoExtractor;

    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Importar()
    {
        var modelo = new ProductoImportacionViewModel { IdOperacion = Guid.NewGuid() };
        await CargarOpcionesImportacionAsync(modelo);
        return View(modelo);
    }

    [HttpGet, Authorize(Roles = "Admin")]
    public IActionResult PlantillaImportacionCsv()
    {
        var texto = "Nombre;Descripcion;Precio;Stock;CategoriaNombre\r\nNotebook de ejemplo;Equipo portátil;1234,56;5;Notebooks\r\n";
        var utf8 = new UTF8Encoding(true);
        return File(utf8.GetPreamble().Concat(utf8.GetBytes(texto)).ToArray(), "text/csv; charset=utf-8", "plantilla-catalogo.csv");
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024, ValueCountLimit = 20000)]
    public async Task<IActionResult> PrevisualizarImportacion(ProductoImportacionViewModel modelo)
    {
        modelo.PuedeConfirmar = false;
        if (modelo.Archivo is not null)
        {
            var resultado = await _catalogoExtractor.ExtraerAsync(modelo.Archivo, HttpContext.RequestAborted);
            modelo.Filas = resultado.Filas;
            modelo.IdOperacion = Guid.NewGuid();
            // Un archivo nuevo reemplaza el borrador anterior, incluidos sus errores de binding.
            ModelState.Clear();
            foreach (var error in resultado.Errores) ModelState.AddModelError(nameof(modelo.Archivo), error);
            modelo.Advertencias.AddRange(resultado.Advertencias);
        }
        await CargarOpcionesImportacionAsync(modelo);
        var filas = ValidarImportacion(modelo);
        if (modelo.IdOperacion == Guid.Empty) ModelState.AddModelError("", "La operación no es válida. Volvé a cargar el archivo.");
        if (ModelState.IsValid)
        {
            try
            {
                using var respuesta = await _productosApiService.PrevisualizarImportacionAsync(new()
                    { IdProveedor = modelo.IdProveedor!.Value, Filas = filas }, HttpContext.RequestAborted);
                if (respuesta.StatusCode is HttpStatusCode.OK or HttpStatusCode.UnprocessableEntity)
                {
                    var preview = await respuesta.Content.ReadFromJsonAsync<ProductoImportacionPrevisualizacionResponse>(HttpContext.RequestAborted);
                    if (preview is null || preview.Filas is null || preview.Errores is null || preview.Errores.Any(e => e is null)
                        || preview.Filas.Count != modelo.Filas.Count || preview.Filas.Any(f => f is null)
                        || !preview.Filas.Select(f => f.NumeroFila).Order().SequenceEqual(Enumerable.Range(1, modelo.Filas.Count)))
                        ModelState.AddModelError("", "La API devolvió una previsualización incompleta. Volvé a validar el lote.");
                    else
                    {
                        AplicarFilasNormalizadas(modelo, preview.Filas);
                        AgregarErroresImportacion(preview.Errores, modelo.Filas.Count);
                        ValidarImportacion(modelo);
                        modelo.PuedeConfirmar = respuesta.StatusCode == HttpStatusCode.OK && preview.PuedeConfirmar && ModelState.IsValid;
                        if (!modelo.PuedeConfirmar && ModelState.IsValid)
                            ModelState.AddModelError("", "La API todavía no permite confirmar este lote. Revisá las filas y volvé a validar.");
                    }
                }
                else ErrorRespuestaImportacion(respuesta.StatusCode);
            }
            catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested) { throw; }
            catch (TaskCanceledException) { ModelState.AddModelError("", "La validación tardó demasiado. Las filas se conservan; podés volver a validarlas."); }
            catch (HttpRequestException) { ModelState.AddModelError("", "No pudimos validar el lote con la API. Las filas extraídas se conservan y falta la validación Backend."); }
            catch (JsonException) { ModelState.AddModelError("", "La API devolvió una respuesta no válida. Volvé a validar el lote."); }
        }
        return View("Importar", modelo);
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024, ValueCountLimit = 20000)]
    public async Task<IActionResult> ConfirmarImportacion(ProductoImportacionViewModel modelo)
    {
        modelo.PuedeConfirmar = false;
        await CargarOpcionesImportacionAsync(modelo);
        var filas = ValidarImportacion(modelo);
        if (modelo.IdOperacion == Guid.Empty) ModelState.AddModelError("", "La operación no es válida. Volvé a cargar el archivo.");
        if (ModelState.IsValid)
        {
            try
            {
                using var respuesta = await _productosApiService.ConfirmarImportacionAsync(new()
                    { IdOperacion = modelo.IdOperacion, IdProveedor = modelo.IdProveedor!.Value, Filas = filas }, HttpContext.RequestAborted);
                if (respuesta.StatusCode == HttpStatusCode.OK)
                {
                    var resultado = await respuesta.Content.ReadFromJsonAsync<ProductoImportacionConfirmacionResponse>(HttpContext.RequestAborted);
                    if (resultado is not null && resultado.IdOperacion == modelo.IdOperacion && resultado.Importados == filas.Count)
                    {
                        TempData["Mensaje"] = $"Se importaron {resultado.Importados} productos correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                    ConfirmacionIncierta();
                }
                else if (respuesta.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    var error = await respuesta.Content.ReadFromJsonAsync<ProductoImportacionPrevisualizacionResponse>(HttpContext.RequestAborted);
                    AgregarErroresImportacion(error?.Errores ?? [], modelo.Filas.Count);
                    ModelState.AddModelError("", "No se importó el lote. Corregí las filas y volvé a validar.");
                }
                else if ((int)respuesta.StatusCode >= 500) ConfirmacionIncierta();
                else ErrorRespuestaImportacion(respuesta.StatusCode);
            }
            catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested) { throw; }
            catch (TaskCanceledException) { ConfirmacionIncierta(); }
            catch (HttpRequestException) { ConfirmacionIncierta(); }
            catch (JsonException) { ConfirmacionIncierta(); }
        }
        return View("Importar", modelo);
    }

    private async Task CargarOpcionesImportacionAsync(ProductoImportacionViewModel modelo)
    {
        try
        {
            var categorias = _categoriasApiService.ObtenerTodosAsync(HttpContext.RequestAborted);
            var proveedores = _proveedoresApiService.ObtenerTodosAsync(HttpContext.RequestAborted);
            await Task.WhenAll(categorias, proveedores);
            modelo.Categorias = await categorias;
            modelo.Proveedores = (await proveedores).Where(p => p.Activo).OrderBy(p => p.RazonSocial).ToList();
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested) { throw; }
        catch (TaskCanceledException) { ModelState.AddModelError("", "La consulta de categorías y proveedores tardó demasiado. Podés reintentar."); }
        catch (HttpRequestException) { ModelState.AddModelError("", "No pudimos cargar categorías y proveedores. Podés extraer el archivo; falta validación Backend."); }
        catch (JsonException) { ModelState.AddModelError("", "No pudimos interpretar las categorías y proveedores. Volvé a validar más tarde."); }
    }

    private List<ProductoImportacionFilaRequest> ValidarImportacion(ProductoImportacionViewModel modelo)
    {
        var resultado = new List<ProductoImportacionFilaRequest>();
        if (modelo.Filas.Count is 0 or > CatalogoArchivoExtractor.MaximoFilas)
            ModelState.AddModelError("", "Seleccioná un archivo con entre 1 y 1.000 productos.");
        if (!modelo.Proveedores.Any(p => p.IdProveedor == modelo.IdProveedor))
            ModelState.AddModelError(nameof(modelo.IdProveedor), "Seleccioná un proveedor activo para el lote.");
        var nombres = new HashSet<string>();
        for (var i = 0; i < modelo.Filas.Count; i++)
        {
            var fila = modelo.Filas[i];
            var prefijo = $"Filas[{i}].";
            fila.Nombre = CatalogoImportacionNormalizador.Espacios(fila.Nombre);
            fila.Descripcion = CatalogoImportacionNormalizador.Espacios(fila.Descripcion);
            if (string.IsNullOrWhiteSpace(fila.Nombre)) ModelState.AddModelError(prefijo + "Nombre", "Ingresá el nombre.");
            else if (!nombres.Add(CatalogoImportacionNormalizador.Clave(fila.Nombre)))
                ModelState.AddModelError(prefijo + "Nombre", "El nombre está repetido en este lote.");
            foreach (var (campo, valor) in new[] { ("Nombre", fila.Nombre), ("Descripcion", fila.Descripcion), ("CategoriaNombre", fila.CategoriaNombre) })
                if (valor?.StartsWith('=') == true) ModelState.AddModelError(prefijo + campo, "Reemplazá la fórmula por un valor.");
            var precioValido = CatalogoImportacionNormalizador.IntentarPrecio(fila.Precio, out var precio);
            if (!precioValido) ModelState.AddModelError(prefijo + "Precio", "Ingresá un precio no negativo, sin ambigüedad y con hasta dos decimales.");
            var stockValido = CatalogoImportacionNormalizador.IntentarStock(fila.Stock, out var stock);
            if (!stockValido) ModelState.AddModelError(prefijo + "Stock", "Ingresá un stock entero no negativo.");
            if (fila.IdCategoria is null && !string.IsNullOrWhiteSpace(fila.CategoriaNombre))
            {
                var coincidencias = modelo.Categorias.Where(c => CatalogoImportacionNormalizador.Clave(c.Nombre)
                    == CatalogoImportacionNormalizador.Clave(fila.CategoriaNombre)).ToList();
                if (coincidencias.Count == 1)
                {
                    fila.IdCategoria = coincidencias[0].IdCategoria;
                    ModelState.Remove(prefijo + "IdCategoria");
                }
            }
            if (!modelo.Categorias.Any(c => c.IdCategoria == fila.IdCategoria))
                ModelState.AddModelError(prefijo + "IdCategoria", "Seleccioná una categoría existente.");
            resultado.Add(new() { NumeroFila = i + 1, Nombre = fila.Nombre, Descripcion = fila.Descripcion,
                Precio = precioValido ? precio : null, Stock = stockValido ? stock : null,
                CategoriaNombre = fila.CategoriaNombre, IdCategoria = fila.IdCategoria });
        }
        return resultado;
    }

    private void AplicarFilasNormalizadas(ProductoImportacionViewModel modelo, List<ProductoImportacionFilaRequest> filas)
    {
        foreach (var normalizada in filas)
        {
            var i = normalizada.NumeroFila - 1;
            var fila = modelo.Filas[i];
            fila.Nombre = normalizada.Nombre;
            fila.Descripcion = normalizada.Descripcion;
            fila.Precio = normalizada.Precio?.ToString("0.00", CultureInfo.InvariantCulture) ?? "";
            fila.Stock = normalizada.Stock?.ToString(CultureInfo.InvariantCulture) ?? "";
            fila.CategoriaNombre = normalizada.CategoriaNombre ?? "";
            fila.IdCategoria = normalizada.IdCategoria;
            foreach (var campo in new[] { "Nombre", "Descripcion", "Precio", "Stock", "CategoriaNombre", "IdCategoria" })
                ModelState.Remove($"Filas[{i}].{campo}");
        }
    }

    private void AgregarErroresImportacion(IEnumerable<ProductoImportacionErrorResponse> errores, int cantidad)
    {
        var campos = new[] { "Nombre", "Descripcion", "Precio", "Stock", "CategoriaNombre", "IdCategoria" };
        foreach (var error in errores)
        {
            if (error is null)
            {
                ModelState.AddModelError("", "La API devolvió un error de validación incompleto. Volvé a validar el lote.");
                continue;
            }
            var campo = campos.FirstOrDefault(c => c.Equals(error.Campo, StringComparison.OrdinalIgnoreCase));
            var clave = error.NumeroFila is > 0 && error.NumeroFila <= cantidad && campo is not null
                ? $"Filas[{error.NumeroFila - 1}].{campo}" : "";
            ModelState.AddModelError(clave, string.IsNullOrWhiteSpace(error.Mensaje) ? "La API rechazó una fila del lote." : error.Mensaje);
        }
    }

    private void ErrorRespuestaImportacion(HttpStatusCode estado) => ModelState.AddModelError("", estado switch
    {
        HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed => "La importación todavía no está disponible en Backend. Las filas se conservan; no se confirmó ningún resultado.",
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "Tu sesión no permite importar productos. Volvé a iniciar sesión como administrador.",
        HttpStatusCode.Conflict => "El lote tiene un conflicto: puede haber nombres existentes o una operación usada con otros datos. Revisá el catálogo antes de volver a validar.",
        _ => "La API no pudo procesar el lote. Revisá los datos y volvé a validar."
    });

    private void ConfirmacionIncierta() => ModelState.AddModelError("",
        "No pudimos confirmar el resultado de la importación. Revisá el catálogo antes de reintentar. Se conserva el identificador de operación; no se reintentó automáticamente.");
}
