using System.ComponentModel.DataAnnotations;

namespace TiroTime.Infrastructure.Options;

/// <summary>
/// Konfiguration für die JWT-Ausstellung (Abschnitt "Jwt" in appsettings / Umgebungsvariablen Jwt__*).
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32, ErrorMessage = "Jwt:Secret muss mindestens 32 Zeichen lang sein (HS256)")]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 24 * 60)]
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    [Range(1, 365)]
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
