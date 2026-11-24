using Connectly.Domain.Common;
using Connectly.Domain.Entities.Groups.Enums;
using Connectly.Domain.Entities.UserProfiles;

namespace Connectly.Domain.Entities.Groups;

/// <summary>Membership row. Pending = join request waiting for admin approval (private groups only).</summary>
public class GroupMember : HasCreatedAt
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public UserProfile User { get; set; } = null!;
    public GroupRole Role { get; set; }
    public MembershipStatus Status { get; set; }
}
