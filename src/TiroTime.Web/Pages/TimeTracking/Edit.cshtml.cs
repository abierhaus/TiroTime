using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.TimeTracking;

[Authorize]
public class EditModel : PageModel
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        ITimeEntryService timeEntryService,
        ICurrentUserService currentUserService,
        ILogger<EditModel> logger)
    {
        _timeEntryService = timeEntryService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ProjectName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;

    public class InputModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Datum ist erforderlich")]
        [Display(Name = "Datum")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Startzeit ist erforderlich")]
        [Display(Name = "Startzeit")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Endzeit ist erforderlich")]
        [Display(Name = "Endzeit")]
        public TimeSpan EndTime { get; set; }

        [StringLength(2000, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Beschreibung")]
        public string? Description { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _timeEntryService.GetTimeEntryByIdAsync(userId, id);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToPage("./Index");
        }

        var timeEntry = result.Value;

        if (timeEntry.IsRunning)
        {
            TempData["ErrorMessage"] = "Laufende Zeiteinträge können nicht bearbeitet werden.";
            return RedirectToPage("./Index");
        }

        ProjectName = timeEntry.ProjectName;
        ClientName = timeEntry.ClientName;

        var localStartTime = timeEntry.StartTime.ToLocalTime();

        Input = new InputModel
        {
            Id = timeEntry.Id,
            Date = localStartTime.Date,
            StartTime = localStartTime.TimeOfDay,
            EndTime = timeEntry.EndTime!.Value.ToLocalTime().TimeOfDay,
            Description = timeEntry.Description
        };

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (!ModelState.IsValid)
        {
            var timeEntry = await LoadTimeEntryAsync();
            if (timeEntry == null) return RedirectToPage("./Index");
            return Page();
        }

        var userId = _currentUserService.UserId!.Value;

        // Combine date and time (local time)
        var localStartDateTime = Input.Date.Date + Input.StartTime;
        var localEndDateTime = Input.Date.Date + Input.EndTime;

        // Handle overnight entries
        if (localEndDateTime <= localStartDateTime)
        {
            localEndDateTime = localEndDateTime.AddDays(1);
        }

        // Convert local time to UTC
        var startDateTime = DateTime.SpecifyKind(localStartDateTime, DateTimeKind.Local).ToUniversalTime();
        var endDateTime = DateTime.SpecifyKind(localEndDateTime, DateTimeKind.Local).ToUniversalTime();

        var dto = new UpdateTimeEntryDto(
            Input.Id,
            startDateTime,
            endDateTime,
            Input.Description);

        var result = await _timeEntryService.UpdateTimeEntryAsync(userId, dto);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Zeiteintrag aktualisiert: {TimeEntryId}", Input.Id);
            TempData["SuccessMessage"] = "Zeiteintrag wurde erfolgreich aktualisiert.";
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, result.Error);
        var timeEntryData = await LoadTimeEntryAsync();
        if (timeEntryData == null) return RedirectToPage("./Index");
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _timeEntryService.DeleteTimeEntryAsync(userId, Input.Id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Zeiteintrag gelöscht: {TimeEntryId}", Input.Id);
            TempData["SuccessMessage"] = "Zeiteintrag wurde erfolgreich gelöscht.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToPage("./Index");
    }

    private async Task<TimeEntryDto?> LoadTimeEntryAsync()
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _timeEntryService.GetTimeEntryByIdAsync(userId, Input.Id);

        if (result.IsSuccess)
        {
            ProjectName = result.Value.ProjectName;
            ClientName = result.Value.ClientName;
            return result.Value;
        }

        return null;
    }
}
