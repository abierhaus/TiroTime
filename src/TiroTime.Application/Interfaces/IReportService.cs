using TiroTime.Application.Common;
using TiroTime.Application.DTOs;

namespace TiroTime.Application.Interfaces;

public interface IReportService
{
    Task<Result<IEnumerable<TimeEntryReportDto>>> GetTimeEntriesReportAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<ReportSummaryDto>> GetReportSummaryAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<byte[]>> ExportToCsvAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<byte[]>> ExportToExcelAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<(byte[] Data, string FileName)>> ExportDetailedEntriesToExcelAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<(byte[] Data, string FileName)>> ExportDetailedEntriesToPdfAsync(
        Guid userId,
        GenerateReportDto dto,
        CancellationToken cancellationToken = default);
}
