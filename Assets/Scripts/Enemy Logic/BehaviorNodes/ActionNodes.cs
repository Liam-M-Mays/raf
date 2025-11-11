using UnityEngine;

/// <summary>
/// Common action nodes that perform specific behaviors
/// </summary>
public static class ActionNodes
{
    /// <summary>
    /// Execute attack behavior
    /// </summary>
    public static void Attack(BehaviorContext ctx, bool setAnimation = true)
    {
        if (setAnimation)
        {
            ctx.anim.SetBool("Attacking", true);
            ctx.anim.SetBool("Moving", false);
        }
        
        // Attack logic happens here
        // The actual damage dealing would be handled by animation events or other systems
    }
    
    /// <summary>
    /// Stop attacking
    /// </summary>
    public static void StopAttack(BehaviorContext ctx, bool setAnimation = true)
    {
        if (setAnimation)
        {
            ctx.anim.SetBool("Attacking", false);
            ctx.anim.SetBool("Moving", true);
        }
    }
    
    /// <summary>
    /// Respawn near target
    /// </summary>
    public static void Respawn(BehaviorContext ctx, float range, bool triggerAnimation = true)
    {
        if (triggerAnimation)
        {
            ctx.anim.SetTrigger("Respawn");
        }
        
        // Generate random position near target
        Vector2 offset = Random.insideUnitCircle.normalized * (range - 4f) + Random.insideUnitCircle * 1f;
        Vector3 newPosition = ctx.target.position + new Vector3(offset.x, offset.y, 0f);
        ctx.self.position = newPosition;
    }
    
    /// <summary>
    /// Set movement animation state
    /// </summary>
    public static void SetMoving(BehaviorContext ctx, bool isMoving)
    {
        ctx.anim.SetBool("Moving", isMoving);
    }
}
