using Connectly.Domain.Common;
using Connectly.Domain.Entities.UserProfiles;

namespace Connectly.Domain.Entities.Posts;

public class Comment : AuditableEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public UserProfile Author { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
}
