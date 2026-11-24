using Connectly.Application.Groups.Models;
using Connectly.Domain.Entities.Groups.Enums;
using MediatR;

namespace Connectly.Application.Groups.Commands.CreateGroup;

// ---------- create / update ----------
public record CreateGroupCommand(string Name, string? Description, GroupPrivacy Privacy) : IRequest<GroupDetailsDto>;
