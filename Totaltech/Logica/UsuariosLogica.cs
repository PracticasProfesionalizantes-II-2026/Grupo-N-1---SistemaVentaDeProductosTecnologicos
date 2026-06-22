using Microsoft.AspNetCore.Identity;
using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IUsuariosLogica : ILogica<Usuario>
    {
        Task<Usuario?> LoginAsync(LoginDto dto);
        Task<ResultadoOperacion<Usuario>> RegistrarAsync(Usuario usuario);
        Task<bool> RecuperarContrasenaAsync(RecuperarContrasenaDto dto);
    }

    public class UsuariosLogica : Logica<Usuario>, IUsuariosLogica
    {
        private readonly IUsuariosRepositorio _repositorio;
        private readonly PasswordHasher<Usuario> _passwordHasher = new();

        public UsuariosLogica(IUsuariosRepositorio repositorio) : base(repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<Usuario?> LoginAsync(LoginDto dto)
        {
            var usuario = await _repositorio.ObtenerPorEmailAsync(dto.Email);

            if (usuario is null)
            {
                return null;
            }

            // La contrasena se verifica contra hash y nunca debe devolverse en respuestas publicas.
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

        public Task<ResultadoOperacion<Usuario>> RegistrarAsync(Usuario usuario)
        {
            return CrearValidadoAsync(usuario);
        }

        public async Task<bool> RecuperarContrasenaAsync(RecuperarContrasenaDto dto)
        {
            var usuario = await _repositorio.ObtenerPorEmailAsync(dto.Email);
            return usuario is not null;
        }

        public override async Task<ResultadoOperacion<Usuario>> CrearValidadoAsync(Usuario usuario)
        {
            var validacion = await ValidarUsuarioAsync(usuario, null);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Usuario>.BadRequest(validacion.Error ?? "El usuario no es valido.");
            }

            var existente = await _repositorio.ObtenerPorEmailAsync(usuario.Email);

            if (existente is not null)
            {
                return ResultadoOperacion<Usuario>.Conflict("Ya existe un usuario registrado con ese email.");
            }

            if (usuario.FechaRegistro == default)
            {
                usuario.FechaRegistro = DateTime.Now;
            }

            usuario.Contrasena = _passwordHasher.HashPassword(usuario, usuario.Contrasena);
            await _repositorio.CrearAsync(usuario);
            return ResultadoOperacion<Usuario>.Ok(usuario);
        }

        public override async Task<ResultadoOperacion<Usuario>> ActualizarValidadoAsync(int id, Usuario usuario)
        {
            var existente = await _repositorio.ObtenerPorIdAsync(id);

            if (existente is null)
            {
                return ResultadoOperacion<Usuario>.NotFound("El usuario indicado no existe.");
            }

            var validacion = await ValidarUsuarioAsync(usuario, id);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Usuario>.BadRequest(validacion.Error ?? "El usuario no es valido.");
            }

            var usuarioConEmail = await _repositorio.ObtenerPorEmailAsync(usuario.Email);

            if (usuarioConEmail is not null && usuarioConEmail.IdUsuario != id)
            {
                return ResultadoOperacion<Usuario>.Conflict("Ya existe otro usuario registrado con ese email.");
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
            return ResultadoOperacion<Usuario>.Ok(existente);
        }

        private Task<ResultadoOperacion> ValidarUsuarioAsync(Usuario usuario, int? idActual)
        {
            if (string.IsNullOrWhiteSpace(usuario.Nombre) || string.IsNullOrWhiteSpace(usuario.Apellido))
            {
                return Task.FromResult(ResultadoOperacion.BadRequest("El nombre y el apellido son obligatorios."));
            }

            if (string.IsNullOrWhiteSpace(usuario.Email))
            {
                return Task.FromResult(ResultadoOperacion.BadRequest("El email es obligatorio."));
            }

            if (!idActual.HasValue && string.IsNullOrWhiteSpace(usuario.Contrasena))
            {
                return Task.FromResult(ResultadoOperacion.BadRequest("La contrasena es obligatoria."));
            }

            return Task.FromResult(ResultadoOperacion.Ok());
        }
    }
}
