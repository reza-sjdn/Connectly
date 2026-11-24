using Connectly.Application.Common.Dtos;
using Connectly.Application.Groups.Models;
using MediatR;

namespace Connectly.Application.Groups.Queries.GetGroupMessages;

// ---------- group chat history (members only, newest first) ----------
public record GetGroupMessagesQuery(Guid GroupId, string? Cursor = null, int PageSize = 30) : IRequest<CursorPage<GroupMessageDto>>;
