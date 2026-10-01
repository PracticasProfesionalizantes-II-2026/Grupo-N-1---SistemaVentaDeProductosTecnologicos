using System.Text;

namespace Frontend.Services;

public sealed class ProductoImagenResolver
{
    private static readonly IReadOnlyDictionary<string, string> Imagenes =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Normalizar("Notebook Lenovo Yoga 7 2 en 1")] = "/images/categorias/Notebooks/1compragamer_Imganen_general_45898_Notebook_Lenovo_Yoga_7_2en1_14AH.jpg",
            [Normalizar("Notebook ASUS TUF Gaming A16")] = "/images/categorias/Notebooks/2 compragamer_Imganen_general_56118_Notebook_ASUS_TUF_Gaming_A16.jpg",
            [Normalizar("Notebook HP Victus Gaming 15")] = "/images/categorias/Notebooks/3compragamer_Imganen_general_56094_Notebook_HP_Victus_Gaming_15_15.jpg",
            [Normalizar("Notebook ASUS ROG Strix")] = "/images/categorias/Notebooks/4 compragamer_Imganen_general_56109_Notebook_Gamer_ASUS_ROG_Strix_G1.jpg",
            [Normalizar("Notebook HP OMEN 14")] = "/images/categorias/Notebooks/5compragamer_Imganen_general_56088_Notebook_HP_OMEN_14_14__Intel_Co.jpg",
            [Normalizar("Notebook Lenovo LOQ 15")] = "/images/categorias/Notebooks/6 compragamer_Imganen_general_0_Notebook_Lenovo_LOQ_15AHP10_15.6__AM.jpg",
            [Normalizar("Notebook Acer Nitro V 15")] = "/images/categorias/Notebooks/7compragamer_Imganen_general_52944_Notebook_Acer_Nitro_V_15_15.6__A.jpg",
            [Normalizar("Notebook Samsung Book 4")] = "/images/categorias/Notebooks/8 compragamer_Imganen_general_50306_Notebook_Samsung_Book_4_FHD_15.6.jpg",
            [Normalizar("Auriculares HyperX Cloud Stinger")] = "/images/categorias/Perisfericos/Auriculares/1compragamer_Imganen_general_39287_Auriculares_Hyperx_Cloud_Stinger.jpg",
            [Normalizar("Auriculares inalámbricos HyperX")] = "/images/categorias/Perisfericos/Auriculares/2compragamer_Imganen_general_24258_Auriculares_Hyper.jpg",
            [Normalizar("Auriculares gamer Redragon")] = "/images/categorias/Perisfericos/Auriculares/3compragamer_Imganen_general_36399_Auriculares_Redra.jpg",
            [Normalizar("Auriculares gamer Razer")] = "/images/categorias/Perisfericos/Auriculares/4compragamer_Imganen_general_42454_Auriculares_Razer.jpg",
            [Normalizar("Auriculares inalámbricos JBL")] = "/images/categorias/Perisfericos/Auriculares/5compragamer_Imganen_general_55829_Auriculares_JBL_Q.jpg",
            [Normalizar("Auriculares gamer SteelSeries")] = "/images/categorias/Perisfericos/Auriculares/6compragamer_Imganen_general_50095_Auriculares_Steel.jpg",
            [Normalizar("Teclado inalámbrico Logitech TKL")] = "/images/categorias/Perisfericos/Teclados/1compragamer_Imganen_general_42700_Teclado_Mecanico_.jpg",
            [Normalizar("Teclado mecánico HyperX")] = "/images/categorias/Perisfericos/Teclados/2compragamer_Imganen_general_52725_Teclado_Mecanico_Hyp.jpg",
            [Normalizar("Teclado Logitech G213")] = "/images/categorias/Perisfericos/Teclados/3compragamer_Imganen_general_12000_Teclado_Logitech_G21.jpg",
            [Normalizar("Teclado mecánico Redragon")] = "/images/categorias/Perisfericos/Teclados/4compragamer_Imganen_general_43025_Teclado_Mecanico_Red.jpg",
            [Normalizar("Teclado mecánico Razer")] = "/images/categorias/Perisfericos/Teclados/5compragamer_Imganen_general_47210_Teclado_Mecanico_Raz.jpg",
            [Normalizar("Teclado mecánico ASUS")] = "/images/categorias/Perisfericos/Teclados/6compragamer_Imganen_general_48522_Teclado_Mecanico_ASU.jpg"
        };

    public string? Resolver(string nombre)
    {
        return Imagenes.GetValueOrDefault(Normalizar(nombre));
    }

    private static string Normalizar(string valor)
    {
        return valor.Trim().Normalize(NormalizationForm.FormKC).ToUpperInvariant();
    }
}
