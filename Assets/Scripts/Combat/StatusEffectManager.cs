using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Status effect manager for tracking applied effects (slow, stun, etc.)
/// Lightweight system that doesn't require modifying existing code.
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    [System.Serializable]
    public class StatusEffect
    {
        public string name;
        public float duration;
        public float factor; // 0-1, multiplier or reduction
    }

    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    private PlayerMovement player;

    void Start()
    {
        player = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Decay active effects
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].duration -= Time.deltaTime;
            if (activeEffects[i].duration <= 0)
            {
                activeEffects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Apply a slow effect to the player
    /// </summary>
    public void ApplySlowEffect(float slowFactor, float duration)
    {
        activeEffects.Add(new StatusEffect
        {
            name = "Slow",
            duration = duration,
            factor = slowFactor
        });
    }

    /// <summary>
    /// Get the cumulative slow multiplier
    /// </summary>
    public float GetSpeedMultiplier()
    {
        float multiplier = 1f;
        foreach (var effect in activeEffects)
        {
            if (effect.name == "Slow")
                multiplier *= effect.factor;
        }
        return Mathf.Clamp01(multiplier);
    }

    /// <summary>
    /// Check if any effect is active
    /// </summary>
    public bool HasEffect(string effectName)
    {
        return activeEffects.Exists(e => e.name == effectName);
    }
}
