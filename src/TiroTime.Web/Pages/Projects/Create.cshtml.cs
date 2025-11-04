using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.Projects;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IProjectService _projectService;
    private readonly IClientService _clientService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        IProjectService projectService,
        IClientService clientService,
        ILogger<CreateModel> logger)
    {
        _projectService = projectService;
        _clientService = clientService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ClientSelectList { get; set; } = new SelectList(Array.Empty<object>());

    public class InputModel
    {
        [Required(ErrorMessage = "Projektname ist erforderlich")]
        [StringLength(200, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Projektname")]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Beschreibung")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Kunde ist erforderlich")]
        [Display(Name = "Kunde")]
        public Guid ClientId { get; set; }

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

    public async Task OnGetAsync()
    {
        // Ensure default values are set
        Input.HourlyRateCurrency = "EUR";
        Input.BudgetCurrency = "EUR";

        await LoadClientsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("=== OnPostAsync wurde aufgerufen ===");
        _logger.LogInformation("Input.HourlyRateCurrency: '{Currency}' (Länge: {Length})",
            Input.HourlyRateCurrency ?? "NULL",
            Input.HourlyRateCurrency?.Length ?? 0);

        // Fix for empty currency - set default if empty and clear validation errors
        if (string.IsNullOrWhiteSpace(Input.HourlyRateCurrency))
        {
            _logger.LogWarning("HourlyRateCurrency war leer, setze auf EUR");
            Input.HourlyRateCurrency = "EUR";
            ModelState.Remove("Input.HourlyRateCurrency");
        }

        _logger.LogInformation("ModelState.IsValid: {IsValid}", ModelState.IsValid);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState ist ungültig");
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key]?.Errors;
                if (errors != null && errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        _logger.LogWarning("Validierungsfehler für {Key}: {Error}", key, error.ErrorMessage);
                    }
                }
            }

            // Set default again before returning to page
            if (string.IsNullOrWhiteSpace(Input.HourlyRateCurrency))
            {
                Input.HourlyRateCurrency = "EUR";
            }

            await LoadClientsAsync();
            return Page();
        }

        // If Budget is provided but BudgetCurrency is empty, use HourlyRateCurrency
        var budgetCurrency = Input.Budget.HasValue
            ? (string.IsNullOrWhiteSpace(Input.BudgetCurrency) ? Input.HourlyRateCurrency : Input.BudgetCurrency)
            : null;

        var dto = new CreateProjectDto(
            Input.Name,
            Input.Description,
            Input.ClientId,
            Input.HourlyRate,
            Input.HourlyRateCurrency,
            Input.Budget,
            budgetCurrency,
            Input.ColorCode,
            Input.StartDate,
            Input.EndDate);

        var result = await _projectService.CreateProjectAsync(dto);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Projekt '{ProjectName}' wurde erstellt", Input.Name);
            TempData["SuccessMessage"] = "Projekt wurde erfolgreich erstellt.";
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, result.Error);
        await LoadClientsAsync();
        return Page();
    }

    private async Task LoadClientsAsync()
    {
        var clientsResult = await _clientService.GetAllClientsAsync(includeInactive: false);

        if (clientsResult.IsSuccess)
        {
            ClientSelectList = new SelectList(clientsResult.Value, "Id", "Name");
        }
    }
}
