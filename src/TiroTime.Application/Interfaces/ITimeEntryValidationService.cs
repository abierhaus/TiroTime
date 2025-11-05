using TiroTime.Application.DTOs;

namespace TiroTime.Application.Interfaces;

public interface ITimeEntryValidationService
{
    IEnumerable<TimeEntryValidationWarning> ValidateTimeEntries(IEnumerable<TimeEntryDto> entries);
}

public class TimeEntryValidationWarning
{
    public string Type { get; set; } = string.Empty; // "weekend" or "overlap"
    public string Message { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<Guid> AffectedEntryIds { get; set; } = new();
}
