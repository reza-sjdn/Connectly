using Connectly.Application.Search;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Connectly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/search")]
public class SearchController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SearchResultDto>> Search([FromQuery(Name = "q")] string q, CancellationToken ct)
        => Ok(await sender.Send(new SearchQuery(q ?? string.Empty), ct));
}
