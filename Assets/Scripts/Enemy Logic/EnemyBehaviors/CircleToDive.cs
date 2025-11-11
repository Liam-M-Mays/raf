// CircleChaseBehavior.cs
using UnityEngine;
using System;

[Serializable]
public class CircleChaseBehaviorCfg : BehaviorCfg
{
    [Header("Circle Chase – Tunables")]
    public float maxSpeed = 2f;
    public float speed = 1f;
    public float attackRange = 1f;
    public float outOfRange = 10f;
    public float respawnRange = 8f;
    public float divergeRange = 2f;
    public float divergeWeight = 1f;

    [Header("Orbiting")]
    public float OrbitDistance = 5f;
    public bool randDir = false;
    public float direction = 1f; // +1 or -1; you were randomizing this earlier

    [Header("Zigzag")]
    [Min(0f)] public float zigzagAmplitude = 5f; // how wide the zigzag
    [Min(0f)] public float zigzagFrequency = 2f; // how fast it zigzags

     [Header("Charge")]
    [Min(0f)] public float chargeDistance  = 15f; // how far back to position before charging
    [Min(0f)] public float chargeWindupTime= 1f;  // wait before charging
    [Min(0f)] public float chargeCooldown  = 3f;  // time between charges

    public override IBehavior CreateRuntimeBehavior() => new CircleChaseBehavior_Modular(this);
}

public class CircleChaseBehavior_Modular : IBehavior
{
    private BehaviorContext ctx;
    private CircleChaseBehaviorCfg config;

    // State tracking
    private bool isAttacking = false;
    private bool charging = false;
    private float circleDirection;
    private float orbit;
    private bool obs = true;
    
    public CircleChaseBehavior_Modular(CircleChaseBehaviorCfg cfg)
    {
        config = cfg;
    }
    
    public void OnEnter(Transform _self, Animator _anim)
    {
        // Initialize context
        ctx = new BehaviorContext(_self, GameObject.FindGameObjectWithTag("Raft").transform, _anim);
        
        // Copy config values to context
        ctx.maxSpeed = config.maxSpeed;
        ctx.speed = config.speed;
        ctx.attackRange = config.attackRange;
        ctx.outOfRange = config.outOfRange;
        ctx.respawnRange = config.respawnRange;
        ctx.divergeRange = config.divergeRange;
        ctx.divergeWeight = config.divergeWeight;

        // Set circle direction
        if (config.randDir)
        {
            circleDirection = (UnityEngine.Random.value < 0.5f) ? -1f : 1f;
            orbit = config.OrbitDistance + (circleDirection/2f);
        }
        else
            circleDirection = config.direction;
    }
    
    public void OnExit()
    {
        RaftTracker.removeSelf(this);
    }
    
    public void OnUpdate()
    {
        // Update per-frame data
        ctx.UpdateFrame();
        
        // Check if out of range first
        if (UtilityNodes.IsOutOfRange(ctx))
        {
            RaftTracker.removeSelf(this);
            ActionNodes.Respawn(ctx, ctx.respawnRange);
            isAttacking = false;
            return;
        }
        // Check if in attack range
        if (UtilityNodes.IsInAttackRange(ctx))
        {
            if (!isAttacking)
            {
                obs = true;
                ActionNodes.Attack(ctx);
                isAttacking = true;
            }
        }
        else
        {
            // Not in attack range
            if (isAttacking)
            {
                RaftTracker.removeSelf(this);
                ActionNodes.StopAttack(ctx);
                obs = true;
                isAttacking = false;
            }
            if (ctx.distanceToTarget > 5) ZigzagMovement.Execute(ctx, config.zigzagAmplitude, config.zigzagFrequency);
            else if (!UtilityNodes.obstructed(ctx) || !obs)
            {
                if (RaftTracker.atRaft(this))
                {
                    obs = false;
                    if (!charging) charging = ChargeMovement.Windup(ctx, config.chargeWindupTime);
                    else
                    {
                        charging = !ChargeMovement.ExecuteCharge(ctx, 2.5f, orbit);
                    }
                }
                else
                {
                    obs = true;
                    RaftTracker.removeSelf(this);
                    CircleMovement.Execute(ctx, orbit, orbit+1f, circleDirection);
                }
            }
            else
            {
                RaftTracker.removeSelf(this);
                CircleMovement.Execute(ctx, orbit, orbit+1f, circleDirection);
            }  
        }
    }
    
    public void OnLateUpdate() { }
}
