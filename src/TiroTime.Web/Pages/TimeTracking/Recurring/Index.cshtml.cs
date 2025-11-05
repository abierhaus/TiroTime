using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.TimeTracking.Recurring;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IRecurringTimeEntryService _recurringService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IRecurringTimeEntryService recurringService,
        ICurrentUserService currentUserService,
        ILogger<IndexModel> logger)
    {
        _recurringService = recurringService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public IEnumerable<RecurringTimeEntryDto> RecurringEntries { get; set; } = Array.Empty<RecurringTimeEntryDto>();

    [BindProperty(SupportsGet = true)]
    public bool ShowInactive { get; set; }

    public async Task OnGetAsync()
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _recurringService.GetAllAsync(userId, ShowInactive);

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
        var userId = _currentUserService.UserId!.Value;

        var result = isActive
            ? await _recurringService.DeactivateAsync(userId, id)
            : await _recurringService.ActivateAsync(userId, id);

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

        return RedirectToPage();
    }
}
