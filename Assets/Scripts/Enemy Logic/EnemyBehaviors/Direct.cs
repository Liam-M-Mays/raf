// DirectChaseBehavior.cs
using UnityEngine;
using System;

[Serializable]
public class DirectCfg : BehaviorCfg
{
    public float maxSpeed = 2f;
    public float speed = 1f;
    public float attackRange = 1f;
    public float attackRangeMax = 1f;
    public float outOfRange = 10f;
    public float respawnRange = 8f;
    public float divergeRange = 2f;
    public float divergeWeight = 1f;
    public bool AlwaysHittable = true;

    public override IBehavior CreateRuntimeBehavior() => new Direct(this);
}

public class Direct : IBehavior
{
    public BehaviorContext ctx;
    private DirectCfg config;
    
    
    public Direct(DirectCfg cfg)
    {
        config = cfg;
    }

    public BehaviorContext CTX() => ctx;
    
    public void OnEnter(Transform _self, Animator _anim)
    {
        // Initialize context with all the shared data
        ctx = new BehaviorContext(_self, GameObject.FindGameObjectWithTag("Raft").transform, _anim);
        ctx.hittable = true;
        // Copy config values to context
        ctx.maxSpeed = config.maxSpeed;
        ctx.speed = config.speed;
        ctx.attackRange = config.attackRange;
        ctx.attackRangeMax = config.attackRangeMax;
        ctx.outOfRange = config.outOfRange;
        ctx.respawnRange = config.respawnRange;
        ctx.divergeRange = config.divergeRange;
        ctx.divergeWeight = config.divergeWeight;
    }
    
    public void OnExit() 
    {
        // Cleanup if needed
    }
    
    public void OnUpdate()
    {
        // Update per-frame data
        ctx.UpdateFrame();

        // Check if out of range first
        if (UtilityNodes.IsOutOfRange(ctx))
        {
            ActionNodes.Respawn(ctx, ctx.respawnRange);
            return;
        }
        
        // Check if in attack range
        if (UtilityNodes.IsInAttackMax(ctx))
        {
            ActionNodes.Attack(ctx);
            ctx.hittable = true;
        }
        else
        {
            // Not in attack range
            ActionNodes.StopAttack(ctx);
            if (!config.AlwaysHittable) ctx.hittable = false;
        }
        // Execute chase movement
        if (!UtilityNodes.IsInAttackRange(ctx)) DirectChaseMovement.Execute(ctx);
        
    }
    
    public void OnLateUpdate() 
    {
        // Any late update logic if needed
    }
}
