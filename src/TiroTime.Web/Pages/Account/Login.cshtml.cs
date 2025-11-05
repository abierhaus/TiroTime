using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Domain.Identity;

namespace TiroTime.Web.Pages.Account;

public class LoginModel(
    SignInManager<ApplicationUser> signInManager,
    ILogger<LoginModel> logger) : PageModel
{

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "E-Mail-Adresse ist erforderlich")]
        [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse")]
        [Display(Name = "E-Mail-Adresse")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passwort ist erforderlich")]
        [DataType(DataType.Password)]
        [Display(Name = "Passwort")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Angemeldet bleiben")]
        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            Input.Email,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("Benutzer hat sich angemeldet.");
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("Benutzerkonto ist gesperrt.");
            ModelState.AddModelError(string.Empty, "Ihr Konto ist gesperrt. Bitte versuchen Sie es später erneut.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Ungültige Anmeldedaten.");
        return Page();
    }
}
