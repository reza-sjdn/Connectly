using Connectly.Application.Common.Models;
using Connectly.Application.Follows.Commands;
using Connectly.Application.Follows.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Connectly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:guid}")]
public class FollowsController(ISender sender) : ControllerBase
{
    [HttpPost("follow")]
    public async Task<IActionResult> Follow(Guid userId, CancellationToken ct)
    {
        await sender.Send(new FollowUserCommand(userId), ct);
        return NoContent();
    }

    [HttpDelete("follow")]
    public async Task<IActionResult> Unfollow(Guid userId, CancellationToken ct)
    {
        await sender.Send(new UnfollowUserCommand(userId), ct);
        return NoContent();
    }

    [HttpGet("followers")]
    public async Task<ActionResult<PagedResult<UserSummaryDto>>> Followers(
        Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await sender.Send(new GetFollowListQuery(userId, FollowDirection.Followers, page, pageSize), ct));

    [HttpGet("following")]
    public async Task<ActionResult<PagedResult<UserSummaryDto>>> Following(
        Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await sender.Send(new GetFollowListQuery(userId, FollowDirection.Following, page, pageSize), ct));
}
