using Connectly.Domain.Common;

namespace Connectly.Domain.Entities;

/// <summary>A story is either a photo (ImageUrl) or text on a colored background (Text + BackgroundColor).</summary>
public class Story : BaseEntity, IHasCreatedAt
{
    public Guid AuthorId { get; set; }
    public UserProfile Author { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string? Text { get; set; }
    public string? BackgroundColor { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}

public class StoryView
{
    public Guid StoryId { get; set; }
    public Guid ViewerId { get; set; }
    public DateTime ViewedAtUtc { get; set; }
}
