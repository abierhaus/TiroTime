using TiroTime.Domain.Common;
using TiroTime.Domain.Exceptions;
using TiroTime.Domain.ValueObjects;

namespace TiroTime.Domain.Entities;

public class RecurringTimeEntry : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public RecurringPattern Pattern { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime? LastGeneratedDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public Project? Project { get; private set; }

    private RecurringTimeEntry() { }

    public static RecurringTimeEntry Create(
        Guid userId,
        Guid projectId,
        string title,
        TimeSpan startTime,
        TimeSpan endTime,
        RecurringPattern pattern,
        string? description = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Benutzer-ID ist erforderlich");

        if (projectId == Guid.Empty)
            throw new DomainException("Projekt-ID ist erforderlich");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Titel ist erforderlich");

        if (title.Length > 200)
            throw new DomainException("Titel darf maximal 200 Zeichen lang sein");

        if (description?.Length > 2000)
            throw new DomainException("Beschreibung darf maximal 2000 Zeichen lang sein");

        if (endTime <= startTime)
            throw new DomainException("Endzeit muss nach der Startzeit liegen");

        var duration = endTime - startTime;
        if (duration.TotalHours > 24)
            throw new DomainException("Zeiterfassung darf nicht länger als 24 Stunden sein");

        var recurringEntry = new RecurringTimeEntry
        {
            UserId = userId,
            ProjectId = projectId,
            Title = title.Trim(),
            Description = description?.Trim(),
            StartTime = startTime,
            EndTime = endTime,
            Pattern = pattern,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return recurringEntry;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("Wiederholung ist bereits aktiv");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Wiederholung ist bereits inaktiv");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string title,
        TimeSpan startTime,
        TimeSpan endTime,
        RecurringPattern pattern,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Titel ist erforderlich");

        if (title.Length > 200)
            throw new DomainException("Titel darf maximal 200 Zeichen lang sein");

        if (description?.Length > 2000)
            throw new DomainException("Beschreibung darf maximal 2000 Zeichen lang sein");

        if (endTime <= startTime)
            throw new DomainException("Endzeit muss nach der Startzeit liegen");

        var duration = endTime - startTime;
        if (duration.TotalHours > 24)
            throw new DomainException("Zeiterfassung darf nicht länger als 24 Stunden sein");

        Title = title.Trim();
        Description = description?.Trim();
        StartTime = startTime;
        EndTime = endTime;
        Pattern = pattern;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        if (description?.Length > 2000)
            throw new DomainException("Beschreibung darf maximal 2000 Zeichen lang sein");

        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsGenerated(DateTime date)
    {
        LastGeneratedDate = date.Date;
        UpdatedAt = DateTime.UtcNow;
    }

    public IEnumerable<(DateTime Date, TimeSpan StartTime, TimeSpan EndTime)> GenerateOccurrences(
        DateTime fromDate,
        DateTime toDate,
        int? existingOccurrenceCount = null)
    {
        if (!IsActive)
            yield break;

        var occurrenceCount = existingOccurrenceCount ?? 0;
        var currentDate = fromDate.Date;

        while (currentDate <= toDate.Date)
        {
            var nextOccurrence = Pattern.GetNextOccurrence(currentDate, occurrenceCount);

            if (!nextOccurrence.HasValue || nextOccurrence.Value > toDate.Date)
                break;

            yield return (nextOccurrence.Value, StartTime, EndTime);

            occurrenceCount++;
            currentDate = nextOccurrence.Value.AddDays(1);
        }
    }
}
