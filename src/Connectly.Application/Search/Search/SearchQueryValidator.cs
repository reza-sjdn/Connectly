using FluentValidation;

namespace Connectly.Application.Search.Search;

public class SearchQueryValidator : AbstractValidator<SearchQuery>
{
    public SearchQueryValidator()
        => RuleFor(x => x.Term).NotEmpty().Must(t => t.Trim().Length is >= 2 and <= 100)
            .WithMessage("Search for 2 to 100 characters.");
}
