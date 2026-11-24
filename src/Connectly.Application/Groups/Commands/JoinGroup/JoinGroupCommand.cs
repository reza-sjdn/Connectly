using Connectly.Application.Groups.Models;
using MediatR;

namespace Connectly.Application.Groups.Commands.JoinGroup;

// ---------- join / leave ----------
public record JoinGroupCommand(Guid GroupId) : IRequest<GroupDetailsDto>;
