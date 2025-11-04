using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiroTime.Application.Common;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportService> _logger;

    public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<TimeEntryReportDto>>> GetTimeEntriesReportAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.TimeEntries
                .Include(te => te.Project)
                .ThenInclude(p => p.Client)
                .Where(te => te.UserId == userId
                    && !te.IsRunning
                    && te.StartTime >= dto.StartDate
                    && te.StartTime < dto.EndDate.AddDays(1));

            if (dto.ProjectId.HasValue)
            {
                query = query.Where(te => te.ProjectId == dto.ProjectId.Value);
            }

            if (dto.ClientId.HasValue)
            {
                query = query.Where(te => te.Project.ClientId == dto.ClientId.Value);
            }

            var entries = await query
                .OrderBy(te => te.StartTime)
                .Select(te => new TimeEntryReportDto(
                    te.StartTime.Date,
                    te.Project.Name,
                    te.Project.Client.Name,
                    te.Description,
                    te.StartTime.TimeOfDay,
                    te.EndTime!.Value.TimeOfDay,
                    te.Duration,
                    te.Project.HourlyRate.Amount,
                    te.Project.HourlyRate.Currency,
                    (decimal)te.Duration.TotalHours * te.Project.HourlyRate.Amount))
                .ToListAsync(cancellationToken);

            return Result.Success(entries.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden des Reports");
            return Result.Failure<IEnumerable<TimeEntryReportDto>>("Fehler beim Laden des Reports");
        }
    }

    public async Task<Result<ReportSummaryDto>> GetReportSummaryAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default)
    {
        var entriesResult = await GetTimeEntriesReportAsync(userId, dto, cancellationToken);

        if (!entriesResult.IsSuccess)
        {
            return Result.Failure<ReportSummaryDto>(entriesResult.Error);
        }

        var entries = entriesResult.Value.ToList();

        var projectSummaries = entries
            .GroupBy(e => new { e.ProjectName, e.ClientName, e.Currency })
            .Select(g => new ProjectSummaryDto(
                g.Key.ProjectName,
                g.Key.ClientName,
                g.Count(),
                TimeSpan.FromTicks(g.Sum(e => e.Duration.Ticks)),
                g.Sum(e => e.TotalAmount),
                g.Key.Currency))
            .ToList();

        var summary = new ReportSummaryDto(
            entries.Count,
            TimeSpan.FromTicks(entries.Sum(e => e.Duration.Ticks)),
            entries.Sum(e => e.TotalAmount),
            dto.StartDate,
            dto.EndDate,
            projectSummaries);

        return Result.Success(summary);
    }

    public async Task<Result<byte[]>> ExportToCsvAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default)
    {
        var entriesResult = await GetTimeEntriesReportAsync(userId, dto, cancellationToken);

        if (!entriesResult.IsSuccess)
        {
            return Result.Failure<byte[]>(entriesResult.Error);
        }

        var csv = new StringBuilder();
        csv.AppendLine("Datum;Projekt;Kunde;Beschreibung;Startzeit;Endzeit;Dauer;Stundensatz;Währung;Betrag");

        foreach (var entry in entriesResult.Value)
        {
            csv.AppendLine($"{entry.Date:dd.MM.yyyy};" +
                          $"{EscapeCsv(entry.ProjectName)};" +
                          $"{EscapeCsv(entry.ClientName)};" +
                          $"{EscapeCsv(entry.Description ?? "")};" +
                          $"{entry.StartTime:hh\\:mm};" +
                          $"{entry.EndTime:hh\\:mm};" +
                          $"{entry.Duration:hh\\:mm};" +
                          $"{entry.HourlyRate:F2};" +
                          $"{entry.Currency};" +
                          $"{entry.TotalAmount:F2}");
        }

        return Result.Success(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    public async Task<Result<byte[]>> ExportToExcelAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default)
    {
        var entriesResult = await GetTimeEntriesReportAsync(userId, dto, cancellationToken);

        if (!entriesResult.IsSuccess)
        {
            return Result.Failure<byte[]>(entriesResult.Error);
        }

        var summaryResult = await GetReportSummaryAsync(userId, dto, cancellationToken);

        if (!summaryResult.IsSuccess)
        {
            return Result.Failure<byte[]>(summaryResult.Error);
        }

        using var workbook = new XLWorkbook();

        // Zeiteinträge Sheet
        var entriesSheet = workbook.Worksheets.Add("Zeiteinträge");

        // Header
        entriesSheet.Cell(1, 1).Value = "Datum";
        entriesSheet.Cell(1, 2).Value = "Projekt";
        entriesSheet.Cell(1, 3).Value = "Kunde";
        entriesSheet.Cell(1, 4).Value = "Beschreibung";
        entriesSheet.Cell(1, 5).Value = "Startzeit";
        entriesSheet.Cell(1, 6).Value = "Endzeit";
        entriesSheet.Cell(1, 7).Value = "Dauer (Std)";
        entriesSheet.Cell(1, 8).Value = "Stundensatz";
        entriesSheet.Cell(1, 9).Value = "Währung";
        entriesSheet.Cell(1, 10).Value = "Betrag";

        var headerRange = entriesSheet.Range(1, 1, 1, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data
        var row = 2;
        foreach (var entry in entriesResult.Value)
        {
            entriesSheet.Cell(row, 1).Value = entry.Date.ToString("dd.MM.yyyy");
            entriesSheet.Cell(row, 2).Value = entry.ProjectName;
            entriesSheet.Cell(row, 3).Value = entry.ClientName;
            entriesSheet.Cell(row, 4).Value = entry.Description ?? "";
            entriesSheet.Cell(row, 5).Value = entry.StartTime.ToString(@"hh\:mm");
            entriesSheet.Cell(row, 6).Value = entry.EndTime.ToString(@"hh\:mm");
            entriesSheet.Cell(row, 7).Value = entry.Duration.TotalHours;
            entriesSheet.Cell(row, 7).Style.NumberFormat.Format = "0.00";
            entriesSheet.Cell(row, 8).Value = entry.HourlyRate;
            entriesSheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
            entriesSheet.Cell(row, 9).Value = entry.Currency;
            entriesSheet.Cell(row, 10).Value = entry.TotalAmount;
            entriesSheet.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        entriesSheet.Columns().AdjustToContents();

        // Zusammenfassung Sheet
        var summarySheet = workbook.Worksheets.Add("Zusammenfassung");
        var summary = summaryResult.Value;

        summarySheet.Cell(1, 1).Value = "Berichtszeitraum";
        summarySheet.Cell(1, 1).Style.Font.Bold = true;
        summarySheet.Cell(1, 2).Value = $"{summary.StartDate:dd.MM.yyyy} - {summary.EndDate:dd.MM.yyyy}";

        summarySheet.Cell(3, 1).Value = "Gesamteinträge:";
        summarySheet.Cell(3, 2).Value = summary.TotalEntries;

        summarySheet.Cell(4, 1).Value = "Gesamtstunden:";
        summarySheet.Cell(4, 2).Value = summary.TotalDuration.TotalHours;
        summarySheet.Cell(4, 2).Style.NumberFormat.Format = "0.00";

        summarySheet.Cell(5, 1).Value = "Gesamtbetrag:";
        summarySheet.Cell(5, 2).Value = summary.TotalAmount;
        summarySheet.Cell(5, 2).Style.NumberFormat.Format = "#,##0.00";

        // Projekt-Zusammenfassung
        summarySheet.Cell(7, 1).Value = "Projekt";
        summarySheet.Cell(7, 2).Value = "Kunde";
        summarySheet.Cell(7, 3).Value = "Einträge";
        summarySheet.Cell(7, 4).Value = "Stunden";
        summarySheet.Cell(7, 5).Value = "Betrag";
        summarySheet.Cell(7, 6).Value = "Währung";

        var summaryHeaderRange = summarySheet.Range(7, 1, 7, 6);
        summaryHeaderRange.Style.Font.Bold = true;
        summaryHeaderRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        row = 8;
        foreach (var projectSummary in summary.ProjectSummaries)
        {
            summarySheet.Cell(row, 1).Value = projectSummary.ProjectName;
            summarySheet.Cell(row, 2).Value = projectSummary.ClientName;
            summarySheet.Cell(row, 3).Value = projectSummary.EntryCount;
            summarySheet.Cell(row, 4).Value = projectSummary.TotalDuration.TotalHours;
            summarySheet.Cell(row, 4).Style.NumberFormat.Format = "0.00";
            summarySheet.Cell(row, 5).Value = projectSummary.TotalAmount;
            summarySheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            summarySheet.Cell(row, 6).Value = projectSummary.Currency;
            row++;
        }

        summarySheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Result.Success(stream.ToArray());
    }

    public async Task<Result<(byte[] Data, string FileName)>> ExportDetailedEntriesToExcelAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default)
    {
        var entriesResult = await GetTimeEntriesReportAsync(userId, dto, cancellationToken);

        if (!entriesResult.IsSuccess)
        {
            return Result.Failure<(byte[], string)>(entriesResult.Error);
        }

        var entries = entriesResult.Value.ToList();
        var fileName = GenerateFileName(entries, dto.StartDate, dto.EndDate, "xlsx");

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Zeiteinträge");

        // Set landscape orientation
        sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        sheet.PageSetup.FitToPages(1, 0); // Fit to 1 page wide

        // Header (without "Betrag" and "Stundensatz" columns)
        sheet.Cell(1, 1).Value = "Datum";
        sheet.Cell(1, 2).Value = "Projekt";
        sheet.Cell(1, 3).Value = "Kunde";
        sheet.Cell(1, 4).Value = "Beschreibung";
        sheet.Cell(1, 5).Value = "Von";
        sheet.Cell(1, 6).Value = "Bis";
        sheet.Cell(1, 7).Value = "Dauer (Std)";
        sheet.Cell(1, 8).Value = "Währung";

        var headerRange = sheet.Range(1, 1, 1, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data
        var row = 2;
        foreach (var entry in entries)
        {
            sheet.Cell(row, 1).Value = entry.Date.ToString("dd.MM.yyyy");
            sheet.Cell(row, 2).Value = entry.ProjectName;
            sheet.Cell(row, 3).Value = entry.ClientName;
            sheet.Cell(row, 4).Value = entry.Description ?? "";
            sheet.Cell(row, 5).Value = entry.StartTime.ToString(@"hh\:mm");
            sheet.Cell(row, 6).Value = entry.EndTime.ToString(@"hh\:mm");
            sheet.Cell(row, 7).Value = entry.Duration.TotalHours;
            sheet.Cell(row, 7).Style.NumberFormat.Format = "0.00";
            sheet.Cell(row, 8).Value = entry.Currency;
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Result.Success((stream.ToArray(), fileName));
    }

    public async Task<Result<(byte[] Data, string FileName)>> ExportDetailedEntriesToPdfAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default)
    {
        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;

        var entriesResult = await GetTimeEntriesReportAsync(userId, dto, cancellationToken);

        if (!entriesResult.IsSuccess)
        {
            return Result.Failure<(byte[], string)>(entriesResult.Error);
        }

        var entries = entriesResult.Value.ToList();
        var fileName = GenerateFileName(entries, dto.StartDate, dto.EndDate, "pdf");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Text($"Zeiteinträge: {dto.StartDate:dd.MM.yyyy} - {dto.EndDate:dd.MM.yyyy}")
                    .FontSize(14)
                    .Bold()
                    .AlignCenter();

                page.Content().Table(table =>
                {
                    // Define columns (without "Betrag" and "Stundensatz" columns)
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.5f); // Datum
                        columns.RelativeColumn(2);    // Projekt
                        columns.RelativeColumn(2);    // Kunde
                        columns.RelativeColumn(3);    // Beschreibung
                        columns.RelativeColumn(1);    // Von
                        columns.RelativeColumn(1);    // Bis
                        columns.RelativeColumn(1.2f); // Dauer
                        columns.RelativeColumn(1);    // Währung
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Datum").Bold();
                        header.Cell().Element(CellStyle).Text("Projekt").Bold();
                        header.Cell().Element(CellStyle).Text("Kunde").Bold();
                        header.Cell().Element(CellStyle).Text("Beschreibung").Bold();
                        header.Cell().Element(CellStyle).Text("Von").Bold();
                        header.Cell().Element(CellStyle).Text("Bis").Bold();
                        header.Cell().Element(CellStyle).Text("Dauer").Bold();
                        header.Cell().Element(CellStyle).Text("Währung").Bold();

                        static IContainer CellStyle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Background(Colors.Grey.Lighten3)
                                .Padding(5);
                        }
                    });

                    // Data rows
                    foreach (var entry in entries)
                    {
                        table.Cell().Element(CellStyle).Text(entry.Date.ToString("dd.MM.yyyy"));
                        table.Cell().Element(CellStyle).Text(entry.ProjectName);
                        table.Cell().Element(CellStyle).Text(entry.ClientName);
                        table.Cell().Element(CellStyle).Text(entry.Description ?? "-");
                        table.Cell().Element(CellStyle).Text(entry.StartTime.ToString(@"hh\:mm"));
                        table.Cell().Element(CellStyle).Text(entry.EndTime.ToString(@"hh\:mm"));
                        table.Cell().Element(CellStyle).Text(entry.Duration.ToString(@"hh\:mm"));
                        table.Cell().Element(CellStyle).Text(entry.Currency);

                        static IContainer CellStyle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(5);
                        }
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Seite ");
                        text.CurrentPageNumber();
                        text.Span(" von ");
                        text.TotalPages();
                    });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return Result.Success((pdfBytes, fileName));
    }

    private static string GenerateFileName(
        IEnumerable<TimeEntryReportDto> entries,
        DateTime startDate,
        DateTime endDate,
        string extension)
    {
        var entryList = entries.ToList();

        // Get unique client names
        var clientNames = entryList.Select(e => e.ClientName).Distinct().ToList();

        // Use client name if only one, otherwise "Alle"
        var clientPart = clientNames.Count == 1 ? clientNames[0] : "Alle";

        // Sanitize filename
        clientPart = SanitizeFileName(clientPart);

        return $"{clientPart}-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.{extension}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
