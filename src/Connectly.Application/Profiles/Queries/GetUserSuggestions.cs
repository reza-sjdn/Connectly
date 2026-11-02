using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Profiles.Queries;

/// <summary>People the current user does not follow yet (newest first). Real search arrives in a later slice.</summary>
public record GetUserSuggestionsQuery(int Take = 20) : IRequest<IReadOnlyList<UserSummaryDto>>;

public class GetUserSuggestionsQueryValidator : AbstractValidator<GetUserSuggestionsQuery>
{
    public GetUserSuggestionsQueryValidator() => RuleFor(x => x.Take).InclusiveBetween(1, 50);
}

public class GetUserSuggestionsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetUserSuggestionsQuery, IReadOnlyList<UserSummaryDto>>
{
    public async Task<IReadOnlyList<UserSummaryDto>> Handle(GetUserSuggestionsQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();

        return await db.UserProfiles
            .AsNoTracking()
            .Where(p => p.Id != me && !db.Follows.Any(f => f.FollowerId == me && f.FolloweeId == p.Id))
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(request.Take)
            .Select(p => new UserSummaryDto(p.Id, p.FirstName, p.LastName, p.AvatarUrl, false))
            .ToListAsync(ct);
    }
}
