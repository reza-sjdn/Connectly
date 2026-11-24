using MediatR;

namespace Connectly.Application.Groups.Commands.RemoveMember;

public record RemoveMemberCommand(Guid GroupId, Guid UserId) : IRequest;
