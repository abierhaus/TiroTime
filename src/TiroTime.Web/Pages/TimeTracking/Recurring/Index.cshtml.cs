using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.TimeTracking.Recurring;

[Authorize]
public class IndexModel(
    IRecurringTimeEntryService recurringService,
    ICurrentUserService currentUserService,
    ILogger<IndexModel> logger) : PageModel
{
    public IEnumerable<RecurringTimeEntryDto> RecurringEntries { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public bool ShowInactive { get; set; }

    public async Task OnGetAsync()
    {
        var userId = currentUserService.UserId!.Value;
        var result = await recurringService.GetAllAsync(userId, ShowInactive);

        if (result.IsSuccess)
        {
            RecurringEntries = result.Value;
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(Guid id, bool isActive)
    {
        var userId = currentUserService.UserId!.Value;

        var result = isActive
            ? await recurringService.DeactivateAsync(userId, id)
            : await recurringService.ActivateAsync(userId, id);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = isActive
                ? "Wiederholung wurde deaktiviert."
                : "Wiederholung wurde aktiviert.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var userId = currentUserService.UserId!.Value;
        var result = await recurringService.DeleteAsync(userId, id);

        if (result.IsSuccess)
        {
            logger.LogInformation("Wiederkehrende Zeiterfassung gelöscht: {Id}", id);
            TempData["SuccessMessage"] = "Wiederholung wurde gelöscht.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToPage();
    }
}
