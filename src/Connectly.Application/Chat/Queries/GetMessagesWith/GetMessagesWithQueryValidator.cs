using Connectly.Application.Common.Helpers;
using FluentValidation;

namespace Connectly.Application.Chat.Queries.GetMessagesWith;

public class GetMessagesWithQueryValidator : AbstractValidator<GetMessagesWithQuery>
{
    public GetMessagesWithQueryValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.Cursor).Must(c => c is null || PageCursor.TryDecode(c, out _, out _)).WithMessage("Invalid cursor.");
    }
}
