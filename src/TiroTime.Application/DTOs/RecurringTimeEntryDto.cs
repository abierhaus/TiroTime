using TiroTime.Domain.Enums;

namespace TiroTime.Application.DTOs;

public record RecurringTimeEntryDto(
    Guid Id,
    Guid UserId,
    Guid ProjectId,
    string ProjectName,
    string ClientName,
    string? ProjectColorCode,
    string Title,
    string? Description,
    TimeSpan StartTime,
    TimeSpan EndTime,
    RecurringPatternDto Pattern,
    bool IsActive,
    DateTime? LastGeneratedDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record RecurringPatternDto(
    RecurrenceType Frequency,
    int Interval,
    DayOfWeek[]? DaysOfWeek,
    int? DayOfMonth,
    DateTime StartDate,
    DateTime? EndDate,
    int? MaxOccurrences);

public record CreateRecurringTimeEntryDto(
    Guid ProjectId,
    string Title,
    string? Description,
    TimeSpan StartTime,
    TimeSpan EndTime,
    RecurringPatternDto Pattern);

public record UpdateRecurringTimeEntryDto(
    string Title,
    string? Description,
    TimeSpan StartTime,
    TimeSpan EndTime,
    RecurringPatternDto Pattern);
