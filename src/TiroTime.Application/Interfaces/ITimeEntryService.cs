using TiroTime.Application.Common;
using TiroTime.Application.DTOs;

namespace TiroTime.Application.Interfaces;

public interface ITimeEntryService
{
    // Timer operations
    Task<Result<TimeEntryDto>> StartTimerAsync(Guid userId, StartTimerDto dto, CancellationToken cancellationToken = default);
    Task<Result<TimeEntryDto>> StopTimerAsync(Guid userId, Guid timeEntryId, CancellationToken cancellationToken = default);
    Task<Result<TimeEntryDto>> GetActiveTimerAsync(Guid userId, CancellationToken cancellationToken = default);

    // Manual time entry operations
    Task<Result<TimeEntryDto>> CreateManualTimeEntryAsync(Guid userId, CreateManualTimeEntryDto dto, CancellationToken cancellationToken = default);
    Task<Result<TimeEntryDto>> UpdateTimeEntryAsync(Guid userId, UpdateTimeEntryDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteTimeEntryAsync(Guid userId, Guid timeEntryId, CancellationToken cancellationToken = default);

    // Query operations
    Task<Result<TimeEntryDto>> GetTimeEntryByIdAsync(Guid userId, Guid timeEntryId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TimeEntryDto>>> GetTimeEntriesByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TimeEntryDto>>> GetTimeEntriesByProjectAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TimeEntrySummaryDto>>> GetTimeEntriesSummaryByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    // Statistics
    Task<Result<TimeEntryStatisticsDto>> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken = default);
}
