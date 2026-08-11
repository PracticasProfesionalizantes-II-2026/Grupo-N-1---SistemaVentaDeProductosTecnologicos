using Microsoft.AspNetCore.Identity;
using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IUsuariosLogica
    {
        Task<List<Usuario>> ObtenerTodosAsync();
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<Usuario?> LoginAsync(LoginDto dto);
        Task<string?> CrearAsync(Usuario usuario);
        Task<string?> RegistrarAsync(Usuario usuario);
        Task<string?> ActualizarAsync(int id, Usuario usuario);
        Task<bool> EliminarAsync(int id);
        Task<bool> RecuperarContrasenaAsync(RecuperarContrasenaDto dto);
    }

    public class UsuariosLogica : IUsuariosLogica
    {
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

        public async Task<Usuario?> LoginAsync(LoginDto dto)
        {
            var usuario = await _repositorio.ObtenerPorEmailAsync(dto.Email);
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

            if (resultadoHash == PasswordVerificationResult.Success || resultadoHash == PasswordVerificationResult.SuccessRehashNeeded)
            {
                return usuario;
            }

            if (usuario.Contrasena == dto.Contrasena)
            {
                usuario.Contrasena = _passwordHasher.HashPassword(usuario, dto.Contrasena);
                await _repositorio.ActualizarAsync(usuario);
                return usuario;
            }

            return null;
        }

        public Task<string?> RegistrarAsync(Usuario usuario)
        {
            return CrearAsync(usuario);
        }

        public async Task<string?> CrearAsync(Usuario usuario)
        {
            var error = ValidarUsuario(usuario, necesitaContrasena: true);
            if (error is not null)
            {
                return error;
            }

            var existente = await _repositorio.ObtenerPorEmailAsync(usuario.Email);
            if (existente is not null)
            {
                return "Ya existe un usuario registrado con ese email.";
            }

            if (usuario.FechaRegistro == default)
            {
                usuario.FechaRegistro = DateTime.Now;
            }

            usuario.Contrasena = _passwordHasher.HashPassword(usuario, usuario.Contrasena);
            await _repositorio.CrearAsync(usuario);
            return null;
        }

        public async Task<string?> ActualizarAsync(int id, Usuario usuario)
        {
            var existente = await _repositorio.ObtenerPorIdAsync(id);
            if (existente is null)
            {
                return "El usuario indicado no existe.";
            }

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
            var usuario = await _repositorio.ObtenerPorEmailAsync(dto.Email);
            return usuario is not null;
        }

        private static string? ValidarUsuario(Usuario usuario, bool necesitaContrasena)
        {
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
