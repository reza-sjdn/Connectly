using Connectly.Domain.Common;

namespace Connectly.Domain.Entities.Follows;

/// <summary>Instagram-style, one-way edge: FollowerId follows FolloweeId. Composite key.</summary>
public class Follow : HasCreatedAt
{
    public Guid FollowerId { get; set; }
    public Guid FolloweeId { get; set; }
}
