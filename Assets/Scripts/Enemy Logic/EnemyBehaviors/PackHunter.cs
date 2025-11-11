using UnityEngine;
using System;

#region Inline Config (shows in Enemy/Archetype SO)

[Serializable]
public class PackHunterBehaviorCfg : BehaviorCfg
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

    [Header("Pack Behavior")]
    [Min(0f)] public float packDistance      = 8f; // preferred distance from pack center
    [Min(0f)] public float formationSpacing  = 3f; // spacing between pack members

    public override IBehavior CreateRuntimeBehavior() => new PackHunterBehavior_Modular(this);
}

#endregion


public class PackHunterBehavior_Modular : IBehavior
{
    private BehaviorContext ctx;
    private PackHunterBehaviorCfg config;
    
    // State tracking
    private enum PackState
    {
        FormingUp,
        PackHunting,
        Attacking
    }
    
    private PackState currentState = PackState.FormingUp;
    
    public PackHunterBehavior_Modular(PackHunterBehaviorCfg cfg)
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
        
        currentState = PackState.FormingUp;
    }
    
    public void OnExit() { }
    
    public void OnUpdate()
    {
        // Update per-frame data
        ctx.UpdateFrame();
        
        switch (currentState)
        {
            case PackState.FormingUp:
                // Form up with pack
                if (PackFormationMovement.FormUp(ctx, config.formationSpacing))
                {
                    currentState = PackState.PackHunting;
                }
                break;
                
            case PackState.PackHunting:
                // Check if in attack range
                if (UtilityNodes.IsInAttackRange(ctx))
                {
                    currentState = PackState.Attacking;
                }
                else
                {
                    // Hunt as a pack
                    PackFormationMovement.PackChase(ctx, config.formationSpacing);
                }
                break;
                
            case PackState.Attacking:
                ActionNodes.Attack(ctx);
                
                // Return to pack hunting when out of range
                if (!UtilityNodes.IsInAttackRange(ctx))
                {
                    ActionNodes.StopAttack(ctx);
                    currentState = PackState.PackHunting;
                }
                break;
        }
    }
    
    public void OnLateUpdate() { }
}
