using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Domain.Entities.Posts;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Comments.Commands.DeleteComment;

public class DeleteCommentCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IUserNotificationPublisher notifications)
    : IRequestHandler<DeleteCommentCommand>
{
    public async Task Handle(DeleteCommentCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();

        var row = await db.Comments
            .Where(c => c.Id == request.CommentId)
            .Select(c => new { Comment = c, PostAuthorId = c.Post.AuthorId })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Comment), request.CommentId);

        if (row.Comment.AuthorId != me && row.PostAuthorId != me)
            throw new ForbiddenException("You cannot delete this comment.");

        var postId = row.Comment.PostId;
        db.Comments.Remove(row.Comment);
        await db.SaveChangesAsync(ct);

        await notifications.PushPostStatsAsync(postId, row.PostAuthorId, me, ct);
    }
}
