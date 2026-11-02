using Connectly.Application.Chat.Commands;
using Connectly.Application.Chat.Models;
using Connectly.Application.Chat.Queries;
using Connectly.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Connectly.Api.Controllers;

public record SendMessageRequest(string Content);

[ApiController]
[Authorize]
[Route("api/chat")]
public class ChatController(ISender sender) : ControllerBase
{
    [HttpGet("conversations")]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> Conversations(CancellationToken ct)
        => Ok(await sender.Send(new GetConversationsQuery(), ct));

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken ct)
        => Ok(new { count = await sender.Send(new GetUnreadMessageCountQuery(), ct) });

    // Newest first; the client reverses for display and passes nextCursor to load older messages.
    [HttpGet("with/{userId:guid}/messages")]
    public async Task<ActionResult<CursorPage<MessageDto>>> Messages(
        Guid userId, [FromQuery] string? cursor, [FromQuery] int pageSize = 30, CancellationToken ct = default)
        => Ok(await sender.Send(new GetMessagesWithQuery(userId, cursor, pageSize), ct));

    [HttpPost("with/{userId:guid}/messages")]
    public async Task<ActionResult<MessageDto>> Send(Guid userId, SendMessageRequest body, CancellationToken ct)
        => Ok(await sender.Send(new SendMessageCommand(userId, body.Content), ct));

    [HttpPost("with/{userId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid userId, CancellationToken ct)
    {
        await sender.Send(new MarkConversationReadCommand(userId), ct);
        return NoContent();
    }
}
