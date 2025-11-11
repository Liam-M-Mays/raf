using UnityEngine;
using System;

#region Inline Config (shows in Enemy/Archetype SO)

[Serializable]
public class ZigzagApproachBehaviorCfg : BehaviorCfg
{
    [Header("Speeds")]
    [Min(0f)] public float maxSpeed = 2f;
    [Min(0f)] public float speed    = 1f;

    [Header("Ranges")]
    [Min(0f)] public float attackRange   = 1f;
    [Min(0f)] public float outOfRange    = 10f; // (kept for parity; not used in this logic)
    [Min(0f)] public float respawnRange  = 8f;  // (kept for parity; not used in this logic)

    [Header("Separation")]
    [Min(0f)] public float divergeRange  = 2f;
    public float divergeWeight           = 1f;

    [Header("Zigzag")]
    [Min(0f)] public float zigzagAmplitude = 5f; // how wide the zigzag
    [Min(0f)] public float zigzagFrequency = 2f; // how fast it zigzags

    public override IBehavior CreateRuntimeBehavior() => new ZigzagApproachBehavior_Modular(this);
}

#endregion



public class ZigzagApproachBehavior_Modular : IBehavior
{
    private BehaviorContext ctx;
    private ZigzagApproachBehaviorCfg config;
    
    // State tracking
    private bool isAttacking = false;
    
    public ZigzagApproachBehavior_Modular(ZigzagApproachBehaviorCfg cfg)
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
        
        // Simple decision tree
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
            // Not in attack range - zigzag approach
            if (isAttacking)
            {
                ActionNodes.StopAttack(ctx);
                isAttacking = false;
            }
            
            // Execute zigzag movement
            ZigzagMovement.Execute(ctx, config.zigzagAmplitude, config.zigzagFrequency);
        }
    }
    
    public void OnLateUpdate() { }
}
