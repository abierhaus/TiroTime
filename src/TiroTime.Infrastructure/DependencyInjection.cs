using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using TiroTime.Application.Interfaces;
using TiroTime.Infrastructure.Options;
using TiroTime.Infrastructure.Persistence;
using TiroTime.Infrastructure.Services;

namespace TiroTime.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database: DbContext-Pooling spart die Kosten der Context-Erzeugung pro Request,
        // EnableRetryOnFailure fängt transiente SQL-Server-Fehler (z. B. Container-Start) ab.
        services.AddDbContextPool<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                }));

        // Options
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<MailjetOptions>()
            .Bind(configuration.GetSection(MailjetOptions.SectionName));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITimeEntryService, TimeEntryService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IRecurringTimeEntryService, RecurringTimeEntryService>();
        services.AddHttpClient<IEmailService, EmailService>();

        // Background Services
        services.AddHostedService<RecurringEntryGenerationService>();

        // QuestPDF-Lizenz einmalig beim Start setzen statt bei jedem Export
        QuestPDF.Settings.License = LicenseType.Community;

        return services;
    }
}
