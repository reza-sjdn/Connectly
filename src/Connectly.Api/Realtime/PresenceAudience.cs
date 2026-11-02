using Connectly.Application.Common.Interfaces;

namespace Connectly.Api.Realtime;

public static class PresenceAudience
{
    /// <summary>Users who may see my presence (and whose presence I may see): follow edges either way + chat partners.</summary>
    public static IQueryable<Guid> RelatedTo(IApplicationDbContext db, Guid me) =>
        db.Follows.Where(f => f.FollowerId == me).Select(f => f.FolloweeId)
            .Union(db.Follows.Where(f => f.FolloweeId == me).Select(f => f.FollowerId))
            .Union(db.Conversations.Where(c => c.UserAId == me).Select(c => c.UserBId))
            .Union(db.Conversations.Where(c => c.UserBId == me).Select(c => c.UserAId));
}
