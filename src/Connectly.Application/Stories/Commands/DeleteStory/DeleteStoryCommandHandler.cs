using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Domain.Entities.Stories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Stories.Commands.DeleteStory;

public class DeleteStoryCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IFileStorage storage)
    : IRequestHandler<DeleteStoryCommand>
{
    public async Task Handle(DeleteStoryCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var story = await db.Stories.FirstOrDefaultAsync(s => s.Id == request.StoryId, ct)
                    ?? throw new NotFoundException(nameof(Story), request.StoryId);
        if (story.AuthorId != me) throw new ForbiddenException("You can only delete your own stories.");

        var image = story.ImageUrl;
        db.Stories.Remove(story);                 // views are removed by FK cascade
        await db.SaveChangesAsync(ct);
        if (image is not null) await storage.DeleteAsync(image, ct);
    }
}
