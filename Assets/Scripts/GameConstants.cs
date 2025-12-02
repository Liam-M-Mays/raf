using UnityEngine;

/// <summary>
/// Centralized constants for the game to avoid magic numbers scattered throughout code
/// </summary>
public static class GameConstants
{
    [System.Serializable]
    public static class Tags
    {
        public const string RAFT = "Raft";
        public const string PLAYER = "Player";
        public const string ENEMY = "Enemy";
    }

    [System.Serializable]
    public static class Combat
    {
        /// <summary>Maximum number of enemies that can attack the raft simultaneously</summary>
        public const int MAX_ATTACKING_ENEMIES = 3;
    }

    [System.Serializable]
    public static class UI
    {
        public const float SORTING_ORDER_OFFSET = 1f;
    }
}
