using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.TimeTracking;

[Authorize]
public class CreateModel(
    ITimeEntryService timeEntryService,
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

        [Required(ErrorMessage = "Datum ist erforderlich")]
        [Display(Name = "Datum")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Startzeit ist erforderlich")]
        [Display(Name = "Startzeit")]
        public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0);

        [Required(ErrorMessage = "Endzeit ist erforderlich")]
        [Display(Name = "Endzeit")]
        public TimeSpan EndTime { get; set; } = new TimeSpan(16, 0, 0);

        [StringLength(2000, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Beschreibung")]
        public string? Description { get; set; }
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

        var userId = currentUserService.UserId!.Value;

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

        var dto = new CreateManualTimeEntryDto(
            Input.ProjectId,
            startDateTime,
            endDateTime,
            Input.Description);

        var result = await timeEntryService.CreateManualTimeEntryAsync(userId, dto);

        if (result.IsSuccess)
        {
            logger.LogInformation("Manueller Zeiteintrag erstellt für Projekt {ProjectId}", Input.ProjectId);
            TempData["SuccessMessage"] = "Zeiteintrag wurde erfolgreich erstellt.";
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
