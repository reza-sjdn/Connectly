using Connectly.Application.Common.Dtos;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Groups.Models;
using Connectly.Domain.Entities.Groups.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Queries.GetGroupMembers;

public class GetGroupMembersQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetGroupMembersQuery, PagedResult<GroupMemberDto>>
{
    public async Task<PagedResult<GroupMemberDto>> Handle(GetGroupMembersQuery request, CancellationToken ct)
    {
        var group = await GroupAccess.GetGroupAsync(db, request.GroupId, ct);
        await GroupAccess.RequireCanViewAsync(db, group, currentUser.UserId, ct);

        var query = db.GroupMembers.AsNoTracking()
            .Where(m => m.GroupId == request.GroupId && m.Status == MembershipStatus.Active);
        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(m => m.Role).ThenBy(m => m.CreatedAtUtc)   // admins first
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(m => new GroupMemberDto(
                new AuthorDto(m.User.Id, m.User.FirstName, m.User.LastName, m.User.AvatarUrl), m.Role, m.CreatedAtUtc))
            .ToListAsync(ct);

        return new PagedResult<GroupMemberDto>(items, request.Page, request.PageSize, total);
    }
}
