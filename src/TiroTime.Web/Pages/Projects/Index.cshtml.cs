using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.Projects;

[Authorize]
public class IndexModel(
    IProjectService projectService,
    ILogger<IndexModel> logger) : PageModel
{
    public IEnumerable<ProjectDto> Projects { get; set; } = [];

    public async Task OnGetAsync()
    {
        var result = await projectService.GetAllProjectsAsync(includeInactive: false);

        if (result.IsSuccess)
        {
            Projects = result.Value;
        }
        else
        {
            logger.LogError("Fehler beim Laden der Projekte: {Error}", result.Error);
            Projects = [];
        }
    }
}
