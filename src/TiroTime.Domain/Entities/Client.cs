using TiroTime.Domain.Common;
using TiroTime.Domain.Exceptions;
using TiroTime.Domain.ValueObjects;

namespace TiroTime.Domain.Entities;

public class Client : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? ContactPerson { get; private set; }
    public Email? Email { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public Address? Address { get; private set; }
    public string? TaxId { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Client() { }

    public static Client Create(
        string name,
        string? contactPerson = null,
        Email? email = null,
        PhoneNumber? phoneNumber = null,
        Address? address = null,
        string? taxId = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Kundenname darf nicht leer sein");

        var client = new Client
        {
            Name = name.Trim(),
            ContactPerson = contactPerson?.Trim(),
            Email = email,
            PhoneNumber = phoneNumber,
            Address = address,
            TaxId = taxId?.Trim(),
            Notes = notes?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return client;
    }

    public void Update(
        string name,
        string? contactPerson = null,
        Email? email = null,
        PhoneNumber? phoneNumber = null,
        Address? address = null,
        string? taxId = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Kundenname darf nicht leer sein");

        Name = name.Trim();
        ContactPerson = contactPerson?.Trim();
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        TaxId = taxId?.Trim();
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
