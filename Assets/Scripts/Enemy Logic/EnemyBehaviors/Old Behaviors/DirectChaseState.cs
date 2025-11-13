// DirectChaseBehavior.cs
using UnityEngine;
using System;

[Serializable]
public class DirectChaseBehaviorCfg : BehaviorCfg
{
    [Header("Direct Chase – Tunables")]
    public float maxSpeed = 2f;
    public float speed = 1f;
    public float attackRange = 1f;
    public float outOfRange = 10f;
    public float respawnRange = 8f;
    public float divergeRange = 2f;
    public float divergeWeight = 1f;

    public override IBehavior CreateRuntimeBehavior() => new DirectChaseBehavior_Modular(this);
}

public class DirectChaseBehavior_Modular : IBehavior
{
    private BehaviorContext ctx;
    private DirectChaseBehaviorCfg config;
    
    // State tracking
    private bool isAttacking = false;
    
    public DirectChaseBehavior_Modular(DirectChaseBehaviorCfg cfg)
    {
        config = cfg;
    }
    
    public void OnEnter(Transform _self, Animator _anim)
    {
        // Initialize context with all the shared data
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
    
    public void OnExit() 
    {
        // Cleanup if needed
    }
    
    public void OnUpdate()
    {
        // Update per-frame data
        ctx.UpdateFrame();
        
        // Root decision tree - check conditions and execute appropriate nodes
        
        // Check if out of range first
        if (UtilityNodes.IsOutOfRange(ctx))
        {
            ActionNodes.Respawn(ctx, ctx.respawnRange);
            isAttacking = false;
            return;
        }
        
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
            // Not in attack range
            if (isAttacking)
            {
                ActionNodes.StopAttack(ctx);
                isAttacking = false;
            }
            
            // Execute chase movement
            DirectChaseMovement.Execute(ctx);
        }
    }
    
    public void OnLateUpdate() 
    {
        // Any late update logic if needed
    }
}
