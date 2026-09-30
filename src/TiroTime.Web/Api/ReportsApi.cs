using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Identity;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Web.Api;

public static class ReportsApi
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static void MapReportsApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports").AllowAnonymous();
        group.MapGet("/summary", GetSummaryAsync);
        group.MapGet("/timesheet", GetTimesheetAsync);
    }

    // GET /api/reports/summary?client=Nomos&month=2026-09[&userEmail=...]
    private static async Task<IResult> GetSummaryAsync(
        string client,
        string month,
        string? userEmail,
        HttpContext httpContext,
        IConfiguration configuration,
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        var (error, userId, dto, clientName) = await PrepareAsync(
            client, month, userEmail, httpContext, configuration, dbContext, userManager, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        var result = await reportService.GetReportSummaryAsync(userId, dto!, cancellationToken);
        if (!result.IsSuccess)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        var summary = result.Value!;
        return Results.Ok(new
        {
            client = clientName,
            month,
            startDate = dto!.StartDate.ToString("yyyy-MM-dd"),
            endDate = dto.EndDate.ToString("yyyy-MM-dd"),
            totalEntries = summary.TotalEntries,
            totalHours = Math.Round(summary.TotalDuration.TotalHours, 2),
            totalAmount = summary.TotalAmount,
            projects = summary.ProjectSummaries
                .OrderBy(p => p.ProjectName)
                .Select(p => new
                {
                    project = p.ProjectName,
                    entries = p.EntryCount,
                    hours = Math.Round(p.TotalDuration.TotalHours, 2),
                    hourlyRate = p.TotalDuration.TotalHours > 0
                        ? Math.Round(p.TotalAmount / (decimal)p.TotalDuration.TotalHours, 2)
                        : 0,
                    amount = p.TotalAmount,
                    currency = p.Currency
                })
        });
    }

    // GET /api/reports/timesheet?client=Nomos&month=2026-09&format=xlsx|pdf[&userEmail=...]
    private static async Task<IResult> GetTimesheetAsync(
        string client,
        string month,
        string? format,
        string? userEmail,
        HttpContext httpContext,
        IConfiguration configuration,
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IReportService reportService,
        CancellationToken cancellationToken)
    {
        var (error, userId, dto, _) = await PrepareAsync(
            client, month, userEmail, httpContext, configuration, dbContext, userManager, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        var result = (format ?? "xlsx").ToLowerInvariant() switch
        {
            "xlsx" => await reportService.ExportDetailedEntriesToExcelAsync(userId, dto!, cancellationToken),
            "pdf" => await reportService.ExportDetailedEntriesToPdfAsync(userId, dto!, cancellationToken),
            _ => null
        };

        if (result is null)
        {
            return Results.BadRequest(new { error = "format muss 'xlsx' oder 'pdf' sein" });
        }

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        var (data, fileName) = result.Value;
        var contentType = fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? "application/pdf" : ExcelContentType;
        return Results.File(data, contentType, fileName);
    }

    private static async Task<(IResult? Error, Guid UserId, GenerateReportDto? Dto, string? ClientName)> PrepareAsync(
        string client,
        string month,
        string? userEmail,
        HttpContext httpContext,
        IConfiguration configuration,
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        // Pflicht: ohne konfigurierten ReportsApi:ApiKey bleibt die API geschlossen
        var configuredApiKey = configuration["ReportsApi:ApiKey"];
        if (string.IsNullOrEmpty(configuredApiKey) ||
            httpContext.Request.Headers["X-Api-Key"] != configuredApiKey)
        {
            return (Results.Json(new { error = "Ungültiger API-Key" }, statusCode: StatusCodes.Status401Unauthorized), default, null, null);
        }

        if (!DateOnly.TryParseExact(month + "-01", "yyyy-MM-dd", out var firstDay))
        {
            return (Results.BadRequest(new { error = "month muss im Format yyyy-MM angegeben werden" }), default, null, null);
        }

        var clientEntity = await dbContext.Clients
            .FirstOrDefaultAsync(c => c.Name == client, cancellationToken);
        if (clientEntity is null)
        {
            var available = await dbContext.Clients.Select(c => c.Name).ToListAsync(cancellationToken);
            return (Results.NotFound(new { error = $"Kunde '{client}' wurde nicht gefunden", availableClients = available }), default, null, null);
        }

        var user = await ResolveUserAsync(userEmail, dbContext, userManager, cancellationToken);
        if (user is null)
        {
            return (Results.BadRequest(new { error = "Benutzer konnte nicht ermittelt werden. Bitte 'userEmail' angeben." }), default, null, null);
        }

        // EndDate ist der letzte Tag des Monats; der ReportService rechnet selbst + 1 Tag
        var start = firstDay.ToDateTime(TimeOnly.MinValue);
        var end = firstDay.AddMonths(1).AddDays(-1).ToDateTime(TimeOnly.MinValue);
        var dto = new GenerateReportDto(start, end, ClientId: clientEntity.Id);

        return (null, user.Id, dto, clientEntity.Name);
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

        var users = await dbContext.Users.Take(2).ToListAsync(cancellationToken);
        return users.Count == 1 ? users[0] : null;
    }
}
