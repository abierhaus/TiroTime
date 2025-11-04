using TiroTime.Application.Common;
using TiroTime.Application.DTOs;

namespace TiroTime.Application.Interfaces;

public interface IClientService
{
    Task<Result<IEnumerable<ClientDto>>> GetAllClientsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<Result<ClientDto>> GetClientByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ClientDto>> CreateClientAsync(CreateClientDto dto, CancellationToken cancellationToken = default);
    Task<Result<ClientDto>> UpdateClientAsync(UpdateClientDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteClientAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> ActivateClientAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> DeactivateClientAsync(Guid id, CancellationToken cancellationToken = default);
}
