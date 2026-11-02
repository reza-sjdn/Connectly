using Connectly.Application.Common.Models;

namespace Connectly.Application.Stories.Models;

public record StoryDto(
    Guid Id, string? ImageUrl, string? Text, string? BackgroundColor,
    DateTime CreatedAtUtc, DateTime ExpiresAtUtc, bool IsSeenByMe, int ViewsCount);

public record StoryGroupDto(AuthorDto Author, bool IsMe, bool HasUnseen, IReadOnlyList<StoryDto> Stories);

public record StoryViewerDto(AuthorDto User, DateTime ViewedAtUtc);
