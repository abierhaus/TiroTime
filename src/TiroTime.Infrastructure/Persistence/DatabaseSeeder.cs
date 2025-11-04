using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TiroTime.Domain.Identity;

namespace TiroTime.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
            var configuration = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

            await SeedRolesAsync(roleManager, logger);
            await SeedAdminUserAsync(userManager, logger);
            await SeedStandardUserAsync(userManager, configuration, logger);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
            logger.LogError(ex, "Ein Fehler ist beim Seeden der Datenbank aufgetreten.");
        }
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        string[] roles = { "Admin", "Manager", "User" };

        foreach (var roleName in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
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
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var adminEmail = "admin@tirotime.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
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
    }

    private static async Task SeedStandardUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        // Load user data from configuration (user secrets in development)
        var email = configuration["SeedUsers:StandardUser:Email"];
        var firstName = configuration["SeedUsers:StandardUser:FirstName"];
        var lastName = configuration["SeedUsers:StandardUser:LastName"];
        var password = configuration["SeedUsers:StandardUser:Password"];

        // Only seed if all required data is present
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName) ||
            string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(password))
        {
            logger.LogInformation("Standard-Benutzer-Daten nicht in Konfiguration gefunden. Überspringe Seeding.");
            return;
        }

        var standardUser = await userManager.FindByEmailAsync(email);

        if (standardUser == null)
        {
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
}
