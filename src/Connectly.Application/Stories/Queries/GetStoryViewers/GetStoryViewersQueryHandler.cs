using Connectly.Application.Common.Dtos;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Stories.Dtos;
using Connectly.Domain.Entities.Stories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Stories.Queries.GetStoryViewers;

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
