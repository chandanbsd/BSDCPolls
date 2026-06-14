using System.Collections.Concurrent;

namespace BSDCPolls.BFF.Hubs;

/// <summary>Thread-safe in-memory implementation of <see cref="IPollSessionTracker"/>.</summary>
public sealed class PollSessionTracker : IPollSessionTracker
{
    private readonly ConcurrentDictionary<Guid, string> _creatorConnections = new();

    /// <inheritdoc />
    public void RegisterCreator(Guid pollUid, string connectionId) =>
        _creatorConnections[pollUid] = connectionId;

    /// <inheritdoc />
    public void UnregisterCreator(Guid pollUid, string connectionId) =>
        _creatorConnections.TryRemove(new KeyValuePair<Guid, string>(pollUid, connectionId));

    /// <inheritdoc />
    public string? GetCreatorConnectionId(Guid pollUid) =>
        _creatorConnections.TryGetValue(pollUid, out var connectionId) ? connectionId : null;
}
