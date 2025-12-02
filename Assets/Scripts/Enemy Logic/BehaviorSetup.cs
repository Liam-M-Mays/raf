using UnityEngine;

/// <summary>
/// Base helper class for common behavior setup patterns.
/// Reduces boilerplate in individual behavior implementations.
/// </summary>
public static class BehaviorSetup
{
    /// <summary>
    /// Initialize a BehaviorContext with standard setup from a config.
    /// Handles null checks and error reporting.
    /// </summary>
    public static BehaviorContext InitializeContext(Transform self, BehaviorCfg config, Animator anim)
    {
        Transform raftTarget = GameServices.GetRaft();
        
        if (raftTarget == null)
        {
            Debug.LogError($"BehaviorSetup: Could not find Raft. Behavior may not function correctly.");
            return null;
        }

        BehaviorContext ctx = new BehaviorContext(self, raftTarget, anim);
        
        // Copy config values to context - safe for common properties
        if (config is HammerDefaultCfg hammerCfg)
        {
            ctx.maxSpeed = hammerCfg.maxSpeed;
            ctx.speed = hammerCfg.speed;
            ctx.attackRange = hammerCfg.attackRange;
            ctx.attackRangeMax = hammerCfg.attackRangeMax;
            ctx.outOfRange = hammerCfg.outOfRange;
            ctx.respawnRange = hammerCfg.respawnRange;
            ctx.divergeRange = hammerCfg.divergeRange;
            ctx.divergeWeight = hammerCfg.divergeWeight;
        }
        else if (config is DirectCfg directCfg)
        {
            ctx.maxSpeed = directCfg.maxSpeed;
            ctx.speed = directCfg.speed;
            ctx.attackRange = directCfg.attackRange;
            ctx.attackRangeMax = directCfg.attackRangeMax;
            ctx.outOfRange = directCfg.outOfRange;
            ctx.respawnRange = directCfg.respawnRange;
            ctx.divergeRange = directCfg.divergeRange;
            ctx.divergeWeight = directCfg.divergeWeight;
        }
        else if (config is SharkDefaultCfg sharkCfg)
        {
            ctx.maxSpeed = sharkCfg.maxSpeed;
            ctx.speed = sharkCfg.speed;
            ctx.attackRange = sharkCfg.attackRange;
            ctx.attackRangeMax = sharkCfg.attackRangeMax;
            ctx.outOfRange = sharkCfg.outOfRange;
            ctx.respawnRange = sharkCfg.respawnRange;
            ctx.divergeRange = sharkCfg.divergeRange;
            ctx.divergeWeight = sharkCfg.divergeWeight;
        }
        else if (config is RangeOrbitCfg orbitCfg)
        {
            ctx.maxSpeed = orbitCfg.maxSpeed;
            ctx.speed = orbitCfg.speed;
            ctx.attackRange = orbitCfg.attackRange;
            ctx.attackRangeMax = orbitCfg.attackRangeMax;
            ctx.outOfRange = orbitCfg.outOfRange;
            ctx.respawnRange = orbitCfg.respawnRange;
            ctx.divergeRange = orbitCfg.divergeRange;
            ctx.divergeWeight = orbitCfg.divergeWeight;
        }

        return ctx;
    }
}
