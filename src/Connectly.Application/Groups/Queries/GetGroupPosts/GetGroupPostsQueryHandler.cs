using Connectly.Application.Common.Dtos;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Posts;
using Connectly.Application.Posts.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Queries.GetGroupPosts;

public class GetGroupPostsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetGroupPostsQuery, CursorPage<PostDto>>
{
    public async Task<CursorPage<PostDto>> Handle(GetGroupPostsQuery request, CancellationToken ct)
    {
        var group = await GroupAccess.GetGroupAsync(db, request.GroupId, ct);
        await GroupAccess.RequireCanViewAsync(db, group, currentUser.UserId, ct);

        var posts = db.Posts.AsNoTracking().Where(p => p.GroupId == request.GroupId);
        return await PostProjection.PageAsync(posts, db, currentUser.UserId, request.Cursor, request.PageSize, ct);
    }
}
