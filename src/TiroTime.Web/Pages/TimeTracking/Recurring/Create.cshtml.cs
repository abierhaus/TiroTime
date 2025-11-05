using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Enums;

namespace TiroTime.Web.Pages.TimeTracking.Recurring;

[Authorize]
public class CreateModel(
    IRecurringTimeEntryService recurringService,
    IProjectService projectService,
    ICurrentUserService currentUserService,
    ILogger<CreateModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ProjectSelectList { get; set; } = new(Array.Empty<object>());

    public class InputModel
    {
        [Required(ErrorMessage = "Projekt ist erforderlich")]
        [Display(Name = "Projekt")]
        public Guid ProjectId { get; set; }

        [Required(ErrorMessage = "Titel ist erforderlich")]
        [Display(Name = "Titel")]
        [StringLength(200, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Beschreibung")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Startzeit ist erforderlich")]
        [Display(Name = "Startzeit")]
        public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0);

        [Required(ErrorMessage = "Endzeit ist erforderlich")]
        [Display(Name = "Endzeit")]
        public TimeSpan EndTime { get; set; } = new TimeSpan(16, 0, 0);

        [Required(ErrorMessage = "Wiederholungstyp ist erforderlich")]
        [Display(Name = "Wiederholungstyp")]
        public RecurrenceType Frequency { get; set; } = RecurrenceType.Monthly;

        [Required(ErrorMessage = "Intervall ist erforderlich")]
        [Display(Name = "Intervall")]
        [Range(1, 365, ErrorMessage = "{0} muss zwischen {1} und {2} liegen")]
        public int Interval { get; set; } = 1;

        [Display(Name = "Wochentage")]
        public List<DayOfWeek> DaysOfWeek { get; set; } = new();

        [Display(Name = "Tag im Monat")]
        [Range(1, 31, ErrorMessage = "{0} muss zwischen {1} und {2} liegen")]
        public int? DayOfMonth { get; set; } = 1;

        [Required(ErrorMessage = "Startdatum ist erforderlich")]
        [Display(Name = "Startdatum")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Display(Name = "Enddatum")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Maximale Anzahl")]
        [Range(1, 999, ErrorMessage = "{0} muss zwischen {1} und {2} liegen")]
        public int? MaxOccurrences { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadProjectsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadProjectsAsync();
            return Page();
        }

        // Validierung: EndDate und MaxOccurrences
        if (Input.EndDate.HasValue && Input.MaxOccurrences.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Enddatum und maximale Anzahl können nicht gleichzeitig angegeben werden.");
            await LoadProjectsAsync();
            return Page();
        }

        // Validierung: Frequency-spezifisch
        if (Input.Frequency == RecurrenceType.Weekly && !Input.DaysOfWeek.Any())
        {
            ModelState.AddModelError(nameof(Input.DaysOfWeek), "Bei wöchentlicher Wiederholung muss mindestens ein Wochentag ausgewählt werden.");
            await LoadProjectsAsync();
            return Page();
        }

        if (Input.Frequency == RecurrenceType.Monthly && !Input.DayOfMonth.HasValue)
        {
            ModelState.AddModelError(nameof(Input.DayOfMonth), "Bei monatlicher Wiederholung muss ein Tag im Monat angegeben werden.");
            await LoadProjectsAsync();
            return Page();
        }

        var userId = currentUserService.UserId!.Value;

        var dto = new CreateRecurringTimeEntryDto(
            Input.ProjectId,
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

        var result = await recurringService.CreateAsync(userId, dto);

        if (result.IsSuccess)
        {
            logger.LogInformation("Wiederkehrende Zeiterfassung erstellt: {Title}", Input.Title);
            TempData["SuccessMessage"] = "Wiederholung wurde erfolgreich erstellt.";
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, result.Error);
        await LoadProjectsAsync();
        return Page();
    }

    private async Task LoadProjectsAsync()
    {
        var projectsResult = await projectService.GetAllProjectsAsync(includeInactive: false);

        if (projectsResult.IsSuccess)
        {
            ProjectSelectList = new SelectList(
                projectsResult.Value.Select(p => new
                {
                    p.Id,
                    DisplayName = $"{p.Name} ({p.ClientName})"
                }),
                "Id",
                "DisplayName");
        }
    }
}
