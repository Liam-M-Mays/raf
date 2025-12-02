using UnityEngine;

/// <summary>
/// Service locator for global game objects and references.
/// Caches the raft and player to avoid repeated FindGameObjectWithTag calls.
/// </summary>
public static class GameServices
{
    private static Transform raftTransform;
    private static Transform playerTransform;
    private static Health raftHealth;
    private static PlayerMovement playerMovement;

    /// <summary>Gets the raft transform, caching the result for performance</summary>
    public static Transform GetRaft()
    {
        if (raftTransform == null)
        {
            GameObject raft = GameObject.FindGameObjectWithTag(GameConstants.Tags.RAFT);
            if (raft == null)
            {
                Debug.LogError("GameServices: Could not find GameObject with tag 'Raft'. Ensure the raft has the correct tag.");
                return null;
            }
            raftTransform = raft.transform;
        }
        return raftTransform;
    }

    /// <summary>Gets the player transform, caching the result for performance</summary>
    public static Transform GetPlayer()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(GameConstants.Tags.PLAYER);
            if (player == null)
            {
                Debug.LogError("GameServices: Could not find GameObject with tag 'Player'. Ensure the player has the correct tag.");
                return null;
            }
            playerTransform = player.transform;
        }
        return playerTransform;
    }

    /// <summary>Gets the raft's Health component</summary>
    public static Health GetRaftHealth()
    {
        if (raftHealth == null)
        {
            Transform raft = GetRaft();
            if (raft != null)
            {
                raftHealth = raft.GetComponent<Health>();
                if (raftHealth == null)
                {
                    Debug.LogError("GameServices: Raft does not have a Health component.");
                }
            }
        }
        return raftHealth;
    }

    /// <summary>Gets the PlayerMovement component</summary>
    public static PlayerMovement GetPlayerMovement()
    {
        if (playerMovement == null)
        {
            Transform player = GetPlayer();
            if (player != null)
            {
                playerMovement = player.GetComponent<PlayerMovement>();
                if (playerMovement == null)
                {
                    Debug.LogError("GameServices: Player does not have a PlayerMovement component.");
                }
            }
        }
        return playerMovement;
    }

    /// <summary>Clears all cached references. Call this when transitioning scenes or for testing.</summary>
    public static void ClearCache()
    {
        raftTransform = null;
        playerTransform = null;
        raftHealth = null;
        playerMovement = null;
    }
}
