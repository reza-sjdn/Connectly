using Connectly.Application.Common.Interfaces;
using Connectly.Application.Common.Models;
using Connectly.Application.Notifications.Models;
using Connectly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Connectly.Application.Notifications;

internal static class NotificationProjection
{
    public static IQueryable<NotificationDto> ToDto(this IQueryable<Notification> q) =>
        q.Select(n => new NotificationDto(
            n.Id, n.Type,
            new AuthorDto(n.Actor.Id, n.Actor.FirstName, n.Actor.LastName, n.Actor.AvatarUrl),
            n.PostId, n.GroupId, n.Group == null ? null : n.Group.Name,
            n.IsRead, n.CreatedAtUtc));
}

public class NotificationPublisher(
    IApplicationDbContext db, IRealtimeNotifier realtime, ILogger<NotificationPublisher> logger)
    : IUserNotificationPublisher
{
    public Task PublishAsync(Guid recipientId, Guid actorId, NotificationType type, Guid? postId, CancellationToken ct = default)
        => PublishCoreAsync(recipientId, actorId, type, postId, null, ct);

    public Task PublishGroupAsync(Guid recipientId, Guid actorId, NotificationType type, Guid groupId, CancellationToken ct = default)
        => PublishCoreAsync(recipientId, actorId, type, null, groupId, ct);

    private async Task PublishCoreAsync(
        Guid recipientId, Guid actorId, NotificationType type, Guid? postId, Guid? groupId, CancellationToken ct)
    {
        if (recipientId == actorId) return;   // never notify yourself

        try
        {
            // Likes/follows are deduplicated for good; group events only while the previous one is still unread.
            var groupEvent = groupId != null;
            if (type != NotificationType.PostCommented &&
                await db.Notifications.AnyAsync(n => n.RecipientId == recipientId && n.ActorId == actorId
                                                     && n.Type == type && n.PostId == postId && n.GroupId == groupId
                                                     && (!groupEvent || !n.IsRead), ct))
                return;

            var entity = new Notification
            {
                RecipientId = recipientId, ActorId = actorId, Type = type, PostId = postId, GroupId = groupId
            };
            db.Notifications.Add(entity);
            await db.SaveChangesAsync(ct);

            var dto = await db.Notifications.AsNoTracking().Where(n => n.Id == entity.Id).ToDto().FirstAsync(ct);
            var unread = await db.Notifications.CountAsync(n => n.RecipientId == recipientId && !n.IsRead, ct);

            await realtime.NotificationReceivedAsync(recipientId, dto, unread, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not publish {Type} notification to {Recipient}", type, recipientId);
        }
    }

    public async Task PushPostStatsAsync(Guid postId, Guid authorId, Guid actorId, CancellationToken ct = default)
    {
        if (authorId == actorId) return;

        try
        {
            var likes = await db.PostLikes.CountAsync(l => l.PostId == postId, ct);
            var comments = await db.Comments.CountAsync(c => c.PostId == postId, ct);
            await realtime.PostStatsChangedAsync(authorId, postId, likes, comments, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not push stats for post {PostId}", postId);
        }
    }
}
