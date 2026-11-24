namespace Connectly.Application.Profiles.Dtos;

public record PublicProfileDto(
    Guid Id, string FirstName, string LastName, string? Bio,
    string? AvatarUrl, string? CoverUrl, DateTime CreatedAtUtc,
    int FollowersCount, int FollowingCount, bool IsMe, bool IsFollowedByMe);
