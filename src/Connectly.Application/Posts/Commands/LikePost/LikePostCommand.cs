using Connectly.Application.Posts.Dtos;
using MediatR;

namespace Connectly.Application.Posts.Commands.LikePost;

public record LikePostCommand(Guid PostId) : IRequest<LikeStateDto>;
