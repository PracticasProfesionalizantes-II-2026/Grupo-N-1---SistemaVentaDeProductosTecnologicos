using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Frontend.Services;

public static class CatalogoImportacionNormalizador
{
    public static string Espacios(string? valor) => Regex.Replace(valor ?? "", @"\s+", " ").Trim();

    public static string Clave(string? valor) => new(Espacios(valor).Normalize(NormalizationForm.FormD)
        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
        .Select(char.ToUpperInvariant).ToArray());

    public static bool IntentarPrecio(string? texto, out decimal valor)
    {
        valor = 0;
        var numero = Espacios(texto).Replace("ARS", "", StringComparison.OrdinalIgnoreCase).Replace("$", "").Trim();
        if (!Regex.IsMatch(numero, @"^\d+(?:[.,]\d+)*$")) return false;
        var punto = numero.LastIndexOf('.');
        var coma = numero.LastIndexOf(',');
        if (punto >= 0 && coma >= 0)
        {
            var decimalSeparador = punto > coma ? '.' : ',';
            var milesSeparador = decimalSeparador == '.' ? ',' : '.';
            var patron = @"^\d{1,3}(?:" + Regex.Escape(milesSeparador.ToString()) + @"\d{3})+"
                + Regex.Escape(decimalSeparador.ToString()) + @"\d{1,2}$";
            if (!Regex.IsMatch(numero, patron)) return false;
            numero = numero.Replace(milesSeparador.ToString(), "").Replace(decimalSeparador, '.');
        }
        else if (punto >= 0 || coma >= 0)
        {
            // Un único separador con tres dígitos finales puede significar miles o decimales.
            var separador = punto >= 0 ? '.' : ',';
            if (numero.Count(c => c == separador) != 1 || numero.Length - Math.Max(punto, coma) - 1 is < 1 or > 2)
                return false;
            numero = numero.Replace(separador, '.');
        }
        return decimal.TryParse(numero, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out valor)
            && valor <= 9999999999999999.99m;
    }

    public static bool IntentarStock(string? texto, out int stock) => int.TryParse(Espacios(texto),
        NumberStyles.None, CultureInfo.InvariantCulture, out stock) && stock >= 0;

    public static string SugerirCategoria(string nombre)
    {
        var clave = Clave(nombre);
        var candidatos = new[]
        {
            ("Notebooks", new[] { "NOTEBOOK", "LAPTOP", "PORTATIL" }),
            ("Celulares", new[] { "CELULAR", "SMARTPHONE", "IPHONE" }),
            ("Almacenamiento", new[] { "SSD", "DISCO", "PENDRIVE", "MICROSD" }),
            ("Gabinetes", new[] { "GABINETE", "CHASIS" }),
            ("Periféricos", new[] { "TECLADO", "MOUSE", "AURICULAR", "MONITOR" }),
            ("Placas de Video", new[] { "GEFORCE", "RADEON", "TARJETA GRAFICA", "PLACA DE VIDEO" })
        }.Where(x => x.Item2.Any(p => Regex.IsMatch(clave, @"\b" + p + @"\b"))).ToList();
        return candidatos.Count == 1 ? candidatos[0].Item1 : "";
    }
}
