using MediatR;

namespace Connectly.Application.Notifications.Queries.GetUnreadNotificationCount;

public record GetUnreadNotificationCountQuery : IRequest<int>;
