using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Posts.Dtos;
using Connectly.Domain.Entities.Posts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Posts.Commands.UnlikePost;

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
