using Connectly.Domain.Common;

namespace Connectly.Domain.Entities;

/// <summary>Instagram-style, one-way edge: FollowerId follows FolloweeId. Composite key.</summary>
public class Follow : IHasCreatedAt
{
    public Guid FollowerId { get; set; }
    public Guid FolloweeId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
