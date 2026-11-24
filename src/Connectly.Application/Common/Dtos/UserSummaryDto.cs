namespace Connectly.Application.Common.Dtos;

public record UserSummaryDto(Guid Id, string FirstName, string LastName, string? AvatarUrl, bool IsFollowedByMe);
