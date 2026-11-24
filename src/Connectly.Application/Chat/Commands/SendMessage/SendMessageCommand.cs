using Connectly.Application.Chat.Dtos;
using MediatR;

namespace Connectly.Application.Chat.Commands.SendMessage;

public record SendMessageCommand(Guid RecipientId, string Content) : IRequest<MessageDto>;
