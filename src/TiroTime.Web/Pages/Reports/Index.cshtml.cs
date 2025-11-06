using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.Reports;

[Authorize]
public class IndexModel(
    IReportService reportService,
    IProjectService projectService,
    IClientService clientService,
    ICurrentUserService currentUserService,
    IEmailService emailService,
    ILogger<IndexModel> logger) : PageModel
{
    [BindProperty]
    public DateTime StartDate { get; set; }

    [BindProperty]
    public DateTime EndDate { get; set; }

    [BindProperty]
    public Guid? ProjectId { get; set; }

    [BindProperty]
    public Guid? ClientId { get; set; }

    public IEnumerable<ProjectDto> Projects { get; set; } = [];
    public IEnumerable<ClientDto> Clients { get; set; } = [];
    public ReportSummaryDto? Summary { get; set; }
    public IEnumerable<TimeEntryReportDto> Entries { get; set; } = [];

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
        var userId = currentUserService.UserId!.Value;
        var dto = new GenerateReportDto(StartDate, EndDate, ProjectId, ClientId);

        var result = await reportService.ExportToCsvAsync(userId, dto);

        if (result.IsSuccess)
        {
            var fileName = $"TiroTime_Report_{StartDate:yyyy-MM-dd}_to_{EndDate:yyyy-MM-dd}.csv";
            return File(result.Value, "text/csv", fileName);
        }

        logger.LogError("CSV export failed: {Error}", result.Error);
        TempData["ErrorMessage"] = result.Error;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExportExcelAsync()
    {
        var userId = currentUserService.UserId!.Value;
        var dto = new GenerateReportDto(StartDate, EndDate, ProjectId, ClientId);

        var result = await reportService.ExportToExcelAsync(userId, dto);

        if (result.IsSuccess)
        {
            var fileName = $"TiroTime_Report_{StartDate:yyyy-MM-dd}_to_{EndDate:yyyy-MM-dd}.xlsx";
            return File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        logger.LogError("Excel export failed: {Error}", result.Error);
        TempData["ErrorMessage"] = result.Error;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExportDetailedExcelAsync()
    {
        var userId = currentUserService.UserId!.Value;
        var dto = new GenerateReportDto(StartDate, EndDate, ProjectId, ClientId);

        var result = await reportService.ExportDetailedEntriesToExcelAsync(userId, dto);

        if (result.IsSuccess)
        {
            return File(result.Value.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Value.FileName);
        }

        logger.LogError("Detailed Excel export failed: {Error}", result.Error);
        TempData["ErrorMessage"] = result.Error;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExportDetailedPdfAsync()
    {
        var userId = currentUserService.UserId!.Value;
        var dto = new GenerateReportDto(StartDate, EndDate, ProjectId, ClientId);

        var result = await reportService.ExportDetailedEntriesToPdfAsync(userId, dto);

        if (result.IsSuccess)
        {
            return File(result.Value.Data, "application/pdf", result.Value.FileName);
        }

        logger.LogError("Detailed PDF export failed: {Error}", result.Error);
        TempData["ErrorMessage"] = result.Error;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendReportByEmailAsync()
    {
        var userId = currentUserService.UserId!.Value;
        var userEmail = currentUserService.Email;

        if (string.IsNullOrEmpty(userEmail))
        {
            TempData["ErrorMessage"] = "E-Mail-Adresse des Benutzers konnte nicht ermittelt werden.";
            return RedirectToPage();
        }

        var dto = new GenerateReportDto(StartDate, EndDate, ProjectId, ClientId);

        // Generate Excel file
        var excelResult = await reportService.ExportDetailedEntriesToExcelAsync(userId, dto);
        if (!excelResult.IsSuccess)
        {
            logger.LogError("Excel generation failed: {Error}", excelResult.Error);
            TempData["ErrorMessage"] = $"Excel-Export fehlgeschlagen: {excelResult.Error}";
            return RedirectToPage();
        }

        // Generate PDF file
        var pdfResult = await reportService.ExportDetailedEntriesToPdfAsync(userId, dto);
        if (!pdfResult.IsSuccess)
        {
            logger.LogError("PDF generation failed: {Error}", pdfResult.Error);
            TempData["ErrorMessage"] = $"PDF-Export fehlgeschlagen: {pdfResult.Error}";
            return RedirectToPage();
        }

        // Prepare email attachments
        var attachments = new List<EmailAttachmentDto>
        {
            new(excelResult.Value.FileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelResult.Value.Data),
            new(pdfResult.Value.FileName, "application/pdf", pdfResult.Value.Data)
        };

        // Prepare email content
        var subject = $"TiroTime Bericht: {StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";
        var textBody = $"Ihr TiroTime Bericht für den Zeitraum {StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy} ist bereit.\n\n" +
                       "Die Berichte sind als Excel- und PDF-Datei angehängt.\n\n" +
                       "Mit freundlichen Grüßen,\nIhr TiroTime Team";
        var htmlBody = $"<html><body>" +
                       $"<h2>TiroTime Bericht</h2>" +
                       $"<p>Ihr TiroTime Bericht für den Zeitraum <strong>{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}</strong> ist bereit.</p>" +
                       $"<p>Die Berichte sind als Excel- und PDF-Datei angehängt.</p>" +
                       $"<br><p>Mit freundlichen Grüßen,<br>Ihr TiroTime Team</p>" +
                       $"</body></html>";

        var emailDto = new SendEmailDto(userEmail, userEmail, subject, textBody, htmlBody, attachments);

        // Send email
        var emailResult = await emailService.SendEmailWithAttachmentsAsync(emailDto);

        if (emailResult.IsSuccess)
        {
            logger.LogInformation("Report sent by email to {Email}", userEmail);
            TempData["SuccessMessage"] = $"Bericht wurde erfolgreich an {userEmail} gesendet.";
        }
        else
        {
            logger.LogError("Email sending failed: {Error}", emailResult.Error);
            TempData["ErrorMessage"] = emailResult.Error;
        }

        return RedirectToPage();
    }

    private async Task LoadDropdownsAsync()
    {
        var projectsResult = await projectService.GetAllProjectsAsync(includeInactive: false);
        if (projectsResult.IsSuccess)
        {
            Projects = projectsResult.Value;
        }

        var clientsResult = await clientService.GetAllClientsAsync(includeInactive: false);
        if (clientsResult.IsSuccess)
        {
            Clients = clientsResult.Value;
        }
    }

    private async Task LoadReportAsync()
    {
        var userId = currentUserService.UserId!.Value;
        var dto = new GenerateReportDto(StartDate, EndDate, ProjectId, ClientId);

        var summaryResult = await reportService.GetReportSummaryAsync(userId, dto);
        if (summaryResult.IsSuccess)
        {
            Summary = summaryResult.Value;
        }

        var entriesResult = await reportService.GetTimeEntriesReportAsync(userId, dto);
        if (entriesResult.IsSuccess)
        {
            Entries = entriesResult.Value;
        }
    }
}
