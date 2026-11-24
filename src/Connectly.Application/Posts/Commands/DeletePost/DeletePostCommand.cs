using MediatR;

namespace Connectly.Application.Posts.Commands.DeletePost;

public record DeletePostCommand(Guid PostId) : IRequest;
