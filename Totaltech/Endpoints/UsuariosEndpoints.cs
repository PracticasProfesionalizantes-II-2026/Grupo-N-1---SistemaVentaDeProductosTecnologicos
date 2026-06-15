using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class UsuariosEndpoints
    {
        public static void MapUsuariosEndpoints(this WebApplication app)
        {
            app.MapCrud<Usuario, IUsuariosLogica>("/usuarios", "Usuarios", usuario => usuario.IdUsuario);
        }
    }
}
