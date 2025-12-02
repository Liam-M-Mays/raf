using UnityEngine;
using System;

#region Inline Config (shows in Enemy/Archetype SO)

[Serializable]
public class HammerDefaultCfg : BehaviorCfg
{
    [Min(0f)] public float maxSpeed        = 2f;
    [Min(0f)] public float speed           = 1f;

    [Header("Ranges")]
    [Min(0f)] public float attackRange     = 1f;
    [Min(0f)] public float attackRangeMax  = 1f;
    [Min(0f)] public float outOfRange      = 10f;
    [Min(0f)] public float respawnRange    = 8f;

    [Header("Separation")]
    [Min(0f)] public float divergeRange    = 2f;
    public float divergeWeight             = 1f;

    [Header("Charge")]    
    [Min(0f)] public float chargeWindupTime= 1f;  // wait before charging
    [Min(0f)] public float chargeCooldown = 3f;  // time between charges
    [Min(1f)] public float chargeSpeed = 2.5f;

    [Header("Orbiting")]
    public float OrbitDistance = 5f;
    public float OrbitMax = 1f;
    public float OrbitRange = 0.5f;
    public float attackTimer = 3f;
    public bool randDir = false;
    public float direction = 1f; // +1 or -1; you were randomizing this earlier

    public override IBehavior CreateRuntimeBehavior() => new HammerDefault(this);
}

#endregion


public class HammerDefault : IBehavior
{
    public BehaviorContext ctx;
    public BehaviorContext CTX() => ctx;
    private HammerDefaultCfg config;
    
    // State tracking for charge sequence
    private enum ChargeState
    {
        Positioning,
        WindingUp,
        Charging,
        Attacking,
        Cooldown
    }

    private ChargeState currentState = ChargeState.Positioning;
    private float cooldown;
    private float circleDirection;
    private float orbit;
    private float attackTimer;
    private float chargeDistance;
    
    public HammerDefault(HammerDefaultCfg cfg)
    {
        config = cfg;
    }
    
    public void OnEnter(Transform _self, Animator _anim)
    {

        // Initialize context
        Transform raftTarget = GameServices.GetRaft();
        if (raftTarget == null)
        {
            Debug.LogError("HammerDefault behavior: Could not find Raft. Disabling behavior.");
            return;
        }
        
        ctx = new BehaviorContext(_self, raftTarget, _anim);
        ctx.hittable = false;
        // Copy config values to context
        ctx.maxSpeed = config.maxSpeed;
        ctx.speed = config.speed;
        ctx.attackRange = config.attackRange;
        ctx.attackRangeMax = config.attackRangeMax;
        ctx.outOfRange = config.outOfRange;
        ctx.respawnRange = config.respawnRange;
        ctx.divergeRange = config.divergeRange;
        ctx.divergeWeight = config.divergeWeight;

        currentState = ChargeState.Positioning;
        attackTimer = config.attackTimer;
        cooldown = config.chargeCooldown;

        if (config.randDir)
        {
            circleDirection = (UnityEngine.Random.value < 0.5f) ? -1f : 1f;
        }
        else circleDirection = config.direction;
        orbit = config.OrbitDistance + (UnityEngine.Random.Range(0.2f, 0.2f+config.OrbitRange)*circleDirection);
        // Apply per-instance speed variance
        EnemySpeedVariance.ApplySpeedVariance(ctx, 0.15f);
    }
    
    public void OnExit()
    {
        RaftTracker.removeSelf(this);
    }
    
    public void OnUpdate()
    {
        // Update per-frame data
        ctx.UpdateFrame();
        
        // Execute charge sequence based on current state
        switch (currentState)
        {
            case ChargeState.Positioning:
                // Move to charging position
                //RaftTracker.removeSelf(this);
                ctx.hittable = false;
                CircleMovement.Execute(ctx, orbit, config.OrbitMax, circleDirection);
                if(ctx.distanceToTarget < config.OrbitMax && !UtilityNodes.Obstructed(ctx, 2)) attackTimer -= ctx.deltaTime;
                if (attackTimer <= 0f && RaftTracker.addSelf(this))
                {
                    attackTimer = config.attackTimer;
                    AnimatorUtils.SafeSetBool(ctx.anim, "Lurk", true);
                    AnimatorUtils.SafeSetBool(ctx.anim, "Moving", false);
                    currentState = ChargeState.WindingUp;
                }
                break;

            case ChargeState.WindingUp:
                // Wind up for charge
                ChargeMovement.PositionForCharge(ctx, ctx.distanceToTarget + 0.5f);
                ctx.hittable = true;
                if (ChargeMovement.Windup(ctx, config.chargeWindupTime) && !UtilityNodes.Obstructed(ctx))
                {
                    AnimatorUtils.SafeSetBool(ctx.anim, "Lurk", false);
                    AnimatorUtils.SafeSetBool(ctx.anim, "Moving", true);
                    chargeDistance = ctx.distanceToTarget;
                    ctx.hittable = false;
                    currentState = ChargeState.Charging;
                }
                break;
                
            case ChargeState.Charging:
                // Execute the charge
                bool chargeComplete = ChargeMovement.ExecuteCharge(ctx, config.chargeSpeed, chargeDistance * 2f);

                // Check if hit player
                if (UtilityNodes.IsInAttackRange(ctx))
                {
                    ctx.anim.SetBool("Moving", false);
                    ctx.anim.SetBool("Dazed", true);
                    ctx.anim.SetTrigger("Hit");
                    ctx.hittable = true;
                    currentState = ChargeState.Cooldown;
                }
                else if (chargeComplete)
                {
                    // MISS - Charge completed without hitting target
                    ctx.anim.SetBool("Moving", false);
                    ctx.anim.SetBool("Dazed", true);
                    ctx.hittable = true;
                    RaftTracker.removeSelf(this);
                    // Extended cooldown for missed charges (1.5x normal)
                    cooldown = config.chargeCooldown * 1.5f;
                    currentState = ChargeState.Cooldown;
                }
                break;

            case ChargeState.Cooldown:
                cooldown -= ctx.deltaTime;
                ctx.hittable = true;
                if (!UtilityNodes.IsInAttackMax(ctx)) RaftTracker.removeSelf(this);
                if (cooldown <= 0f)
                {
                    cooldown = config.chargeCooldown;
                    ctx.anim.SetBool("Moving", true);
                    ctx.anim.SetBool("Dazed", false);
                    RaftTracker.removeSelf(this);
                    currentState = ChargeState.Positioning;
                    ctx.customData = null;
                }
                break;

        }
    }
    
    public void OnLateUpdate() { }
}
