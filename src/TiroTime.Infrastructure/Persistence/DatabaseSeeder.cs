using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TiroTime.Domain.Identity;

namespace TiroTime.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    private static readonly string[] Roles = ["Admin", "Manager", "User"];

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

            await SeedRolesAsync(roleManager, logger);

            // Benutzer-Seeding ist bewusst deaktiviert (Registrierung über die Oberfläche):
            // await SeedAdminUserAsync(services.GetRequiredService<UserManager<ApplicationUser>>(), logger);
            // await SeedStandardUserAsync(services.GetRequiredService<UserManager<ApplicationUser>>(), services.GetRequiredService<IConfiguration>(), logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ein Fehler ist beim Seeden der Datenbank aufgetreten.");
        }
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        foreach (var roleName in Roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var role = new ApplicationRole(roleName)
            {
                Description = roleName switch
                {
                    "Admin" => "Systemadministrator mit vollen Rechten",
                    "Manager" => "Manager mit erweiterten Rechten",
                    "User" => "Normaler Benutzer",
                    _ => null
                }
            };

            var result = await roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                logger.LogInformation("Rolle '{RoleName}' wurde erstellt.", roleName);
            }
            else
            {
                logger.LogError("Fehler beim Erstellen der Rolle '{RoleName}': {Errors}",
                    roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    // Wird derzeit nicht aufgerufen – bleibt als Vorlage für ein optionales Admin-Seeding erhalten.
    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        const string adminEmail = "admin@tirotime.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser != null)
        {
            return;
        }

        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };

        // Default admin password - CHANGE THIS IN PRODUCTION!
        var result = await userManager.CreateAsync(adminUser, "Admin123!@#$");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Admin-Benutzer wurde erstellt: {Email}", adminEmail);
            logger.LogWarning("WICHTIG: Ändern Sie das Standardpasswort des Admin-Benutzers!");
        }
        else
        {
            logger.LogError("Fehler beim Erstellen des Admin-Benutzers: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    // Wird derzeit nicht aufgerufen – Standardbenutzer aus SeedUsers:StandardUser:* (User Secrets / Umgebungsvariablen).
    private static async Task SeedStandardUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var email = configuration["SeedUsers:StandardUser:Email"];
        var firstName = configuration["SeedUsers:StandardUser:FirstName"];
        var lastName = configuration["SeedUsers:StandardUser:LastName"];
        var password = configuration["SeedUsers:StandardUser:Password"];

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName) ||
            string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(password))
        {
            logger.LogInformation("Standard-Benutzer-Daten nicht in Konfiguration gefunden. Überspringe Seeding.");
            return;
        }

        var standardUser = await userManager.FindByEmailAsync(email);
        if (standardUser != null)
        {
            return;
        }

        standardUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            Status = UserStatus.Active
        };

        var result = await userManager.CreateAsync(standardUser, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(standardUser, "User");
            logger.LogInformation("Standard-Benutzer wurde erstellt: {Email}", email);
        }
        else
        {
            logger.LogError("Fehler beim Erstellen des Standard-Benutzers: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
