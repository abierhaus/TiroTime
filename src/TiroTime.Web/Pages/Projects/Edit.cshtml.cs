using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.Projects;

[Authorize]
public class EditModel(
    IProjectService projectService,
    ILogger<EditModel> logger) : PageModel
{

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ClientName { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public class InputModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Projektname ist erforderlich")]
        [StringLength(200, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Projektname")]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Beschreibung")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Stundensatz ist erforderlich")]
        [Range(0, 999999, ErrorMessage = "{0} muss zwischen {1} und {2} liegen")]
        [Display(Name = "Stundensatz")]
        public decimal HourlyRate { get; set; }

        [Required(ErrorMessage = "Währung ist erforderlich")]
        [StringLength(3, ErrorMessage = "{0} muss genau {1} Zeichen lang sein")]
        [Display(Name = "Stundensatz-Währung")]
        public string HourlyRateCurrency { get; set; } = "EUR";

        [Range(0, 999999999, ErrorMessage = "{0} muss zwischen {1} und {2} liegen")]
        [Display(Name = "Budget")]
        public decimal? Budget { get; set; }

        [StringLength(3, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Budget-Währung")]
        public string? BudgetCurrency { get; set; }

        [StringLength(7, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Projektfarbe")]
        public string? ColorCode { get; set; }

        [Display(Name = "Startdatum")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Enddatum")]
        public DateTime? EndDate { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var result = await projectService.GetProjectByIdAsync(id);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToPage("./Index");
        }

        var project = result.Value;

        ClientName = project.ClientName;
        IsActive = project.IsActive;

        Input = new InputModel
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            HourlyRate = project.HourlyRate,
            HourlyRateCurrency = project.HourlyRateCurrency,
            Budget = project.Budget,
            BudgetCurrency = project.BudgetCurrency,
            ColorCode = project.ColorCode,
            StartDate = project.StartDate,
            EndDate = project.EndDate
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Fix for empty currency - set default if empty and clear validation errors
        if (string.IsNullOrWhiteSpace(Input.HourlyRateCurrency))
        {
            Input.HourlyRateCurrency = "EUR";
            ModelState.Remove("Input.HourlyRateCurrency");
        }

        if (!ModelState.IsValid)
        {
            // Reload client name
            var projectResult = await projectService.GetProjectByIdAsync(Input.Id);
            if (projectResult.IsSuccess)
            {
                ClientName = projectResult.Value.ClientName;
                IsActive = projectResult.Value.IsActive;
            }

            // Set default again before returning to page
            if (string.IsNullOrWhiteSpace(Input.HourlyRateCurrency))
            {
                Input.HourlyRateCurrency = "EUR";
            }

            return Page();
        }

        // If Budget is provided but BudgetCurrency is empty, use HourlyRateCurrency
        var budgetCurrency = Input.Budget.HasValue
            ? (string.IsNullOrWhiteSpace(Input.BudgetCurrency) ? Input.HourlyRateCurrency : Input.BudgetCurrency)
            : null;

        var dto = new UpdateProjectDto(
            Input.Id,
            Input.Name,
            Input.Description,
            Input.HourlyRate,
            Input.HourlyRateCurrency,
            Input.Budget,
            budgetCurrency,
            Input.ColorCode,
            Input.StartDate,
            Input.EndDate);

        var result = await projectService.UpdateProjectAsync(dto);

        if (result.IsSuccess)
        {
            logger.LogInformation("Projekt '{ProjectName}' wurde aktualisiert", Input.Name);
            TempData["SuccessMessage"] = "Projekt wurde erfolgreich aktualisiert.";
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, result.Error);

        // Reload client name
        var reloadResult = await projectService.GetProjectByIdAsync(Input.Id);
        if (reloadResult.IsSuccess)
        {
            ClientName = reloadResult.Value.ClientName;
            IsActive = reloadResult.Value.IsActive;
        }

        return Page();
    }
}
