using Connectly.Api.Realtime;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Api.Controllers;

public record PresenceRequest(Guid[] Ids);

[ApiController]
[Authorize]
[Route("api/presence")]
public class PresenceController(IPresenceTracker presence, IApplicationDbContext db, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Which of these users are online? Only users related to me (follow edge / chat partner) are revealed.</summary>
    [HttpPost("online")]
    public async Task<IActionResult> Online(PresenceRequest request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var ids = request.Ids.Distinct().Take(200).ToList();

        var related = await PresenceAudience.RelatedTo(db, me).Where(id => ids.Contains(id)).ToListAsync(ct);
        return Ok(new { onlineIds = presence.FilterOnline(related) });
    }
}
