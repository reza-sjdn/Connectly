using Connectly.Domain.Common;

namespace Connectly.Domain.Entities;

/// <summary>
/// 1:1 conversation. The pair is stored in canonical order (UserAId &lt; UserBId by Guid.CompareTo),
/// so a unique index on (UserAId, UserBId) guarantees exactly one conversation per pair.
/// </summary>
public class Conversation : BaseEntity, IHasCreatedAt
{
    public Guid UserAId { get; set; }
    public Guid UserBId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastMessageAtUtc { get; set; }
}

public class Message : BaseEntity, IHasCreatedAt
{
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }   // set when the recipient opens the conversation
}
