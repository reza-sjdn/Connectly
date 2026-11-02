using Connectly.Application.Common.Models;

namespace Connectly.Application.Chat.Models;

public record MessageDto(
    Guid Id, Guid ConversationId, Guid SenderId, Guid RecipientId,
    string Content, DateTime CreatedAtUtc, DateTime? ReadAtUtc);

public record ConversationDto(
    Guid Id, AuthorDto OtherUser, string? LastMessage, DateTime LastMessageAtUtc, int UnreadCount);
