using MediatR;

namespace Connectly.Application.Chat.Queries.GetUnreadMessageCount;

// ---------- Badge ----------
public record GetUnreadMessageCountQuery : IRequest<int>;
