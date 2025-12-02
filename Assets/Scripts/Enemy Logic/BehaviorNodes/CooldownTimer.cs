using UnityEngine;

/// <summary>
/// Reusable cooldown timer for behaviors. Can be used directly or as part of BehaviorContext.customData.
/// </summary>
public class CooldownTimer
{
    private float remainingTime;
    private float totalDuration;
    private bool isActive;

    public CooldownTimer(float duration)
    {
        totalDuration = duration;
        remainingTime = 0f;
        isActive = false;
    }

    /// <summary>Starts the cooldown.</summary>
    public void Start()
    {
        remainingTime = totalDuration;
        isActive = true;
    }

    /// <summary>Starts the cooldown with a custom duration.</summary>
    public void Start(float customDuration)
    {
        totalDuration = customDuration;
        remainingTime = totalDuration;
        isActive = true;
    }

    /// <summary>Updates the cooldown. Returns true if cooldown is complete.</summary>
    public bool Update(float deltaTime)
    {
        if (!isActive)
            return false;

        remainingTime -= deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isActive = false;
            return true;
        }
        return false;
    }

    /// <summary>Returns true if the cooldown is currently active.</summary>
    public bool IsActive() => isActive;

    /// <summary>Returns the remaining time on the cooldown.</summary>
    public float GetRemainingTime() => Mathf.Max(0f, remainingTime);

    /// <summary>Returns the normalized progress (0-1) of the cooldown.</summary>
    public float GetProgress() => Mathf.Clamp01(1f - (remainingTime / totalDuration));

    /// <summary>Resets and deactivates the cooldown.</summary>
    public void Reset()
    {
        remainingTime = 0f;
        isActive = false;
    }
}
