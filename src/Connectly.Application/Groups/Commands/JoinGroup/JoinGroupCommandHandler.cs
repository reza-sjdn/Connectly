using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Groups.Models;
using Connectly.Domain.Entities.Groups;
using Connectly.Domain.Entities.Groups.Enums;
using Connectly.Domain.Entities.Notifications.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Commands.JoinGroup;

public class JoinGroupCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IUserNotificationPublisher notifications)
    : IRequestHandler<JoinGroupCommand, GroupDetailsDto>
{
    public async Task<GroupDetailsDto> Handle(JoinGroupCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var group = await GroupAccess.GetGroupAsync(db, request.GroupId, ct);

        // Idempotent: already a member (or already requested) -> just return the current state.
        if (await GroupAccess.MembershipAsync(db, group.Id, me, ct) is null)
        {
            var isPublic = group.Privacy == GroupPrivacy.Public;
            db.GroupMembers.Add(new GroupMember
            {
                GroupId = group.Id,
                UserId = me,
                Role = GroupRole.Member,
                Status = isPublic ? MembershipStatus.Active : MembershipStatus.Pending
            });

            try { await db.SaveChangesAsync(ct); }
            catch (DbUpdateException) { return await GroupProjection.DetailsAsync(db, group.Id, me, ct); }   // double click

            if (!isPublic)
            {
                var adminIds = await db.GroupMembers
                    .Where(m => m.GroupId == group.Id && m.Role == GroupRole.Admin && m.Status == MembershipStatus.Active)
                    .Select(m => m.UserId).ToListAsync(ct);
                foreach (var adminId in adminIds)
                    await notifications.PublishGroupAsync(adminId, me, NotificationType.GroupJoinRequested, group.Id, ct);
            }
        }

        return await GroupProjection.DetailsAsync(db, group.Id, me, ct);
    }
}
