using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TiroTime.Application.Interfaces;

namespace TiroTime.Infrastructure.Services;

public class RecurringEntryGenerationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringEntryGenerationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);
    private DateTime _lastRunDate = DateTime.MinValue;

    public RecurringEntryGenerationService(
        IServiceProvider serviceProvider,
        ILogger<RecurringEntryGenerationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RecurringEntryGenerationService gestartet");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var today = now.Date;

                // Prüfe ob wir heute schon generiert haben
                if (_lastRunDate.Date != today)
                {
                    // Generiere nur einmal pro Tag um 00:30 Uhr oder beim ersten Start nach Mitternacht
                    if (now.Hour == 0 && now.Minute >= 30 || _lastRunDate == DateTime.MinValue)
                    {
                        _logger.LogInformation("Starte Generierung wiederkehrender Zeiteinträge für die nächsten 7 Tage");

                        using var scope = _serviceProvider.CreateScope();
                        var service = scope.ServiceProvider.GetRequiredService<IRecurringTimeEntryService>();

                        var totalGenerated = 0;

                        // Generiere Einträge für heute + nächste 7 Tage
                        for (int i = 0; i <= 7; i++)
                        {
                            var targetDate = today.AddDays(i);
                            var result = await service.GenerateScheduledEntriesAsync(targetDate, stoppingToken);

                            if (result.IsSuccess)
                            {
                                totalGenerated += result.Value;
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Fehler bei Generierung für {Date}: {Error}",
                                    targetDate, result.Error);
                            }
                        }

                        _logger.LogInformation(
                            "Generierung abgeschlossen. {Count} Zeiteinträge erstellt",
                            totalGenerated);

                        _lastRunDate = today;
                    }
                }

                // Warte eine Stunde bis zur nächsten Prüfung
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("RecurringEntryGenerationService wird beendet");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Fehler bei der Generierung wiederkehrender Einträge");

                // Warte 5 Minuten nach einem Fehler
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("RecurringEntryGenerationService gestoppt");
    }
}
