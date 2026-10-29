using Totaltech.Entidades;

namespace Totaltech.Logica.DTOs;

public enum EstadoOperacionUsuario { Exito, Invalido, NoEncontrado, Conflicto }
public sealed record ResultadoUsuario(EstadoOperacionUsuario Estado, string? Error = null, Usuario? Usuario = null);
