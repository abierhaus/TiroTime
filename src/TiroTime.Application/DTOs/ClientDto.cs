namespace TiroTime.Application.DTOs;

public record ClientDto(
    Guid Id,
    string Name,
    string? ContactPerson,
    string? Email,
    string? PhoneNumber,
    string? AddressStreet,
    string? AddressCity,
    string? AddressPostalCode,
    string? AddressCountry,
    string? TaxId,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateClientDto(
    string Name,
    string? ContactPerson,
    string? Email,
    string? PhoneNumber,
    string? AddressStreet,
    string? AddressCity,
    string? AddressPostalCode,
    string? AddressCountry,
    string? TaxId,
    string? Notes);

public record UpdateClientDto(
    Guid Id,
    string Name,
    string? ContactPerson,
    string? Email,
    string? PhoneNumber,
    string? AddressStreet,
    string? AddressCity,
    string? AddressPostalCode,
    string? AddressCountry,
    string? TaxId,
    string? Notes);
