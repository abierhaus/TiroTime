using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        ITimeEntryService timeEntryService,
        ICurrentUserService currentUserService,
        ILogger<IndexModel> logger)
    {
        _timeEntryService = timeEntryService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public TimeEntryStatisticsDto? Statistics { get; set; }

    public async Task OnGetAsync()
    {
        if (_currentUserService.IsAuthenticated)
        {
            var userId = _currentUserService.UserId!.Value;
            var result = await _timeEntryService.GetStatisticsAsync(userId);

            if (result.IsSuccess)
            {
                Statistics = result.Value;
            }
            else
            {
                _logger.LogWarning("Fehler beim Laden der Statistiken: {Error}", result.Error);
            }
        }
    }
}
