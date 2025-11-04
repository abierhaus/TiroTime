using Microsoft.EntityFrameworkCore;
using TiroTime.Application.Common;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Entities;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Infrastructure.Services;

public class TimeEntryService : ITimeEntryService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<TimeEntry> _timeEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TimeEntryService(
        ApplicationDbContext context,
        IRepository<TimeEntry> timeEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _timeEntryRepository = timeEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TimeEntryDto>> StartTimerAsync(
        Guid userId,
        StartTimerDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user already has a running timer
            var activeTimer = await _context.TimeEntries
                .FirstOrDefaultAsync(te => te.UserId == userId && te.IsRunning, cancellationToken);

            if (activeTimer != null)
                return Result.Failure<TimeEntryDto>("Es läuft bereits ein Timer. Bitte stoppen Sie diesen zuerst.");

            // Verify project exists
            var project = await _context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId, cancellationToken);

            if (project == null)
                return Result.Failure<TimeEntryDto>("Projekt nicht gefunden");

            var timeEntry = TimeEntry.StartTimer(userId, dto.ProjectId, dto.Description);

            await _timeEntryRepository.AddAsync(timeEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with project
            var createdEntry = await _context.TimeEntries
                .Include(te => te.Project)
                .ThenInclude(p => p!.Client)
                .FirstAsync(te => te.Id == timeEntry.Id, cancellationToken);

            return Result.Success(MapToDto(createdEntry));
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
            var timeEntry = await _context.TimeEntries
                .Include(te => te.Project)
                .ThenInclude(p => p!.Client)
                .FirstOrDefaultAsync(te => te.Id == timeEntryId && te.UserId == userId, cancellationToken);

            if (timeEntry == null)
                return Result.Failure<TimeEntryDto>("Zeiterfassung nicht gefunden");

            timeEntry.StopTimer();
            _timeEntryRepository.Update(timeEntry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        var activeTimer = await _context.TimeEntries
            .Include(te => te.Project)
            .ThenInclude(p => p!.Client)
            .FirstOrDefaultAsync(te => te.UserId == userId && te.IsRunning, cancellationToken);

        if (activeTimer == null)
            return Result.Failure<TimeEntryDto>("Kein aktiver Timer gefunden");

        return Result.Success(MapToDto(activeTimer));
    }

    public async Task<Result<TimeEntryDto>> CreateManualTimeEntryAsync(
        Guid userId,
        CreateManualTimeEntryDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify project exists
            var project = await _context.Projects
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

            await _timeEntryRepository.AddAsync(timeEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with project
            var createdEntry = await _context.TimeEntries
                .Include(te => te.Project)
                .ThenInclude(p => p!.Client)
                .FirstAsync(te => te.Id == timeEntry.Id, cancellationToken);

            return Result.Success(MapToDto(createdEntry));
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
            var timeEntry = await _context.TimeEntries
                .Include(te => te.Project)
                .ThenInclude(p => p!.Client)
                .FirstOrDefaultAsync(te => te.Id == dto.Id && te.UserId == userId, cancellationToken);

            if (timeEntry == null)
                return Result.Failure<TimeEntryDto>("Zeiterfassung nicht gefunden");

            timeEntry.Update(dto.StartTime, dto.EndTime, dto.Description);
            _timeEntryRepository.Update(timeEntry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(MapToDto(timeEntry));
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
        var timeEntry = await _timeEntryRepository
            .FirstOrDefaultAsync(te => te.Id == timeEntryId && te.UserId == userId, cancellationToken);

        if (timeEntry == null)
            return Result.Failure("Zeiterfassung nicht gefunden");

        if (timeEntry.IsRunning)
            return Result.Failure("Laufende Zeiterfassung kann nicht gelöscht werden. Bitte stoppen Sie diese zuerst.");

        _timeEntryRepository.Remove(timeEntry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<TimeEntryDto>> GetTimeEntryByIdAsync(
        Guid userId,
        Guid timeEntryId,
        CancellationToken cancellationToken = default)
    {
        var timeEntry = await _context.TimeEntries
            .Include(te => te.Project)
            .ThenInclude(p => p!.Client)
            .FirstOrDefaultAsync(te => te.Id == timeEntryId && te.UserId == userId, cancellationToken);

        if (timeEntry == null)
            return Result.Failure<TimeEntryDto>("Zeiterfassung nicht gefunden");

        return Result.Success(MapToDto(timeEntry));
    }

    public async Task<Result<IEnumerable<TimeEntryDto>>> GetTimeEntriesByDateRangeAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var timeEntries = await _context.TimeEntries
            .Include(te => te.Project)
            .ThenInclude(p => p!.Client)
            .Where(te => te.UserId == userId &&
                        te.StartTime >= startDate &&
                        te.StartTime <= endDate)
            .OrderByDescending(te => te.StartTime)
            .ToListAsync(cancellationToken);

        var dtos = timeEntries.Select(MapToDto);
        return Result.Success(dtos);
    }

    public async Task<Result<IEnumerable<TimeEntryDto>>> GetTimeEntriesByProjectAsync(
        Guid userId,
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var timeEntries = await _context.TimeEntries
            .Include(te => te.Project)
            .ThenInclude(p => p!.Client)
            .Where(te => te.UserId == userId && te.ProjectId == projectId)
            .OrderByDescending(te => te.StartTime)
            .ToListAsync(cancellationToken);

        var dtos = timeEntries.Select(MapToDto);
        return Result.Success(dtos);
    }

    public async Task<Result<IEnumerable<TimeEntrySummaryDto>>> GetTimeEntriesSummaryByDateRangeAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var timeEntries = await _context.TimeEntries
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
                g.Select(MapToDto).OrderByDescending(te => te.StartTime)))
            .OrderByDescending(s => s.Date)
            .AsEnumerable();

        return Result.Success(summaries);
    }

    public async Task<Result<TimeEntryStatisticsDto>> GetStatisticsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var entries = await _context.TimeEntries
            .Where(te => te.UserId == userId &&
                        !te.IsRunning &&
                        te.StartTime >= monthStart)
            .ToListAsync(cancellationToken);

        var todayEntries = entries.Where(te => te.StartTime >= todayStart).ToList();
        var weekEntries = entries.Where(te => te.StartTime >= weekStart).ToList();
        var monthEntries = entries;

        var statistics = new TimeEntryStatisticsDto(
            TimeSpan.FromTicks(todayEntries.Sum(te => te.Duration.Ticks)),
            TimeSpan.FromTicks(weekEntries.Sum(te => te.Duration.Ticks)),
            TimeSpan.FromTicks(monthEntries.Sum(te => te.Duration.Ticks)),
            todayEntries.Count,
            weekEntries.Count,
            monthEntries.Count);

        return Result.Success(statistics);
    }

    private static TimeEntryDto MapToDto(TimeEntry timeEntry)
    {
        return new TimeEntryDto(
            timeEntry.Id,
            timeEntry.UserId,
            timeEntry.ProjectId,
            timeEntry.Project?.Name ?? "Unknown",
            timeEntry.Project?.Client?.Name ?? "Unknown",
            timeEntry.Project?.ColorCode,
            timeEntry.Description,
            timeEntry.StartTime,
            timeEntry.EndTime,
            timeEntry.GetCurrentDuration(),
            timeEntry.IsRunning,
            timeEntry.CreatedAt,
            timeEntry.UpdatedAt);
    }
}
