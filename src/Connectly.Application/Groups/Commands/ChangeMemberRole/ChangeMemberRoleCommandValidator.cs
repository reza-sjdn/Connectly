using FluentValidation;

namespace Connectly.Application.Groups.Commands.ChangeMemberRole;

public class ChangeMemberRoleCommandValidator : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleCommandValidator() => RuleFor(x => x.Role).IsInEnum();
}
