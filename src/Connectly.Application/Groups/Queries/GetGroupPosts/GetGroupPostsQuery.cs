using Connectly.Application.Common.Dtos;
using Connectly.Application.Posts.Dtos;
using MediatR;

namespace Connectly.Application.Groups.Queries.GetGroupPosts;

// ---------- group posts ----------
public record GetGroupPostsQuery(Guid GroupId, string? Cursor = null, int PageSize = 10) : IRequest<CursorPage<PostDto>>;
