using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Enums;

namespace TiroTime.Web.Pages.TimeTracking.Recurring;

[Authorize]
public class EditModel : PageModel
{
    private readonly IRecurringTimeEntryService _recurringService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        IRecurringTimeEntryService recurringService,
        ICurrentUserService currentUserService,
        ILogger<EditModel> logger)
    {
        _recurringService = recurringService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ProjectDisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public class InputModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Titel ist erforderlich")]
        [Display(Name = "Titel")]
        [StringLength(200, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Beschreibung")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Startzeit ist erforderlich")]
        [Display(Name = "Startzeit")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Endzeit ist erforderlich")]
        [Display(Name = "Endzeit")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Wiederholungstyp ist erforderlich")]
        [Display(Name = "Wiederholungstyp")]
        public RecurrenceType Frequency { get; set; }

        [Required(ErrorMessage = "Intervall ist erforderlich")]
        [Display(Name = "Intervall")]
        [Range(1, 365, ErrorMessage = "{0} muss zwischen {1} und {2} liegen")]
        public int Interval { get; set; }

        [Display(Name = "Wochentage")]
        public List<DayOfWeek> DaysOfWeek { get; set; } = new();

        [Display(Name = "Tag im Monat")]
        [Range(1, 31, ErrorMessage = "{0} muss zwischen {1} und {2} liegen")]
        public int? DayOfMonth { get; set; }

        [Required(ErrorMessage = "Startdatum ist erforderlich")]
        [Display(Name = "Startdatum")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Enddatum")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Maximale Anzahl")]
        [Range(1, 999, ErrorMessage = "{0} muss zwischen {1} und {2} liegen")]
        public int? MaxOccurrences { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _recurringService.GetByIdAsync(userId, id);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToPage("./Index");
        }

        var entry = result.Value;

        ProjectDisplayName = $"{entry.ProjectName} ({entry.ClientName})";
        IsActive = entry.IsActive;

        Input = new InputModel
        {
            Id = entry.Id,
            Title = entry.Title,
            Description = entry.Description,
            StartTime = entry.StartTime,
            EndTime = entry.EndTime,
            Frequency = entry.Pattern.Frequency,
            Interval = entry.Pattern.Interval,
            DaysOfWeek = entry.Pattern.DaysOfWeek?.ToList() ?? new List<DayOfWeek>(),
            DayOfMonth = entry.Pattern.DayOfMonth,
            StartDate = entry.Pattern.StartDate,
            EndDate = entry.Pattern.EndDate,
            MaxOccurrences = entry.Pattern.MaxOccurrences
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = _currentUserService.UserId!.Value;

        if (!ModelState.IsValid)
        {
            // Reload display data
            var entryResult = await _recurringService.GetByIdAsync(userId, Input.Id);
            if (entryResult.IsSuccess)
            {
                ProjectDisplayName = $"{entryResult.Value.ProjectName} ({entryResult.Value.ClientName})";
                IsActive = entryResult.Value.IsActive;
            }
            return Page();
        }

        // Validierung: EndDate und MaxOccurrences
        if (Input.EndDate.HasValue && Input.MaxOccurrences.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Enddatum und maximale Anzahl können nicht gleichzeitig angegeben werden.");
            return Page();
        }

        // Validierung: Frequency-spezifisch
        if (Input.Frequency == RecurrenceType.Weekly && !Input.DaysOfWeek.Any())
        {
            ModelState.AddModelError(nameof(Input.DaysOfWeek), "Bei wöchentlicher Wiederholung muss mindestens ein Wochentag ausgewählt werden.");
            return Page();
        }

        if (Input.Frequency == RecurrenceType.Monthly && !Input.DayOfMonth.HasValue)
        {
            ModelState.AddModelError(nameof(Input.DayOfMonth), "Bei monatlicher Wiederholung muss ein Tag im Monat angegeben werden.");
            return Page();
        }

        var dto = new UpdateRecurringTimeEntryDto(
            Input.Title,
            Input.Description,
            Input.StartTime,
            Input.EndTime,
            new RecurringPatternDto(
                Input.Frequency,
                Input.Interval,
                Input.Frequency == RecurrenceType.Weekly ? Input.DaysOfWeek.ToArray() : null,
                Input.Frequency == RecurrenceType.Monthly ? Input.DayOfMonth : null,
                Input.StartDate,
                Input.EndDate,
                Input.MaxOccurrences));

        var result = await _recurringService.UpdateAsync(userId, Input.Id, dto);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Wiederkehrende Zeiterfassung aktualisiert: {Id}", Input.Id);
            TempData["SuccessMessage"] = "Wiederholung wurde erfolgreich aktualisiert.";
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, result.Error);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _recurringService.DeleteAsync(userId, id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Wiederkehrende Zeiterfassung gelöscht: {Id}", id);
            TempData["SuccessMessage"] = "Wiederholung wurde gelöscht.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToPage("./Index");
    }
}
