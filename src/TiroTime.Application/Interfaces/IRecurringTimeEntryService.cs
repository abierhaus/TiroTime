using TiroTime.Application.Common;
using TiroTime.Application.DTOs;

namespace TiroTime.Application.Interfaces;

public interface IRecurringTimeEntryService
{
    // CRUD Operations
    Task<Result<RecurringTimeEntryDto>> CreateAsync(Guid userId, CreateRecurringTimeEntryDto dto, CancellationToken cancellationToken = default);
    Task<Result<RecurringTimeEntryDto>> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<RecurringTimeEntryDto>>> GetAllAsync(Guid userId, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<Result<RecurringTimeEntryDto>> UpdateAsync(Guid userId, Guid id, UpdateRecurringTimeEntryDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    // Activation Control
    Task<Result> ActivateAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    // Generation
    Task<Result<int>> GenerateScheduledEntriesAsync(DateTime forDate, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TimeEntryDto>>> PreviewOccurrencesAsync(Guid userId, Guid id, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}
