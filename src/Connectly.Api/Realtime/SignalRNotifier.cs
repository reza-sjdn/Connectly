using Connectly.Application.Chat.Models;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Groups.Models;
using Connectly.Application.Notifications.Models;
using Connectly.Application.Posts.Models;
using Microsoft.AspNetCore.SignalR;

namespace Connectly.Api.Realtime;

public class SignalRNotifier(IHubContext<AppHub> hub, ILogger<SignalRNotifier> logger) : IRealtimeNotifier
{
    public Task NotificationReceivedAsync(Guid userId, NotificationDto notification, int unreadCount, CancellationToken ct = default)
        => SendAsync([userId], "NotificationReceived", [notification, unreadCount], ct);

    public Task MessageReceivedAsync(IEnumerable<Guid> userIds, MessageDto message, CancellationToken ct = default)
        => SendAsync(userIds, "MessageReceived", [message], ct);

    public Task MessagesReadAsync(Guid userId, Guid readerId, DateTime readAtUtc, CancellationToken ct = default)
        => SendAsync([userId], "MessagesRead", [readerId, readAtUtc], ct);

    public Task CommentAddedAsync(Guid userId, Guid postId, CommentDto comment, CancellationToken ct = default)
        => SendAsync([userId], "CommentAdded", [postId, comment], ct);

    public Task GroupMessageReceivedAsync(IEnumerable<Guid> userIds, GroupMessageDto message, CancellationToken ct = default)
        => SendAsync(userIds, "GroupMessageReceived", [message], ct);

    public Task PostStatsChangedAsync(Guid userId, Guid postId, int likesCount, int commentsCount, CancellationToken ct = default)
        => SendAsync([userId], "PostStatsChanged", [postId, likesCount, commentsCount], ct);

    // Real-time is best effort: a push failure must never fail the request that triggered it.
    private async Task SendAsync(IEnumerable<Guid> userIds, string method, object?[] args, CancellationToken ct)
    {
        try
        {
            var users = userIds.Distinct().Select(id => id.ToString()).ToList();
            await hub.Clients.Users(users).SendCoreAsync(method, args, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "SignalR push '{Method}' failed", method);
        }
    }
}
