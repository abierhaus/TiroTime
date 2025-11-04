namespace TiroTime.Application.DTOs;

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    Guid ClientId,
    string ClientName,
    decimal HourlyRate,
    string HourlyRateCurrency,
    decimal? Budget,
    string? BudgetCurrency,
    string? ColorCode,
    bool IsActive,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateProjectDto(
    string Name,
    string? Description,
    Guid ClientId,
    decimal HourlyRate,
    string HourlyRateCurrency,
    decimal? Budget,
    string? BudgetCurrency,
    string? ColorCode,
    DateTime? StartDate,
    DateTime? EndDate);

public record UpdateProjectDto(
    Guid Id,
    string Name,
    string? Description,
    decimal HourlyRate,
    string HourlyRateCurrency,
    decimal? Budget,
    string? BudgetCurrency,
    string? ColorCode,
    DateTime? StartDate,
    DateTime? EndDate);
