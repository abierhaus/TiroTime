using System.Text.RegularExpressions;
using TiroTime.Domain.Common;
using TiroTime.Domain.Exceptions;

namespace TiroTime.Domain.ValueObjects;

public partial class Email : ValueObject
{
    public string Value { get; private set; } = string.Empty;

    private Email() { }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email address cannot be empty");

        email = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(email))
            throw new DomainException($"'{email}' is not a valid email address");

        return new Email(email);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
