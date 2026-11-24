using MediatR;

namespace Connectly.Application.Follows.Commands.FollowUser;

public record FollowUserCommand(Guid UserId) : IRequest;
