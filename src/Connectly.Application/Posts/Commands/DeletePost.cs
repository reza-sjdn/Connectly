using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Posts.Commands;

public record DeletePostCommand(Guid PostId) : IRequest;

public class DeletePostCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IFileStorage storage)
    : IRequestHandler<DeletePostCommand>
{
    public async Task Handle(DeletePostCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == request.PostId, ct)
                   ?? throw new NotFoundException(nameof(Post), request.PostId);

        if (post.AuthorId != me) throw new ForbiddenException("You can only delete your own posts.");

        var image = post.ImageUrl;
        db.Posts.Remove(post);                  // comments and likes are removed by FK cascade
        await db.SaveChangesAsync(ct);

        if (image is not null) await storage.DeleteAsync(image, ct);   // after the DB commit
    }
}
