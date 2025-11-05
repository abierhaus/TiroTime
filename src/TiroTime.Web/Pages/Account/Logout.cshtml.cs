using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Domain.Identity;

namespace TiroTime.Web.Pages.Account;

public class LogoutModel(
    SignInManager<ApplicationUser> signInManager,
    ILogger<LogoutModel> logger) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("Benutzer hat sich abgemeldet.");
        return RedirectToPage("/Index");
    }
}
