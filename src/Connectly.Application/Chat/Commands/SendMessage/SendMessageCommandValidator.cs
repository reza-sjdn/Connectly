using FluentValidation;

namespace Connectly.Application.Chat.Commands.SendMessage;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator() => RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
}
