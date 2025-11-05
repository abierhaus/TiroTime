using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages;

public class IndexModel(
    ITimeEntryService timeEntryService,
    ICurrentUserService currentUserService,
    ILogger<IndexModel> logger) : PageModel
{
    public TimeEntryStatisticsDto? Statistics { get; set; }

    public async Task OnGetAsync()
    {
        if (currentUserService.IsAuthenticated)
        {
            var userId = currentUserService.UserId!.Value;
            var result = await timeEntryService.GetStatisticsAsync(userId);

            if (result.IsSuccess)
            {
                Statistics = result.Value;
            }
            else
            {
                logger.LogWarning("Fehler beim Laden der Statistiken: {Error}", result.Error);
            }
        }
    }
}
