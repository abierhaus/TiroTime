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

public class ReportService(ApplicationDbContext context, ILogger<ReportService> logger) : IReportService
{
    public async Task<Result<IEnumerable<TimeEntryReportDto>>> GetTimeEntriesReportAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entries = await LoadEntriesAsync(userId, dto, cancellationToken);
            return Result.Success<IEnumerable<TimeEntryReportDto>>(entries);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler beim Laden des Reports");
            return Result.Failure<IEnumerable<TimeEntryReportDto>>("Fehler beim Laden des Reports");
        }
    }

    public async Task<Result<ReportSummaryDto>> GetReportSummaryAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default)
    {
        var entriesResult = await GetTimeEntriesReportAsync(userId, dto, cancellationToken);

        return entriesResult.IsSuccess
            ? Result.Success(BuildSummary(entriesResult.Value.ToList(), dto))
            : Result.Failure<ReportSummaryDto>(entriesResult.Error);
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
            csv.Append(entry.Date.ToString("dd.MM.yyyy")).Append(';')
               .Append(EscapeCsv(entry.ProjectName)).Append(';')
               .Append(EscapeCsv(entry.ClientName)).Append(';')
               .Append(EscapeCsv(entry.Description ?? "")).Append(';')
               .Append(entry.StartTime.ToString(@"hh\:mm")).Append(';')
               .Append(entry.EndTime.ToString(@"hh\:mm")).Append(';')
               .Append(entry.Duration.ToString(@"hh\:mm")).Append(';')
               .Append(entry.HourlyRate.ToString("F2")).Append(';')
               .Append(entry.Currency).Append(';')
               .Append(entry.TotalAmount.ToString("F2"))
               .AppendLine();
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

        // Einträge nur einmal laden; die Zusammenfassung wird daraus berechnet
        var entries = entriesResult.Value.ToList();
        var summary = BuildSummary(entries, dto);

        using var workbook = new XLWorkbook();

        // Zeiteinträge Sheet
        var entriesSheet = workbook.Worksheets.Add("Zeiteinträge");

        WriteHeader(entriesSheet, 1, "Datum", "Projekt", "Kunde", "Beschreibung", "Startzeit", "Endzeit", "Dauer (Std)", "Stundensatz", "Währung", "Betrag");

        var row = 2;
        foreach (var entry in entries)
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

        WriteHeader(summarySheet, 7, "Projekt", "Kunde", "Einträge", "Stunden", "Betrag", "Währung");

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

        sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        sheet.PageSetup.FitToPages(1, 0); // Fit to 1 page wide

        // Header (without "Betrag", "Stundensatz" and "Währung" columns)
        WriteHeader(sheet, 1, "Datum", "Projekt", "Kunde", "Beschreibung", "Von", "Bis", "Dauer (Std)");

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
                    // Define columns (without "Betrag", "Stundensatz" and "Währung" columns)
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.5f); // Datum
                        columns.RelativeColumn(2);    // Projekt
                        columns.RelativeColumn(2);    // Kunde
                        columns.RelativeColumn(3);    // Beschreibung
                        columns.RelativeColumn(1);    // Von
                        columns.RelativeColumn(1);    // Bis
                        columns.RelativeColumn(1.2f); // Dauer
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCellStyle).Text("Datum").Bold();
                        header.Cell().Element(HeaderCellStyle).Text("Projekt").Bold();
                        header.Cell().Element(HeaderCellStyle).Text("Kunde").Bold();
                        header.Cell().Element(HeaderCellStyle).Text("Beschreibung").Bold();
                        header.Cell().Element(HeaderCellStyle).Text("Von").Bold();
                        header.Cell().Element(HeaderCellStyle).Text("Bis").Bold();
                        header.Cell().Element(HeaderCellStyle).Text("Dauer").Bold();
                    });

                    foreach (var entry in entries)
                    {
                        table.Cell().Element(BodyCellStyle).Text(entry.Date.ToString("dd.MM.yyyy"));
                        table.Cell().Element(BodyCellStyle).Text(entry.ProjectName);
                        table.Cell().Element(BodyCellStyle).Text(entry.ClientName);
                        table.Cell().Element(BodyCellStyle).Text(entry.Description ?? "-");
                        table.Cell().Element(BodyCellStyle).Text(entry.StartTime.ToString(@"hh\:mm"));
                        table.Cell().Element(BodyCellStyle).Text(entry.EndTime.ToString(@"hh\:mm"));
                        table.Cell().Element(BodyCellStyle).Text(entry.Duration.ToString(@"hh\:mm"));
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

    /// <summary>
    /// Lädt die Einträge als Projektion direkt aus der Datenbank (keine Entities, kein Change Tracking).
    /// </summary>
    private async Task<List<TimeEntryReportDto>> LoadEntriesAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken)
    {
        var endExclusive = dto.EndDate.AddDays(1);

        var query = context.TimeEntries
            .AsNoTracking()
            .Where(te => te.UserId == userId
                && !te.IsRunning
                && te.StartTime >= dto.StartDate
                && te.StartTime < endExclusive);

        if (dto.ProjectId.HasValue)
        {
            query = query.Where(te => te.ProjectId == dto.ProjectId.Value);
        }

        if (dto.ClientId.HasValue)
        {
            query = query.Where(te => te.Project!.ClientId == dto.ClientId.Value);
        }

        return await query
            .OrderBy(te => te.StartTime)
            .Select(te => new TimeEntryReportDto(
                te.StartTime.Date,
                te.Project!.Name,
                te.Project.Client!.Name,
                te.Description,
                te.StartTime.TimeOfDay,
                te.EndTime!.Value.TimeOfDay,
                te.Duration,
                te.Project.HourlyRate.Amount,
                te.Project.HourlyRate.Currency,
                (decimal)te.Duration.TotalHours * te.Project.HourlyRate.Amount))
            .ToListAsync(cancellationToken);
    }

    private static ReportSummaryDto BuildSummary(List<TimeEntryReportDto> entries, GenerateReportDto dto)
    {
        var clientSummaries = entries
            .GroupBy(e => e.ClientName)
            .Select(g => new ClientSummaryDto(
                g.Key,
                g.Count(),
                TimeSpan.FromTicks(g.Sum(e => e.Duration.Ticks)),
                g.Sum(e => e.TotalAmount)))
            .ToList();

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

        return new ReportSummaryDto(
            entries.Count,
            TimeSpan.FromTicks(entries.Sum(e => e.Duration.Ticks)),
            entries.Sum(e => e.TotalAmount),
            dto.StartDate,
            dto.EndDate,
            clientSummaries,
            projectSummaries);
    }

    private static void WriteHeader(IXLWorksheet sheet, int row, params string[] titles)
    {
        for (var i = 0; i < titles.Length; i++)
        {
            sheet.Cell(row, i + 1).Value = titles[i];
        }

        var headerRange = sheet.Range(row, 1, row, titles.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
    }

    private static IContainer HeaderCellStyle(IContainer container) =>
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten3)
            .Padding(5);

    private static IContainer BodyCellStyle(IContainer container) =>
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5);

    private static string GenerateFileName(
        IReadOnlyCollection<TimeEntryReportDto> entries,
        DateTime startDate,
        DateTime endDate,
        string extension)
    {
        var clientNames = entries.Select(e => e.ClientName).Distinct().ToList();

        // Use client name if only one, otherwise "Alle"
        var clientPart = SanitizeFileName(clientNames.Count == 1 ? clientNames[0] : "Alle");

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
