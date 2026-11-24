using Connectly.Application.Common.Helpers;
using FluentValidation;

namespace Connectly.Application.Posts.Commands.CreatePost;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Content).MaximumLength(5000);
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Content) || x.Image is not null)
            .OverridePropertyName("Content")
            .WithMessage("A post needs some text or an image.");
        When(x => x.Image is not null, () => RuleFor(x => x.Image!).SetValidator(new ImageUploadValidator()));
    }
}
