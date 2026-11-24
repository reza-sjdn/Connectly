using Connectly.Application.Chat.Dtos;
using Connectly.Application.Common.Dtos;
using MediatR;

namespace Connectly.Application.Chat.Queries.GetMessagesWith;

// ---------- Thread with one user (newest first; client reverses for display) ----------
public record GetMessagesWithQuery(Guid UserId, string? Cursor = null, int PageSize = 30) : IRequest<CursorPage<MessageDto>>;
