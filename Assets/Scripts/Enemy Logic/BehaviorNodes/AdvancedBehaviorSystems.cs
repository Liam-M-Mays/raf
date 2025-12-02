using UnityEngine;

/// <summary>
/// Utility system for adding variance to enemy speeds
/// Makes each enemy feel unique even if they're the same type
/// </summary>
public static class EnemySpeedVariance
{
    /// <summary>Apply random speed variance to a single behavior context</summary>
    public static void ApplySpeedVariance(BehaviorContext ctx, float variancePercent = 0.15f)
    {
        if (ctx == null) return;
        
        // Random variance between (1 - percent) and (1 + percent)
        float variance = Random.Range(1f - variancePercent, 1f + variancePercent);
        ctx.speed *= variance;
        ctx.maxSpeed *= variance;
    }

    /// <summary>Apply variance to config values before runtime</summary>
    public static void ApplyVarianceToConfig(ref float speed, ref float maxSpeed, float variancePercent = 0.15f)
    {
        float variance = Random.Range(1f - variancePercent, 1f + variancePercent);
        speed *= variance;
        maxSpeed *= variance;
    }
}

/// <summary>
/// Manages underwater dive/surface mechanics for enemies
/// Enemies can dive to become non-hittable and surface to attack
/// </summary>
public class UnderwaterState
{
    public bool isUnderwater = false;
    public float diveTimer = 0f;
    public float diveDuration = 2.5f;  // Time underwater before surfacing
    public float surfaceInterval = 4f;  // Time above water before diving again
    
    public bool JustSurfaced = false;
    public bool JustDived = false;
}

/// <summary>
/// Manages underwater state transitions for enemies
/// </summary>
public static class UnderwaterManager
{
    /// <summary>Update underwater state and toggle hittable status</summary>
    public static void UpdateUnderwaterState(BehaviorContext ctx, UnderwaterState state)
    {
        if (ctx == null || state == null) return;
        
        state.JustSurfaced = false;
        state.JustDived = false;
        state.diveTimer -= ctx.deltaTime;
        
        if (state.diveTimer <= 0f)
        {
            // Toggle between underwater and surface
            state.isUnderwater = !state.isUnderwater;
            
            if (state.isUnderwater)
            {
                // Going UNDERWATER
                state.diveTimer = state.diveDuration;
                ctx.hittable = false;
                ctx.anim.SetBool("Underwater", true);
                state.JustDived = true;
                Debug.Log($"[Underwater] {ctx.self.name} DIVED underwater");
            }
            else
            {
                // SURFACING
                state.diveTimer = state.surfaceInterval;
                ctx.hittable = true;
                ctx.anim.SetBool("Underwater", false);
                state.JustSurfaced = true;
                Debug.Log($"[Underwater] {ctx.self.name} SURFACED");
            }
        }
    }

    /// <summary>Check if enemy should dive defensively when taking damage</summary>
    public static void TriggerDiveOnDamage(BehaviorContext ctx, UnderwaterState state, float chance = 0.3f)
    {
        if (ctx == null || state == null) return;
        if (state.isUnderwater) return;  // Already underwater
        if (Random.value > chance) return;  // Random chance
        
        // Force dive immediately
        state.isUnderwater = true;
        state.diveTimer = state.diveDuration;
        ctx.hittable = false;
        // Note: Underwater animator parameter doesn't exist, so skip animation update
        // ctx.anim.SetBool("Underwater", true);
    }
}

/// <summary>
/// Tactical behavior system - allows enemies to make strategic decisions
/// Can be toggled per behavior for easy tuning
/// </summary>
public class TacticalDecision
{
    public enum TacticType
    {
        Aggressive,    // Rush player
        Ranged,        // Keep distance, attack from safe range
        Defensive,     // Dodge and avoid
        Coordinated    // Work with allies
    }
    
    public bool enableTactics = true;
    public TacticType currentTactic = TacticType.Aggressive;
    public float tacticChangeTimer = 0f;
    public float tacticChangeDuration = 5f;  // How often to re-evaluate tactics
    
    /// <summary>Re-evaluate tactics based on game state</summary>
    public void UpdateTactic(BehaviorContext ctx, float playerHealth, float playerHealthMax)
    {
        if (!enableTactics) return;
        
        tacticChangeTimer -= Time.deltaTime;
        if (tacticChangeTimer > 0f) return;
        
        tacticChangeTimer = tacticChangeDuration;
        
        // Simple tactical decision system
        float playerHealthPercent = playerHealth / playerHealthMax;
        
        if (playerHealthPercent > 0.7f)
        {
            // Player strong - be aggressive to end quickly
            currentTactic = TacticType.Aggressive;
        }
        else if (playerHealthPercent > 0.3f)
        {
            // Player medium - be smart about it
            currentTactic = Random.value > 0.5f ? TacticType.Aggressive : TacticType.Coordinated;
        }
        else
        {
            // Player weak - be aggressive to finish
            currentTactic = TacticType.Aggressive;
        }
    }
    
    /// <summary>Apply tactic modifiers to movement speed/attack rate</summary>
    public float GetSpeedModifier()
    {
        return currentTactic switch
        {
            TacticType.Aggressive => 1.3f,
            TacticType.Ranged => 0.8f,
            TacticType.Defensive => 0.9f,
            TacticType.Coordinated => 1.0f,
            _ => 1.0f
        };
    }
    
    public float GetAttackRateModifier()
    {
        return currentTactic switch
        {
            TacticType.Aggressive => 1.5f,
            TacticType.Ranged => 0.7f,
            TacticType.Defensive => 0.8f,
            TacticType.Coordinated => 1.0f,
            _ => 1.0f
        };
    }
}
