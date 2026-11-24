using Connectly.Domain.Common;
using Connectly.Domain.Entities.UserProfiles;

namespace Connectly.Domain.Entities.Groups;

/// <summary>Group chat room message (members only).</summary>
public class GroupMessage : MiddleBaseEntity
{
    public Guid GroupId { get; set; }
    public Guid SenderId { get; set; }
    public UserProfile Sender { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
}
