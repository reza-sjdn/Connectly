using FluentValidation;

namespace Connectly.Application.Profiles.Commands.UpdateMyProfile;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Bio).MaximumLength(500);
        RuleFor(x => x.DateOfBirth)
            .Must(d => d is null || d <= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-13))
            .WithMessage("You must be at least 13 years old.");
    }
}
