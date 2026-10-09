using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiroTime.Domain.Identity;

namespace TiroTime.Web.Middleware;

/// <summary>
/// Meldet den Benutzer automatisch an, wenn genau ein Benutzer existiert (Single-User-Betrieb).
/// Statische Dateien, API- und Health-Endpunkte werden übersprungen, damit nicht bei jedem
/// anonymen Request eine Datenbankabfrage und ein Cookie-Login ausgelöst werden.
/// </summary>
public class AutoLoginMiddleware(RequestDelegate next, ILogger<AutoLoginMiddleware> logger)
{
    public async Task InvokeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        if (context.User.Identity?.IsAuthenticated == true || ShouldSkip(context.Request.Path))
        {
            await next(context);
            return;
        }

        try
        {
            // TOP 2 reicht, um "genau ein Benutzer" zu erkennen, ohne alle Benutzer zu laden
            var users = await userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .Take(2)
                .ToListAsync(context.RequestAborted);

            if (users.Count == 1)
            {
                var user = users[0];
                await signInManager.SignInAsync(user, isPersistent: true);
                logger.LogInformation("Auto-login: Benutzer '{Email}' wurde automatisch angemeldet", user.Email);
            }
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler beim Auto-Login");
        }

        await next(context);
    }

    private static bool ShouldSkip(PathString path)
    {
        if (path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Statische Dateien (css/js/ico/...) haben eine Dateiendung, Razor Pages nicht
        return Path.HasExtension(path.Value);
    }
}
