using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Web.Pages.Clients;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IClientService _clientService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IClientService clientService,
        ILogger<IndexModel> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    public IEnumerable<ClientDto> Clients { get; set; } = Array.Empty<ClientDto>();

    public async Task OnGetAsync()
    {
        var result = await _clientService.GetAllClientsAsync(includeInactive: false);

        if (result.IsSuccess)
        {
            Clients = result.Value;
        }
        else
        {
            _logger.LogError("Fehler beim Laden der Kunden: {Error}", result.Error);
            Clients = Array.Empty<ClientDto>();
        }
    }
}
