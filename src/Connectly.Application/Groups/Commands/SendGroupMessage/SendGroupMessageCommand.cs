using Connectly.Application.Groups.Models;
using MediatR;

namespace Connectly.Application.Groups.Commands.SendGroupMessage;

// ---------- group chat ----------
public record SendGroupMessageCommand(Guid GroupId, string Content) : IRequest<GroupMessageDto>;
