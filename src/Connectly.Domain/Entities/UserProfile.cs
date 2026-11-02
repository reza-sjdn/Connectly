using Connectly.Domain.Common;

namespace Connectly.Domain.Entities;

/// <summary>Public-facing profile. Shares its primary key with the Identity user.</summary>
public class UserProfile : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}
