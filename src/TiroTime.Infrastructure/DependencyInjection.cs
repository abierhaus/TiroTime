using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TiroTime.Application.Interfaces;
using TiroTime.Infrastructure.Persistence;
using TiroTime.Infrastructure.Services;

namespace TiroTime.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITimeEntryService, TimeEntryService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IRecurringTimeEntryService, RecurringTimeEntryService>();
        services.AddScoped<IEmailService, EmailService>();

        // Background Services
        services.AddHostedService<RecurringEntryGenerationService>();

        return services;
    }
}
