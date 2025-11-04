using TiroTime.Domain.Common;
using TiroTime.Domain.Exceptions;

namespace TiroTime.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;

    private Address() { }

    private Address(string street, string city, string postalCode, string country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(string street, string city, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Straße darf nicht leer sein");

        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("Stadt darf nicht leer sein");

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new DomainException("Postleitzahl darf nicht leer sein");

        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException("Land darf nicht leer sein");

        return new Address(street.Trim(), city.Trim(), postalCode.Trim(), country.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() => $"{Street}, {PostalCode} {City}, {Country}";
}
