using Microsoft.EntityFrameworkCore;
using TiroTime.Application.Common;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Entities;
using TiroTime.Domain.ValueObjects;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Infrastructure.Services;

public class ProjectService(
    ApplicationDbContext context,
    IRepository<Project> projectRepository,
    IRepository<Client> clientRepository,
    IUnitOfWork unitOfWork) : IProjectService
{
    public async Task<Result<IEnumerable<ProjectDto>>> GetAllProjectsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var projects = await context.Projects
            .AsNoTracking()
            .Include(p => p.Client)
            .Where(p => includeInactive || p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ProjectDto>>(projects.Select(MapToDto).ToList());
    }

    public async Task<Result<IEnumerable<ProjectDto>>> GetProjectsByClientIdAsync(
        Guid clientId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var projects = await context.Projects
            .AsNoTracking()
            .Include(p => p.Client)
            .Where(p => p.ClientId == clientId && (includeInactive || p.IsActive))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ProjectDto>>(projects.Select(MapToDto).ToList());
    }

    public async Task<Result<ProjectDto>> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await context.Projects
            .AsNoTracking()
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return project == null
            ? Result.Failure<ProjectDto>("Projekt nicht gefunden")
            : Result.Success(MapToDto(project));
    }

    public async Task<Result<ProjectDto>> CreateProjectAsync(
        CreateProjectDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await clientRepository.GetByIdAsync(dto.ClientId, cancellationToken);
            if (client == null)
                return Result.Failure<ProjectDto>("Kunde nicht gefunden");

            var hourlyRate = Money.Create(dto.HourlyRate, dto.HourlyRateCurrency);
            var budget = dto.Budget.HasValue ? Money.Create(dto.Budget.Value, dto.BudgetCurrency ?? "EUR") : null;

            var project = Project.Create(
                dto.Name,
                dto.ClientId,
                hourlyRate,
                dto.Description,
                budget,
                dto.ColorCode,
                dto.StartDate,
                dto.EndDate);

            await projectRepository.AddAsync(project, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Kunde ist bereits geladen – kein erneutes Nachladen des Projekts nötig
            return Result.Success(MapToDto(project, client.Name));
        }
        catch (Exception ex)
        {
            return Result.Failure<ProjectDto>(ex.Message);
        }
    }

    public async Task<Result<ProjectDto>> UpdateProjectAsync(
        UpdateProjectDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);

            if (project == null)
                return Result.Failure<ProjectDto>("Projekt nicht gefunden");

            var hourlyRate = Money.Create(dto.HourlyRate, dto.HourlyRateCurrency);
            var budget = dto.Budget.HasValue ? Money.Create(dto.Budget.Value, dto.BudgetCurrency ?? "EUR") : null;

            project.Update(
                dto.Name,
                hourlyRate,
                dto.Description,
                budget,
                dto.ColorCode,
                dto.StartDate,
                dto.EndDate);

            projectRepository.Update(project);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(MapToDto(project));
        }
        catch (Exception ex)
        {
            return Result.Failure<ProjectDto>(ex.Message);
        }
    }

    public async Task<Result> DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null)
            return Result.Failure("Projekt nicht gefunden");

        var hasTimeEntries = await context.TimeEntries.AnyAsync(te => te.ProjectId == id, cancellationToken);
        if (hasTimeEntries)
            return Result.Failure("Projekt kann nicht gelöscht werden, da noch Zeiteinträge zugeordnet sind");

        projectRepository.Remove(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ActivateProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null)
            return Result.Failure("Projekt nicht gefunden");

        project.Activate();
        projectRepository.Update(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeactivateProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null)
            return Result.Failure("Projekt nicht gefunden");

        project.Deactivate();
        projectRepository.Update(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static ProjectDto MapToDto(Project project) =>
        MapToDto(project, project.Client?.Name ?? "Unknown");

    private static ProjectDto MapToDto(Project project, string clientName)
    {
        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.ClientId,
            clientName,
            project.HourlyRate.Amount,
            project.HourlyRate.Currency,
            project.Budget?.Amount,
            project.Budget?.Currency,
            project.ColorCode,
            project.IsActive,
            project.StartDate,
            project.EndDate,
            project.CreatedAt,
            project.UpdatedAt);
    }
}
