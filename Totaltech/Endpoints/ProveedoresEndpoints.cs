using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class ProveedoresEndpoints
    {
        public static void MapProveedoresEndpoints(this WebApplication app)
        {
            app.MapCrud<Proveedor, IProveedoresLogica>("/proveedores", "Proveedores", proveedor => proveedor.IdProveedor);
        }
    }
}
