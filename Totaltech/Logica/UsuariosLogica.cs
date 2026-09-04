using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
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
        Task<string?> ActualizarAsync(int id, Usuario usuario);
        Task<bool> EliminarAsync(int id);
        Task<bool> RecuperarContrasenaAsync(RecuperarContrasenaDto dto);
    }

    public class UsuariosLogica : IUsuariosLogica
    {
        private const string ErrorEmailDuplicado = "Ya existe un usuario registrado con ese email.";
        private readonly IUsuariosRepositorio _repositorio;
        private readonly PasswordHasher<Usuario> _passwordHasher = new();

        public UsuariosLogica(IUsuariosRepositorio repositorio)
        {
            _repositorio = repositorio;
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
                await _repositorio.ActualizarAsync(usuario);
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

            var requiereActualizacion = false;

            if (!string.Equals(administrador.Email, emailNormalizado, StringComparison.Ordinal))
            {
                administrador.Email = emailNormalizado;
                requiereActualizacion = true;
            }

            if (administrador.Rol != RolUsuario.Administrador)
            {
                administrador.Rol = RolUsuario.Administrador;
                requiereActualizacion = true;
            }

            if (administrador.Contrasena == contrasena)
            {
                administrador.Contrasena = _passwordHasher.HashPassword(administrador, contrasena);
                requiereActualizacion = true;
            }

            if (requiereActualizacion)
            {
                await _repositorio.ActualizarAsync(administrador);
            }
        }

        public Task<string?> RegistrarAsync(Usuario usuario)
        {
            usuario.Rol = RolUsuario.Cliente;
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

        public async Task<string?> ActualizarAsync(int id, Usuario usuario)
        {
            var existente = await _repositorio.ObtenerPorIdAsync(id);
            if (existente is null)
            {
                return "El usuario indicado no existe.";
            }

            usuario.Email = NormalizarEmail(usuario.Email);

            var error = ValidarUsuario(usuario, necesitaContrasena: false);
            if (error is not null)
            {
                return error;
            }

            var usuarioConEmail = await _repositorio.ObtenerPorEmailAsync(usuario.Email);
            if (usuarioConEmail is not null && usuarioConEmail.IdUsuario != id)
            {
                return "Ya existe otro usuario registrado con ese email.";
            }

            existente.Nombre = usuario.Nombre;
            existente.Apellido = usuario.Apellido;
            existente.Email = usuario.Email;
            existente.Telefono = usuario.Telefono;
            existente.Rol = usuario.Rol;
            existente.FechaRegistro = usuario.FechaRegistro == default ? existente.FechaRegistro : usuario.FechaRegistro;

            if (!string.IsNullOrWhiteSpace(usuario.Contrasena))
            {
                existente.Contrasena = _passwordHasher.HashPassword(existente, usuario.Contrasena);
            }

            await _repositorio.ActualizarAsync(existente);
            return null;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var usuario = await _repositorio.ObtenerPorIdAsync(id);
            if (usuario is null)
            {
                return false;
            }

            await _repositorio.EliminarAsync(usuario);
            return true;
        }

        public async Task<bool> RecuperarContrasenaAsync(RecuperarContrasenaDto dto)
        {
            var usuario = await _repositorio.ObtenerPorEmailAsync(NormalizarEmail(dto.Email));
            return usuario is not null;
        }

        private static string NormalizarEmail(string email)
        {
            return email.Trim();
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

            if (necesitaContrasena && string.IsNullOrWhiteSpace(usuario.Contrasena))
            {
                return "La contrasena es obligatoria.";
            }

            return null;
        }
    }
}
