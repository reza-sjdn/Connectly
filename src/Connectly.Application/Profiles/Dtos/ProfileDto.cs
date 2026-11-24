namespace Connectly.Application.Profiles.Dtos;

/// <summary>The signed-in user's own profile (includes private fields such as date of birth).</summary>
public record ProfileDto(
    Guid Id, string FirstName, string LastName, string? Bio,
    string? AvatarUrl, string? CoverUrl, DateOnly? DateOfBirth, DateTime CreatedAtUtc);
