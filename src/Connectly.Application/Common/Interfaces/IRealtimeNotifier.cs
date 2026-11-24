using Connectly.Application.Chat.Dtos;
using Connectly.Application.Groups.Models;
using Connectly.Application.Notifications.Dtos;
using Connectly.Application.Posts.Dtos;

namespace Connectly.Application.Common.Interfaces;

/// <summary>Pushes events to connected clients. Implemented with SignalR in the Api layer; must never throw.</summary>
public interface IRealtimeNotifier
{
    Task NotificationReceivedAsync(Guid userId, NotificationDto notification, int unreadCount, CancellationToken ct = default);
    Task MessageReceivedAsync(IEnumerable<Guid> userIds, MessageDto message, CancellationToken ct = default);
    Task MessagesReadAsync(Guid userId, Guid readerId, DateTime readAtUtc, CancellationToken ct = default);
    Task CommentAddedAsync(Guid userId, Guid postId, CommentDto comment, CancellationToken ct = default);
    Task GroupMessageReceivedAsync(IEnumerable<Guid> userIds, GroupMessageDto message, CancellationToken ct = default);
    Task PostStatsChangedAsync(Guid userId, Guid postId, int likesCount, int commentsCount, CancellationToken ct = default);
}
