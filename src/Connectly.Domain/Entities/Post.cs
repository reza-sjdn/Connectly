using Connectly.Domain.Common;

namespace Connectly.Domain.Entities;

public class Post : AuditableEntity
{
    public Guid AuthorId { get; set; }
    public UserProfile Author { get; set; } = null!;
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }   // at most one image per post
    public Guid? GroupId { get; set; }      // null = normal post on the author's timeline
    public Group? Group { get; set; }
}

public class Comment : AuditableEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public UserProfile Author { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
}

/// <summary>Like-only reactions for now. Composite key (PostId, UserId) prevents double likes.</summary>
public class PostLike : IHasCreatedAt
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
