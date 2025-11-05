using TiroTime.Domain.Common;
using TiroTime.Domain.Enums;
using TiroTime.Domain.Exceptions;

namespace TiroTime.Domain.ValueObjects;

public class RecurringPattern : ValueObject
{
    public RecurrenceType Frequency { get; private set; }
    public int Interval { get; private set; }
    public DayOfWeek[]? DaysOfWeek { get; private set; }
    public int? DayOfMonth { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public int? MaxOccurrences { get; private set; }

    private RecurringPattern() { }

    private RecurringPattern(
        RecurrenceType frequency,
        int interval,
        DayOfWeek[]? daysOfWeek,
        int? dayOfMonth,
        DateTime startDate,
        DateTime? endDate,
        int? maxOccurrences)
    {
        Frequency = frequency;
        Interval = interval;
        DaysOfWeek = daysOfWeek;
        DayOfMonth = dayOfMonth;
        StartDate = startDate;
        EndDate = endDate;
        MaxOccurrences = maxOccurrences;
    }

    public static RecurringPattern Create(
        RecurrenceType frequency,
        int interval,
        DateTime startDate,
        DayOfWeek[]? daysOfWeek = null,
        int? dayOfMonth = null,
        DateTime? endDate = null,
        int? maxOccurrences = null)
    {
        // Validierung: Interval
        if (interval < 1)
            throw new DomainException("Interval muss mindestens 1 sein");

        // Validierung: EndDate und MaxOccurrences
        if (endDate.HasValue && maxOccurrences.HasValue)
            throw new DomainException("EndDate und MaxOccurrences können nicht gleichzeitig angegeben werden");

        if (endDate.HasValue && endDate.Value <= startDate)
            throw new DomainException("EndDate muss nach StartDate liegen");

        if (maxOccurrences.HasValue && maxOccurrences.Value < 1)
            throw new DomainException("MaxOccurrences muss mindestens 1 sein");

        // Validierung: Frequency-spezifisch
        if (frequency == RecurrenceType.Weekly)
        {
            if (daysOfWeek == null || daysOfWeek.Length == 0)
                throw new DomainException("Bei wöchentlicher Wiederholung muss mindestens ein Wochentag angegeben werden");
        }

        if (frequency == RecurrenceType.Monthly)
        {
            if (!dayOfMonth.HasValue)
                throw new DomainException("Bei monatlicher Wiederholung muss ein Tag im Monat angegeben werden");

            if (dayOfMonth.Value < 1 || dayOfMonth.Value > 31)
                throw new DomainException("DayOfMonth muss zwischen 1 und 31 liegen");
        }

        return new RecurringPattern(
            frequency,
            interval,
            daysOfWeek,
            dayOfMonth,
            startDate.Date,
            endDate?.Date,
            maxOccurrences);
    }

    public DateTime? GetNextOccurrence(DateTime fromDate, int currentOccurrenceCount = 0)
    {
        // Prüfe ob abgelaufen
        if (HasExpired(fromDate, currentOccurrenceCount))
            return null;

        var candidateDate = fromDate.Date > StartDate.Date ? fromDate.Date : StartDate.Date;

        // Finde nächstes Vorkommen basierend auf Frequency
        return Frequency switch
        {
            RecurrenceType.Daily => GetNextDailyOccurrence(candidateDate),
            RecurrenceType.Weekly => GetNextWeeklyOccurrence(candidateDate),
            RecurrenceType.Monthly => GetNextMonthlyOccurrence(candidateDate),
            _ => null
        };
    }

    private DateTime? GetNextDailyOccurrence(DateTime fromDate)
    {
        var nextDate = fromDate;

        // Wenn fromDate vor StartDate, beginne bei StartDate
        if (nextDate < StartDate.Date)
            nextDate = StartDate.Date;
        else
        {
            // Berechne nächsten Tag basierend auf Interval
            var daysSinceStart = (nextDate - StartDate.Date).Days;
            var remainder = daysSinceStart % Interval;

            if (remainder != 0)
                nextDate = nextDate.AddDays(Interval - remainder);
            else if (nextDate == fromDate)
                nextDate = nextDate.AddDays(Interval);
        }

        // Prüfe EndDate
        if (EndDate.HasValue && nextDate > EndDate.Value.Date)
            return null;

        return nextDate;
    }

    private DateTime? GetNextWeeklyOccurrence(DateTime fromDate)
    {
        if (DaysOfWeek == null || DaysOfWeek.Length == 0)
            return null;

        var sortedDays = DaysOfWeek.OrderBy(d => d).ToArray();
        var nextDate = fromDate;

        // Wenn fromDate vor StartDate, beginne bei StartDate
        if (nextDate < StartDate.Date)
            nextDate = StartDate.Date;

        // Suche nach dem nächsten passenden Wochentag
        for (int i = 0; i <= 7 * Interval; i++)
        {
            var testDate = nextDate.AddDays(i);

            // Prüfe ob dieser Tag in der aktuellen Intervallwoche liegt
            var weeksSinceStart = (testDate - StartDate.Date).Days / 7;
            var isInIntervalWeek = weeksSinceStart % Interval == 0;

            if (isInIntervalWeek && sortedDays.Contains(testDate.DayOfWeek) && testDate > fromDate.Date)
            {
                if (EndDate.HasValue && testDate > EndDate.Value.Date)
                    return null;

                return testDate;
            }
        }

        return null;
    }

    private DateTime? GetNextMonthlyOccurrence(DateTime fromDate)
    {
        if (!DayOfMonth.HasValue)
            return null;

        var nextDate = fromDate;

        // Wenn fromDate vor StartDate, beginne bei StartDate
        if (nextDate < StartDate.Date)
            nextDate = StartDate.Date;

        // Berechne nächsten Monat basierend auf Interval
        var monthsSinceStart = (nextDate.Year - StartDate.Year) * 12 + (nextDate.Month - StartDate.Month);
        var remainder = monthsSinceStart % Interval;

        DateTime candidateDate;
        if (remainder != 0)
        {
            // Nächster Intervallmonat
            candidateDate = StartDate.AddMonths(monthsSinceStart - remainder + Interval);
        }
        else
        {
            // Aktueller Intervallmonat
            candidateDate = new DateTime(nextDate.Year, nextDate.Month, 1);
        }

        // Setze den Tag
        var targetDay = Math.Min(DayOfMonth.Value, DateTime.DaysInMonth(candidateDate.Year, candidateDate.Month));
        candidateDate = new DateTime(candidateDate.Year, candidateDate.Month, targetDay);

        // Wenn candidateDate in der Vergangenheit liegt, nehme nächsten Intervallmonat
        if (candidateDate <= fromDate.Date)
        {
            candidateDate = candidateDate.AddMonths(Interval);
            targetDay = Math.Min(DayOfMonth.Value, DateTime.DaysInMonth(candidateDate.Year, candidateDate.Month));
            candidateDate = new DateTime(candidateDate.Year, candidateDate.Month, targetDay);
        }

        // Prüfe EndDate
        if (EndDate.HasValue && candidateDate > EndDate.Value.Date)
            return null;

        return candidateDate;
    }

    public bool IsActiveOn(DateTime date)
    {
        var dateOnly = date.Date;

        if (dateOnly < StartDate.Date)
            return false;

        if (EndDate.HasValue && dateOnly > EndDate.Value.Date)
            return false;

        return true;
    }

    public bool HasExpired(DateTime currentDate, int currentOccurrenceCount = 0)
    {
        var dateOnly = currentDate.Date;

        if (EndDate.HasValue && dateOnly > EndDate.Value.Date)
            return true;

        if (MaxOccurrences.HasValue && currentOccurrenceCount >= MaxOccurrences.Value)
            return true;

        return false;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Frequency;
        yield return Interval;
        yield return DaysOfWeek;
        yield return DayOfMonth;
        yield return StartDate;
        yield return EndDate;
        yield return MaxOccurrences;
    }

    public override string ToString()
    {
        var description = Frequency switch
        {
            RecurrenceType.Daily => Interval == 1 ? "Täglich" : $"Alle {Interval} Tage",
            RecurrenceType.Weekly => Interval == 1
                ? $"Wöchentlich ({GetDaysOfWeekString()})"
                : $"Alle {Interval} Wochen ({GetDaysOfWeekString()})",
            RecurrenceType.Monthly => Interval == 1
                ? $"Monatlich am {DayOfMonth}."
                : $"Alle {Interval} Monate am {DayOfMonth}.",
            _ => "Unbekannt"
        };

        if (EndDate.HasValue)
            description += $" bis {EndDate.Value:dd.MM.yyyy}";
        else if (MaxOccurrences.HasValue)
            description += $" ({MaxOccurrences}x)";

        return description;
    }

    private string GetDaysOfWeekString()
    {
        if (DaysOfWeek == null || DaysOfWeek.Length == 0)
            return "";

        var dayNames = DaysOfWeek.Select(d => d switch
        {
            System.DayOfWeek.Monday => "Mo",
            System.DayOfWeek.Tuesday => "Di",
            System.DayOfWeek.Wednesday => "Mi",
            System.DayOfWeek.Thursday => "Do",
            System.DayOfWeek.Friday => "Fr",
            System.DayOfWeek.Saturday => "Sa",
            System.DayOfWeek.Sunday => "So",
            _ => ""
        });

        return string.Join(", ", dayNames.OrderBy(d => d));
    }
}
