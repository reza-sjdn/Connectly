using Connectly.Domain.Common;

namespace Connectly.Domain.Entities.Chats;

public class Message : MiddleBaseEntity
{
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime? ReadAtUtc { get; set; }   // set when the recipient opens the conversation
}
