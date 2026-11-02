using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Helpers;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Common.Models;
using Connectly.Application.Posts.Models;
using Connectly.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Posts.Queries;

// ---------- Feed: my posts + posts of people I follow, newest first ----------
public record GetFeedQuery(string? Cursor = null, int PageSize = 10) : IRequest<CursorPage<PostDto>>;

public class GetFeedQueryValidator : AbstractValidator<GetFeedQuery>
{
    public GetFeedQueryValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 30);
        RuleFor(x => x.Cursor).Must(c => c is null || PageCursor.TryDecode(c, out _, out _)).WithMessage("Invalid cursor.");
    }
}

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

// ---------- A user's own timeline (profile page) ----------
public record GetUserPostsQuery(Guid UserId, string? Cursor = null, int PageSize = 10) : IRequest<CursorPage<PostDto>>;

public class GetUserPostsQueryValidator : AbstractValidator<GetUserPostsQuery>
{
    public GetUserPostsQueryValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 30);
        RuleFor(x => x.Cursor).Must(c => c is null || PageCursor.TryDecode(c, out _, out _)).WithMessage("Invalid cursor.");
    }
}

public class GetUserPostsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetUserPostsQuery, CursorPage<PostDto>>
{
    public async Task<CursorPage<PostDto>> Handle(GetUserPostsQuery request, CancellationToken ct)
    {
        if (!await db.UserProfiles.AnyAsync(p => p.Id == request.UserId, ct))
            throw new NotFoundException(nameof(UserProfile), request.UserId);

        // Group posts live in their group, not on the author's public timeline.
        var posts = db.Posts.AsNoTracking().Where(p => p.AuthorId == request.UserId && p.GroupId == null);
        return await PostProjection.PageAsync(posts, db, currentUser.UserId, request.Cursor, request.PageSize, ct);
    }
}

// ---------- Single post ----------
public record GetPostQuery(Guid PostId) : IRequest<PostDto>;

public class GetPostQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetPostQuery, PostDto>
{
    public async Task<PostDto> Handle(GetPostQuery request, CancellationToken ct)
        => await db.Posts.AsNoTracking().Where(p => p.Id == request.PostId)
               .VisibleTo(db, currentUser.UserId).ToDto(db, currentUser.UserId).FirstOrDefaultAsync(ct)
           ?? throw new NotFoundException(nameof(Post), request.PostId);
}
