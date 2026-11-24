using Connectly.Domain.Common;
using Connectly.Domain.Entities.Groups;
using Connectly.Domain.Entities.Notifications.Enums;
using Connectly.Domain.Entities.UserProfiles;

namespace Connectly.Domain.Entities.Notifications;

public class Notification : MiddleBaseEntity
{
    public Guid RecipientId { get; set; }
    public Guid ActorId { get; set; }
    public UserProfile Actor { get; set; } = null!;
    public NotificationType Type { get; set; }
    public Guid? PostId { get; set; }
    public Guid? GroupId { get; set; }
    public Group? Group { get; set; }
    public bool IsRead { get; set; }
}
