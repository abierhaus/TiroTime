using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.TimeTracking;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly IProjectService _projectService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        ITimeEntryService timeEntryService,
        IProjectService projectService,
        ICurrentUserService currentUserService,
        ILogger<IndexModel> logger)
    {
        _timeEntryService = timeEntryService;
        _projectService = projectService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public TimeEntryDto? ActiveTimer { get; set; }
    public IEnumerable<ProjectDto> Projects { get; set; } = Array.Empty<ProjectDto>();
    public IEnumerable<TimeEntryDto> MonthEntries { get; set; } = Array.Empty<TimeEntryDto>();
    public TimeSpan MonthTotal { get; set; }
    public int CurrentYear { get; set; }
    public int CurrentMonth { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public List<ValidationWarning> ValidationWarnings { get; set; } = new();

    public class ValidationWarning
    {
        public string Type { get; set; } = string.Empty; // "weekend" or "overlap"
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<Guid> AffectedEntryIds { get; set; } = new();
    }

    public async Task OnGetAsync(int? year, int? month)
    {
        var userId = _currentUserService.UserId!.Value;

        // Determine which month to show
        var today = DateTime.Today;
        CurrentYear = year ?? today.Year;
        CurrentMonth = month ?? today.Month;

        // Validate month/year
        if (CurrentMonth < 1 || CurrentMonth > 12)
        {
            CurrentMonth = today.Month;
            CurrentYear = today.Year;
        }

        var monthStart = new DateTime(CurrentYear, CurrentMonth, 1);
        var monthEnd = monthStart.AddMonths(1);
        MonthName = monthStart.ToString("MMMM yyyy", new System.Globalization.CultureInfo("de-DE"));

        // Get active timer
        var activeTimerResult = await _timeEntryService.GetActiveTimerAsync(userId);
        if (activeTimerResult.IsSuccess)
        {
            ActiveTimer = activeTimerResult.Value;
        }

        // Get projects for dropdown
        var projectsResult = await _projectService.GetAllProjectsAsync(includeInactive: false);
        if (projectsResult.IsSuccess)
        {
            Projects = projectsResult.Value;
        }

        // Get month's entries
        var entriesResult = await _timeEntryService.GetTimeEntriesByDateRangeAsync(userId, monthStart, monthEnd);

        if (entriesResult.IsSuccess)
        {
            MonthEntries = entriesResult.Value.Where(e => !e.IsRunning).OrderByDescending(e => e.StartTime);
            MonthTotal = TimeSpan.FromTicks(MonthEntries.Sum(e => e.Duration.Ticks));

            // Validate entries
            ValidateEntries(MonthEntries);
        }
    }

    private void ValidateEntries(IEnumerable<TimeEntryDto> entries)
    {
        ValidationWarnings.Clear();

        // Group entries by date
        var entriesByDate = entries
            .GroupBy(e => e.StartTime.ToLocalTime().Date)
            .ToList();

        foreach (var dateGroup in entriesByDate)
        {
            var date = dateGroup.Key;
            var dayEntries = dateGroup.OrderBy(e => e.StartTime).ToList();

            // Check for weekend work
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                var dayName = date.ToString("dddd", new System.Globalization.CultureInfo("de-DE"));
                ValidationWarnings.Add(new ValidationWarning
                {
                    Type = "weekend",
                    Message = $"Wochenendarbeit am {date:dd.MM.yyyy} ({dayName})",
                    Date = date,
                    AffectedEntryIds = dayEntries.Select(e => e.Id).ToList()
                });
            }

            // Check for overlapping time entries
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
                        ValidationWarnings.Add(new ValidationWarning
                        {
                            Type = "overlap",
                            Message = $"Zeitüberschneidung am {date:dd.MM.yyyy}: {currentStart:HH:mm}-{currentEnd.Value:HH:mm} und {nextStart:HH:mm}-{nextEnd.Value:HH:mm}",
                            Date = date,
                            AffectedEntryIds = new List<Guid> { current.Id, next.Id }
                        });
                    }
                }
            }
        }
    }

    public async Task<IActionResult> OnPostStartTimerAsync(Guid projectId, string? description)
    {
        var userId = _currentUserService.UserId!.Value;

        var dto = new StartTimerDto(projectId, description);
        var result = await _timeEntryService.StartTimerAsync(userId, dto);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Timer gestartet für Projekt {ProjectId}", projectId);
            TempData["SuccessMessage"] = "Timer wurde gestartet.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostStopTimerAsync(Guid timeEntryId)
    {
        var userId = _currentUserService.UserId!.Value;

        var result = await _timeEntryService.StopTimerAsync(userId, timeEntryId);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Timer gestoppt: {TimeEntryId}", timeEntryId);
            TempData["SuccessMessage"] = $"Timer gestoppt: {result.Value.Duration:hh\\:mm\\:ss}";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostQuickUpdateAsync(Guid id, DateTime date, TimeSpan startTime, TimeSpan endTime)
    {
        var userId = _currentUserService.UserId!.Value;

        // Get the existing entry to preserve description
        var existingResult = await _timeEntryService.GetTimeEntryByIdAsync(userId, id);
        if (!existingResult.IsSuccess)
        {
            return new JsonResult(new { success = false, error = "Eintrag nicht gefunden" });
        }

        // Combine date and time (local time)
        var localStartDateTime = date.Date + startTime;
        var localEndDateTime = date.Date + endTime;

        // Handle overnight entries
        if (localEndDateTime <= localStartDateTime)
        {
            localEndDateTime = localEndDateTime.AddDays(1);
        }

        // Convert local time to UTC
        var startDateTime = DateTime.SpecifyKind(localStartDateTime, DateTimeKind.Local).ToUniversalTime();
        var endDateTime = DateTime.SpecifyKind(localEndDateTime, DateTimeKind.Local).ToUniversalTime();

        // Preserve the existing description
        var dto = new UpdateTimeEntryDto(id, startDateTime, endDateTime, existingResult.Value.Description);
        var result = await _timeEntryService.UpdateTimeEntryAsync(userId, dto);

        if (result.IsSuccess)
        {
            return new JsonResult(new { success = true, duration = result.Value.Duration.ToString(@"hh\:mm") });
        }

        return new JsonResult(new { success = false, error = result.Error });
    }

    public async Task<IActionResult> OnPostDuplicateEntryAsync(Guid id)
    {
        var userId = _currentUserService.UserId!.Value;

        // Get the original entry
        var originalResult = await _timeEntryService.GetTimeEntryByIdAsync(userId, id);
        if (!originalResult.IsSuccess)
        {
            return new JsonResult(new { success = false, error = "Eintrag nicht gefunden" });
        }

        var original = originalResult.Value;
        var today = DateTime.Today;

        // Extract time components from original entry
        var startTime = original.StartTime.ToLocalTime().TimeOfDay;
        var endTime = original.EndTime?.ToLocalTime().TimeOfDay ?? TimeSpan.Zero;

        // Combine with today's date (local time)
        var localStartDateTime = today + startTime;
        var localEndDateTime = today + endTime;

        // Handle overnight entries
        if (localEndDateTime <= localStartDateTime && original.EndTime.HasValue)
        {
            localEndDateTime = localEndDateTime.AddDays(1);
        }

        // Convert local time to UTC
        var newStartDateTime = DateTime.SpecifyKind(localStartDateTime, DateTimeKind.Local).ToUniversalTime();
        var newEndDateTime = DateTime.SpecifyKind(localEndDateTime, DateTimeKind.Local).ToUniversalTime();

        // Create new entry with today's date
        var dto = new CreateManualTimeEntryDto(
            original.ProjectId,
            newStartDateTime,
            newEndDateTime,
            original.Description);

        var result = await _timeEntryService.CreateManualTimeEntryAsync(userId, dto);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Zeiteintrag dupliziert: Original {OriginalId}, Neu {NewId}", id, result.Value.Id);
            return new JsonResult(new { success = true });
        }

        return new JsonResult(new { success = false, error = result.Error });
    }
}
