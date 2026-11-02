using Connectly.Application.Auth.Models;
using Connectly.Application.Chat.Models;
using Connectly.Application.Groups.Models;
using Connectly.Application.Notifications.Models;
using Connectly.Application.Posts.Models;
using Connectly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Follow> Follows { get; }
    DbSet<Post> Posts { get; }
    DbSet<Comment> Comments { get; }
    DbSet<PostLike> PostLikes { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }
    DbSet<Group> Groups { get; }
    DbSet<GroupMember> GroupMembers { get; }
    DbSet<GroupMessage> GroupMessages { get; }
    DbSet<Story> Stories { get; }
    DbSet<StoryView> StoryViews { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICurrentUser
{
    Guid? UserId { get; }
}

public interface IIdentityService
{
    /// <summary>Creates the Identity account. Throws ConflictException / ValidationException on failure.</summary>
    Task<Guid> CreateUserAsync(string email, string password, CancellationToken ct = default);

    /// <summary>Returns the user id when credentials are valid, otherwise null (handles lockout).</summary>
    Task<Guid?> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);
}

public interface ITokenService
{
    Task<AuthResponse> IssueAsync(Guid userId, CancellationToken ct = default);
    Task<AuthResponse> RotateAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeAsync(string refreshToken, CancellationToken ct = default);
}

public interface IFileStorage
{
    /// <returns>Public relative URL of the stored file, e.g. /media/posts/abc.jpg</returns>
    Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken ct = default);
    Task DeleteAsync(string relativeUrl, CancellationToken ct = default);
}

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

public interface IPresenceTracker
{
    IReadOnlyCollection<Guid> FilterOnline(IEnumerable<Guid> userIds);
}

/// <summary>Stores a notification and pushes it live. Failures are logged, never thrown, so they cannot break the main action.</summary>
public interface IUserNotificationPublisher
{
    Task PublishAsync(Guid recipientId, Guid actorId, NotificationType type, Guid? postId, CancellationToken ct = default);
    Task PublishGroupAsync(Guid recipientId, Guid actorId, NotificationType type, Guid groupId, CancellationToken ct = default);
    Task PushPostStatsAsync(Guid postId, Guid authorId, Guid actorId, CancellationToken ct = default);
}
