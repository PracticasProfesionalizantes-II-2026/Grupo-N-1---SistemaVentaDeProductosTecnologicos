using System.IO.Compression;
using System.Text;
using Frontend.Models.ViewModels.Productos;
using Frontend.Services;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace Frontend.UnitTests;

public sealed class CatalogoArchivoExtractorTests
{
    private static CatalogoArchivoExtractor Extractor(ICatalogoExtractorAgent? agent = null) => new(agent ?? new CatalogoExtractorHeuristico());
    private static IFormFile Archivo(string texto, string nombre = "catalogo.csv") => Archivo(Encoding.UTF8.GetBytes(texto), nombre);
    private static IFormFile Archivo(byte[] bytes, string nombre) => new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Archivo", nombre);

    [Theory]
    [InlineData(';', "1234,56")]
    [InlineData(',', "1234.56")]
    public async Task CsvDetectaSeparadorYEncabezadosConTildes(char sep, string precio)
    {
        var texto = $"\uFEFFNómbre{sep}Descripción{sep}Precio{sep}Stock{sep}Categoria\r\nNotebook{sep}Equipo{sep}\"{precio}\"{sep}5{sep}Notebooks\r\n";
        var resultado = await Extractor().ExtraerAsync(Archivo(texto));
        Assert.Empty(resultado.Errores);
        var fila = Assert.Single(resultado.Filas);
        Assert.Equal("Notebook", fila.Nombre);
        Assert.Equal("Notebooks", fila.CategoriaNombre);
        Assert.True(CatalogoImportacionNormalizador.IntentarPrecio(fila.Precio, out var valor));
        Assert.Equal(1234.56m, valor);
    }

    [Fact]
    public async Task CsvConservaComillasEscapadasSeparadoresYSaltosDentroDeCampos()
    {
        var csv = """"
            Nombre;Descripcion;Precio;Stock
            "Mouse; ""Pro""";"Dos
            líneas";10.50;0
            """";
        var resultado = await Extractor().ExtraerAsync(Archivo(csv));
        Assert.Empty(resultado.Errores);
        var fila = Assert.Single(resultado.Filas);
        Assert.Equal("Mouse; \"Pro\"", fila.Nombre);
        Assert.Equal("Dos líneas", fila.Descripcion);
        Assert.Equal("Periféricos", fila.CategoriaNombre);
    }

    [Theory]
    [InlineData("Nombre;Precio;Stock\nNotebook;no-numérico;\n")]
    [InlineData("Nombre;Precio;Stock\nNotebook;1.234;-1\n")]
    public async Task CsvConservaValoresInvalidosParaSuCorreccion(string texto)
    {
        var resultado = await Extractor().ExtraerAsync(Archivo(texto));
        Assert.Empty(resultado.Errores);
        var fila = Assert.Single(resultado.Filas);
        Assert.False(CatalogoImportacionNormalizador.IntentarPrecio(fila.Precio, out _));
        Assert.False(CatalogoImportacionNormalizador.IntentarStock(fila.Stock, out _));
    }

    [Theory]
    [InlineData("Nombre;Precio\nNotebook;2")]
    [InlineData("Nombre;Precio;Stock\n\"Sin cerrar;2;4")]
    [InlineData("Nombre;Precio;Stock;Stock\nNotebook;2;4;4")]
    [InlineData("Nombre;Precio;Stock\nNotebook;2;4;extra")]
    [InlineData("%PDF-1.7 contenido incompatible")]
    public async Task CsvRechazaEstructuraInvalida(string texto) =>
        Assert.NotEmpty((await Extractor().ExtraerAsync(Archivo(texto))).Errores);

    [Theory]
    [InlineData("0", "0")]
    [InlineData("1234,56", "1234.56")]
    [InlineData("1.234,56", "1234.56")]
    [InlineData("1,234.56", "1234.56")]
    [InlineData("ARS $ 99,90", "99.90")]
    [InlineData("9999999999999999.99", "9999999999999999.99")]
    public void PrecioAdmiteSoloImportesRepresentables(string texto, string esperado)
    {
        Assert.True(CatalogoImportacionNormalizador.IntentarPrecio(texto, out var valor));
        Assert.Equal(decimal.Parse(esperado, System.Globalization.CultureInfo.InvariantCulture), valor);
    }

    [Theory]
    [InlineData("1.234")][InlineData("1,234")][InlineData("1,234,567")]
    [InlineData("-1")][InlineData("1.9999")][InlineData("10000000000000000")]
    [InlineData("")][InlineData("USD 5")][InlineData("1e3")]
    public void PrecioRechazaAmbiguedadNegativosYDesbordes(string texto) =>
        Assert.False(CatalogoImportacionNormalizador.IntentarPrecio(texto, out _));

    [Fact]
    public async Task XlsxLeeSharedStringsRichTextInlineYNumerosConColumnasAusentes()
    {
        var strings = "<sst xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><si><r><t>Note</t></r><r><t>book</t></r></si></sst>";
        var fila = "<row r=\"8\"><c r=\"A8\" t=\"s\"><v>0</v></c><c r=\"C8\"><v>1234.56</v></c><c r=\"D8\"><v>5</v></c></row>";
        var resultado = await Extractor().ExtraerAsync(Archivo(Xlsx(fila, strings), "catalogo.xlsx"));
        Assert.Empty(resultado.Errores);
        var producto = Assert.Single(resultado.Filas);
        Assert.Equal("Notebook", producto.Nombre);
        Assert.Equal("", producto.Descripcion);
        Assert.Equal("1234.56", producto.Precio);
        Assert.Equal("XLSX, fila 8", producto.Origen);
    }

    [Fact]
    public async Task XlsxNoEvaluaFormulasNiUsaValorCacheado()
    {
        var resultado = await Extractor().ExtraerAsync(Archivo(Xlsx("<row><c r=\"A2\" t=\"inlineStr\"><is><t>Notebook</t></is></c><c r=\"C2\"><f>2+3</f><v>5</v></c><c r=\"D2\"><v>0</v></c></row>"), "catalogo.xlsx"));
        Assert.Empty(resultado.Errores);
        Assert.StartsWith("=FÓRMULA", Assert.Single(resultado.Filas).Precio);
    }

    [Theory]
    [InlineData("no es un zip")]
    [InlineData("")]
    public async Task XlsxDanadoDevuelveErrorSeguro(string contenido) =>
        Assert.NotEmpty((await Extractor().ExtraerAsync(Archivo(contenido, "catalogo.xlsx"))).Errores);

    [Fact]
    public async Task XlsxRechazaExpansionXmlExcesivaSinTruncar()
    {
        var bytes = Xlsx("<!--" + new string('x', (int)CatalogoArchivoExtractor.MaximoXmlBytes) + "-->");
        Assert.True(bytes.Length < CatalogoArchivoExtractor.MaximoBytes);
        var resultado = await Extractor().ExtraerAsync(Archivo(bytes, "bomba.xlsx"));
        Assert.Contains(resultado.Errores, e => e.Contains("20 MiB"));
        Assert.Empty(resultado.Filas);
    }

    [Fact]
    public async Task XlsxRechazaDtdYSharedStringFueraDeRango()
    {
        var resultado = await Extractor().ExtraerAsync(Archivo(Xlsx("<row><c r=\"A2\" t=\"s\"><v>5</v></c></row>"), "catalogo.xlsx"));
        Assert.NotEmpty(resultado.Errores);
        var dtd = "<!DOCTYPE sst [<!ENTITY secreto SYSTEM 'file:///no-leer'>]><sst>&secreto;</sst>";
        Assert.NotEmpty((await Extractor().ExtraerAsync(Archivo(Xlsx("", dtd), "catalogo.xlsx"))).Errores);
    }

    [Fact]
    public async Task PdfRealExtraeVariasPaginasYNormalizaDeterministicamente()
    {
        var bytes = Pdf("Notebook | Equipo portatil | 1234.56 | 5 | Notebooks", "Mouse | Inalambrico | 12.50 | 0 | Perifericos");
        var a = await Extractor().ExtraerAsync(Archivo(bytes, "catalogo.pdf"));
        var b = await Extractor().ExtraerAsync(Archivo(bytes, "catalogo.pdf"));
        Assert.Empty(a.Errores);
        Assert.Equal(2, a.Filas.Count);
        Assert.Equal("Notebook", a.Filas[0].Nombre);
        Assert.Contains("página 2", a.Filas[1].Origen);
        Assert.Equal(a.Filas.Select(f => f.Nombre + f.Precio), b.Filas.Select(f => f.Nombre + f.Precio));
    }

    [Theory]
    [InlineData("Nombre: Notebook; Descripcion: Equipo; Precio: 10.50; Stock: 5; Categoria: Notebooks")]
    [InlineData("Notebook $10.50 5")]
    public async Task PdfAdmiteCamposRotuladosYMarcaMonetaria(string texto)
    {
        var resultado = await Extractor().ExtraerAsync(Archivo(Pdf(texto), "catalogo.pdf"));
        Assert.Empty(resultado.Errores);
        Assert.Equal("Notebook", Assert.Single(resultado.Filas).Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Documento sin filas interpretables")]
    public async Task PdfSinTextoOProductosNoInventaFilas(string texto)
    {
        var resultado = await Extractor().ExtraerAsync(Archivo(Pdf(texto), "catalogo.pdf"));
        Assert.NotEmpty(resultado.Errores);
        Assert.Empty(resultado.Filas);
    }

    [Fact]
    public async Task PdfRechazaFirmaFalsaDanadoYPaginasExcesivas()
    {
        Assert.NotEmpty((await Extractor().ExtraerAsync(Archivo("Nombre;Precio;Stock", "falso.pdf"))).Errores);
        Assert.NotEmpty((await Extractor().ExtraerAsync(Archivo("%PDF-1.7 inválido", "danado.pdf"))).Errores);
        var resultado = await Extractor().ExtraerAsync(Archivo(Pdf(Enumerable.Repeat("Notebook $1.00 1", 101).ToArray()), "largo.pdf"));
        Assert.Contains(resultado.Errores, e => e.Contains("100 páginas"));
    }

    [Theory]
    [InlineData("fallo")][InlineData("invalido")][InlineData("vacio")][InlineData("timeout")]
    public async Task PdfUsaFallbackSiElAgenteFallaOEntregaResultadoInvalido(string modo)
    {
        var agent = new Agente((lineas, _) => modo switch
        {
            "fallo" => throw new HttpRequestException("detalle privado"),
            "timeout" => throw new TaskCanceledException(),
            "invalido" => Task.FromResult<IReadOnlyList<ProductoImportacionFilaViewModel>>([new() { Nombre = "inventado", Precio = "no", Origen = "inexistente" }]),
            _ => Task.FromResult<IReadOnlyList<ProductoImportacionFilaViewModel>>([])
        });
        var resultado = await Extractor(agent).ExtraerAsync(Archivo(Pdf("Notebook $10.50 5"), "catalogo.pdf"));
        Assert.Empty(resultado.Errores);
        Assert.Equal("Notebook", Assert.Single(resultado.Filas).Nombre);
        Assert.Contains(resultado.Advertencias, e => e.Contains("heurística local"));
    }

    [Fact]
    public async Task PdfAceptaAgenteInyectadoYPropagaSuCancelacion()
    {
        var agente = new Agente((lineas, _) => Task.FromResult<IReadOnlyList<ProductoImportacionFilaViewModel>>([
            new() { Nombre = "Normalizado", Precio = "15.00", Stock = "2", Origen = lineas[0].Origen }]));
        var resultado = await Extractor(agente).ExtraerAsync(Archivo(Pdf("Notebook $10.50 5"), "catalogo.pdf"));
        Assert.Equal("Normalizado", Assert.Single(resultado.Filas).Nombre);
        using var cts = new CancellationTokenSource();
        var cancelado = new Agente((_, ct) => { cts.Cancel(); ct.ThrowIfCancellationRequested(); throw new Exception(); });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Extractor(cancelado)
            .ExtraerAsync(Archivo(Pdf("Notebook $10.50 5"), "catalogo.pdf"), cts.Token));
    }

    [Fact]
    public async Task LimiteRealDetectaLongitudFalsaYLeeSoloUnByteExtra()
    {
        var stream = new StreamNoPosicionable(Enumerable.Repeat((byte)'x', (int)CatalogoArchivoExtractor.MaximoBytes + 100).ToArray());
        var resultado = await Extractor().ExtraerAsync(new ArchivoDeclarado(stream, 1));
        Assert.Contains(resultado.Errores, e => e.Contains("5 MB"));
        Assert.Equal(CatalogoArchivoExtractor.MaximoBytes + 1, stream.Leidos);
    }

    [Fact]
    public async Task AdmiteExactamenteCincoMiBYStreamNoPosicionable()
    {
        var encabezado = "Nombre;Precio;Stock\nNotebook;1.00;1\n";
        var texto = encabezado + new string('\n', (int)CatalogoArchivoExtractor.MaximoBytes - Encoding.UTF8.GetByteCount(encabezado));
        var stream = new StreamNoPosicionable(Encoding.UTF8.GetBytes(texto));
        var resultado = await Extractor().ExtraerAsync(new ArchivoDeclarado(stream, CatalogoArchivoExtractor.MaximoBytes));
        Assert.Empty(resultado.Errores);
        Assert.Single(resultado.Filas);
    }

    [Fact]
    public async Task RechazaVacioExtensionAjenaTamanoDeclaradoYMilUnaFilas()
    {
        Assert.NotEmpty((await Extractor().ExtraerAsync(Archivo(""))).Errores);
        Assert.NotEmpty((await Extractor().ExtraerAsync(Archivo("algo", "ejecutable.exe"))).Errores);
        Assert.NotEmpty((await Extractor().ExtraerAsync(new ArchivoDeclarado(new MemoryStream(), CatalogoArchivoExtractor.MaximoBytes + 1))).Errores);
        var texto = "Nombre;Precio;Stock\n" + string.Concat(Enumerable.Repeat("Notebook;1;1\n", 1001));
        var resultado = await Extractor().ExtraerAsync(Archivo(texto));
        Assert.NotEmpty(resultado.Errores);
        Assert.Empty(resultado.Filas);
    }

    [Fact]
    public async Task PropagaCancelacionAntesYDuranteLaLectura()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Extractor().ExtraerAsync(Archivo("a"), cts.Token));
        var stream = new StreamCancelado();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Extractor().ExtraerAsync(new ArchivoDeclarado(stream, 1), stream.Cts.Token));
    }

    internal static byte[] Pdf(params string[] paginas)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        foreach (var texto in paginas)
        {
            var pagina = builder.AddPage(PageSize.A4);
            if (texto.Length > 0) pagina.AddText(texto, 9, new PdfPoint(20, 700), font);
        }
        return builder.Build();
    }

    private static byte[] Xlsx(string filas, string? shared = null)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            void Agregar(string nombre, string contenido)
            {
                using var writer = new StreamWriter(zip.CreateEntry(nombre).Open(), new UTF8Encoding(false));
                writer.Write(contenido);
            }
            if (shared is not null) Agregar("xl/sharedStrings.xml", shared);
            var header = "<row r=\"1\">" + string.Concat(new[] { "Nombre", "Descripcion", "Precio", "Stock" }.Select((c, i) =>
                $"<c r=\"{(char)('A' + i)}1\" t=\"inlineStr\"><is><t>{c}</t></is></c>")) + "</row>";
            Agregar("xl/worksheets/sheet1.xml", "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>" + header + filas + "</sheetData></worksheet>");
        }
        return ms.ToArray();
    }

    private sealed class Agente(Func<IReadOnlyList<CatalogoLineaExtraida>, CancellationToken, Task<IReadOnlyList<ProductoImportacionFilaViewModel>>> fn) : ICatalogoExtractorAgent
    {
        public Task<IReadOnlyList<ProductoImportacionFilaViewModel>> NormalizarAsync(IReadOnlyList<CatalogoLineaExtraida> lineas, CancellationToken cancellationToken = default) => fn(lineas, cancellationToken);
    }

    private sealed class ArchivoDeclarado(Stream stream, long longitud) : IFormFile
    {
        public string ContentType => "application/octet-stream";
        public string ContentDisposition => "";
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public long Length => longitud;
        public string Name => "Archivo";
        public string FileName => "catalogo.csv";
        public Stream OpenReadStream() => stream;
        public void CopyTo(Stream target) => stream.CopyTo(target);
        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) => stream.CopyToAsync(target, cancellationToken);
    }

    private sealed class StreamNoPosicionable(byte[] datos) : Stream
    {
        private readonly MemoryStream _inner = new(datos);
        public long Leidos { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        { var n = await _inner.ReadAsync(buffer, cancellationToken); Leidos += n; return n; }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { if (disposing) _inner.Dispose(); base.Dispose(disposing); }
    }

    private sealed class StreamCancelado : MemoryStream
    {
        public CancellationTokenSource Cts { get; } = new();
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        { Cts.Cancel(); cancellationToken.ThrowIfCancellationRequested(); return ValueTask.FromResult(0); }
    }
}
