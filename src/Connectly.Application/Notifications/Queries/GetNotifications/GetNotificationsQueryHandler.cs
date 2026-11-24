using Connectly.Application.Common.Dtos;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Helpers;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Notifications.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetNotificationsQuery, CursorPage<NotificationDto>>
{
    public async Task<CursorPage<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();

        var query = db.Notifications.AsNoTracking().Where(n => n.RecipientId == me);
        if (PageCursor.TryDecode(request.Cursor, out var at, out var id))
            query = query.Where(n => n.CreatedAtUtc < at || (n.CreatedAtUtc == at && n.Id.CompareTo(id) < 0));

        var rows = await query
            .OrderByDescending(n => n.CreatedAtUtc).ThenByDescending(n => n.Id)
            .Take(request.PageSize + 1)
            .ToDto()
            .ToListAsync(ct);

        var hasMore = rows.Count > request.PageSize;
        var items = hasMore ? rows.Take(request.PageSize).ToList() : rows;
        return new CursorPage<NotificationDto>(items, hasMore ? PageCursor.Encode(items[^1].CreatedAtUtc, items[^1].Id) : null);
    }
}
