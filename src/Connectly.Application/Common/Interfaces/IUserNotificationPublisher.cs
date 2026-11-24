using Connectly.Domain.Entities.Notifications.Enums;

namespace Connectly.Application.Common.Interfaces;

/// <summary>Stores a notification and pushes it live. Failures are logged, never thrown, so they cannot break the main action.</summary>
public interface IUserNotificationPublisher
{
    Task PublishAsync(Guid recipientId, Guid actorId, NotificationType type, Guid? postId, CancellationToken ct = default);
    Task PublishGroupAsync(Guid recipientId, Guid actorId, NotificationType type, Guid groupId, CancellationToken ct = default);
    Task PushPostStatsAsync(Guid postId, Guid authorId, Guid actorId, CancellationToken ct = default);
}
