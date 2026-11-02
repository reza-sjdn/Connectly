using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Posts;
using Connectly.Application.Posts.Models;
using Connectly.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Comments.Commands;

public record AddCommentCommand(Guid PostId, string Content) : IRequest<CommentDto>;

public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator() => RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
}

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

public record DeleteCommentCommand(Guid CommentId) : IRequest;

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
