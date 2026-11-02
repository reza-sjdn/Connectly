using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Follows.Queries;

public enum FollowDirection { Followers, Following }

public record GetFollowListQuery(Guid UserId, FollowDirection Direction, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<UserSummaryDto>>;

public class GetFollowListQueryValidator : AbstractValidator<GetFollowListQuery>
{
    public GetFollowListQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}

public class GetFollowListQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetFollowListQuery, PagedResult<UserSummaryDto>>
{
    public async Task<PagedResult<UserSummaryDto>> Handle(GetFollowListQuery request, CancellationToken ct)
    {
        if (!await db.UserProfiles.AnyAsync(p => p.Id == request.UserId, ct))
            throw new NotFoundException(nameof(Domain.Entities.UserProfile), request.UserId);

        var me = currentUser.UserId;
        var target = request.UserId;

        // "Other" side of each edge: followers of the target, or people the target follows.
        var edges = request.Direction == FollowDirection.Followers
            ? db.Follows.Where(f => f.FolloweeId == target).Select(f => new { OtherId = f.FollowerId, f.CreatedAtUtc })
            : db.Follows.Where(f => f.FollowerId == target).Select(f => new { OtherId = f.FolloweeId, f.CreatedAtUtc });

        var total = await edges.CountAsync(ct);

        var items = await (
                from e in edges
                join p in db.UserProfiles on e.OtherId equals p.Id
                orderby e.CreatedAtUtc descending
                select new UserSummaryDto(
                    p.Id, p.FirstName, p.LastName, p.AvatarUrl,
                    db.Follows.Any(f => f.FollowerId == me && f.FolloweeId == p.Id)))
            .AsNoTracking()
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<UserSummaryDto>(items, request.Page, request.PageSize, total);
    }
}
