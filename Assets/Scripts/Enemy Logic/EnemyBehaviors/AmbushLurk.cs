using UnityEngine;
using System;

#region Inline Config (shows in Enemy/Archetype SO)

[Serializable]
public class AmbushLurkBehaviorCfg : BehaviorCfg
{
    [Header("Speeds")]
    [Min(0f)] public float maxSpeed = 2f;
    [Min(0f)] public float speed    = 1f;

    [Header("Ranges")]
    [Min(0f)] public float attackRange   = 1f;
    [Min(0f)] public float outOfRange    = 10f;
    [Min(0f)] public float respawnRange  = 8f;

    [Header("Separation")]
    [Min(0f)] public float divergeRange  = 2f;
    public float divergeWeight           = 1f;

    [Header("Ambush/Lurk")]
    [Min(0f)] public float lurkDistance  = 15f; // distance to maintain while lurking
    [Min(0f)] public float lurkTime      = 3f;  // time to lurk before attacking
    [Min(0f)] public float retreatDistance = 20f; // retreat distance after attack

    public override IBehavior CreateRuntimeBehavior() => new AmbushLurkBehavior_Modular(this);
}

#endregion


public class AmbushLurkBehavior_Modular : IBehavior
{
    private BehaviorContext ctx;
    private AmbushLurkBehaviorCfg config;
    
    // State tracking for ambush sequence
    private enum AmbushState
    {
        Lurking,
        Charging,
        Attacking,
        Retreating
    }
    
    private AmbushState currentState = AmbushState.Lurking;
    private float lurkTimer;
    
    public AmbushLurkBehavior_Modular(AmbushLurkBehaviorCfg cfg)
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
        
        currentState = AmbushState.Lurking;
        lurkTimer = config.lurkTime;
    }
    
    public void OnExit() { }
    
    public void OnUpdate()
    {
        // Update per-frame data
        ctx.UpdateFrame();
        
        switch (currentState)
        {
            case AmbushState.Lurking:
                // Lurk at distance, waiting for the right moment
                LurkMovement.Execute(ctx, config.lurkDistance);
                
                lurkTimer -= ctx.deltaTime;
                
                // Transition to charge when timer expires
                if (lurkTimer <= 0f)
                {
                    currentState = AmbushState.Charging;
                    // Clear custom data for the charge
                    ctx.customData = null;
                }
                break;
                
            case AmbushState.Charging:
                // Fast charge toward player (similar to direct chase but faster)
                ctx.maxSpeed = config.maxSpeed * 1.5f; // Temporary speed boost
                DirectChaseMovement.Execute(ctx);
                ctx.maxSpeed = config.maxSpeed; // Reset speed
                
                // Check if reached attack range
                if (UtilityNodes.IsInAttackRange(ctx))
                {
                    currentState = AmbushState.Attacking;
                }
                break;
                
            case AmbushState.Attacking:
                // Execute attack
                ActionNodes.Attack(ctx);
                
                // Transition to retreat when out of range
                if (!UtilityNodes.IsInAttackRange(ctx))
                {
                    ActionNodes.StopAttack(ctx);
                    currentState = AmbushState.Retreating;
                    ctx.customData = null; // Clear for retreat
                }
                break;
                
            case AmbushState.Retreating:
                // Retreat away from player
                LurkMovement.Retreat(ctx, config.retreatDistance);
                
                // Check if reached retreat distance
                float distFromTarget = Vector2.Distance(ctx.self.position, ctx.target.position);
                if (distFromTarget >= config.retreatDistance - 2f)
                {
                    // Reset to lurking
                    currentState = AmbushState.Lurking;
                    lurkTimer = config.lurkTime;
                    ctx.customData = null; // Clear for new lurk position
                }
                break;
        }
    }
    
    public void OnLateUpdate() { }
}
