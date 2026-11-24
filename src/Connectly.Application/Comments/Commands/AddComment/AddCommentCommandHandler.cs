using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Posts;
using Connectly.Application.Posts.Dtos;
using Connectly.Domain.Entities.Notifications.Enums;
using Connectly.Domain.Entities.Posts;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Comments.Commands.AddComment;

public class AddCommentCommandHandler(
    IApplicationDbContext db, ICurrentUser currentUser, IUserNotificationPublisher notifications, IRealtimeNotifier realtime)
    : IRequestHandler<AddCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(AddCommentCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();

        var authorId = await db.Posts.Where(p => p.Id == request.PostId).VisibleTo(db, me)
                           .Select(p => (Guid?)p.AuthorId).FirstOrDefaultAsync(ct)
                       ?? throw new NotFoundException(nameof(Post), request.PostId);

        var comment = new Comment { PostId = request.PostId, AuthorId = me, Content = request.Content.Trim() };
        db.Comments.Add(comment);
        await db.SaveChangesAsync(ct);

        var dto = await db.Comments.AsNoTracking().Where(c => c.Id == comment.Id).ToDto(me).FirstAsync(ct);

        if (authorId != me)
        {
            await notifications.PublishAsync(authorId, me, NotificationType.PostCommented, request.PostId, ct);
            await realtime.CommentAddedAsync(authorId, request.PostId, dto, ct);   // appears live under the open post
            await notifications.PushPostStatsAsync(request.PostId, authorId, me, ct);
        }
        return dto;
    }
}
