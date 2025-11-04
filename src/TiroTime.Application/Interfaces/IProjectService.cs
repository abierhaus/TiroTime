using TiroTime.Application.Common;
using TiroTime.Application.DTOs;

namespace TiroTime.Application.Interfaces;

public interface IProjectService
{
    Task<Result<IEnumerable<ProjectDto>>> GetAllProjectsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ProjectDto>>> GetProjectsByClientIdAsync(Guid clientId, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> CreateProjectAsync(CreateProjectDto dto, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> UpdateProjectAsync(UpdateProjectDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> ActivateProjectAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> DeactivateProjectAsync(Guid id, CancellationToken cancellationToken = default);
}
