using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TiroTime.Application.Interfaces;

namespace TiroTime.Infrastructure.Services;

/// <summary>
/// Erzeugt einmal täglich (bzw. direkt nach dem Start) die Zeiteinträge aus aktiven Wiederholungen
/// für heute und die nächsten 7 Tage.
/// </summary>
public sealed class RecurringEntryGenerationService(
    IServiceProvider serviceProvider,
    ILogger<RecurringEntryGenerationService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan ErrorBackoff = TimeSpan.FromMinutes(5);
    private const int DaysAhead = 7;

    private DateTime _lastRunDate = DateTime.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RecurringEntryGenerationService gestartet");

        using var timer = new PeriodicTimer(CheckInterval);

        try
        {
            do
            {
                try
                {
                    await RunIfDueAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Fehler bei der Generierung wiederkehrender Einträge");
                    await Task.Delay(ErrorBackoff, stoppingToken);
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // regulärer Shutdown
        }

        logger.LogInformation("RecurringEntryGenerationService gestoppt");
    }

    private async Task RunIfDueAsync(CancellationToken stoppingToken)
    {
        var now = DateTime.Now;
        var today = now.Date;

        if (_lastRunDate.Date == today)
        {
            return;
        }

        // Nur einmal pro Tag ab 00:30 Uhr – oder beim ersten Lauf nach dem Start
        var isFirstRun = _lastRunDate == DateTime.MinValue;
        if (!isFirstRun && !(now.Hour == 0 && now.Minute >= 30))
        {
            return;
        }

        logger.LogInformation("Starte Generierung wiederkehrender Zeiteinträge für die nächsten {Days} Tage", DaysAhead);

        using var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRecurringTimeEntryService>();

        var totalGenerated = 0;

        for (var i = 0; i <= DaysAhead; i++)
        {
            var targetDate = today.AddDays(i);
            var result = await service.GenerateScheduledEntriesAsync(targetDate, stoppingToken);

            if (result.IsSuccess)
            {
                totalGenerated += result.Value;
            }
            else
            {
                logger.LogWarning("Fehler bei Generierung für {Date}: {Error}", targetDate, result.Error);
            }
        }

        logger.LogInformation("Generierung abgeschlossen. {Count} Zeiteinträge erstellt", totalGenerated);

        _lastRunDate = today;
    }
}
