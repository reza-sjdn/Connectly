using Connectly.Application.Chat.Dtos;
using MediatR;

namespace Connectly.Application.Chat.Queries.GetConversations;

// ---------- Inbox ----------
public record GetConversationsQuery : IRequest<IReadOnlyList<ConversationDto>>;
