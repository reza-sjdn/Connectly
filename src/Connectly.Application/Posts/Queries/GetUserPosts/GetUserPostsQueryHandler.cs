using Connectly.Application.Common.Dtos;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Posts.Dtos;
using Connectly.Domain.Entities.UserProfiles;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Posts.Queries.GetUserPosts;

public class GetUserPostsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetUserPostsQuery, CursorPage<PostDto>>
{
    public async Task<CursorPage<PostDto>> Handle(GetUserPostsQuery request, CancellationToken ct)
    {
        if (!await db.UserProfiles.AnyAsync(p => p.Id == request.UserId, ct))
            throw new NotFoundException(nameof(UserProfile), request.UserId);

        // Group posts live in their group, not on the author's public timeline.
        var posts = db.Posts.AsNoTracking().Where(p => p.AuthorId == request.UserId && p.GroupId == null);
        return await PostProjection.PageAsync(posts, db, currentUser.UserId, request.Cursor, request.PageSize, ct);
    }
}
