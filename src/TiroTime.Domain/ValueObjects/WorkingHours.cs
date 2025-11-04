using TiroTime.Domain.Common;
using TiroTime.Domain.Exceptions;

namespace TiroTime.Domain.ValueObjects;

public class WorkingHours : ValueObject
{
    public decimal HoursPerWeek { get; private set; }

    private WorkingHours() { }

    private WorkingHours(decimal hoursPerWeek)
    {
        HoursPerWeek = hoursPerWeek;
    }

    public static WorkingHours Create(decimal hoursPerWeek)
    {
        if (hoursPerWeek < 0)
            throw new DomainException("Working hours cannot be negative");

        if (hoursPerWeek > 168) // Max hours in a week
            throw new DomainException("Working hours cannot exceed 168 hours per week");

        return new WorkingHours(hoursPerWeek);
    }

    public static WorkingHours FullTime => new(40);
    public static WorkingHours PartTime => new(20);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return HoursPerWeek;
    }

    public override string ToString() => $"{HoursPerWeek:F2}h/week";
}
