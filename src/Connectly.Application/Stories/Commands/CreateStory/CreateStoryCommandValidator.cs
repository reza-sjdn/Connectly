using Connectly.Application.Common.Helpers;
using FluentValidation;

namespace Connectly.Application.Stories.Commands.CreateStory;

public class CreateStoryCommandValidator : AbstractValidator<CreateStoryCommand>
{
    public CreateStoryCommandValidator()
    {
        RuleFor(x => x.Text).MaximumLength(280);
        RuleFor(x => x)
            .Must(x => (x.Image is not null) ^ !string.IsNullOrWhiteSpace(x.Text))
            .OverridePropertyName("Text")
            .WithMessage("Share either a photo or some text.");
        RuleFor(x => x.BackgroundColor)
            .Must(c => c is null || System.Text.RegularExpressions.Regex.IsMatch(c, "^#[0-9a-fA-F]{6}$"))
            .WithMessage("Background color must look like #1877f2.");
        When(x => x.Image is not null, () => RuleFor(x => x.Image!).SetValidator(new ImageUploadValidator()));
    }
}
