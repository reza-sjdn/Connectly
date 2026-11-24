using Connectly.Domain.Common;
using Connectly.Domain.Entities.Groups;
using Connectly.Domain.Entities.UserProfiles;

namespace Connectly.Domain.Entities.Posts;

public class Post : AuditableEntity
{
    public Guid AuthorId { get; set; }
    public UserProfile Author { get; set; } = null!;
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }   // at most one image per post
    public Guid? GroupId { get; set; }      // null = normal post on the author's timeline
    public Group? Group { get; set; }
}
