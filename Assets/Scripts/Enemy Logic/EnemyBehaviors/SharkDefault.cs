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
    public float direction = 1f;

    [Header("Zigzag")]
    [Min(0f)] public float zigzagAmplitude = 5f;
    [Min(0f)] public float zigzagFrequency = 2f;

    public override IBehavior CreateRuntimeBehavior() => new SharkDefault(this);
}

public class SharkDefault : IBehavior
{
    public BehaviorContext ctx;
    public BehaviorContext CTX() => ctx;
    private SharkDefaultCfg config;

    // Simplified state machine
    private enum State { Approach, Orbiting, Attacking }
    private State state = State.Approach;

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
        Transform raftTarget = GameServices.GetRaft();
        if (raftTarget == null)
        {
            Debug.LogError("SharkDefault behavior: Could not find Raft. Disabling behavior.");
            return;
        }
        
        ctx = new BehaviorContext(_self, raftTarget, _anim);
        ctx.hittable = true;
        ctx.maxSpeed = config.maxSpeed;
        ctx.speed = config.speed;
        ctx.attackRange = config.attackRange;
        ctx.attackRangeMax = config.attackRangeMax;
        ctx.outOfRange = config.outOfRange;
        ctx.respawnRange = config.respawnRange;
        ctx.divergeRange = config.divergeRange;
        ctx.divergeWeight = config.divergeWeight;
        
        circleDirection = config.randDir ? ((UnityEngine.Random.value < 0.5f) ? -1f : 1f) : config.direction;
        orbit = config.OrbitDistance + (UnityEngine.Random.Range(0.2f, 0.2f + config.OrbitRange) * circleDirection);
        attackTimer = config.attackTimer;
        
        EnemySpeedVariance.ApplySpeedVariance(ctx, 0.15f);
        state = State.Approach;
    }
    
    public void OnExit()
    {
        RaftTracker.removeSelf(this);
    }
    
    public void OnUpdate()
    {
        ctx.UpdateFrame();
        UnderwaterManager.UpdateUnderwaterState(ctx, ctx.underwaterState);
        var pm = GameServices.GetPlayerMovement();
        if (pm != null && pm.matressHealth != null)
        {
            ctx.tacticalDecision.UpdateTactic(ctx, pm.matressHealth.GetCurrentHealth(), pm.matressHealth.GetMaxHealth());
        }

        // Respawn if too far
        if (UtilityNodes.IsOutOfRange(ctx))
        {
            RaftTracker.removeSelf(this);
            ActionNodes.Respawn(ctx, ctx.respawnRange);
            state = State.Approach;
            attackTimer = config.attackTimer;
            return;
        }

        // State machine
        switch (state)
        {
            case State.Approach:
                // If far from orbit distance, zigzag closer
                if (ctx.distanceToTarget > orbit + config.OrbitMax)
                {
                    ZigzagMovement.Execute(ctx, config.zigzagAmplitude, config.zigzagFrequency);
                }
                else
                {
                    // Reached orbit distance - move to orbiting state
                    state = State.Orbiting;
                    attackTimer = config.attackTimer;
                    ActionNodes.StopAttack(ctx);
                }
                break;

            case State.Orbiting:
                // Circle at safe distance
                CircleMovement.Execute(ctx, orbit, orbit + config.OrbitMax, circleDirection);
                attackTimer -= ctx.deltaTime;

                // Ready to attack?
                if (attackTimer <= 0f && RaftTracker.addSelf(this) && !UtilityNodes.Obstructed(ctx))
                {
                    state = State.Attacking;
                    ActionNodes.Attack(ctx);
                    attackStart = (Vector2)ctx.target.position;
                    ctx.hittable = true;
                }
                break;

            case State.Attacking:
                // Chase and bite
                float distFromAttackStart = Vector2.Distance(attackStart, (Vector2)ctx.target.position);

                if (distFromAttackStart > ctx.attackRangeMax)
                {
                    // Player moved too far - give up and return to orbit
                    state = State.Orbiting;
                    ActionNodes.StopAttack(ctx);
                    RaftTracker.removeSelf(this);
                    attackTimer = config.attackTimer;
                    ctx.hittable = false;
                }
                else if (ctx.distanceToTarget <= ctx.attackRange)
                {
                    // Close enough - bite! Keep hittable for retaliation
                    ctx.hittable = true;
                }
                else
                {
                    // Still chasing - move toward target
                    DirectChaseMovement.SlowExecute(ctx);
                    ctx.hittable = true;
                }
                break;
        }
    }
    
    public void OnLateUpdate() { }
}
