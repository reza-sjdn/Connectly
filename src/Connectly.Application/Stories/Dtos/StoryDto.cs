namespace Connectly.Application.Stories.Dtos;

public record StoryDto(
    Guid Id, string? ImageUrl, string? Text, string? BackgroundColor,
    DateTime CreatedAtUtc, DateTime ExpiresAtUtc, bool IsSeenByMe, int ViewsCount);
