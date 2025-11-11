using UnityEngine;
using System;

#region Inline Config (shows in Enemy/Archetype SO)

[Serializable]
public class RamChargeBehaviorCfg : BehaviorCfg
{
    [Header("Ram Charge – Tunables")]
    [Min(0f)] public float maxSpeed        = 2f;
    [Min(0f)] public float speed           = 1f;

    [Header("Ranges")]
    [Min(0f)] public float attackRange     = 1f;
    [Min(0f)] public float outOfRange      = 10f;
    [Min(0f)] public float respawnRange    = 8f;

    [Header("Separation")]
    [Min(0f)] public float divergeRange    = 2f;
    public float divergeWeight             = 1f;

    [Header("Charge")]
    [Min(0f)] public float chargeDistance  = 15f; // how far back to position before charging
    [Min(0f)] public float chargeWindupTime= 1f;  // wait before charging
    [Min(0f)] public float chargeCooldown  = 3f;  // time between charges

    public override IBehavior CreateRuntimeBehavior() => new RamChargeBehavior_Modular(this);
}

#endregion


public class RamChargeBehavior_Modular : IBehavior
{
    private BehaviorContext ctx;
    private RamChargeBehaviorCfg config;
    
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
    
    public RamChargeBehavior_Modular(RamChargeBehaviorCfg cfg)
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
        
        currentState = ChargeState.Positioning;
    }
    
    public void OnExit() { }
    
    public void OnUpdate()
    {
        // Update per-frame data
        ctx.UpdateFrame();
        
        // Execute charge sequence based on current state
        switch (currentState)
        {
            case ChargeState.Positioning:
                // Move to charging position
                ChargeMovement.PositionForCharge(ctx, config.chargeDistance);
                
                // Check if in position
                Vector2 dirToTarget = ((Vector2)ctx.target.position - (Vector2)ctx.self.position).normalized;
                Vector2 idealPos = (Vector2)ctx.target.position - dirToTarget * config.chargeDistance;
                float distToIdeal = Vector2.Distance(ctx.self.position, idealPos);
                
                if (distToIdeal < 2f)
                {
                    currentState = ChargeState.WindingUp;
                    ctx.customData = null; // Reset custom data for windup
                }
                break;
                
            case ChargeState.WindingUp:
                // Wind up for charge
                if (ChargeMovement.Windup(ctx, config.chargeWindupTime))
                {
                    currentState = ChargeState.Charging;
                }
                break;
                
            case ChargeState.Charging:
                // Execute the charge
                bool chargeComplete = ChargeMovement.ExecuteCharge(ctx, 2.5f, config.chargeDistance * 2f);
                
                // Check if hit player
                if (UtilityNodes.IsInAttackRange(ctx))
                {
                    currentState = ChargeState.Attacking;
                }
                else if (chargeComplete)
                {
                    currentState = ChargeState.Cooldown;
                    ctx.customData = null; // Reset for cooldown
                }
                break;
                
            case ChargeState.Attacking:
                // Attack the player
                ActionNodes.Attack(ctx);
                
                // Exit attack if out of range
                if (!UtilityNodes.IsInAttackRange(ctx))
                {
                    ActionNodes.StopAttack(ctx);
                    currentState = ChargeState.Cooldown;
                    ctx.customData = null; // Reset for cooldown
                }
                break;
                
            case ChargeState.Cooldown:
                // Cooldown period with circular movement
                if (ChargeMovement.Cooldown(ctx, config.chargeCooldown))
                {
                    currentState = ChargeState.Positioning;
                    ctx.customData = null; // Reset for next charge
                }
                break;
        }
    }
    
    public void OnLateUpdate() { }
}
