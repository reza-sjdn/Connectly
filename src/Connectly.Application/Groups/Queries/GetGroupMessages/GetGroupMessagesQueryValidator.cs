using Connectly.Application.Common.Helpers;
using FluentValidation;

namespace Connectly.Application.Groups.Queries.GetGroupMessages;

public class GetGroupMessagesQueryValidator : AbstractValidator<GetGroupMessagesQuery>
{
    public GetGroupMessagesQueryValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.Cursor).Must(c => c is null || PageCursor.TryDecode(c, out _, out _)).WithMessage("Invalid cursor.");
    }
}
