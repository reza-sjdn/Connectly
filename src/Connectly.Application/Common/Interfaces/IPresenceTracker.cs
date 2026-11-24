namespace Connectly.Application.Common.Interfaces;

public interface IPresenceTracker
{
    IReadOnlyCollection<Guid> FilterOnline(IEnumerable<Guid> userIds);
}
