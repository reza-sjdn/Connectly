namespace Connectly.Application.Chat.Dtos;

public record MessageDto(
    Guid Id, Guid ConversationId, Guid SenderId, Guid RecipientId,
    string Content, DateTime CreatedAtUtc, DateTime? ReadAtUtc);
