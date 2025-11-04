using TiroTime.Domain.Common;
using TiroTime.Domain.Exceptions;
using TiroTime.Domain.ValueObjects;

namespace TiroTime.Domain.Entities;

public class Project : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid ClientId { get; private set; }
    public Money HourlyRate { get; private set; } = Money.Zero;
    public Money? Budget { get; private set; }
    public string? ColorCode { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    // Navigation property
    public Client? Client { get; private set; }

    private Project() { }

    public static Project Create(
        string name,
        Guid clientId,
        Money hourlyRate,
        string? description = null,
        Money? budget = null,
        string? colorCode = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Projektname darf nicht leer sein");

        if (clientId == Guid.Empty)
            throw new DomainException("Kunden-ID ist erforderlich");

        if (hourlyRate.Amount < 0)
            throw new DomainException("Stundensatz darf nicht negativ sein");

        if (budget != null && budget.Amount < 0)
            throw new DomainException("Budget darf nicht negativ sein");

        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
            throw new DomainException("Enddatum muss nach dem Startdatum liegen");

        var project = new Project
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            ClientId = clientId,
            HourlyRate = hourlyRate,
            Budget = budget,
            ColorCode = colorCode?.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return project;
    }

    public void Update(
        string name,
        Money hourlyRate,
        string? description = null,
        Money? budget = null,
        string? colorCode = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Projektname darf nicht leer sein");

        if (hourlyRate.Amount < 0)
            throw new DomainException("Stundensatz darf nicht negativ sein");

        if (budget != null && budget.Amount < 0)
            throw new DomainException("Budget darf nicht negativ sein");

        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
            throw new DomainException("Enddatum muss nach dem Startdatum liegen");

        Name = name.Trim();
        Description = description?.Trim();
        HourlyRate = hourlyRate;
        Budget = budget;
        ColorCode = colorCode?.Trim();
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
