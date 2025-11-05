using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Domain.Identity;

namespace TiroTime.Web.Pages.Account;

[Authorize]
public class ProfileModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<ProfileModel> logger) : PageModel
{

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? Email { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Aktuelles Passwort ist erforderlich")]
        [DataType(DataType.Password)]
        [Display(Name = "Aktuelles Passwort")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Neues Passwort ist erforderlich")]
        [StringLength(100, ErrorMessage = "{0} muss mindestens {2} und maximal {1} Zeichen lang sein.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Neues Passwort")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passwortbestätigung ist erforderlich")]
        [DataType(DataType.Password)]
        [Display(Name = "Neues Passwort bestätigen")]
        [Compare("NewPassword", ErrorMessage = "Das neue Passwort und die Bestätigung stimmen nicht überein.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Benutzer konnte nicht geladen werden.");
        }

        Email = user.Email;
        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        if (!ModelState.IsValid)
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                Email = user.Email;
            }
            return Page();
        }

        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return NotFound($"Benutzer konnte nicht geladen werden.");
        }

        Email = currentUser.Email;

        var changePasswordResult = await userManager.ChangePasswordAsync(
            currentUser,
            Input.CurrentPassword,
            Input.NewPassword);

        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        await signInManager.RefreshSignInAsync(currentUser);
        logger.LogInformation("Benutzer hat sein Passwort erfolgreich geändert.");
        TempData["SuccessMessage"] = "Ihr Passwort wurde erfolgreich geändert.";

        return RedirectToPage();
    }
}
