namespace Connectly.Application.Common.Models;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public record UserSummaryDto(Guid Id, string FirstName, string LastName, string? AvatarUrl, bool IsFollowedByMe);

/// <summary>Transport-agnostic upload so the Application layer never touches IFormFile.</summary>
public sealed record FileUpload(Stream Content, string FileName, string ContentType, long Length);
