using Microsoft.EntityFrameworkCore;
using TiroTime.Application.Common;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Entities;
using TiroTime.Domain.ValueObjects;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Infrastructure.Services;

public class ClientService(
    ApplicationDbContext context,
    IRepository<Client> clientRepository,
    IUnitOfWork unitOfWork) : IClientService
{
    public async Task<Result<IEnumerable<ClientDto>>> GetAllClientsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var clients = await context.Clients
            .AsNoTracking()
            .Where(c => includeInactive || c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ClientDto>>(clients.Select(MapToDto).ToList());
    }

    public async Task<Result<ClientDto>> GetClientByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return client == null
            ? Result.Failure<ClientDto>("Kunde nicht gefunden")
            : Result.Success(MapToDto(client));
    }

    public async Task<Result<ClientDto>> CreateClientAsync(
        CreateClientDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = Client.Create(
                dto.Name,
                dto.ContactPerson,
                CreateEmail(dto.Email),
                CreatePhoneNumber(dto.PhoneNumber),
                CreateAddress(dto.AddressStreet, dto.AddressCity, dto.AddressPostalCode, dto.AddressCountry),
                dto.TaxId,
                dto.Notes);

            await clientRepository.AddAsync(client, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(MapToDto(client));
        }
        catch (Exception ex)
        {
            return Result.Failure<ClientDto>(ex.Message);
        }
    }

    public async Task<Result<ClientDto>> UpdateClientAsync(
        UpdateClientDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await clientRepository.GetByIdAsync(dto.Id, cancellationToken);
            if (client == null)
                return Result.Failure<ClientDto>("Kunde nicht gefunden");

            client.Update(
                dto.Name,
                dto.ContactPerson,
                CreateEmail(dto.Email),
                CreatePhoneNumber(dto.PhoneNumber),
                CreateAddress(dto.AddressStreet, dto.AddressCity, dto.AddressPostalCode, dto.AddressCountry),
                dto.TaxId,
                dto.Notes);

            clientRepository.Update(client);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(MapToDto(client));
        }
        catch (Exception ex)
        {
            return Result.Failure<ClientDto>(ex.Message);
        }
    }

    public async Task<Result> DeleteClientAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await clientRepository.GetByIdAsync(id, cancellationToken);
        if (client == null)
            return Result.Failure("Kunde nicht gefunden");

        var hasProjects = await context.Projects.AnyAsync(p => p.ClientId == id, cancellationToken);
        if (hasProjects)
            return Result.Failure("Kunde kann nicht gelöscht werden, da noch Projekte zugeordnet sind");

        clientRepository.Remove(client);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ActivateClientAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await clientRepository.GetByIdAsync(id, cancellationToken);
        if (client == null)
            return Result.Failure("Kunde nicht gefunden");

        client.Activate();
        clientRepository.Update(client);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeactivateClientAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await clientRepository.GetByIdAsync(id, cancellationToken);
        if (client == null)
            return Result.Failure("Kunde nicht gefunden");

        client.Deactivate();
        clientRepository.Update(client);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static Email? CreateEmail(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : Email.Create(value);

    private static PhoneNumber? CreatePhoneNumber(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : PhoneNumber.Create(value);

    private static Address? CreateAddress(string? street, string? city, string? postalCode, string? country)
    {
        if (string.IsNullOrWhiteSpace(street) ||
            string.IsNullOrWhiteSpace(city) ||
            string.IsNullOrWhiteSpace(postalCode) ||
            string.IsNullOrWhiteSpace(country))
        {
            return null;
        }

        return Address.Create(street, city, postalCode, country);
    }

    private static ClientDto MapToDto(Client client)
    {
        return new ClientDto(
            client.Id,
            client.Name,
            client.ContactPerson,
            client.Email?.Value,
            client.PhoneNumber?.Value,
            client.Address?.Street,
            client.Address?.City,
            client.Address?.PostalCode,
            client.Address?.Country,
            client.TaxId,
            client.Notes,
            client.IsActive,
            client.CreatedAt,
            client.UpdatedAt);
    }
}
