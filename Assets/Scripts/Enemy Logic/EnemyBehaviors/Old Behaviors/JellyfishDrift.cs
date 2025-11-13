using UnityEngine;
using System;

#region Inline Config (shows in Enemy/Archetype SO)

[Serializable]
public class JellyfishDriftBehaviorCfg : BehaviorCfg
{
    [Header("Speeds")]
    [Min(0f)] public float maxSpeed = 2f;
    [Min(0f)] public float speed    = 1f;

    [Header("Ranges")]
    [Min(0f)] public float attackRange   = 1f;
    [Min(0f)] public float outOfRange    = 10f; // kept for parity (not used here)
    [Min(0f)] public float respawnRange  = 8f;  // kept for parity (not used here)

    [Header("Separation")]
    [Min(0f)] public float divergeRange  = 2f;
    public float divergeWeight           = 1f;

    [Header("Jellyfish Motion")]
    [Min(0f)] public float pulseStrength = 2f;   // strength per pulse
    [Min(0f)] public float pulseInterval = 1.5f; // time between pulses
    [Min(0f)] public float driftSpeed    = 0.5f; // passive drift speed

    public override IBehavior CreateRuntimeBehavior() => new JellyfishDriftBehavior_Modular(this);
}

#endregion


public class JellyfishDriftBehavior_Modular : IBehavior
{
    private BehaviorContext ctx;
    private JellyfishDriftBehaviorCfg config;
    
    // State tracking
    private bool isAttacking = false;
    
    public JellyfishDriftBehavior_Modular(JellyfishDriftBehaviorCfg cfg)
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
    }
    
    public void OnExit() { }
    
    public void OnUpdate()
    {
        // Update per-frame data
        ctx.UpdateFrame();
        
        // Check if in attack range
        if (UtilityNodes.IsInAttackRange(ctx))
        {
            if (!isAttacking)
            {
                ActionNodes.Attack(ctx);
                isAttacking = true;
            }
        }
        else
        {
            // Not in attack range - drift with pulses
            if (isAttacking)
            {
                ActionNodes.StopAttack(ctx);
                isAttacking = false;
            }
            
            // Execute jellyfish drift movement
            DriftMovement.Execute(ctx, config.driftSpeed, config.pulseInterval, config.pulseStrength);
        }
    }
    
    public void OnLateUpdate() { }
}
