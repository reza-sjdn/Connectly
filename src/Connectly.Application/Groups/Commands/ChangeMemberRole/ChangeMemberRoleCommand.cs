using Connectly.Domain.Entities.Groups.Enums;
using MediatR;

namespace Connectly.Application.Groups.Commands.ChangeMemberRole;

// ---------- admin: roles and removal ----------
public record ChangeMemberRoleCommand(Guid GroupId, Guid UserId, GroupRole Role) : IRequest;
