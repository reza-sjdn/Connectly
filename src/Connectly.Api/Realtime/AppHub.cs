using Connectly.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Api.Realtime;

/// <summary>Maps the JWT "sub" claim to the SignalR user id so Clients.User(id) reaches all of a user's tabs.</summary>
public class SubUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) => connection.User?.FindFirst("sub")?.Value;
}

/// <summary>
/// One hub for everything. Server -> client events: NotificationReceived, MessageReceived, MessagesRead,
/// CommentAdded, PostStatsChanged, PresenceChanged, Typing. Client -> server: Typing.
/// </summary>
[Authorize]
public class AppHub(PresenceTracker presence, IApplicationDbContext db) : Hub
{
    private Guid? Me => Guid.TryParse(Context.User?.FindFirst("sub")?.Value, out var id) ? id : null;

    public override async Task OnConnectedAsync()
    {
        if (Me is { } me && presence.Connected(me, Context.ConnectionId))
            await BroadcastPresenceAsync(me, true);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Me is { } me && presence.Disconnected(me, Context.ConnectionId))
            await BroadcastPresenceAsync(me, false);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>Typing indicator, relayed only between users who are related (chat partners or follow edge).</summary>
    public async Task Typing(Guid otherUserId, bool isTyping)
    {
        if (Me is not { } me || me == otherUserId) return;

        var (a, b) = me.CompareTo(otherUserId) < 0 ? (me, otherUserId) : (otherUserId, me);
        var related = await db.Conversations.AnyAsync(c => c.UserAId == a && c.UserBId == b)
                      || await db.Follows.AnyAsync(f =>
                          (f.FollowerId == me && f.FolloweeId == otherUserId) ||
                          (f.FollowerId == otherUserId && f.FolloweeId == me));
        if (!related) return;

        await Clients.User(otherUserId.ToString()).SendAsync("Typing", me, isTyping);
    }

    private async Task BroadcastPresenceAsync(Guid me, bool isOnline)
    {
        var audience = await PresenceAudience.RelatedTo(db, me).ToListAsync();
        if (audience.Count == 0) return;
        await Clients.Users(audience.Select(id => id.ToString()).ToList()).SendAsync("PresenceChanged", me, isOnline);
    }
}
