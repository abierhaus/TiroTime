using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TiroTime.Application.Common;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Entities;
using TiroTime.Domain.Enums;
using TiroTime.Domain.ValueObjects;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Infrastructure.Services;

public class RecurringTimeEntryService : IRecurringTimeEntryService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<RecurringTimeEntry> _recurringRepository;
    private readonly IRepository<TimeEntry> _timeEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecurringTimeEntryService> _logger;

    public RecurringTimeEntryService(
        ApplicationDbContext context,
        IRepository<RecurringTimeEntry> recurringRepository,
        IRepository<TimeEntry> timeEntryRepository,
        IUnitOfWork unitOfWork,
        ILogger<RecurringTimeEntryService> logger)
    {
        _context = context;
        _recurringRepository = recurringRepository;
        _timeEntryRepository = timeEntryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<RecurringTimeEntryDto>> CreateAsync(
        Guid userId,
        CreateRecurringTimeEntryDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify project exists
            var project = await _context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId, cancellationToken);

            if (project == null)
                return Result.Failure<RecurringTimeEntryDto>("Projekt nicht gefunden");

            // Create RecurringPattern value object
            var pattern = RecurringPattern.Create(
                dto.Pattern.Frequency,
                dto.Pattern.Interval,
                dto.Pattern.StartDate,
                dto.Pattern.DaysOfWeek,
                dto.Pattern.DayOfMonth,
                dto.Pattern.EndDate,
                dto.Pattern.MaxOccurrences);

            // Create RecurringTimeEntry entity
            var recurringEntry = RecurringTimeEntry.Create(
                userId,
                dto.ProjectId,
                dto.Title,
                dto.StartTime,
                dto.EndTime,
                pattern,
                dto.Description);

            await _recurringRepository.AddAsync(recurringEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with project
            var createdEntry = await _context.RecurringTimeEntries
                .Include(r => r.Project)
                .ThenInclude(p => p!.Client)
                .FirstAsync(r => r.Id == recurringEntry.Id, cancellationToken);

            _logger.LogInformation(
                "Wiederkehrende Zeiterfassung erstellt: {Title} für Benutzer {UserId}",
                dto.Title, userId);

            return Result.Success(MapToDto(createdEntry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen der wiederkehrenden Zeiterfassung");
            return Result.Failure<RecurringTimeEntryDto>(ex.Message);
        }
    }

    public async Task<Result<RecurringTimeEntryDto>> GetByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var recurringEntry = await _context.RecurringTimeEntries
            .Include(r => r.Project)
            .ThenInclude(p => p!.Client)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);

        if (recurringEntry == null)
            return Result.Failure<RecurringTimeEntryDto>("Wiederkehrende Zeiterfassung nicht gefunden");

        return Result.Success(MapToDto(recurringEntry));
    }

    public async Task<Result<IEnumerable<RecurringTimeEntryDto>>> GetAllAsync(
        Guid userId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.RecurringTimeEntries
                .Include(r => r.Project)
                .ThenInclude(p => p!.Client)
                .Where(r => r.UserId == userId);

            if (!includeInactive)
                query = query.Where(r => r.IsActive);

            var entries = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);

            return Result.Success(entries.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der wiederkehrenden Zeiterfassungen");
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
            var recurringEntry = await _context.RecurringTimeEntries
                .Include(r => r.Project)
                .ThenInclude(p => p!.Client)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);

            if (recurringEntry == null)
                return Result.Failure<RecurringTimeEntryDto>("Wiederkehrende Zeiterfassung nicht gefunden");

            // Create updated pattern
            var pattern = RecurringPattern.Create(
                dto.Pattern.Frequency,
                dto.Pattern.Interval,
                dto.Pattern.StartDate,
                dto.Pattern.DaysOfWeek,
                dto.Pattern.DayOfMonth,
                dto.Pattern.EndDate,
                dto.Pattern.MaxOccurrences);

            // Update entity
            recurringEntry.Update(
                dto.Title,
                dto.StartTime,
                dto.EndTime,
                pattern,
                dto.Description);

            _recurringRepository.Update(recurringEntry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Wiederkehrende Zeiterfassung aktualisiert: {Id}",
                id);

            return Result.Success(MapToDto(recurringEntry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren der wiederkehrenden Zeiterfassung");
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
            var recurringEntry = await _context.RecurringTimeEntries
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);

            if (recurringEntry == null)
                return Result.Failure("Wiederkehrende Zeiterfassung nicht gefunden");

            _recurringRepository.Remove(recurringEntry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Wiederkehrende Zeiterfassung gelöscht: {Id}",
                id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen der wiederkehrenden Zeiterfassung");
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> ActivateAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recurringEntry = await _context.RecurringTimeEntries
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);

            if (recurringEntry == null)
                return Result.Failure("Wiederkehrende Zeiterfassung nicht gefunden");

            recurringEntry.Activate();
            _recurringRepository.Update(recurringEntry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Wiederkehrende Zeiterfassung aktiviert: {Id}",
                id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Aktivieren der wiederkehrenden Zeiterfassung");
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeactivateAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recurringEntry = await _context.RecurringTimeEntries
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);

            if (recurringEntry == null)
                return Result.Failure("Wiederkehrende Zeiterfassung nicht gefunden");

            recurringEntry.Deactivate();
            _recurringRepository.Update(recurringEntry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Wiederkehrende Zeiterfassung deaktiviert: {Id}",
                id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Deaktivieren der wiederkehrenden Zeiterfassung");
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
            var generatedCount = 0;

            // Get all active recurring entries
            var activeRecurringEntries = await _context.RecurringTimeEntries
                .Include(r => r.Project)
                .Where(r => r.IsActive)
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Starte Generierung von Zeiteinträgen für {Date}. {Count} aktive Wiederholungen gefunden.",
                targetDate, activeRecurringEntries.Count);

            foreach (var recurringEntry in activeRecurringEntries)
            {
                // Check if this recurring entry should generate an entry for this date
                var occurrences = recurringEntry.GenerateOccurrences(targetDate, targetDate).ToList();

                if (!occurrences.Any())
                    continue;

                // Check if entry already exists for this date
                var existingEntry = await _context.TimeEntries
                    .AnyAsync(te =>
                        te.RecurringTimeEntryId == recurringEntry.Id &&
                        te.StartTime.Date == targetDate,
                        cancellationToken);

                if (existingEntry)
                {
                    _logger.LogDebug(
                        "Eintrag für {Date} bereits vorhanden (RecurringId: {RecurringId})",
                        targetDate, recurringEntry.Id);
                    continue;
                }

                // Generate time entry
                foreach (var (date, startTime, endTime) in occurrences)
                {
                    var startDateTime = date.Date + startTime;
                    var endDateTime = date.Date + endTime;

                    var timeEntry = TimeEntry.CreateManual(
                        recurringEntry.UserId,
                        recurringEntry.ProjectId,
                        startDateTime,
                        endDateTime,
                        recurringEntry.Description,
                        recurringEntry.Id);

                    await _timeEntryRepository.AddAsync(timeEntry, cancellationToken);
                    generatedCount++;

                    _logger.LogDebug(
                        "Zeiteintrag generiert: {Title} für {Date}",
                        recurringEntry.Title, date);
                }

                // Update LastGeneratedDate
                recurringEntry.MarkAsGenerated(targetDate);
                _recurringRepository.Update(recurringEntry);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Generierung abgeschlossen: {Count} Zeiteinträge für {Date} erstellt",
                generatedCount, targetDate);

            return Result.Success(generatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der Generierung von Zeiteinträgen für {Date}", forDate);
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
            var recurringEntry = await _context.RecurringTimeEntries
                .Include(r => r.Project)
                .ThenInclude(p => p!.Client)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);

            if (recurringEntry == null)
                return Result.Failure<IEnumerable<TimeEntryDto>>("Wiederkehrende Zeiterfassung nicht gefunden");

            var occurrences = recurringEntry.GenerateOccurrences(fromDate.Date, toDate.Date);

            var previewEntries = occurrences.Select(occ =>
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
            }).ToList();

            return Result.Success<IEnumerable<TimeEntryDto>>(previewEntries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der Vorschau der Vorkommen");
            return Result.Failure<IEnumerable<TimeEntryDto>>(ex.Message);
        }
    }

    private static RecurringTimeEntryDto MapToDto(RecurringTimeEntry entry)
    {
        return new RecurringTimeEntryDto(
            entry.Id,
            entry.UserId,
            entry.ProjectId,
            entry.Project?.Name ?? string.Empty,
            entry.Project?.Client?.Name ?? string.Empty,
            entry.Project?.ColorCode,
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
