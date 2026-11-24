using FluentValidation;

namespace Connectly.Application.Follows.Queries.GetFollowList;

public class GetFollowListQueryValidator : AbstractValidator<GetFollowListQuery>
{
    public GetFollowListQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
