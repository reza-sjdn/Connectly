using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Domain.Entities.Groups.Enums;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Commands.LeaveGroup;

public class LeaveGroupCommandHandler(IApplicationDbContext db, ICurrentUser currentUser) : IRequestHandler<LeaveGroupCommand>
{
    public async Task Handle(LeaveGroupCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var membership = await GroupAccess.MembershipAsync(db, request.GroupId, me, ct);
        if (membership is null) return;

        if (membership is { Status: MembershipStatus.Active, Role: GroupRole.Admin })
        {
            var otherAdmins = await db.GroupMembers.CountAsync(m => m.GroupId == request.GroupId && m.UserId != me
                && m.Role == GroupRole.Admin && m.Status == MembershipStatus.Active, ct);
            var otherMembers = await db.GroupMembers.CountAsync(m => m.GroupId == request.GroupId && m.UserId != me
                && m.Status == MembershipStatus.Active, ct);

            if (otherAdmins == 0 && otherMembers > 0)
                throw new ValidationException([new ValidationFailure("GroupId", "Make another member an admin before you leave.")]);
        }

        db.GroupMembers.Remove(membership);
        await db.SaveChangesAsync(ct);
    }
}
