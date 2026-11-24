using Connectly.Application.Common.Dtos;
using Connectly.Application.Posts.Dtos;
using MediatR;

namespace Connectly.Application.Posts.Queries.GetFeed;

// ---------- Feed: my posts + posts of people I follow, newest first ----------
public record GetFeedQuery(string? Cursor = null, int PageSize = 10) : IRequest<CursorPage<PostDto>>;
