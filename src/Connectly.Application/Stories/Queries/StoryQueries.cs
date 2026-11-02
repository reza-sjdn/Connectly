using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Common.Models;
using Connectly.Application.Stories.Models;
using Connectly.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Stories.Queries;

/// <summary>Active stories of me + people I follow, grouped per author: mine first, then unseen, then newest.</summary>
public record GetStoryFeedQuery : IRequest<IReadOnlyList<StoryGroupDto>>;

public class GetStoryFeedQueryHandler(IApplicationDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : IRequestHandler<GetStoryFeedQuery, IReadOnlyList<StoryGroupDto>>
{
    public async Task<IReadOnlyList<StoryGroupDto>> Handle(GetStoryFeedQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var now = clock.GetUtcNow().UtcDateTime;

        var rows = await db.Stories.AsNoTracking()
            .Where(s => s.ExpiresAtUtc > now
                        && (s.AuthorId == me || db.Follows.Any(f => f.FollowerId == me && f.FolloweeId == s.AuthorId)))
            .OrderBy(s => s.CreatedAtUtc)
            .Take(300)
            .Select(s => new
            {
                s.Id, s.AuthorId, s.Author.FirstName, s.Author.LastName, s.Author.AvatarUrl,
                s.ImageUrl, s.Text, s.BackgroundColor, s.CreatedAtUtc, s.ExpiresAtUtc,
                Seen = s.AuthorId == me || db.StoryViews.Any(v => v.StoryId == s.Id && v.ViewerId == me),
                Views = db.StoryViews.Count(v => v.StoryId == s.Id)
            })
            .ToListAsync(ct);

        return rows
            .GroupBy(r => r.AuthorId)
            .Select(g =>
            {
                var first = g.First();
                var stories = g.Select(r => new StoryDto(
                    r.Id, r.ImageUrl, r.Text, r.BackgroundColor, r.CreatedAtUtc, r.ExpiresAtUtc,
                    r.Seen, r.AuthorId == me ? r.Views : 0)).ToList();   // view counts are only shown to the author
                return new StoryGroupDto(
                    new AuthorDto(first.AuthorId, first.FirstName, first.LastName, first.AvatarUrl),
                    first.AuthorId == me, stories.Any(s => !s.IsSeenByMe), stories);
            })
            .OrderByDescending(g => g.IsMe)
            .ThenByDescending(g => g.HasUnseen)
            .ThenByDescending(g => g.Stories[^1].CreatedAtUtc)
            .ToList();
    }
}

/// <summary>"Seen by" list - author only.</summary>
public record GetStoryViewersQuery(Guid StoryId) : IRequest<IReadOnlyList<StoryViewerDto>>;

public class GetStoryViewersQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetStoryViewersQuery, IReadOnlyList<StoryViewerDto>>
{
    public async Task<IReadOnlyList<StoryViewerDto>> Handle(GetStoryViewersQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var authorId = await db.Stories.Where(s => s.Id == request.StoryId)
                           .Select(s => (Guid?)s.AuthorId).FirstOrDefaultAsync(ct)
                       ?? throw new NotFoundException(nameof(Story), request.StoryId);
        if (authorId != me) throw new ForbiddenException("Only the author can see who viewed a story.");

        return await (
                from v in db.StoryViews.AsNoTracking()
                where v.StoryId == request.StoryId
                join p in db.UserProfiles on v.ViewerId equals p.Id
                orderby v.ViewedAtUtc descending
                select new StoryViewerDto(new AuthorDto(p.Id, p.FirstName, p.LastName, p.AvatarUrl), v.ViewedAtUtc))
            .ToListAsync(ct);
    }
}
