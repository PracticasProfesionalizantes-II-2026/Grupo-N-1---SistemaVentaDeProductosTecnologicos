using System.ComponentModel.DataAnnotations;

namespace Totaltech.Seguridad;

public sealed class JwtOptions
{
    public const string Seccion = "Authentication";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required, MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int ExpirationMinutes { get; init; } = 480;
}
