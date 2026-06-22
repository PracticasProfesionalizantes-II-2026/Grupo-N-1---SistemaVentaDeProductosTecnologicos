namespace Totaltech.Logica.DTOs
{
    public enum TipoErrorOperacion
    {
        Ninguno = 0,
        BadRequest = 1,
        NotFound = 2,
        Conflict = 3
    }

    public record ResultadoOperacion(bool Exitoso, string? Error = null, TipoErrorOperacion TipoError = TipoErrorOperacion.Ninguno)
    {
        public static ResultadoOperacion Ok()
        {
            return new ResultadoOperacion(true);
        }

        public static ResultadoOperacion BadRequest(string error)
        {
            return new ResultadoOperacion(false, error, TipoErrorOperacion.BadRequest);
        }

        public static ResultadoOperacion NotFound(string error)
        {
            return new ResultadoOperacion(false, error, TipoErrorOperacion.NotFound);
        }

        public static ResultadoOperacion Conflict(string error)
        {
            return new ResultadoOperacion(false, error, TipoErrorOperacion.Conflict);
        }
    }

    public record ResultadoOperacion<T>(bool Exitoso, T? Datos = default, string? Error = null, TipoErrorOperacion TipoError = TipoErrorOperacion.Ninguno)
    {
        public static ResultadoOperacion<T> Ok(T datos)
        {
            return new ResultadoOperacion<T>(true, datos);
        }

        public static ResultadoOperacion<T> BadRequest(string error)
        {
            return new ResultadoOperacion<T>(false, default, error, TipoErrorOperacion.BadRequest);
        }

        public static ResultadoOperacion<T> NotFound(string error)
        {
            return new ResultadoOperacion<T>(false, default, error, TipoErrorOperacion.NotFound);
        }

        public static ResultadoOperacion<T> Conflict(string error)
        {
            return new ResultadoOperacion<T>(false, default, error, TipoErrorOperacion.Conflict);
        }
    }
}
