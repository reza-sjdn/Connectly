using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Notifications.Commands.MarkAllNotificationsRead;

public class MarkAllNotificationsReadCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<MarkAllNotificationsReadCommand>
{
    public async Task Handle(MarkAllNotificationsReadCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        await db.Notifications
            .Where(n => n.RecipientId == me && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }
}
