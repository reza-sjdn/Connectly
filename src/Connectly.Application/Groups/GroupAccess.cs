using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Groups.Models;
using Connectly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups;

internal static class GroupAccess
{
    public static Task<GroupMember?> MembershipAsync(IApplicationDbContext db, Guid groupId, Guid userId, CancellationToken ct)
        => db.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId, ct);

    public static async Task<Group> GetGroupAsync(IApplicationDbContext db, Guid groupId, CancellationToken ct)
        => await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId, ct)
           ?? throw new NotFoundException(nameof(Group), groupId);

    public static async Task RequireAdminAsync(IApplicationDbContext db, Guid groupId, Guid userId, CancellationToken ct)
    {
        var m = await MembershipAsync(db, groupId, userId, ct);
        if (m is not { Status: MembershipStatus.Active, Role: GroupRole.Admin })
            throw new ForbiddenException("Only group admins can do this.");
    }

    public static async Task RequireMemberAsync(IApplicationDbContext db, Guid groupId, Guid userId, CancellationToken ct)
    {
        var m = await MembershipAsync(db, groupId, userId, ct);
        if (m is not { Status: MembershipStatus.Active })
            throw new ForbiddenException("Only group members can do this.");
    }

    /// <summary>Public groups are readable by everyone; private groups only by active members.</summary>
    public static async Task RequireCanViewAsync(IApplicationDbContext db, Group group, Guid? userId, CancellationToken ct)
    {
        if (group.Privacy == GroupPrivacy.Public) return;
        if (userId is null) throw new ForbiddenException("This group is private.");
        var m = await MembershipAsync(db, group.Id, userId.Value, ct);
        if (m is not { Status: MembershipStatus.Active }) throw new ForbiddenException("This group is private.");
    }
}

internal static class GroupProjection
{
    public static IQueryable<GroupSummaryDto> ToSummary(this IQueryable<Group> groups, IApplicationDbContext db, Guid? me) =>
        groups.Select(g => new GroupSummaryDto(
            g.Id, g.Name, g.Privacy,
            db.GroupMembers.Count(m => m.GroupId == g.Id && m.Status == MembershipStatus.Active),
            db.GroupMembers.Where(m => m.GroupId == g.Id && m.UserId == me)
                .Select(m => (GroupMembershipState?)(m.Status == MembershipStatus.Pending
                    ? GroupMembershipState.Pending
                    : m.Role == GroupRole.Admin ? GroupMembershipState.Admin : GroupMembershipState.Member))
                .FirstOrDefault() ?? GroupMembershipState.None));

    public static async Task<GroupDetailsDto> DetailsAsync(IApplicationDbContext db, Guid groupId, Guid? me, CancellationToken ct)
    {
        var extra = await db.Groups.AsNoTracking().Where(g => g.Id == groupId)
                        .Select(g => new { g.Description, g.CreatedAtUtc }).FirstOrDefaultAsync(ct)
                    ?? throw new NotFoundException(nameof(Group), groupId);
        var s = await db.Groups.AsNoTracking().Where(g => g.Id == groupId).ToSummary(db, me).FirstAsync(ct);
        return new GroupDetailsDto(s.Id, s.Name, extra.Description, s.Privacy, s.MembersCount, s.MyState, extra.CreatedAtUtc);
    }
}
