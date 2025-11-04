using System.Text.RegularExpressions;
using TiroTime.Domain.Common;
using TiroTime.Domain.Exceptions;

namespace TiroTime.Domain.ValueObjects;

public partial class PhoneNumber : ValueObject
{
    public string Value { get; private set; } = string.Empty;

    private PhoneNumber() { }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Telefonnummer darf nicht leer sein");

        // Remove common formatting characters
        var cleaned = phoneNumber.Replace(" ", "")
                                 .Replace("-", "")
                                 .Replace("(", "")
                                 .Replace(")", "")
                                 .Replace("/", "");

        // Basic validation for German phone numbers
        if (!PhoneNumberRegex().IsMatch(cleaned))
            throw new DomainException($"'{phoneNumber}' ist keine gültige Telefonnummer");

        return new PhoneNumber(cleaned);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;

    [GeneratedRegex(@"^\+?[0-9]{6,15}$", RegexOptions.Compiled)]
    private static partial Regex PhoneNumberRegex();
}
