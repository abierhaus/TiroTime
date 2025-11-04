using Microsoft.EntityFrameworkCore;
using TiroTime.Application.Common;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Entities;
using TiroTime.Domain.ValueObjects;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<Client> _clientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(
        ApplicationDbContext context,
        IRepository<Project> projectRepository,
        IRepository<Client> clientRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _projectRepository = projectRepository;
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<ProjectDto>>> GetAllProjectsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var projects = await _context.Projects
            .Include(p => p.Client)
            .Where(p => includeInactive || p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var projectDtos = projects.Select(MapToDto);
        return Result.Success(projectDtos);
    }

    public async Task<Result<IEnumerable<ProjectDto>>> GetProjectsByClientIdAsync(
        Guid clientId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var projects = await _context.Projects
            .Include(p => p.Client)
            .Where(p => p.ClientId == clientId && (includeInactive || p.IsActive))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var projectDtos = projects.Select(MapToDto);
        return Result.Success(projectDtos);
    }

    public async Task<Result<ProjectDto>> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
            return Result.Failure<ProjectDto>("Projekt nicht gefunden");

        return Result.Success(MapToDto(project));
    }

    public async Task<Result<ProjectDto>> CreateProjectAsync(
        CreateProjectDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify client exists
            var client = await _clientRepository.GetByIdAsync(dto.ClientId, cancellationToken);
            if (client == null)
                return Result.Failure<ProjectDto>("Kunde nicht gefunden");

            var hourlyRate = Money.Create(dto.HourlyRate, dto.HourlyRateCurrency ?? "EUR");

            Money? budget = null;
            if (dto.Budget.HasValue)
            {
                budget = Money.Create(dto.Budget.Value, dto.BudgetCurrency ?? "EUR");
            }

            var project = Project.Create(
                dto.Name,
                dto.ClientId,
                hourlyRate,
                dto.Description,
                budget,
                dto.ColorCode,
                dto.StartDate,
                dto.EndDate);

            await _projectRepository.AddAsync(project, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with client
            var createdProject = await _context.Projects
                .Include(p => p.Client)
                .FirstAsync(p => p.Id == project.Id, cancellationToken);

            return Result.Success(MapToDto(createdProject));
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
            var project = await _context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);

            if (project == null)
                return Result.Failure<ProjectDto>("Projekt nicht gefunden");

            var hourlyRate = Money.Create(dto.HourlyRate, dto.HourlyRateCurrency ?? "EUR");

            Money? budget = null;
            if (dto.Budget.HasValue)
            {
                budget = Money.Create(dto.Budget.Value, dto.BudgetCurrency ?? "EUR");
            }

            project.Update(
                dto.Name,
                hourlyRate,
                dto.Description,
                budget,
                dto.ColorCode,
                dto.StartDate,
                dto.EndDate);

            _projectRepository.Update(project);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(MapToDto(project));
        }
        catch (Exception ex)
        {
            return Result.Failure<ProjectDto>(ex.Message);
        }
    }

    public async Task<Result> DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null)
            return Result.Failure("Projekt nicht gefunden");

        // TODO: Check if project has time entries when that feature is implemented

        _projectRepository.Remove(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ActivateProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null)
            return Result.Failure("Projekt nicht gefunden");

        project.Activate();
        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeactivateProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null)
            return Result.Failure("Projekt nicht gefunden");

        project.Deactivate();
        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.ClientId,
            project.Client?.Name ?? "Unknown",
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
