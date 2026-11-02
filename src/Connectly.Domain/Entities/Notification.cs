using Connectly.Domain.Common;

namespace Connectly.Domain.Entities;

public enum NotificationType
{
    NewFollower = 1,
    PostLiked = 2,
    PostCommented = 3,
    GroupJoinRequested = 4,
    GroupRequestApproved = 5
}

public class Notification : BaseEntity, IHasCreatedAt
{
    public Guid RecipientId { get; set; }
    public Guid ActorId { get; set; }
    public UserProfile Actor { get; set; } = null!;
    public NotificationType Type { get; set; }
    public Guid? PostId { get; set; }
    public Guid? GroupId { get; set; }
    public Group? Group { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
