using Connectly.Application.Common.Dtos;
using Connectly.Application.Posts.Dtos;
using MediatR;

namespace Connectly.Application.Posts.Queries.GetUserPosts;

// ---------- A user's own timeline (profile page) ----------
public record GetUserPostsQuery(Guid UserId, string? Cursor = null, int PageSize = 10) : IRequest<CursorPage<PostDto>>;
