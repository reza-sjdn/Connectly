using FluentValidation;

namespace Connectly.Application.Groups.Queries.GetGroupMembers;

public class GetGroupMembersQueryValidator : AbstractValidator<GetGroupMembersQuery>
{
    public GetGroupMembersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
