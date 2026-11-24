using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Domain.Entities.Groups.Enums;
using Connectly.Domain.Entities.Notifications.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Commands.ReviewJoinRequest;

public class ReviewJoinRequestCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IUserNotificationPublisher notifications)
    : IRequestHandler<ReviewJoinRequestCommand>
{
    public async Task Handle(ReviewJoinRequestCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        await GroupAccess.RequireAdminAsync(db, request.GroupId, me, ct);

        var pending = await db.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == request.GroupId
            && m.UserId == request.UserId && m.Status == MembershipStatus.Pending, ct)
            ?? throw new NotFoundException("Join request", request.UserId);

        if (request.Approve)
        {
            pending.Status = MembershipStatus.Active;
            await db.SaveChangesAsync(ct);
            await notifications.PublishGroupAsync(request.UserId, me, NotificationType.GroupRequestApproved, request.GroupId, ct);
        }
        else
        {
            db.GroupMembers.Remove(pending);
            await db.SaveChangesAsync(ct);
        }
    }
}
