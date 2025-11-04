using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.Reports;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IReportService _reportService;
    private readonly IProjectService _projectService;
    private readonly IClientService _clientService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IReportService reportService,
        IProjectService projectService,
        IClientService clientService,
        ICurrentUserService currentUserService,
        ILogger<IndexModel> logger)
    {
        _reportService = reportService;
        _projectService = projectService;
        _clientService = clientService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [BindProperty]
    public DateTime StartDate { get; set; }

    [BindProperty]
    public DateTime EndDate { get; set; }

    [BindProperty]
    public Guid? ProjectId { get; set; }

    [BindProperty]
    public Guid? ClientId { get; set; }

    public IEnumerable<ProjectDto> Projects { get; set; } = Array.Empty<ProjectDto>();
    public IEnumerable<ClientDto> Clients { get; set; } = Array.Empty<ClientDto>();
    public ReportSummaryDto? Summary { get; set; }
    public IEnumerable<TimeEntryReportDto> Entries { get; set; } = Array.Empty<TimeEntryReportDto>();

    public async Task OnGetAsync()
    {
        // Default to current month
        var today = DateTime.Today;
        StartDate = new DateTime(today.Year, today.Month, 1);
        EndDate = StartDate.AddMonths(1).AddDays(-1);

        await LoadDropdownsAsync();
        await LoadReportAsync();
    }

    public async Task<IActionResult> OnPostGenerateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        await LoadDropdownsAsync();
        await LoadReportAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostExportCsvAsync()
    {
        var userId = _currentUserService.UserId!.Value;
        var dto = new GenerateReportDto(StartDate, EndDate, ProjectId, ClientId);

        var result = await _reportService.ExportToCsvAsync(userId, dto);

        if (result.IsSuccess)
        {
            var fileName = $"TiroTime_Report_{StartDate:yyyy-MM-dd}_to_{EndDate:yyyy-MM-dd}.csv";
            return File(result.Value, "text/csv", fileName);
        }

        _logger.LogError("CSV export failed: {Error}", result.Error);
        TempData["ErrorMessage"] = result.Error;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExportExcelAsync()
    {
        var userId = _currentUserService.UserId!.Value;
        var dto = new GenerateReportDto(StartDate, EndDate, ProjectId, ClientId);

        var result = await _reportService.ExportToExcelAsync(userId, dto);

        if (result.IsSuccess)
        {
            var fileName = $"TiroTime_Report_{StartDate:yyyy-MM-dd}_to_{EndDate:yyyy-MM-dd}.xlsx";
            return File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        _logger.LogError("Excel export failed: {Error}", result.Error);
        TempData["ErrorMessage"] = result.Error;
        return RedirectToPage();
    }

    private async Task LoadDropdownsAsync()
    {
        var projectsResult = await _projectService.GetAllProjectsAsync(includeInactive: false);
        if (projectsResult.IsSuccess)
        {
            Projects = projectsResult.Value;
        }

        var clientsResult = await _clientService.GetAllClientsAsync(includeInactive: false);
        if (clientsResult.IsSuccess)
        {
            Clients = clientsResult.Value;
        }
    }

    private async Task LoadReportAsync()
    {
        var userId = _currentUserService.UserId!.Value;
        var dto = new GenerateReportDto(StartDate, EndDate, ProjectId, ClientId);

        var summaryResult = await _reportService.GetReportSummaryAsync(userId, dto);
        if (summaryResult.IsSuccess)
        {
            Summary = summaryResult.Value;
        }

        var entriesResult = await _reportService.GetTimeEntriesReportAsync(userId, dto);
        if (entriesResult.IsSuccess)
        {
            Entries = entriesResult.Value;
        }
    }
}
