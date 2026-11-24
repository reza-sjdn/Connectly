using FluentValidation;

namespace Connectly.Application.Comments.Commands.AddComment;

public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator() => RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
}
