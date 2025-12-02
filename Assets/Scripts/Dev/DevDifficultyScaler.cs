using UnityEngine;

/// <summary>
/// Scale enemy difficulty on-the-fly for playtesting.
/// Press + to increase, - to decrease difficulty multiplier (0.5x to 3.0x).
/// Affects health, damage, and speed of newly spawned enemies.
/// </summary>
public class DevDifficultyScaler : MonoBehaviour
{
    public KeyCode increaseKey = KeyCode.Equals;
    public KeyCode decreaseKey = KeyCode.Minus;

    private float difficultyMultiplier = 1f;
    private float minDifficulty = 0.5f;
    private float maxDifficulty = 3f;
    private float difficultyStep = 0.25f;

    public static float CurrentDifficulty { get; private set; } = 1f;

    void Update()
    {
        if (Input.GetKeyDown(increaseKey))
        {
            difficultyMultiplier = Mathf.Clamp(difficultyMultiplier + difficultyStep, minDifficulty, maxDifficulty);
            CurrentDifficulty = difficultyMultiplier;
            Debug.Log($"Difficulty: {difficultyMultiplier:F2}x");
        }

        if (Input.GetKeyDown(decreaseKey))
        {
            difficultyMultiplier = Mathf.Clamp(difficultyMultiplier - difficultyStep, minDifficulty, maxDifficulty);
            CurrentDifficulty = difficultyMultiplier;
            Debug.Log($"Difficulty: {difficultyMultiplier:F2}x");
        }
    }
}
