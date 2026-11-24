using Connectly.Application.Common.Dtos;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Stories.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Stories.Queries.GetStoryFeed;

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
                s.Id,
                s.AuthorId,
                s.Author.FirstName,
                s.Author.LastName,
                s.Author.AvatarUrl,
                s.ImageUrl,
                s.Text,
                s.BackgroundColor,
                s.CreatedAtUtc,
                s.ExpiresAtUtc,
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
