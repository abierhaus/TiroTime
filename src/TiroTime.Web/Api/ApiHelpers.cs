using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiroTime.Domain.Identity;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Web.Api;

/// <summary>
/// Gemeinsame Bausteine der Minimal-API-Endpunkte (API-Key-Prüfung, Benutzerauflösung).
/// </summary>
internal static class ApiHelpers
{
    public const string ApiKeyHeader = "X-Api-Key";

    /// <summary>
    /// Vergleicht den übergebenen API-Key in konstanter Zeit mit dem konfigurierten Wert.
    /// </summary>
    public static bool HasValidApiKey(HttpRequest request, string configuredApiKey)
    {
        var provided = request.Headers[ApiKeyHeader].ToString();
        if (string.IsNullOrEmpty(provided))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(configuredApiKey));
    }

    public static IResult Unauthorized() =>
        Results.Json(new { error = "Ungültiger API-Key" }, statusCode: StatusCodes.Status401Unauthorized);

    /// <summary>
    /// Ermittelt den Benutzer per E-Mail oder – wie die AutoLoginMiddleware – eindeutig, wenn genau ein Benutzer existiert.
    /// </summary>
    public static async Task<ApplicationUser?> ResolveUserAsync(
        string? userEmail,
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            return await userManager.FindByEmailAsync(userEmail);
        }

        var users = await dbContext.Users.AsNoTracking().Take(2).ToListAsync(cancellationToken);
        return users.Count == 1 ? users[0] : null;
    }
}
