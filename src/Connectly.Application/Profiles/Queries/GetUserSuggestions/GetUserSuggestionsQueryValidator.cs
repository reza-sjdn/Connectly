using FluentValidation;

namespace Connectly.Application.Profiles.Queries.GetUserSuggestions;

public class GetUserSuggestionsQueryValidator : AbstractValidator<GetUserSuggestionsQuery>
{
    public GetUserSuggestionsQueryValidator() => RuleFor(x => x.Take).InclusiveBetween(1, 50);
}
