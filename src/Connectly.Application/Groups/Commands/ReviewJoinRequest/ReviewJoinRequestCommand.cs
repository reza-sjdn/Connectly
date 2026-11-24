using MediatR;

namespace Connectly.Application.Groups.Commands.ReviewJoinRequest;

// ---------- admin: join requests ----------
public record ReviewJoinRequestCommand(Guid GroupId, Guid UserId, bool Approve) : IRequest;
