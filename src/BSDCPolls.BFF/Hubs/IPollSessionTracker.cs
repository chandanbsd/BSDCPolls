namespace BSDCPolls.BFF.Hubs;

/// <summary>
/// Tracks which SignalR connection belongs to the creator of each active poll session.
/// Used to deliver vote-count updates exclusively to the poll creator.
/// </summary>
public interface IPollSessionTracker
{
    /// <summary>Registers <paramref name="connectionId"/> as the creator connection for <paramref name="pollUid"/>.</summary>
    void RegisterCreator(Guid pollUid, string connectionId);

    /// <summary>Removes the creator registration for <paramref name="pollUid"/> if the stored connection matches <paramref name="connectionId"/>.</summary>
    void UnregisterCreator(Guid pollUid, string connectionId);

    /// <summary>Returns the creator's connection ID for <paramref name="pollUid"/>, or <c>null</c> if not found.</summary>
    string? GetCreatorConnectionId(Guid pollUid);
}
