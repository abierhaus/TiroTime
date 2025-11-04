using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.Projects;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IProjectService _projectService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IProjectService projectService,
        ILogger<IndexModel> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    public IEnumerable<ProjectDto> Projects { get; set; } = Array.Empty<ProjectDto>();

    public async Task OnGetAsync()
    {
        var result = await _projectService.GetAllProjectsAsync(includeInactive: false);

        if (result.IsSuccess)
        {
            Projects = result.Value;
        }
        else
        {
            _logger.LogError("Fehler beim Laden der Projekte: {Error}", result.Error);
            Projects = Array.Empty<ProjectDto>();
        }
    }
}
