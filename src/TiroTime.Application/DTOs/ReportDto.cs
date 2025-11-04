namespace TiroTime.Application.DTOs;

public record TimeEntryReportDto(
    DateTime Date,
    string ProjectName,
    string ClientName,
    string? Description,
    TimeSpan StartTime,
    TimeSpan EndTime,
    TimeSpan Duration,
    decimal HourlyRate,
    string Currency,
    decimal TotalAmount);

public record ReportSummaryDto(
    int TotalEntries,
    TimeSpan TotalDuration,
    decimal TotalAmount,
    DateTime StartDate,
    DateTime EndDate,
    IEnumerable<ProjectSummaryDto> ProjectSummaries);

public record ProjectSummaryDto(
    string ProjectName,
    string ClientName,
    int EntryCount,
    TimeSpan TotalDuration,
    decimal TotalAmount,
    string Currency);

public record GenerateReportDto(
    DateTime StartDate,
    DateTime EndDate,
    Guid? ProjectId = null,
    Guid? ClientId = null);
