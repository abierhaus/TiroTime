using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Identity;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Web.Api;

public record LogTimeRequest(
    string Client,
    string Project,
    DateOnly Date,
    TimeOnly From,
    TimeOnly To,
    string? Description,
    string? UserEmail);

public static class TimeEntriesApi
{
    public static void MapTimeEntriesApi(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/time-entries", LogTimeAsync).AllowAnonymous();
    }

    private static async Task<IResult> LogTimeAsync(
        LogTimeRequest request,
        HttpContext httpContext,
        IConfiguration configuration,
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ITimeEntryService timeEntryService,
        CancellationToken cancellationToken)
    {
        // Optional: wenn TimeLoggingApi:ApiKey konfiguriert ist, muss der Header X-Api-Key passen
        var configuredApiKey = configuration["TimeLoggingApi:ApiKey"];
        if (!string.IsNullOrEmpty(configuredApiKey) &&
            httpContext.Request.Headers["X-Api-Key"] != configuredApiKey)
        {
            return Results.Json(new { error = "Ungültiger API-Key" }, statusCode: StatusCodes.Status401Unauthorized);
        }

        if (string.IsNullOrWhiteSpace(request.Client) || string.IsNullOrWhiteSpace(request.Project))
        {
            return Results.BadRequest(new { error = "Kunde und Projekt sind erforderlich" });
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return Results.BadRequest(new { error = "Beschreibung ist erforderlich" });
        }

        var project = await dbContext.Projects
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p =>
                p.Name == request.Project &&
                p.Client != null &&
                p.Client.Name == request.Client,
                cancellationToken);

        if (project is null)
        {
            var available = await dbContext.Projects
                .Include(p => p.Client)
                .Where(p => p.IsActive)
                .Select(p => new { client = p.Client!.Name, project = p.Name })
                .ToListAsync(cancellationToken);

            return Results.NotFound(new
            {
                error = $"Projekt '{request.Project}' von Kunde '{request.Client}' wurde nicht gefunden",
                availableProjects = available
            });
        }

        var user = await ResolveUserAsync(request.UserEmail, dbContext, userManager, cancellationToken);
        if (user is null)
        {
            return Results.BadRequest(new
            {
                error = "Benutzer konnte nicht ermittelt werden. Bitte 'userEmail' im Request angeben."
            });
        }

        // Gleiche Konvertierung wie im UI (Pages/TimeTracking/Create): lokale Zeit -> UTC, über Mitternacht erlaubt
        var localStart = request.Date.ToDateTime(request.From);
        var localEnd = request.Date.ToDateTime(request.To);
        if (localEnd <= localStart)
        {
            localEnd = localEnd.AddDays(1);
        }

        var startUtc = DateTime.SpecifyKind(localStart, DateTimeKind.Local).ToUniversalTime();
        var endUtc = DateTime.SpecifyKind(localEnd, DateTimeKind.Local).ToUniversalTime();

        var dto = new CreateManualTimeEntryDto(project.Id, startUtc, endUtc, request.Description);
        var result = await timeEntryService.CreateManualTimeEntryAsync(user.Id, dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        var entry = result.Value!;
        return Results.Created($"/api/time-entries/{entry.Id}", new
        {
            id = entry.Id,
            client = entry.ClientName,
            project = entry.ProjectName,
            date = request.Date.ToString("yyyy-MM-dd"),
            from = request.From.ToString("HH:mm"),
            to = request.To.ToString("HH:mm"),
            duration = entry.Duration.ToString(@"hh\:mm"),
            description = entry.Description
        });
    }

    private static async Task<ApplicationUser?> ResolveUserAsync(
        string? userEmail,
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            return await userManager.FindByEmailAsync(userEmail);
        }

        // Wie AutoLoginMiddleware: bei genau einem Benutzer ist die Zuordnung eindeutig
        var users = await dbContext.Users.Take(2).ToListAsync(cancellationToken);
        return users.Count == 1 ? users[0] : null;
    }
}
