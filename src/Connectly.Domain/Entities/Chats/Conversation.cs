using Connectly.Domain.Common;

namespace Connectly.Domain.Entities.Chats;

/// <summary>
/// 1:1 conversation. The pair is stored in canonical order (UserAId &lt; UserBId by Guid.CompareTo),
/// so a unique index on (UserAId, UserBId) guarantees exactly one conversation per pair.
/// </summary>
public class Conversation : MiddleBaseEntity
{
    public Guid UserAId { get; set; }
    public Guid UserBId { get; set; }
    public DateTime LastMessageAtUtc { get; set; }
}
