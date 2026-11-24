using MediatR;

namespace Connectly.Application.Chat.Commands.MarkConversationRead;

/// <summary>Marks everything the other user sent me as read and tells them ("Seen").</summary>
public record MarkConversationReadCommand(Guid OtherUserId) : IRequest;
