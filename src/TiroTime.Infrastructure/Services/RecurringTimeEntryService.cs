using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TiroTime.Application.Common;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Entities;
using TiroTime.Domain.ValueObjects;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Infrastructure.Services;

public class RecurringTimeEntryService(
    ApplicationDbContext context,
    IRepository<RecurringTimeEntry> recurringRepository,
    IRepository<TimeEntry> timeEntryRepository,
    IUnitOfWork unitOfWork,
    ILogger<RecurringTimeEntryService> logger) : IRecurringTimeEntryService
{
    public async Task<Result<RecurringTimeEntryDto>> CreateAsync(
        Guid userId,
        CreateRecurringTimeEntryDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await context.Projects
                .AsNoTracking()
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId, cancellationToken);

            if (project == null)
                return Result.Failure<RecurringTimeEntryDto>("Projekt nicht gefunden");

            var pattern = CreatePattern(dto.Pattern);

            var recurringEntry = RecurringTimeEntry.Create(
                userId,
                dto.ProjectId,
                dto.Title,
                dto.StartTime,
                dto.EndTime,
                pattern,
                dto.Description);

            await recurringRepository.AddAsync(recurringEntry, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Wiederkehrende Zeiterfassung erstellt: {Title} für Benutzer {UserId}",
                dto.Title, userId);

            return Result.Success(MapToDto(recurringEntry, project));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler beim Erstellen der wiederkehrenden Zeiterfassung");
            return Result.Failure<RecurringTimeEntryDto>(ex.Message);
        }
    }

    public async Task<Result<RecurringTimeEntryDto>> GetByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var recurringEntry = await context.RecurringTimeEntries
            .AsNoTracking()
            .Include(r => r.Project)
            .ThenInclude(p => p!.Client)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);

        return recurringEntry == null
            ? Result.Failure<RecurringTimeEntryDto>("Wiederkehrende Zeiterfassung nicht gefunden")
            : Result.Success(MapToDto(recurringEntry));
    }

    public async Task<Result<IEnumerable<RecurringTimeEntryDto>>> GetAllAsync(
        Guid userId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = context.RecurringTimeEntries
                .AsNoTracking()
                .Include(r => r.Project)
                .ThenInclude(p => p!.Client)
                .Where(r => r.UserId == userId);

            if (!includeInactive)
                query = query.Where(r => r.IsActive);

            var entries = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);

            return Result.Success<IEnumerable<RecurringTimeEntryDto>>(entries.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler beim Laden der wiederkehrenden Zeiterfassungen");
            return Result.Failure<IEnumerable<RecurringTimeEntryDto>>(ex.Message);
        }
    }

    public async Task<Result<RecurringTimeEntryDto>> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateRecurringTimeEntryDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recurringEntry = await context.RecurringTimeEntries
                .Include(r => r.Project)
                .ThenInclude(p => p!.Client)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);

            if (recurringEntry == null)
                return Result.Failure<RecurringTimeEntryDto>("Wiederkehrende Zeiterfassung nicht gefunden");

            var pattern = CreatePattern(dto.Pattern);

            recurringEntry.Update(
                dto.Title,
                dto.StartTime,
                dto.EndTime,
                pattern,
                dto.Description);

            recurringRepository.Update(recurringEntry);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Wiederkehrende Zeiterfassung aktualisiert: {Id}", id);

            return Result.Success(MapToDto(recurringEntry));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler beim Aktualisieren der wiederkehrenden Zeiterfassung");
            return Result.Failure<RecurringTimeEntryDto>(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recurringEntry = await context.RecurringTimeEntries
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);

            if (recurringEntry == null)
                return Result.Failure("Wiederkehrende Zeiterfassung nicht gefunden");

            recurringRepository.Remove(recurringEntry);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Wiederkehrende Zeiterfassung gelöscht: {Id}", id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler beim Löschen der wiederkehrenden Zeiterfassung");
            return Result.Failure(ex.Message);
        }
    }

    public Task<Result> ActivateAsync(Guid userId, Guid id, CancellationToken cancellationToken = default) =>
        SetActiveAsync(userId, id, activate: true, cancellationToken);

    public Task<Result> DeactivateAsync(Guid userId, Guid id, CancellationToken cancellationToken = default) =>
        SetActiveAsync(userId, id, activate: false, cancellationToken);

    private async Task<Result> SetActiveAsync(Guid userId, Guid id, bool activate, CancellationToken cancellationToken)
    {
        var action = activate ? "aktiviert" : "deaktiviert";

        try
        {
            var recurringEntry = await context.RecurringTimeEntries
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);

            if (recurringEntry == null)
                return Result.Failure("Wiederkehrende Zeiterfassung nicht gefunden");

            if (activate)
                recurringEntry.Activate();
            else
                recurringEntry.Deactivate();

            recurringRepository.Update(recurringEntry);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Wiederkehrende Zeiterfassung {Action}: {Id}", action, id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler beim Ändern des Aktivierungsstatus der wiederkehrenden Zeiterfassung {Id}", id);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<int>> GenerateScheduledEntriesAsync(
        DateTime forDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var targetDate = forDate.Date;
            var nextDay = targetDate.AddDays(1);
            var generatedCount = 0;

            var activeRecurringEntries = await context.RecurringTimeEntries
                .Where(r => r.IsActive)
                .ToListAsync(cancellationToken);

            logger.LogInformation(
                "Starte Generierung von Zeiteinträgen für {Date}. {Count} aktive Wiederholungen gefunden.",
                targetDate, activeRecurringEntries.Count);

            if (activeRecurringEntries.Count == 0)
                return Result.Success(0);

            // Eine Abfrage für alle bereits generierten Einträge des Tages statt einer pro Wiederholung
            var alreadyGenerated = (await context.TimeEntries
                    .AsNoTracking()
                    .Where(te => te.RecurringTimeEntryId != null &&
                                 te.StartTime >= targetDate &&
                                 te.StartTime < nextDay)
                    .Select(te => te.RecurringTimeEntryId!.Value)
                    .Distinct()
                    .ToListAsync(cancellationToken))
                .ToHashSet();

            foreach (var recurringEntry in activeRecurringEntries)
            {
                var occurrences = recurringEntry.GenerateOccurrences(targetDate, targetDate).ToList();

                if (occurrences.Count == 0)
                    continue;

                if (alreadyGenerated.Contains(recurringEntry.Id))
                {
                    logger.LogDebug(
                        "Eintrag für {Date} bereits vorhanden (RecurringId: {RecurringId})",
                        targetDate, recurringEntry.Id);
                    continue;
                }

                foreach (var (date, startTime, endTime) in occurrences)
                {
                    var timeEntry = TimeEntry.CreateManual(
                        recurringEntry.UserId,
                        recurringEntry.ProjectId,
                        date.Date + startTime,
                        date.Date + endTime,
                        recurringEntry.Description,
                        recurringEntry.Id);

                    await timeEntryRepository.AddAsync(timeEntry, cancellationToken);
                    generatedCount++;

                    logger.LogDebug("Zeiteintrag generiert: {Title} für {Date}", recurringEntry.Title, date);
                }

                recurringEntry.MarkAsGenerated(targetDate);
                recurringRepository.Update(recurringEntry);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Generierung abgeschlossen: {Count} Zeiteinträge für {Date} erstellt",
                generatedCount, targetDate);

            return Result.Success(generatedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler bei der Generierung von Zeiteinträgen für {Date}", forDate);
            return Result.Failure<int>(ex.Message);
        }
    }

    public async Task<Result<IEnumerable<TimeEntryDto>>> PreviewOccurrencesAsync(
        Guid userId,
        Guid id,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recurringEntry = await context.RecurringTimeEntries
                .AsNoTracking()
                .Include(r => r.Project)
                .ThenInclude(p => p!.Client)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);

            if (recurringEntry == null)
                return Result.Failure<IEnumerable<TimeEntryDto>>("Wiederkehrende Zeiterfassung nicht gefunden");

            var previewEntries = recurringEntry
                .GenerateOccurrences(fromDate.Date, toDate.Date)
                .Select(occ =>
                {
                    var startDateTime = occ.Date + occ.StartTime;
                    var endDateTime = occ.Date + occ.EndTime;

                    return new TimeEntryDto(
                        Guid.NewGuid(), // Preview only
                        recurringEntry.UserId,
                        recurringEntry.ProjectId,
                        recurringEntry.Project!.Name,
                        recurringEntry.Project.Client!.Name,
                        recurringEntry.Project.ColorCode,
                        recurringEntry.Description,
                        startDateTime,
                        endDateTime,
                        endDateTime - startDateTime,
                        false,
                        DateTime.UtcNow,
                        null);
                })
                .ToList();

            return Result.Success<IEnumerable<TimeEntryDto>>(previewEntries);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler bei der Vorschau der Vorkommen");
            return Result.Failure<IEnumerable<TimeEntryDto>>(ex.Message);
        }
    }

    private static RecurringPattern CreatePattern(RecurringPatternDto dto) =>
        RecurringPattern.Create(
            dto.Frequency,
            dto.Interval,
            dto.StartDate,
            dto.DaysOfWeek,
            dto.DayOfMonth,
            dto.EndDate,
            dto.MaxOccurrences);

    private static RecurringTimeEntryDto MapToDto(RecurringTimeEntry entry) =>
        MapToDto(entry, entry.Project);

    private static RecurringTimeEntryDto MapToDto(RecurringTimeEntry entry, Project? project)
    {
        return new RecurringTimeEntryDto(
            entry.Id,
            entry.UserId,
            entry.ProjectId,
            project?.Name ?? string.Empty,
            project?.Client?.Name ?? string.Empty,
            project?.ColorCode,
            entry.Title,
            entry.Description,
            entry.StartTime,
            entry.EndTime,
            new RecurringPatternDto(
                entry.Pattern.Frequency,
                entry.Pattern.Interval,
                entry.Pattern.DaysOfWeek,
                entry.Pattern.DayOfMonth,
                entry.Pattern.StartDate,
                entry.Pattern.EndDate,
                entry.Pattern.MaxOccurrences),
            entry.IsActive,
            entry.LastGeneratedDate,
            entry.CreatedAt,
            entry.UpdatedAt);
    }
}
