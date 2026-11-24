using FluentValidation;

namespace Connectly.Application.Groups.Commands.SendGroupMessage;

public class SendGroupMessageCommandValidator : AbstractValidator<SendGroupMessageCommand>
{
    public SendGroupMessageCommandValidator() => RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
}
