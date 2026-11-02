namespace Connectly.Application.Common.Models;

public record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor);

public record AuthorDto(Guid Id, string FirstName, string LastName, string? AvatarUrl);
