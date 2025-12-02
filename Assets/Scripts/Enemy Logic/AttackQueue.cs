using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the queue of enemies that are currently attacking the raft.
/// Replaces the static RaftTracker with a more robust, event-driven approach.
/// </summary>
public class AttackQueue : MonoBehaviour
{
    public static AttackQueue Instance { get; private set; }

    private Queue<IBehavior> attackingBehaviors = new Queue<IBehavior>();
    private int maxSimultaneousAttackers = GameConstants.Combat.MAX_ATTACKING_ENEMIES;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Ensures AttackQueue singleton exists. Call from GameManager or bootstrap.
    /// </summary>
    public static AttackQueue EnsureExists()
    {
        if (Instance == null)
        {
            var go = new GameObject("AttackQueue");
            var queue = go.AddComponent<AttackQueue>();
            // Manually set Instance if Awake didn't run yet (shouldn't happen, but defensive)
            if (Instance == null)
            {
                Instance = queue;
                DontDestroyOnLoad(go);
            }
        }
        return Instance;
    }

    /// <summary>
    /// Attempts to add a behavior to the attack queue.
    /// Returns true if the behavior was added or was already in the queue.
    /// Returns false if the queue is full.
    /// </summary>
    public bool TryAddAttacker(IBehavior behavior)
    {
        if (behavior == null)
        {
            Debug.LogWarning("AttackQueue: Attempted to add null behavior.");
            return false;
        }

        if (attackingBehaviors.Contains(behavior))
            return true;

        if (attackingBehaviors.Count >= maxSimultaneousAttackers)
            return false;

        attackingBehaviors.Enqueue(behavior);
        return true;
    }

    /// <summary>
    /// Removes a behavior from the attack queue.
    /// </summary>
    public void RemoveAttacker(IBehavior behavior)
    {
        if (behavior == null)
            return;

        // Create a new queue without this behavior
        var newQueue = new Queue<IBehavior>();
        while (attackingBehaviors.Count > 0)
        {
            var current = attackingBehaviors.Dequeue();
            if (current != behavior)
                newQueue.Enqueue(current);
        }
        attackingBehaviors = newQueue;
    }

    /// <summary>
    /// Gets the number of enemies currently attacking.
    /// </summary>
    public int GetAttackerCount()
    {
        return attackingBehaviors.Count;
    }

    /// <summary>
    /// Checks if the queue is at capacity.
    /// </summary>
    public bool IsAtCapacity()
    {
        return attackingBehaviors.Count >= maxSimultaneousAttackers;
    }

    /// <summary>
    /// Sets the maximum number of simultaneous attackers.
    /// </summary>
    public void SetMaxAttackers(int max)
    {
        maxSimultaneousAttackers = Mathf.Max(1, max);
    }

    /// <summary>
    /// Clears all attackers from the queue.
    /// </summary>
    public void Clear()
    {
        attackingBehaviors.Clear();
    }
}
