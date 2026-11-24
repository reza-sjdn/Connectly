using Connectly.Application.Posts.Dtos;
using MediatR;

namespace Connectly.Application.Posts.Queries.GetPost;

// ---------- Single post ----------
public record GetPostQuery(Guid PostId) : IRequest<PostDto>;
