using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Groups.Enums;
using Connectly.Application.Groups.Models;
using Connectly.Domain.Entities.Groups.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Queries.GetGroups;

public class GetGroupsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetGroupsQuery, IReadOnlyList<GroupSummaryDto>>
{
    public async Task<IReadOnlyList<GroupSummaryDto>> Handle(GetGroupsQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var groups = db.Groups.AsNoTracking();

        // "Mine" = groups I am an active member of; "Discover" = everything else (pending requests included).
        groups = request.Scope == GroupListScope.Mine
            ? groups.Where(g => db.GroupMembers.Any(m => m.GroupId == g.Id && m.UserId == me && m.Status == MembershipStatus.Active))
                    .OrderBy(g => g.Name)
            : groups.Where(g => !db.GroupMembers.Any(m => m.GroupId == g.Id && m.UserId == me && m.Status == MembershipStatus.Active))
                    .OrderByDescending(g => g.CreatedAtUtc);

        return await groups.Take(50).ToSummary(db, me).ToListAsync(ct);
    }
}
