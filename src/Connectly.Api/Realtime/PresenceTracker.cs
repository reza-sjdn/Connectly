using System.Collections.Concurrent;
using Connectly.Application.Common.Interfaces;

namespace Connectly.Api.Realtime;

/// <summary>
/// In-memory presence: a user is online while at least one SignalR connection (tab/device) is open.
/// Single-server only; running several instances needs a shared store (e.g. Redis) plus a SignalR backplane.
/// </summary>
public class PresenceTracker : IPresenceTracker
{
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _connections = new();

    /// <returns>true when this is the user's first connection (they just came online).</returns>
    public bool Connected(Guid userId, string connectionId)
    {
        var set = _connections.GetOrAdd(userId, _ => []);
        lock (set)
        {
            set.Add(connectionId);
            return set.Count == 1;
        }
    }

    /// <returns>true when that was the user's last connection (they just went offline).</returns>
    public bool Disconnected(Guid userId, string connectionId)
    {
        if (!_connections.TryGetValue(userId, out var set)) return false;
        lock (set)
        {
            set.Remove(connectionId);
            if (set.Count > 0) return false;
            _connections.TryRemove(userId, out _);
            return true;
        }
    }

    public IReadOnlyCollection<Guid> FilterOnline(IEnumerable<Guid> userIds)
        => userIds.Distinct().Where(_connections.ContainsKey).ToList();
}
