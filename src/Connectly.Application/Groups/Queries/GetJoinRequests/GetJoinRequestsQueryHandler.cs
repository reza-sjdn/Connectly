using Connectly.Application.Common.Dtos;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Groups.Models;
using Connectly.Domain.Entities.Groups.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Queries.GetJoinRequests;

public class GetJoinRequestsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetJoinRequestsQuery, IReadOnlyList<JoinRequestDto>>
{
    public async Task<IReadOnlyList<JoinRequestDto>> Handle(GetJoinRequestsQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        await GroupAccess.RequireAdminAsync(db, request.GroupId, me, ct);

        return await db.GroupMembers.AsNoTracking()
            .Where(m => m.GroupId == request.GroupId && m.Status == MembershipStatus.Pending)
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new JoinRequestDto(
                new AuthorDto(m.User.Id, m.User.FirstName, m.User.LastName, m.User.AvatarUrl), m.CreatedAtUtc))
            .ToListAsync(ct);
    }
}
