using Microsoft.EntityFrameworkCore;
using TiroTime.Application.Common;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Entities;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Infrastructure.Services;

public class TimeEntryService(
    ApplicationDbContext context,
    IRepository<TimeEntry> timeEntryRepository,
    IUnitOfWork unitOfWork) : ITimeEntryService
{
    public async Task<Result<TimeEntryDto>> StartTimerAsync(
        Guid userId,
        StartTimerDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var hasActiveTimer = await context.TimeEntries
                .AnyAsync(te => te.UserId == userId && te.IsRunning, cancellationToken);

            if (hasActiveTimer)
                return Result.Failure<TimeEntryDto>("Es läuft bereits ein Timer. Bitte stoppen Sie diesen zuerst.");

            var project = await context.Projects
                .AsNoTracking()
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId, cancellationToken);

            if (project == null)
                return Result.Failure<TimeEntryDto>("Projekt nicht gefunden");

            var timeEntry = TimeEntry.StartTimer(userId, dto.ProjectId, dto.Description);

            await timeEntryRepository.AddAsync(timeEntry, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Projekt und Kunde sind bereits geladen – kein erneutes Nachladen nötig
            return Result.Success(MapToDto(timeEntry, project));
        }
        catch (Exception ex)
        {
            return Result.Failure<TimeEntryDto>(ex.Message);
        }
    }

    public async Task<Result<TimeEntryDto>> StopTimerAsync(
        Guid userId,
        Guid timeEntryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var timeEntry = await context.TimeEntries
                .Include(te => te.Project)
                .ThenInclude(p => p!.Client)
                .FirstOrDefaultAsync(te => te.Id == timeEntryId && te.UserId == userId, cancellationToken);

            if (timeEntry == null)
                return Result.Failure<TimeEntryDto>("Zeiterfassung nicht gefunden");

            timeEntry.StopTimer();
            timeEntryRepository.Update(timeEntry);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(MapToDto(timeEntry));
        }
        catch (Exception ex)
        {
            return Result.Failure<TimeEntryDto>(ex.Message);
        }
    }

    public async Task<Result<TimeEntryDto>> GetActiveTimerAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var activeTimer = await context.TimeEntries
            .AsNoTracking()
            .Include(te => te.Project)
            .ThenInclude(p => p!.Client)
            .FirstOrDefaultAsync(te => te.UserId == userId && te.IsRunning, cancellationToken);

        return activeTimer == null
            ? Result.Failure<TimeEntryDto>("Kein aktiver Timer gefunden")
            : Result.Success(MapToDto(activeTimer));
    }

    public async Task<Result<TimeEntryDto>> CreateManualTimeEntryAsync(
        Guid userId,
        CreateManualTimeEntryDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await context.Projects
                .AsNoTracking()
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId, cancellationToken);

            if (project == null)
                return Result.Failure<TimeEntryDto>("Projekt nicht gefunden");

            var timeEntry = TimeEntry.CreateManual(
                userId,
                dto.ProjectId,
                dto.StartTime,
                dto.EndTime,
                dto.Description);

            await timeEntryRepository.AddAsync(timeEntry, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(MapToDto(timeEntry, project));
        }
        catch (Exception ex)
        {
            return Result.Failure<TimeEntryDto>(ex.Message);
        }
    }

    public async Task<Result<TimeEntryDto>> UpdateTimeEntryAsync(
        Guid userId,
        UpdateTimeEntryDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var timeEntry = await context.TimeEntries
                .Include(te => te.Project)
                .ThenInclude(p => p!.Client)
                .FirstOrDefaultAsync(te => te.Id == dto.Id && te.UserId == userId, cancellationToken);

            if (timeEntry == null)
                return Result.Failure<TimeEntryDto>("Zeiterfassung nicht gefunden");

            timeEntry.Update(dto.StartTime, dto.EndTime, dto.Description);
            timeEntryRepository.Update(timeEntry);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(MapToDto(timeEntry));
        }
        catch (Exception ex)
        {
            return Result.Failure<TimeEntryDto>(ex.Message);
        }
    }

    public async Task<Result<TimeEntryDto>> UpdateTimeEntryProjectAsync(
        Guid userId,
        Guid timeEntryId,
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var timeEntry = await context.TimeEntries
                .FirstOrDefaultAsync(te => te.Id == timeEntryId && te.UserId == userId, cancellationToken);

            if (timeEntry == null)
                return Result.Failure<TimeEntryDto>("Zeiterfassung nicht gefunden");

            var project = await context.Projects
                .AsNoTracking()
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

            if (project == null)
                return Result.Failure<TimeEntryDto>("Projekt nicht gefunden");

            timeEntry.ChangeProject(projectId);
            timeEntryRepository.Update(timeEntry);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(MapToDto(timeEntry, project));
        }
        catch (Exception ex)
        {
            return Result.Failure<TimeEntryDto>(ex.Message);
        }
    }

    public async Task<Result> DeleteTimeEntryAsync(
        Guid userId,
        Guid timeEntryId,
        CancellationToken cancellationToken = default)
    {
        var timeEntry = await timeEntryRepository
            .FirstOrDefaultAsync(te => te.Id == timeEntryId && te.UserId == userId, cancellationToken);

        if (timeEntry == null)
            return Result.Failure("Zeiterfassung nicht gefunden");

        if (timeEntry.IsRunning)
            return Result.Failure("Laufende Zeiterfassung kann nicht gelöscht werden. Bitte stoppen Sie diese zuerst.");

        timeEntryRepository.Remove(timeEntry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<TimeEntryDto>> GetTimeEntryByIdAsync(
        Guid userId,
        Guid timeEntryId,
        CancellationToken cancellationToken = default)
    {
        var timeEntry = await context.TimeEntries
            .AsNoTracking()
            .Include(te => te.Project)
            .ThenInclude(p => p!.Client)
            .FirstOrDefaultAsync(te => te.Id == timeEntryId && te.UserId == userId, cancellationToken);

        return timeEntry == null
            ? Result.Failure<TimeEntryDto>("Zeiterfassung nicht gefunden")
            : Result.Success(MapToDto(timeEntry));
    }

    public async Task<Result<IEnumerable<TimeEntryDto>>> GetTimeEntriesByDateRangeAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // endDate ist exklusiv (z. B. erster Tag des Folgemonats), damit Einträge nicht in zwei Zeiträumen auftauchen
        var timeEntries = await context.TimeEntries
            .AsNoTracking()
            .Include(te => te.Project)
            .ThenInclude(p => p!.Client)
            .Where(te => te.UserId == userId &&
                        te.StartTime >= startDate &&
                        te.StartTime < endDate)
            .OrderByDescending(te => te.StartTime)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<TimeEntryDto>>(timeEntries.Select(MapToDto).ToList());
    }

    public async Task<Result<IEnumerable<TimeEntryDto>>> GetTimeEntriesByProjectAsync(
        Guid userId,
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var timeEntries = await context.TimeEntries
            .AsNoTracking()
            .Include(te => te.Project)
            .ThenInclude(p => p!.Client)
            .Where(te => te.UserId == userId && te.ProjectId == projectId)
            .OrderByDescending(te => te.StartTime)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<TimeEntryDto>>(timeEntries.Select(MapToDto).ToList());
    }

    public async Task<Result<IEnumerable<TimeEntrySummaryDto>>> GetTimeEntriesSummaryByDateRangeAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var timeEntries = await context.TimeEntries
            .AsNoTracking()
            .Include(te => te.Project)
            .ThenInclude(p => p!.Client)
            .Where(te => te.UserId == userId &&
                        te.StartTime >= startDate &&
                        te.StartTime <= endDate &&
                        !te.IsRunning)
            .OrderByDescending(te => te.StartTime)
            .ToListAsync(cancellationToken);

        var summaries = timeEntries
            .GroupBy(te => te.StartTime.Date)
            .Select(g => new TimeEntrySummaryDto(
                g.Key,
                TimeSpan.FromTicks(g.Sum(te => te.Duration.Ticks)),
                g.Count(),
                g.Select(MapToDto).OrderByDescending(te => te.StartTime).ToList()))
            .OrderByDescending(s => s.Date)
            .ToList();

        return Result.Success<IEnumerable<TimeEntrySummaryDto>>(summaries);
    }

    public async Task<Result<TimeEntryStatisticsDto>> GetStatisticsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var periodStart = weekStart < monthStart ? weekStart : monthStart;

        // Nur die beiden benötigten Spalten laden statt kompletter Entities
        var entries = await context.TimeEntries
            .AsNoTracking()
            .Where(te => te.UserId == userId &&
                        !te.IsRunning &&
                        te.StartTime >= periodStart)
            .Select(te => new { te.StartTime, te.Duration })
            .ToListAsync(cancellationToken);

        var todayEntries = entries.Where(te => te.StartTime >= todayStart).ToList();
        var weekEntries = entries.Where(te => te.StartTime >= weekStart).ToList();
        var monthEntries = entries.Where(te => te.StartTime >= monthStart).ToList();

        var statistics = new TimeEntryStatisticsDto(
            TimeSpan.FromTicks(todayEntries.Sum(te => te.Duration.Ticks)),
            TimeSpan.FromTicks(weekEntries.Sum(te => te.Duration.Ticks)),
            TimeSpan.FromTicks(monthEntries.Sum(te => te.Duration.Ticks)),
            todayEntries.Count,
            weekEntries.Count,
            monthEntries.Count);

        return Result.Success(statistics);
    }

    private static TimeEntryDto MapToDto(TimeEntry timeEntry) =>
        MapToDto(timeEntry, timeEntry.Project);

    private static TimeEntryDto MapToDto(TimeEntry timeEntry, Project? project)
    {
        return new TimeEntryDto(
            timeEntry.Id,
            timeEntry.UserId,
            timeEntry.ProjectId,
            project?.Name ?? "Unknown",
            project?.Client?.Name ?? "Unknown",
            project?.ColorCode,
            timeEntry.Description,
            timeEntry.StartTime,
            timeEntry.EndTime,
            timeEntry.GetCurrentDuration(),
            timeEntry.IsRunning,
            timeEntry.CreatedAt,
            timeEntry.UpdatedAt);
    }
}
