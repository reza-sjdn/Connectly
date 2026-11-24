using Connectly.Application.Posts.Dtos;
using MediatR;

namespace Connectly.Application.Posts.Commands.UnlikePost;

public record UnlikePostCommand(Guid PostId) : IRequest<LikeStateDto>;
