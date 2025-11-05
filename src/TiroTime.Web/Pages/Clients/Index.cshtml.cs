using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.Clients;

[Authorize]
public class IndexModel(
    IClientService clientService,
    ILogger<IndexModel> logger) : PageModel
{
    public IEnumerable<ClientDto> Clients { get; set; } = [];

    public async Task OnGetAsync()
    {
        var result = await clientService.GetAllClientsAsync(includeInactive: false);

        if (result.IsSuccess)
        {
            Clients = result.Value;
        }
        else
        {
            logger.LogError("Fehler beim Laden der Kunden: {Error}", result.Error);
            Clients = [];
        }
    }
}
