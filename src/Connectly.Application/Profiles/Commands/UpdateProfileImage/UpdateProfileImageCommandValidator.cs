using Connectly.Application.Common.Helpers;
using FluentValidation;

namespace Connectly.Application.Profiles.Commands.UpdateProfileImage;

public class UpdateProfileImageCommandValidator : AbstractValidator<UpdateProfileImageCommand>
{
    public UpdateProfileImageCommandValidator()
        => RuleFor(x => x.File).NotNull().SetValidator(new ImageUploadValidator());
}
