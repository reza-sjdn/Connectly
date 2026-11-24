using Connectly.Application.Common.Dtos;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Posts.Dtos;
using Connectly.Domain.Entities.Groups.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Posts.Queries.GetFeed;

public class GetFeedQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetFeedQuery, CursorPage<PostDto>>
{
    public Task<CursorPage<PostDto>> Handle(GetFeedQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();

        // Normal posts: mine + people I follow. Group posts: only from groups I am an active member of.
        var visible = db.Posts.AsNoTracking().Where(p =>
            (p.GroupId == null && (p.AuthorId == me || db.Follows.Any(f => f.FollowerId == me && f.FolloweeId == p.AuthorId)))
            || (p.GroupId != null && db.GroupMembers.Any(m => m.GroupId == p.GroupId && m.UserId == me
                                                              && m.Status == MembershipStatus.Active)));

        return PostProjection.PageAsync(visible, db, me, request.Cursor, request.PageSize, ct);
    }
}
