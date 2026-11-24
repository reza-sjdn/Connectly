using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Notifications.Queries.GetUnreadNotificationCount;

public class GetUnreadNotificationCountQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    public Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        return db.Notifications.CountAsync(n => n.RecipientId == me && !n.IsRead, ct);
    }
}
