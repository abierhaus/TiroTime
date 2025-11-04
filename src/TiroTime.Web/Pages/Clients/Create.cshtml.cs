using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.Clients;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IClientService _clientService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        IClientService clientService,
        ILogger<CreateModel> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Kundenname ist erforderlich")]
        [StringLength(200, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Kundenname")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Kontaktperson")]
        public string? ContactPerson { get; set; }

        [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse")]
        [StringLength(255, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "E-Mail-Adresse")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Telefonnummer")]
        public string? PhoneNumber { get; set; }

        [StringLength(200, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Straße und Hausnummer")]
        public string? AddressStreet { get; set; }

        [StringLength(100, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Stadt")]
        public string? AddressCity { get; set; }

        [StringLength(20, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Postleitzahl")]
        public string? AddressPostalCode { get; set; }

        [StringLength(100, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Land")]
        public string? AddressCountry { get; set; }

        [StringLength(50, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Steuernummer / USt-IdNr.")]
        public string? TaxId { get; set; }

        [StringLength(2000, ErrorMessage = "{0} darf maximal {1} Zeichen lang sein")]
        [Display(Name = "Notizen")]
        public string? Notes { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var dto = new CreateClientDto(
            Input.Name,
            Input.ContactPerson,
            Input.Email,
            Input.PhoneNumber,
            Input.AddressStreet,
            Input.AddressCity,
            Input.AddressPostalCode,
            Input.AddressCountry,
            Input.TaxId,
            Input.Notes);

        var result = await _clientService.CreateClientAsync(dto);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Kunde '{ClientName}' wurde erstellt", Input.Name);
            TempData["SuccessMessage"] = "Kunde wurde erfolgreich erstellt.";
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, result.Error);
        return Page();
    }
}
