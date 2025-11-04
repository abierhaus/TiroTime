using Microsoft.EntityFrameworkCore;
using TiroTime.Application.Common;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Entities;
using TiroTime.Domain.ValueObjects;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Client> _clientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClientService(
        ApplicationDbContext context,
        IRepository<Client> clientRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<ClientDto>>> GetAllClientsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var clients = await _context.Clients
            .Where(c => includeInactive || c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var clientDtos = clients.Select(MapToDto);
        return Result.Success(clientDtos);
    }

    public async Task<Result<ClientDto>> GetClientByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, cancellationToken);
        if (client == null)
            return Result.Failure<ClientDto>("Kunde nicht gefunden");

        return Result.Success(MapToDto(client));
    }

    public async Task<Result<ClientDto>> CreateClientAsync(
        CreateClientDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Email? email = null;
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                email = Email.Create(dto.Email);
            }

            PhoneNumber? phoneNumber = null;
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                phoneNumber = PhoneNumber.Create(dto.PhoneNumber);
            }

            Address? address = null;
            if (!string.IsNullOrWhiteSpace(dto.AddressStreet) &&
                !string.IsNullOrWhiteSpace(dto.AddressCity) &&
                !string.IsNullOrWhiteSpace(dto.AddressPostalCode) &&
                !string.IsNullOrWhiteSpace(dto.AddressCountry))
            {
                address = Address.Create(
                    dto.AddressStreet,
                    dto.AddressCity,
                    dto.AddressPostalCode,
                    dto.AddressCountry);
            }

            var client = Client.Create(
                dto.Name,
                dto.ContactPerson,
                email,
                phoneNumber,
                address,
                dto.TaxId,
                dto.Notes);

            await _clientRepository.AddAsync(client, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            var client = await _clientRepository.GetByIdAsync(dto.Id, cancellationToken);
            if (client == null)
                return Result.Failure<ClientDto>("Kunde nicht gefunden");

            Email? email = null;
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                email = Email.Create(dto.Email);
            }

            PhoneNumber? phoneNumber = null;
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                phoneNumber = PhoneNumber.Create(dto.PhoneNumber);
            }

            Address? address = null;
            if (!string.IsNullOrWhiteSpace(dto.AddressStreet) &&
                !string.IsNullOrWhiteSpace(dto.AddressCity) &&
                !string.IsNullOrWhiteSpace(dto.AddressPostalCode) &&
                !string.IsNullOrWhiteSpace(dto.AddressCountry))
            {
                address = Address.Create(
                    dto.AddressStreet,
                    dto.AddressCity,
                    dto.AddressPostalCode,
                    dto.AddressCountry);
            }

            client.Update(
                dto.Name,
                dto.ContactPerson,
                email,
                phoneNumber,
                address,
                dto.TaxId,
                dto.Notes);

            _clientRepository.Update(client);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(MapToDto(client));
        }
        catch (Exception ex)
        {
            return Result.Failure<ClientDto>(ex.Message);
        }
    }

    public async Task<Result> DeleteClientAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, cancellationToken);
        if (client == null)
            return Result.Failure("Kunde nicht gefunden");

        // Check if client has active projects
        var hasProjects = await _context.Projects.AnyAsync(p => p.ClientId == id, cancellationToken);
        if (hasProjects)
            return Result.Failure("Kunde kann nicht gelöscht werden, da noch Projekte zugeordnet sind");

        _clientRepository.Remove(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ActivateClientAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, cancellationToken);
        if (client == null)
            return Result.Failure("Kunde nicht gefunden");

        client.Activate();
        _clientRepository.Update(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeactivateClientAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, cancellationToken);
        if (client == null)
            return Result.Failure("Kunde nicht gefunden");

        client.Deactivate();
        _clientRepository.Update(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
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
