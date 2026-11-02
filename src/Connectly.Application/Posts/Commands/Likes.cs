using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Posts.Models;
using Connectly.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Posts.Commands;

public record LikePostCommand(Guid PostId) : IRequest<LikeStateDto>;

public class LikePostCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IUserNotificationPublisher notifications)
    : IRequestHandler<LikePostCommand, LikeStateDto>
{
    public async Task<LikeStateDto> Handle(LikePostCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();

        var authorId = await db.Posts.Where(p => p.Id == request.PostId).VisibleTo(db, me)
                           .Select(p => (Guid?)p.AuthorId).FirstOrDefaultAsync(ct)
                       ?? throw new NotFoundException(nameof(Post), request.PostId);

        // Idempotent: liking twice is a no-op.
        var added = false;
        if (!await db.PostLikes.AnyAsync(l => l.PostId == request.PostId && l.UserId == me, ct))
        {
            db.PostLikes.Add(new PostLike { PostId = request.PostId, UserId = me });
            try { await db.SaveChangesAsync(ct); added = true; }
            catch (DbUpdateException) { /* concurrent double-click: the like already exists */ }
        }

        var count = await db.PostLikes.CountAsync(l => l.PostId == request.PostId, ct);

        if (added)
        {
            await notifications.PublishAsync(authorId, me, NotificationType.PostLiked, request.PostId, ct);
            await notifications.PushPostStatsAsync(request.PostId, authorId, me, ct);
        }
        return new LikeStateDto(true, count);
    }
}

public record UnlikePostCommand(Guid PostId) : IRequest<LikeStateDto>;

public class UnlikePostCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IUserNotificationPublisher notifications)
    : IRequestHandler<UnlikePostCommand, LikeStateDto>
{
    public async Task<LikeStateDto> Handle(UnlikePostCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();

        var authorId = await db.Posts.Where(p => p.Id == request.PostId).VisibleTo(db, me)
                           .Select(p => (Guid?)p.AuthorId).FirstOrDefaultAsync(ct)
                       ?? throw new NotFoundException(nameof(Post), request.PostId);

        await db.PostLikes
            .Where(l => l.PostId == request.PostId && l.UserId == me)
            .ExecuteDeleteAsync(ct);

        var count = await db.PostLikes.CountAsync(l => l.PostId == request.PostId, ct);
        await notifications.PushPostStatsAsync(request.PostId, authorId, me, ct);
        return new LikeStateDto(false, count);
    }
}
