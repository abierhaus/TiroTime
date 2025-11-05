using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Application.Services;

public class TimeEntryValidationService : ITimeEntryValidationService
{
    private readonly List<string> _validationExclusionKeywords = new()
    {
        "Pauschal"
    };

    public IEnumerable<TimeEntryValidationWarning> ValidateTimeEntries(IEnumerable<TimeEntryDto> entries)
    {
        var warnings = new List<TimeEntryValidationWarning>();

        // Filter entries: exclude entries with exclusion keywords in description
        var filteredEntries = entries.Where(e =>
        {
            if (string.IsNullOrWhiteSpace(e.Description))
                return true; // No description, include in validation

            // Check if description contains any exclusion keyword (case-insensitive)
            return !_validationExclusionKeywords.Any(keyword =>
                e.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }).ToList();

        // Group filtered entries by date
        var entriesByDate = filteredEntries
            .GroupBy(e => e.StartTime.ToLocalTime().Date)
            .ToList();

        foreach (var dateGroup in entriesByDate)
        {
            var date = dateGroup.Key;
            var dayEntries = dateGroup.OrderBy(e => e.StartTime).ToList();

            // Check for weekend work
            warnings.AddRange(ValidateWeekendWork(date, dayEntries));

            // Check for overlapping time entries
            warnings.AddRange(ValidateOverlappingEntries(date, dayEntries));
        }

        return warnings;
    }

    private IEnumerable<TimeEntryValidationWarning> ValidateWeekendWork(DateTime date, List<TimeEntryDto> dayEntries)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            var dayName = date.ToString("dddd", new System.Globalization.CultureInfo("de-DE"));
            yield return new TimeEntryValidationWarning
            {
                Type = "weekend",
                Message = $"Wochenendarbeit am {date:dd.MM.yyyy} ({dayName})",
                Date = date,
                AffectedEntryIds = dayEntries.Select(e => e.Id).ToList()
            };
        }
    }

    private IEnumerable<TimeEntryValidationWarning> ValidateOverlappingEntries(DateTime date, List<TimeEntryDto> dayEntries)
    {
        var warnings = new List<TimeEntryValidationWarning>();

        for (int i = 0; i < dayEntries.Count - 1; i++)
        {
            var current = dayEntries[i];
            var currentStart = current.StartTime.ToLocalTime();
            var currentEnd = current.EndTime?.ToLocalTime();

            if (!currentEnd.HasValue) continue;

            for (int j = i + 1; j < dayEntries.Count; j++)
            {
                var next = dayEntries[j];
                var nextStart = next.StartTime.ToLocalTime();
                var nextEnd = next.EndTime?.ToLocalTime();

                if (!nextEnd.HasValue) continue;

                // Check if times overlap
                if (currentStart < nextEnd && nextStart < currentEnd)
                {
                    warnings.Add(new TimeEntryValidationWarning
                    {
                        Type = "overlap",
                        Message = $"Zeitüberschneidung am {date:dd.MM.yyyy}: {currentStart:HH:mm}-{currentEnd.Value:HH:mm} und {nextStart:HH:mm}-{nextEnd.Value:HH:mm}",
                        Date = date,
                        AffectedEntryIds = new List<Guid> { current.Id, next.Id }
                    });
                }
            }
        }

        return warnings;
    }
}
