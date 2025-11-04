using TiroTime.Domain.Common;
using TiroTime.Domain.Exceptions;

namespace TiroTime.Domain.Entities;

public class TimeEntry : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid ProjectId { get; private set; }
    public string? Description { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public TimeSpan Duration { get; private set; }
    public bool IsRunning { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public Project? Project { get; private set; }

    private TimeEntry() { }

    public static TimeEntry StartTimer(
        Guid userId,
        Guid projectId,
        string? description = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Benutzer-ID ist erforderlich");

        if (projectId == Guid.Empty)
            throw new DomainException("Projekt-ID ist erforderlich");

        var timeEntry = new TimeEntry
        {
            UserId = userId,
            ProjectId = projectId,
            Description = description?.Trim(),
            StartTime = DateTime.UtcNow,
            IsRunning = true,
            Duration = TimeSpan.Zero,
            CreatedAt = DateTime.UtcNow
        };

        return timeEntry;
    }

    public static TimeEntry CreateManual(
        Guid userId,
        Guid projectId,
        DateTime startTime,
        DateTime endTime,
        string? description = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Benutzer-ID ist erforderlich");

        if (projectId == Guid.Empty)
            throw new DomainException("Projekt-ID ist erforderlich");

        if (endTime <= startTime)
            throw new DomainException("Endzeit muss nach der Startzeit liegen");

        var duration = endTime - startTime;

        if (duration.TotalHours > 24)
            throw new DomainException("Zeiterfassung darf nicht länger als 24 Stunden sein");

        var timeEntry = new TimeEntry
        {
            UserId = userId,
            ProjectId = projectId,
            Description = description?.Trim(),
            StartTime = startTime,
            EndTime = endTime,
            Duration = duration,
            IsRunning = false,
            CreatedAt = DateTime.UtcNow
        };

        return timeEntry;
    }

    public void StopTimer()
    {
        if (!IsRunning)
            throw new DomainException("Timer läuft nicht");

        EndTime = DateTime.UtcNow;
        Duration = EndTime.Value - StartTime;
        IsRunning = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        DateTime startTime,
        DateTime endTime,
        string? description = null)
    {
        if (IsRunning)
            throw new DomainException("Laufende Zeiterfassung kann nicht bearbeitet werden");

        if (endTime <= startTime)
            throw new DomainException("Endzeit muss nach der Startzeit liegen");

        var duration = endTime - startTime;

        if (duration.TotalHours > 24)
            throw new DomainException("Zeiterfassung darf nicht länger als 24 Stunden sein");

        StartTime = startTime;
        EndTime = endTime;
        Duration = duration;
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public TimeSpan GetCurrentDuration()
    {
        if (IsRunning)
        {
            return DateTime.UtcNow - StartTime;
        }

        return Duration;
    }
}
