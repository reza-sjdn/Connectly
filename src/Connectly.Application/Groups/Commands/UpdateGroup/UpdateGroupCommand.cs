using Connectly.Application.Groups.Models;
using Connectly.Domain.Entities.Groups.Enums;
using MediatR;

namespace Connectly.Application.Groups.Commands.UpdateGroup;

public record UpdateGroupCommand(Guid GroupId, string Name, string? Description, GroupPrivacy Privacy) : IRequest<GroupDetailsDto>;
