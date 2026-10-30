using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Frontend.Models.ViewModels.Productos;
using Frontend.Services.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Frontend.Services;

public sealed class CatalogoExtraccionResultado
{
    public List<ProductoImportacionFilaViewModel> Filas { get; set; } = [];
    public List<string> Advertencias { get; set; } = [];
    public List<string> Errores { get; set; } = [];
}

public sealed class CatalogoArchivoExtractor(ICatalogoExtractorAgent agent)
{
    public const long MaximoBytes = 5 * 1024 * 1024;
    public const int MaximoFilas = 1000;
    public const int MaximoPaginas = 100;
    public const long MaximoXmlBytes = 20 * 1024 * 1024;
    private static readonly XNamespace Excel = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    public async Task<CatalogoExtraccionResultado> ExtraerAsync(IFormFile archivo, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resultado = new CatalogoExtraccionResultado();
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (extension is not (".csv" or ".xlsx" or ".pdf")) return Error("Seleccioná un archivo CSV, XLSX o PDF.");
        if (archivo.Length > MaximoBytes) return Error("El archivo no puede superar los 5 MB.");
        using var contenido = new MemoryStream();
        try
        {
            await using (var origen = archivo.OpenReadStream())
            {
                var buffer = new byte[81920];
                while (true)
                {
                    var leidos = await origen.ReadAsync(buffer.AsMemory(0,
                        (int)Math.Min(buffer.Length, MaximoBytes + 1 - contenido.Length)), cancellationToken);
                    if (leidos == 0) break;
                    await contenido.WriteAsync(buffer.AsMemory(0, leidos), cancellationToken);
                    if (contenido.Length > MaximoBytes) return Error("El archivo no puede superar los 5 MB.");
                }
            }
            if (contenido.Length == 0) return Error("Seleccioná un archivo que no esté vacío.");
            contenido.Position = 0;
            switch (extension)
            {
                case ".csv":
                    using (var lector = new StreamReader(contenido, new UTF8Encoding(false, true), true, leaveOpen: true))
                        resultado.Filas = LeerTabla(LeerCsv(await lector.ReadToEndAsync(cancellationToken)), "CSV");
                    break;
                case ".xlsx": resultado.Filas = await LeerXlsxAsync(contenido, cancellationToken); break;
                case ".pdf": resultado = await LeerPdfAsync(contenido, cancellationToken); break;
            }
            if (resultado.Filas.Count > MaximoFilas) return Error("El catálogo no puede superar los 1.000 productos.");
            if (resultado.Filas.Count == 0 && resultado.Errores.Count == 0)
                resultado.Errores.Add("No encontramos filas de productos interpretables. Revisá el formato o usá la plantilla CSV.");
            return resultado;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex) when (ex is IOException or InvalidDataException or XmlException
            or DecoderFallbackException or FormatException or ArgumentException or OverflowException)
        {
            return Error(ex is CatalogoFormatoException ? ex.Message : "El archivo está dañado o su contenido no corresponde al formato seleccionado.");
        }
    }

    private static CatalogoExtraccionResultado Error(string mensaje) => new() { Errores = [mensaje] };

    private static List<List<string>> LeerCsv(string texto)
    {
        if (texto.Any(c => char.IsControl(c) && c is not ('\t' or '\r' or '\n')))
            throw new CatalogoFormatoException("El CSV contiene caracteres de control no permitidos.");
        CatalogoFormatoException? errorDeFormato = null;
        var candidatos = new[] { ';', ',' }.Select(separador =>
        {
            try { return ParsearCsv(texto, separador); }
            catch (CatalogoFormatoException ex) { errorDeFormato ??= ex; return null; }
        }).Where(tabla => tabla is { Count: > 0 } && TieneEncabezados(tabla[0])).ToList();
        if (candidatos.Count != 1) throw errorDeFormato ?? new CatalogoFormatoException("El CSV debe tener encabezados Nombre, Precio y Stock, separados por ; o ,.");
        return candidatos[0]!;
    }

    private static List<List<string>> ParsearCsv(string texto, char separador)
    {
        var tabla = new List<List<string>>();
        var fila = new List<string>();
        var campo = new StringBuilder();
        var entreComillas = false;
        var comillaCerrada = false;
        for (var i = 0; i < texto.Length; i++)
        {
            var c = texto[i];
            if (entreComillas)
            {
                if (c == '"')
                {
                    if (i + 1 < texto.Length && texto[i + 1] == '"') { campo.Append('"'); i++; }
                    else { entreComillas = false; comillaCerrada = true; }
                }
                else campo.Append(c);
                continue;
            }
            if (c == '"')
            {
                if (campo.Length != 0 || comillaCerrada) throw new CatalogoFormatoException("El CSV contiene comillas mal cerradas.");
                entreComillas = true;
            }
            else if (c == separador || c is '\r' or '\n')
            {
                fila.Add(campo.ToString()); campo.Clear(); comillaCerrada = false;
                if (c != separador)
                {
                    if (fila.Any(x => !string.IsNullOrWhiteSpace(x))) tabla.Add(fila);
                    if (tabla.Count > MaximoFilas + 1) throw new CatalogoFormatoException("El catálogo no puede superar los 1.000 productos.");
                    fila = [];
                    if (c == '\r' && i + 1 < texto.Length && texto[i + 1] == '\n') i++;
                }
            }
            else
            {
                if (comillaCerrada && !char.IsWhiteSpace(c)) throw new CatalogoFormatoException("El CSV contiene comillas mal cerradas.");
                if (!comillaCerrada) campo.Append(c);
            }
        }
        if (entreComillas) throw new CatalogoFormatoException("El CSV contiene comillas mal cerradas.");
        fila.Add(campo.ToString());
        if (fila.Any(x => !string.IsNullOrWhiteSpace(x))) tabla.Add(fila);
        return tabla;
    }

    private static string Encabezado(string texto)
    {
        var clave = CatalogoImportacionNormalizador.Clave(texto.TrimStart('\uFEFF'));
        return clave == "CATEGORIA" ? "CATEGORIANOMBRE" : clave;
    }

    private static bool TieneEncabezados(List<string> fila) => new[] { "NOMBRE", "PRECIO", "STOCK" }
        .All(requerido => fila.Select(Encabezado).Contains(requerido));

    private static List<ProductoImportacionFilaViewModel> LeerTabla(List<List<string>> tabla, string origen,
        List<int>? numerosFila = null)
    {
        if (tabla.Count == 0 || !TieneEncabezados(tabla[0]))
            throw new CatalogoFormatoException("La primera fila debe incluir Nombre, Precio y Stock.");
        var encabezados = tabla[0].Select(Encabezado).ToList();
        if (encabezados.Where(x => x.Length > 0).GroupBy(x => x).Any(g => g.Count() > 1))
            throw new CatalogoFormatoException("Hay encabezados repetidos en el archivo.");
        var filas = new List<ProductoImportacionFilaViewModel>();
        for (var i = 1; i < tabla.Count; i++)
        {
            var valores = tabla[i];
            string Obtener(string nombre) { var indice = encabezados.IndexOf(nombre); return indice >= 0 && indice < valores.Count ? valores[indice] : ""; }
            if (valores.Count > encabezados.Count) throw new CatalogoFormatoException($"La fila {i + 1} contiene más columnas que el encabezado.");
            var nombre = CatalogoImportacionNormalizador.Espacios(Obtener("NOMBRE"));
            var categoria = CatalogoImportacionNormalizador.Espacios(Obtener("CATEGORIANOMBRE"));
            filas.Add(new() { Nombre = nombre, Descripcion = CatalogoImportacionNormalizador.Espacios(Obtener("DESCRIPCION")),
                Precio = Obtener("PRECIO").Trim(), Stock = Obtener("STOCK").Trim(),
                CategoriaNombre = categoria.Length > 0 ? categoria : CatalogoImportacionNormalizador.SugerirCategoria(nombre),
                Origen = $"{origen}, fila {numerosFila?[i] ?? i + 1}" });
        }
        return filas;
    }

    private static async Task<List<ProductoImportacionFilaViewModel>> LeerXlsxAsync(Stream contenido, CancellationToken ct)
    {
        using var zip = new ZipArchive(contenido, ZipArchiveMode.Read, leaveOpen: true);
        long acumulado = 0;
        async Task<XDocument?> Leer(string nombre, bool obligatorio)
        {
            var entradas = zip.Entries.Where(e => e.FullName == nombre).ToList();
            if (entradas.Count > 1) throw new CatalogoFormatoException("El XLSX contiene entradas XML repetidas.");
            var entrada = entradas.SingleOrDefault();
            if (entrada is null)
            {
                if (obligatorio) throw new CatalogoFormatoException("El XLSX no contiene sheet1.xml.");
                return null;
            }
            if (entrada.Length > MaximoXmlBytes - acumulado) throw new CatalogoFormatoException("El XML descomprimido supera los 20 MiB permitidos.");
            await using var stream = entrada.Open();
            using var memoria = new MemoryStream();
            var buffer = new byte[81920];
            while (true)
            {
                var leidos = await stream.ReadAsync(buffer.AsMemory(0, (int)Math.Min(buffer.Length, MaximoXmlBytes + 1 - acumulado)), ct);
                if (leidos == 0) break;
                acumulado += leidos;
                if (acumulado > MaximoXmlBytes) throw new CatalogoFormatoException("El XML descomprimido supera los 20 MiB permitidos.");
                await memoria.WriteAsync(buffer.AsMemory(0, leidos), ct);
            }
            memoria.Position = 0;
            using var lector = XmlReader.Create(memoria, new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null, MaxCharactersInDocument = MaximoXmlBytes });
            return await XDocument.LoadAsync(lector, LoadOptions.None, ct);
        }
        var strings = (await Leer("xl/sharedStrings.xml", false))?.Descendants(Excel + "si")
            .Select(si => string.Concat(si.Descendants(Excel + "t").Select(t => t.Value))).ToArray() ?? [];
        var hoja = await Leer("xl/worksheets/sheet1.xml", true);
        var tabla = new List<List<string>>();
        var numeros = new List<int>();
        foreach (var row in hoja!.Descendants(Excel + "row"))
        {
            ct.ThrowIfCancellationRequested();
            var valores = new Dictionary<int, string>();
            var siguiente = 0;
            foreach (var cell in row.Elements(Excel + "c"))
            {
                var referencia = (string?)cell.Attribute("r");
                var columna = 0;
                if (referencia is not null)
                {
                    foreach (var letra in referencia.TakeWhile(char.IsAsciiLetter))
                        columna = checked(columna * 26 + char.ToUpperInvariant(letra) - 'A' + 1);
                    columna--;
                }
                else columna = siguiente;
                if (columna is < 0 or > 16383) throw new CatalogoFormatoException("Referencia de celda XLSX inválida.");
                siguiente = columna + 1;
                var tipo = (string?)cell.Attribute("t");
                var valor = cell.Element(Excel + "v")?.Value ?? "";
                if (cell.Element(Excel + "f") is not null) valor = "=FÓRMULA NO ADMITIDA: ingresá el valor";
                else if (tipo == "s")
                {
                    if (!int.TryParse(valor, out var indice) || indice < 0 || indice >= strings.Length)
                        throw new CatalogoFormatoException("El XLSX contiene una referencia de texto inválida.");
                    valor = strings[indice];
                }
                else if (tipo == "inlineStr") valor = string.Concat(cell.Descendants(Excel + "t").Select(t => t.Value));
                if (!valores.TryAdd(columna, valor)) throw new CatalogoFormatoException("El XLSX contiene celdas repetidas.");
            }
            if (valores.Values.All(string.IsNullOrWhiteSpace)) continue;
            tabla.Add(Enumerable.Range(0, valores.Keys.Max() + 1).Select(i => valores.GetValueOrDefault(i, "")).ToList());
            numeros.Add(int.TryParse((string?)row.Attribute("r"), out var numero) ? numero : tabla.Count);
            if (tabla.Count > MaximoFilas + 1) throw new CatalogoFormatoException("El catálogo no puede superar los 1.000 productos.");
        }
        return LeerTabla(tabla, "XLSX", numeros);
    }

    private async Task<CatalogoExtraccionResultado> LeerPdfAsync(MemoryStream contenido, CancellationToken ct)
    {
        if (!contenido.GetBuffer().AsSpan(0, (int)contenido.Length).StartsWith("%PDF-"u8))
            return Error("El contenido no corresponde a un archivo PDF.");
        var resultado = new CatalogoExtraccionResultado();
        var lineas = new List<CatalogoLineaExtraida>();
        try
        {
            using var pdf = PdfDocument.Open(contenido);
            if (pdf.NumberOfPages > MaximoPaginas) return Error("El PDF no puede superar las 100 páginas.");
            foreach (var pagina in pdf.GetPages())
            {
                ct.ThrowIfCancellationRequested();
                var texto = ContentOrderTextExtractor.GetText(pagina);
                var numero = 0;
                foreach (var linea in texto.Split('\n'))
                {
                    numero++;
                    if (!string.IsNullOrWhiteSpace(linea)) lineas.Add(new(linea.Trim(), $"PDF, página {pagina.Number}, línea {numero}"));
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Error("No pudimos leer el PDF. Puede estar protegido, dañado o usar un formato no compatible.");
        }
        if (lineas.Count == 0) return Error("El PDF no contiene texto extraíble. Para documentos escaneados usá CSV o XLSX; no se admite OCR.");
        var local = new CatalogoExtractorHeuristico();
        IReadOnlyList<ProductoImportacionFilaViewModel>? filas = null;
        try
        {
            filas = await agent.NormalizarAsync(lineas, ct);
            if (filas is null || filas.Count == 0 || filas.Count > MaximoFilas
                || filas.Any(f => f is null || string.IsNullOrWhiteSpace(f.Nombre)
                    || f.Descripcion is null || f.Precio is null || f.Stock is null || f.CategoriaNombre is null
                    || !CatalogoImportacionNormalizador.IntentarPrecio(f.Precio, out _)
                    || !CatalogoImportacionNormalizador.IntentarStock(f.Stock, out _)
                    || !lineas.Any(l => l.Origen == f.Origen)))
                filas = null;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or InvalidOperationException
            or System.Text.Json.JsonException or ArgumentException or IOException) { filas = null; }
        if (filas is null)
        {
            filas = await local.NormalizarAsync(lineas, ct);
            resultado.Advertencias.Add("Se usó la normalización heurística local. Revisá los valores extraídos.");
        }
        resultado.Filas = filas.ToList();
        resultado.Advertencias.Add("PDF: revisá todas las filas. Encabezados, notas y líneas no interpretables pueden no representar productos.");
        return resultado;
    }

    private sealed class CatalogoFormatoException(string mensaje) : FormatException(mensaje);
}
