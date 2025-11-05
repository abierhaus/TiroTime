using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.TimeTracking;

[Authorize]
public class IndexModel(
    ITimeEntryService timeEntryService,
    IProjectService projectService,
    ICurrentUserService currentUserService,
    ITimeEntryValidationService validationService,
    ILogger<IndexModel> logger) : PageModel
{
    public TimeEntryDto? ActiveTimer { get; set; }
    public IEnumerable<ProjectDto> Projects { get; set; } = [];
    public IEnumerable<TimeEntryDto> MonthEntries { get; set; } = [];
    public TimeSpan MonthTotal { get; set; }
    public int CurrentYear { get; set; }
    public int CurrentMonth { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public List<TimeEntryValidationWarning> ValidationWarnings { get; set; } = [];

    public async Task OnGetAsync(int? year, int? month)
    {
        var userId = currentUserService.UserId!.Value;

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
        var activeTimerResult = await timeEntryService.GetActiveTimerAsync(userId);
        if (activeTimerResult.IsSuccess)
        {
            ActiveTimer = activeTimerResult.Value;
        }

        // Get projects for dropdown
        var projectsResult = await projectService.GetAllProjectsAsync(includeInactive: false);
        if (projectsResult.IsSuccess)
        {
            Projects = projectsResult.Value;
        }

        // Get month's entries
        var entriesResult = await timeEntryService.GetTimeEntriesByDateRangeAsync(userId, monthStart, monthEnd);

        if (entriesResult.IsSuccess)
        {
            MonthEntries = entriesResult.Value.Where(e => !e.IsRunning).OrderByDescending(e => e.StartTime);
            MonthTotal = TimeSpan.FromTicks(MonthEntries.Sum(e => e.Duration.Ticks));

            // Validate entries using the validation service
            ValidationWarnings = validationService.ValidateTimeEntries(MonthEntries).ToList();
        }
    }

    public async Task<IActionResult> OnPostStartTimerAsync(Guid projectId, string? description)
    {
        var userId = currentUserService.UserId!.Value;

        var dto = new StartTimerDto(projectId, description);
        var result = await timeEntryService.StartTimerAsync(userId, dto);

        if (result.IsSuccess)
        {
            logger.LogInformation("Timer gestartet für Projekt {ProjectId}", projectId);
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
        var userId = currentUserService.UserId!.Value;

        var result = await timeEntryService.StopTimerAsync(userId, timeEntryId);

        if (result.IsSuccess)
        {
            logger.LogInformation("Timer gestoppt: {TimeEntryId}", timeEntryId);
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
        var userId = currentUserService.UserId!.Value;

        // Get the existing entry to preserve description
        var existingResult = await timeEntryService.GetTimeEntryByIdAsync(userId, id);
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
        var result = await timeEntryService.UpdateTimeEntryAsync(userId, dto);

        if (result.IsSuccess)
        {
            return new JsonResult(new { success = true, duration = result.Value.Duration.ToString(@"hh\:mm") });
        }

        return new JsonResult(new { success = false, error = result.Error });
    }

    public async Task<IActionResult> OnPostDuplicateEntryAsync(Guid id)
    {
        var userId = currentUserService.UserId!.Value;

        // Get the original entry
        var originalResult = await timeEntryService.GetTimeEntryByIdAsync(userId, id);
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

        var result = await timeEntryService.CreateManualTimeEntryAsync(userId, dto);

        if (result.IsSuccess)
        {
            logger.LogInformation("Zeiteintrag dupliziert: Original {OriginalId}, Neu {NewId}", id, result.Value.Id);
            return new JsonResult(new { success = true });
        }

        return new JsonResult(new { success = false, error = result.Error });
    }
}
