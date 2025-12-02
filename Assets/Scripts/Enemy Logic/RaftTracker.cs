using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Wrapper around AttackQueue for backward compatibility.
/// New code should use AttackQueue directly.
/// </summary>
public static class RaftTracker
{
    /// <summary>Adds a behavior to the attack queue. Returns true if successful.</summary>
    public static bool addSelf(IBehavior B)
    {
        var queue = AttackQueue.Instance ?? AttackQueue.EnsureExists();
        if (queue == null)
        {
            Debug.LogError("RaftTracker: AttackQueue could not be initialized.");
            return false;
        }
        return queue.TryAddAttacker(B);
    }

    /// <summary>Removes a behavior from the attack queue.</summary>
    public static void removeSelf(IBehavior B)
    {
        var queue = AttackQueue.Instance;
        if (queue == null)
        {
            Debug.LogWarning("RaftTracker: AttackQueue Instance was null, attempting to create...");
            queue = AttackQueue.EnsureExists();
        }
        
        if (queue == null)
        {
            Debug.LogError("RaftTracker: Failed to initialize AttackQueue even after EnsureExists()");
            return;
        }
        queue.RemoveAttacker(B);
    }

    /// <summary>Gets the legacy Braft list for backward compatibility (discouraged).</summary>
    [System.Obsolete("Use AttackQueue.Instance directly instead.")]
    public static List<IBehavior> Braft
    {
        get
        {
            if (AttackQueue.Instance == null)
            {
                Debug.LogError("RaftTracker: AttackQueue singleton not initialized.");
                return new List<IBehavior>();
            }
            return new List<IBehavior>();
        }
    }
}