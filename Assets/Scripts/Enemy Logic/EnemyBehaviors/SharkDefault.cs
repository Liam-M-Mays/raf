// CircleChaseBehavior.cs
using UnityEngine;
using System;

[Serializable]
public class SharkDefaultCfg : BehaviorCfg
{
    public float maxSpeed = 2f;
    public float speed = 1f;
    public float attackRange = 1f;
    public float attackRangeMax = 1f;
    public float outOfRange = 10f;
    public float respawnRange = 8f;
    public float divergeRange = 2f;
    public float divergeWeight = 1f;

    [Header("Orbiting")]
    public float OrbitDistance = 5f;
    public float OrbitMax = 1f;
    public float OrbitRange = 0.5f;
    public float attackTimer = 3f;
    public bool randDir = false;
    public float direction = 1f; // +1 or -1; you were randomizing this earlier

    [Header("Zigzag")]
    [Min(0f)] public float zigzagAmplitude = 5f; // how wide the zigzag
    [Min(0f)] public float zigzagFrequency = 2f; // how fast it zigzags

    public override IBehavior CreateRuntimeBehavior() => new SharkDefault(this);
}

public class SharkDefault : IBehavior
{
    private BehaviorContext ctx;
    private SharkDefaultCfg config;

    // State tracking
    private bool isAttacking = false;
    private float circleDirection;
    private float orbit;
    private float attackTimer;
    private Vector2 attackStart;
    
    public SharkDefault(SharkDefaultCfg cfg)
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
        ctx.attackRangeMax = config.attackRangeMax;
        ctx.outOfRange = config.outOfRange;
        ctx.respawnRange = config.respawnRange;
        ctx.divergeRange = config.divergeRange;
        ctx.divergeWeight = config.divergeWeight;
        attackTimer = config.attackTimer;
        // Set orbit direction and distance
        if (config.randDir)
        {
            circleDirection = (UnityEngine.Random.value < 0.5f) ? -1f : 1f;
        }
        else circleDirection = config.direction;
        orbit = config.OrbitDistance + (UnityEngine.Random.Range(0.2f, 0.2f+config.OrbitRange)*circleDirection);
    }
    
    public void OnExit()
    {
        RaftTracker.removeSelf(this);
    }
    
    public void OnUpdate()
    {
        // Update per-frame data
        ctx.UpdateFrame();

        // Respawn Check
        if (UtilityNodes.IsOutOfRange(ctx))
        {
            RaftTracker.removeSelf(this);
            ActionNodes.Respawn(ctx, ctx.respawnRange);
            isAttacking = false;
            attackTimer = config.attackTimer;
            return;
        }
        
        // Attack Check
        if (UtilityNodes.IsInAttackRange(ctx) || (isAttacking && Vector2.Distance(attackStart, (Vector2)ctx.lastPosition) <= ctx.attackRangeMax))
        {
            if (!UtilityNodes.IsInAttackRange(ctx)) DirectChaseMovement.Execute(ctx);
            if (!isAttacking)
            {
                attackTimer = config.attackTimer;
                attackStart = (Vector2)ctx.lastPosition;
                ActionNodes.Attack(ctx);
                isAttacking = true;
                attackTimer = config.attackTimer;
            }
        }
        else
        {
            // Not in attack range // -----> maybe move this function into attack node itself. 
            if (isAttacking)
            {
                if (ctx.distanceToTarget < orbit)
                {
                    Vector2 orbitTarget = UtilityNodes.TargetOnOrbit(ctx, orbit);
                    ZigzagMovement.ExecuteTarget(ctx, orbitTarget, config.zigzagAmplitude, config.zigzagFrequency);
                    attackTimer = config.attackTimer;
                }
                ActionNodes.StopAttack(ctx);
                RaftTracker.removeSelf(this);
                
                isAttacking = false;
            }
            if (ctx.distanceToTarget > orbit)
            {
                ZigzagMovement.Execute(ctx, config.zigzagAmplitude, config.zigzagFrequency);
                attackTimer = config.attackTimer;
            }
            else if (attackTimer > 0f)
            {
                CircleMovement.Execute(ctx, orbit, orbit + config.OrbitMax, circleDirection);
                attackTimer -= ctx.deltaTime;
            }
            else
            {
                if (RaftTracker.addSelf(this) && !UtilityNodes.Obstructed(ctx))
                {
                    DirectChaseMovement.Execute(ctx);
                    ActionNodes.Attack(ctx);
                }
                else
                {
                    RaftTracker.removeSelf(this);
                    attackTimer = config.attackTimer;
                    ActionNodes.StopAttack(ctx);
                }
            }
        }
    }
    
    public void OnLateUpdate() { }
}
