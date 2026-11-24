using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Domain.Entities.Stories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Stories.Commands.MarkStoryViewed;

public class MarkStoryViewedCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : IRequestHandler<MarkStoryViewedCommand>
{
    public async Task Handle(MarkStoryViewedCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var now = clock.GetUtcNow().UtcDateTime;

        var authorId = await db.Stories
            .Where(s => s.Id == request.StoryId && s.ExpiresAtUtc > now
                        && (s.AuthorId == me || db.Follows.Any(f => f.FollowerId == me && f.FolloweeId == s.AuthorId)))
            .Select(s => (Guid?)s.AuthorId)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Story), request.StoryId);

        if (authorId == me) return;
        if (await db.StoryViews.AnyAsync(v => v.StoryId == request.StoryId && v.ViewerId == me, ct)) return;

        db.StoryViews.Add(new StoryView { StoryId = request.StoryId, ViewerId = me, ViewedAtUtc = now });
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { /* viewed from two tabs at once */ }
    }
}
