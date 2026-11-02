using Connectly.Domain.Common;

namespace Connectly.Domain.Entities;

public enum GroupPrivacy { Public = 1, Private = 2 }
public enum GroupRole { Member = 1, Admin = 2 }
public enum MembershipStatus { Pending = 1, Active = 2 }

public class Group : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public GroupPrivacy Privacy { get; set; }
    public Guid CreatedById { get; set; }
}

/// <summary>Membership row. Pending = join request waiting for admin approval (private groups only).</summary>
public class GroupMember : IHasCreatedAt
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public UserProfile User { get; set; } = null!;
    public GroupRole Role { get; set; }
    public MembershipStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

/// <summary>Group chat room message (members only).</summary>
public class GroupMessage : BaseEntity, IHasCreatedAt
{
    public Guid GroupId { get; set; }
    public Guid SenderId { get; set; }
    public UserProfile Sender { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
