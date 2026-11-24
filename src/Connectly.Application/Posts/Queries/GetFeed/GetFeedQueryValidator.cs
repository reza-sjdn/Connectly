using Connectly.Application.Common.Helpers;
using FluentValidation;

namespace Connectly.Application.Posts.Queries.GetFeed;

public class GetFeedQueryValidator : AbstractValidator<GetFeedQuery>
{
    public GetFeedQueryValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 30);
        RuleFor(x => x.Cursor).Must(c => c is null || PageCursor.TryDecode(c, out _, out _)).WithMessage("Invalid cursor.");
    }
}
