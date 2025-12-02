// DirectChaseBehavior.cs
using UnityEngine;
using System;

[Serializable]
public class RangeOrbitCfg : BehaviorCfg
{
    public float maxSpeed = 2f;
    public float speed = 1f;
    public float attackRange = 1f;
    public float attackRangeMax = 1f;
    public float outOfRange = 10f;
    public float respawnRange = 8f;
    public float divergeRange = 2f;
    public float divergeWeight = 1f;

    public float OrbitDistance = 5f;
    public float OrbitMax = 1f;
    public float OrbitRange = 0.5f;
    public bool randDir = false;
    public float direction = 1f; // +1 or -1; you were randomizing this earlier

    public override IBehavior CreateRuntimeBehavior() => new RangeOrbit(this);
}

public class RangeOrbit : IBehavior
{
    public BehaviorContext ctx;
    public BehaviorContext CTX() => ctx;
    private RangeOrbitCfg config;
    private float circleDirection;
    private float orbit;
    
    
    public RangeOrbit(RangeOrbitCfg cfg)
    {
        config = cfg;
    }
    
    public void OnEnter(Transform _self, Animator _anim)
    {
        // Initialize context with all the shared data
        Transform raftTarget = GameServices.GetRaft();
        if (raftTarget == null)
        {
            Debug.LogError("RangeOrbit behavior: Could not find Raft. Disabling behavior.");
            return;
        }
        
        ctx = new BehaviorContext(_self, raftTarget, _anim);
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

        if (config.randDir)
        {
            circleDirection = (UnityEngine.Random.value < 0.5f) ? -1f : 1f;
        }
        else circleDirection = config.direction;
        orbit = config.OrbitDistance + (UnityEngine.Random.Range(0.2f, 0.2f+config.OrbitRange)*circleDirection);
        // Speed variance per instance
        EnemySpeedVariance.ApplySpeedVariance(ctx, 0.12f);
        ActionNodes.Attack(ctx);
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
        
        CircleMovement.Execute(ctx, orbit, orbit + config.OrbitMax, circleDirection);
    }
    
    public void OnLateUpdate() 
    {
        // Any late update logic if needed
    }
}
