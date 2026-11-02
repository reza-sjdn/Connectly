using Connectly.Application.Common.Models;
using Connectly.Application.Groups.Commands;
using Connectly.Application.Groups.Models;
using Connectly.Application.Groups.Queries;
using Connectly.Application.Posts.Models;
using Connectly.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Connectly.Api.Controllers;

public record UpdateGroupRequest(string Name, string? Description, GroupPrivacy Privacy);
public record ChangeRoleRequest(GroupRole Role);
public record SendGroupMessageRequest(string Content);

[ApiController]
[Authorize]
[Route("api/groups")]
public class GroupsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GroupSummaryDto>>> List([FromQuery] GroupListScope scope = GroupListScope.Mine, CancellationToken ct = default)
        => Ok(await sender.Send(new GetGroupsQuery(scope), ct));

    [HttpPost]
    public async Task<ActionResult<GroupDetailsDto>> Create(CreateGroupCommand command, CancellationToken ct)
        => Ok(await sender.Send(command, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GroupDetailsDto>> Get(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetGroupQuery(id), ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GroupDetailsDto>> Update(Guid id, UpdateGroupRequest body, CancellationToken ct)
        => Ok(await sender.Send(new UpdateGroupCommand(id, body.Name, body.Description, body.Privacy), ct));

    [HttpPost("{id:guid}/join")]
    public async Task<ActionResult<GroupDetailsDto>> Join(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new JoinGroupCommand(id), ct));

    [HttpDelete("{id:guid}/membership")]
    public async Task<IActionResult> Leave(Guid id, CancellationToken ct)
    {
        await sender.Send(new LeaveGroupCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<PagedResult<GroupMemberDto>>> Members(
        Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await sender.Send(new GetGroupMembersQuery(id, page, pageSize), ct));

    [HttpPut("{id:guid}/members/{userId:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, Guid userId, ChangeRoleRequest body, CancellationToken ct)
    {
        await sender.Send(new ChangeMemberRoleCommand(id, userId, body.Role), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken ct)
    {
        await sender.Send(new RemoveMemberCommand(id, userId), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/requests")]
    public async Task<ActionResult<IReadOnlyList<JoinRequestDto>>> Requests(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetJoinRequestsQuery(id), ct));

    [HttpPost("{id:guid}/requests/{userId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, Guid userId, CancellationToken ct)
    {
        await sender.Send(new ReviewJoinRequestCommand(id, userId, true), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/requests/{userId:guid}")]
    public async Task<IActionResult> Reject(Guid id, Guid userId, CancellationToken ct)
    {
        await sender.Send(new ReviewJoinRequestCommand(id, userId, false), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/posts")]
    public async Task<ActionResult<CursorPage<PostDto>>> Posts(
        Guid id, [FromQuery] string? cursor, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await sender.Send(new GetGroupPostsQuery(id, cursor, pageSize), ct));

    // Newest first; the client reverses for display and passes nextCursor to load older messages.
    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<CursorPage<GroupMessageDto>>> Messages(
        Guid id, [FromQuery] string? cursor, [FromQuery] int pageSize = 30, CancellationToken ct = default)
        => Ok(await sender.Send(new GetGroupMessagesQuery(id, cursor, pageSize), ct));

    [HttpPost("{id:guid}/messages")]
    public async Task<ActionResult<GroupMessageDto>> SendMessage(Guid id, SendGroupMessageRequest body, CancellationToken ct)
        => Ok(await sender.Send(new SendGroupMessageCommand(id, body.Content), ct));
}
