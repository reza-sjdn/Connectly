using Connectly.Application.Common.Dtos;

namespace Connectly.Application.Chat.Dtos;

public record ConversationDto(
    Guid Id, AuthorDto OtherUser, string? LastMessage, DateTime LastMessageAtUtc, int UnreadCount);
