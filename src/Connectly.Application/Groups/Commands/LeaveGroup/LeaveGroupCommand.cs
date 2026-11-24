using MediatR;

namespace Connectly.Application.Groups.Commands.LeaveGroup;

/// <summary>Leave the group, or cancel my pending join request.</summary>
public record LeaveGroupCommand(Guid GroupId) : IRequest;
