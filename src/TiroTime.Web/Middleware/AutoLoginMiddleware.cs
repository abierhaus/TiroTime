using Microsoft.AspNetCore.Identity;
using TiroTime.Domain.Identity;

namespace TiroTime.Web.Middleware;

public class AutoLoginMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AutoLoginMiddleware> _logger;

    public AutoLoginMiddleware(RequestDelegate next, ILogger<AutoLoginMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IWebHostEnvironment env,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        // Only run in Development environment
        if (!env.IsDevelopment())
        {
            await _next(context);
            return;
        }

        // Only run on localhost
        var host = context.Request.Host.Host;
        if (!IsLocalhost(host))
        {
            await _next(context);
            return;
        }

        // Skip if user is already authenticated
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        try
        {
            // Get all users
            var users = userManager.Users.ToList();

            // Only auto-login if exactly one user exists
            if (users.Count == 1)
            {
                var user = users[0];
                await signInManager.SignInAsync(user, isPersistent: true);
                _logger.LogInformation("Auto-login: Benutzer '{Email}' wurde automatisch angemeldet (Development-Modus)", user.Email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Auto-Login");
        }

        await _next(context);
    }

    private static bool IsLocalhost(string host)
    {
        return host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || host.StartsWith("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || host.StartsWith("::1", StringComparison.OrdinalIgnoreCase);
    }
}
