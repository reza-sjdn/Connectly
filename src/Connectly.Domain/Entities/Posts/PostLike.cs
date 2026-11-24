using Connectly.Domain.Common;

namespace Connectly.Domain.Entities.Posts;

/// <summary>Like-only reactions for now. Composite key (PostId, UserId) prevents double likes.</summary>
public class PostLike : HasCreatedAt
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
}
