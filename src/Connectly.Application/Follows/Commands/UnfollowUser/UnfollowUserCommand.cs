using MediatR;

namespace Connectly.Application.Follows.Commands.UnfollowUser;

public record UnfollowUserCommand(Guid UserId) : IRequest;
