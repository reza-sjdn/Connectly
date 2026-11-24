using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Notifications.Commands.MarkNotificationRead;

public class MarkNotificationReadCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        await db.Notifications
            .Where(n => n.Id == request.NotificationId && n.RecipientId == me && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }
}
