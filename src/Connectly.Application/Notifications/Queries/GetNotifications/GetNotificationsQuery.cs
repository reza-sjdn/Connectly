using Connectly.Application.Common.Dtos;
using Connectly.Application.Notifications.Dtos;
using MediatR;

namespace Connectly.Application.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(string? Cursor = null, int PageSize = 20) : IRequest<CursorPage<NotificationDto>>;
