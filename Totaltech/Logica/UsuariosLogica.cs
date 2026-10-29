using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Observabilidad;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IUsuariosLogica
    {
        Task<List<Usuario>> ObtenerTodosAsync();
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteEmailAsync(string email);
        Task<Usuario?> LoginAsync(LoginDto dto);
        Task AsegurarAdministradorAsync(string email, string contrasena);
        Task<string?> CrearAsync(Usuario usuario);
        Task<string?> RegistrarAsync(Usuario usuario);
        Task<Usuario?> AutenticarAsync(LoginDto dto);
        Task<ResultadoUsuario> ActualizarAsync(int id, UsuarioActualizacionRequest request, int idActor, bool esAdministrador);
        Task<ResultadoUsuario> CambiarEstadoAsync(int id, bool activo, int idActor);
        Task<bool> RecuperarContrasenaAsync(RecuperarContrasenaDto dto);
    }

    public class UsuariosLogica : IUsuariosLogica
    {
        private const string ErrorEmailDuplicado = "Ya existe un usuario registrado con ese email.";
        private readonly IUsuariosRepositorio _repositorio;
        private readonly MetricasNegocio? _metricas;
        private readonly PasswordHasher<Usuario> _passwordHasher = new();

        public UsuariosLogica(IUsuariosRepositorio repositorio, MetricasNegocio? metricas = null)
        {
            _repositorio = repositorio;
            _metricas = metricas;
        }

        public Task<List<Usuario>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return await _repositorio.ObtenerPorEmailAsync(NormalizarEmail(email)) is not null;
        }

        public async Task<Usuario?> LoginAsync(LoginDto dto)
        {
            var usuario = await AutenticarAsync(dto);
            return usuario is { Activo: true } ? usuario : null;
        }

        public async Task<Usuario?> AutenticarAsync(LoginDto dto)
        {
            var usuario = await _repositorio.ObtenerPorEmailAsync(NormalizarEmail(dto.Email));
            if (usuario is null)
            {
                return null;
            }

            var resultadoHash = PasswordVerificationResult.Failed;
            try
            {
                resultadoHash = _passwordHasher.VerifyHashedPassword(usuario, usuario.Contrasena, dto.Contrasena);
            }
            catch (FormatException)
            {
                resultadoHash = PasswordVerificationResult.Failed;
            }

            if (resultadoHash == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.Contrasena = _passwordHasher.HashPassword(usuario, dto.Contrasena);
                await _repositorio.ActualizarContrasenaAsync(usuario);
                return usuario;
            }

            return resultadoHash == PasswordVerificationResult.Success
                ? usuario
                : null;
        }

        public async Task AsegurarAdministradorAsync(string email, string contrasena)
        {
            var emailNormalizado = NormalizarEmail(email);
            var administrador = await _repositorio.ObtenerPorEmailAsync(emailNormalizado);

            if (administrador is null)
            {
                var nuevoAdministrador = new Usuario
                {
                    Nombre = "Administrador",
                    Apellido = "TotalTech",
                    Email = emailNormalizado,
                    Contrasena = contrasena,
                    Telefono = "1122334455",
                    FechaRegistro = DateTime.UtcNow,
                    Rol = RolUsuario.Administrador
                };

                var error = await CrearAsync(nuevoAdministrador);
                if (error is null)
                {
                    return;
                }

                if (error != ErrorEmailDuplicado)
                {
                    throw new InvalidOperationException(error);
                }

                administrador = await _repositorio.ObtenerPorEmailAsync(emailNormalizado)
                    ?? throw new InvalidOperationException(
                        "La cuenta administrativa no pudo recuperarse después de una creación concurrente.");
            }

            // El bootstrap sólo crea cuentas. Una cuenta existente conserva las
            // decisiones administrativas, incluso cuando está desactivada.
        }

        public Task<string?> RegistrarAsync(Usuario usuario)
        {
            usuario.Activo = true;
            usuario.VersionSesion = 1;
            usuario.Rol = RolUsuario.Cliente;
            usuario.FechaRegistro = DateTime.UtcNow;
            return CrearAsync(usuario);
        }

        public async Task<string?> CrearAsync(Usuario usuario)
        {
            usuario.Email = NormalizarEmail(usuario.Email);

            var error = ValidarUsuario(usuario, necesitaContrasena: true);
            if (error is not null)
            {
                return error;
            }

            var existente = await _repositorio.ObtenerPorEmailAsync(usuario.Email);
            if (existente is not null)
            {
                return ErrorEmailDuplicado;
            }

            if (usuario.FechaRegistro == default)
            {
                usuario.FechaRegistro = DateTime.Now;
            }

            usuario.Contrasena = _passwordHasher.HashPassword(usuario, usuario.Contrasena);

            try
            {
                await _repositorio.CrearAsync(usuario);
            }
            catch (DbUpdateException)
            {
                if (await _repositorio.ObtenerPorEmailAsync(usuario.Email) is not null)
                {
                    return ErrorEmailDuplicado;
                }

                throw;
            }

            return null;
        }

        public Task<ResultadoUsuario> ActualizarAsync(
            int id, UsuarioActualizacionRequest request, int idActor, bool esAdministrador) =>
            MedirCambioAsync("update", () => EjecutarCambioAsync(async () =>
            {
                var existente = await _repositorio.ObtenerPorIdAsync(id);
                if (existente is null || (!esAdministrador && idActor != id))
                    return new(EstadoOperacionUsuario.NoEncontrado);

                var nuevo = new Usuario
                {
                    Nombre = request.Nombre?.Trim() ?? string.Empty,
                    Apellido = request.Apellido?.Trim() ?? string.Empty,
                    Email = NormalizarEmail(request.Email),
                    Telefono = request.Telefono?.Trim() ?? string.Empty,
                    Rol = esAdministrador ? request.Rol : existente.Rol
                };
                var error = ValidarUsuario(nuevo, necesitaContrasena: false);
                if (error is not null) return new(EstadoOperacionUsuario.Invalido, error);

                var duplicado = await _repositorio.ObtenerPorEmailAsync(nuevo.Email);
                if (duplicado is not null && duplicado.IdUsuario != id)
                    return new(EstadoOperacionUsuario.Conflicto, ErrorEmailDuplicado);

                if (existente.Activo && existente.Rol == RolUsuario.Administrador &&
                    nuevo.Rol != RolUsuario.Administrador &&
                    await _repositorio.ContarAdministradoresActivosAsync() <= 1)
                    return UltimoAdministrador();

                var campos = new List<string>();
                if (existente.Nombre != nuevo.Nombre) campos.Add("nombre");
                if (existente.Apellido != nuevo.Apellido) campos.Add("apellido");
                if (existente.Email != nuevo.Email) campos.Add("email");
                if (existente.Telefono != nuevo.Telefono) campos.Add("telefono");
                if (existente.Rol != nuevo.Rol) campos.Add("rol");
                if (campos.Count == 0) return new(EstadoOperacionUsuario.Exito, Usuario: existente);

                var auditoria = CrearAuditoria(idActor, id, "Modificacion", campos);
                if (existente.Rol != nuevo.Rol)
                {
                    auditoria.RolAnterior = existente.Rol;
                    auditoria.RolNuevo = nuevo.Rol;
                    existente.VersionSesion++;
                }
                existente.Nombre = nuevo.Nombre;
                existente.Apellido = nuevo.Apellido;
                existente.Email = nuevo.Email;
                existente.Telefono = nuevo.Telefono;
                existente.Rol = nuevo.Rol;
                await _repositorio.GuardarCambioAsync(existente, auditoria);
                return new(EstadoOperacionUsuario.Exito, Usuario: existente);
            }));

        public Task<ResultadoUsuario> CambiarEstadoAsync(int id, bool activo, int idActor) =>
            MedirCambioAsync(activo ? "reactivate" : "deactivate", () => EjecutarCambioAsync(async () =>
            {
                var usuario = await _repositorio.ObtenerPorIdAsync(id);
                if (usuario is null) return new(EstadoOperacionUsuario.NoEncontrado);
                if (usuario.Activo == activo) return new(EstadoOperacionUsuario.Exito, Usuario: usuario);
                if (!activo && usuario.Rol == RolUsuario.Administrador &&
                    await _repositorio.ContarAdministradoresActivosAsync() <= 1)
                    return UltimoAdministrador();

                var auditoria = CrearAuditoria(idActor, id, activo ? "Reactivacion" : "Baja", ["activo"]);
                auditoria.ActivoAnterior = usuario.Activo;
                auditoria.ActivoNuevo = activo;
                usuario.Activo = activo;
                usuario.VersionSesion++;
                await _repositorio.GuardarCambioAsync(usuario, auditoria);
                return new(EstadoOperacionUsuario.Exito, Usuario: usuario);
            }));

        private Task<ResultadoUsuario> MedirCambioAsync(string operacion, Func<Task<ResultadoUsuario>> ejecutar) =>
            _metricas is null ? ejecutar() : _metricas.MedirUsuarioAsync(operacion, ejecutar);

        private async Task<ResultadoUsuario> EjecutarCambioAsync(Func<Task<ResultadoUsuario>> operacion)
        {
            try
            {
                return await _repositorio.EjecutarTransaccionAsync(operacion);
            }
            catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sql &&
                                              sql.Number is 2601 or 2627)
            {
                return new(EstadoOperacionUsuario.Conflicto, ErrorEmailDuplicado);
            }
            catch (Microsoft.EntityFrameworkCore.Storage.RetryLimitExceededException ex)
                when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sql && sql.Number is 1205 or 3960)
            {
                return new(EstadoOperacionUsuario.Conflicto, "La operación encontró un conflicto concurrente. Volvé a intentarlo.");
            }
        }

        private static ResultadoUsuario UltimoAdministrador() =>
            new(EstadoOperacionUsuario.Conflicto, "Debe permanecer al menos un administrador activo.");

        private static AuditoriaUsuario CrearAuditoria(int actor, int usuario, string accion, List<string> campos) =>
            new()
            {
                IdActor = actor, IdUsuario = usuario, Accion = accion,
                FechaUtc = DateTime.UtcNow, CamposModificados = string.Join(",", campos)
            };

        public async Task<bool> RecuperarContrasenaAsync(RecuperarContrasenaDto dto)
        {
            var usuario = await _repositorio.ObtenerPorEmailAsync(NormalizarEmail(dto.Email));
            return usuario is not null;
        }

        private static string NormalizarEmail(string email)
        {
            return email?.Trim() ?? string.Empty;
        }

        private static string? ValidarUsuario(Usuario usuario, bool necesitaContrasena)
        {
            if (!Enum.IsDefined(usuario.Rol))
            {
                return "El rol del usuario no es valido.";
            }

            if (string.IsNullOrWhiteSpace(usuario.Nombre) || string.IsNullOrWhiteSpace(usuario.Apellido))
            {
                return "El nombre y el apellido son obligatorios.";
            }

            if (string.IsNullOrWhiteSpace(usuario.Email))
            {
                return "El email es obligatorio.";
            }

            if (usuario.Email.Length > 256 || !new EmailAddressAttribute().IsValid(usuario.Email))
            {
                return "El formato del email no es valido.";
            }

            if (necesitaContrasena && string.IsNullOrWhiteSpace(usuario.Contrasena))
            {
                return "La contrasena es obligatoria.";
            }

            if (necesitaContrasena && usuario.Contrasena.Length < 8)
            {
                return "La contrasena debe tener al menos 8 caracteres.";
            }

            if (string.IsNullOrWhiteSpace(usuario.Telefono))
            {
                return "El telefono es obligatorio.";
            }

            return null;
        }
    }
}
