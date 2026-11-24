using Connectly.Application.Common.Helpers;
using FluentValidation;

namespace Connectly.Application.Posts.Queries.GetUserPosts;

public class GetUserPostsQueryValidator : AbstractValidator<GetUserPostsQuery>
{
    public GetUserPostsQueryValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 30);
        RuleFor(x => x.Cursor).Must(c => c is null || PageCursor.TryDecode(c, out _, out _)).WithMessage("Invalid cursor.");
    }
}
