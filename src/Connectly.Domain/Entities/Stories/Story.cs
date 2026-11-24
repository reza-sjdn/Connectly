using Connectly.Domain.Common;
using Connectly.Domain.Entities.UserProfiles;

namespace Connectly.Domain.Entities.Stories;

/// <summary>A story is either a photo (ImageUrl) or text on a colored background (Text + BackgroundColor).</summary>
public class Story : MiddleBaseEntity
{
    public Guid AuthorId { get; set; }
    public UserProfile Author { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string? Text { get; set; }
    public string? BackgroundColor { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}
