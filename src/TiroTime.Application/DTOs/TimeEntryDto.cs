namespace TiroTime.Application.DTOs;

public record TimeEntryDto(
    Guid Id,
    Guid UserId,
    Guid ProjectId,
    string ProjectName,
    string ClientName,
    string? ProjectColorCode,
    string? Description,
    DateTime StartTime,
    DateTime? EndTime,
    TimeSpan Duration,
    bool IsRunning,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record StartTimerDto(
    Guid ProjectId,
    string? Description);

public record StopTimerDto(
    Guid TimeEntryId);

public record CreateManualTimeEntryDto(
    Guid ProjectId,
    DateTime StartTime,
    DateTime EndTime,
    string? Description);

public record UpdateTimeEntryDto(
    Guid Id,
    DateTime StartTime,
    DateTime EndTime,
    string? Description);

public record TimeEntrySummaryDto(
    DateTime Date,
    TimeSpan TotalDuration,
    int EntryCount,
    IEnumerable<TimeEntryDto> Entries);

public record TimeEntryStatisticsDto(
    TimeSpan TodayTotal,
    TimeSpan WeekTotal,
    TimeSpan MonthTotal,
    int TodayEntryCount,
    int WeekEntryCount,
    int MonthEntryCount);
