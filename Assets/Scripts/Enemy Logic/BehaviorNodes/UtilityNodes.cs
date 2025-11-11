using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Common utility nodes used across all behaviors
/// </summary>
public static class UtilityNodes
{
    
    public static Vector2 ApplyDivergence(BehaviorContext ctx, Vector2 targetPosition)
    {
        Vector2 newTarget = targetPosition;
        Collider2D[] sharks = Physics2D.OverlapCircleAll(ctx.self.position, ctx.divergeRange, ctx.sharkLayer);

        foreach (Collider2D shark in sharks)
        {
            if (shark.transform == ctx.self) continue;

            Vector2 offset = (Vector2)(ctx.self.position - shark.transform.position);
            float dist = offset.magnitude;

            if (dist <= 0f || dist >= ctx.divergeRange) continue;

            float fallOff = 1f - (dist / ctx.divergeRange);
            fallOff *= fallOff;

            Vector2 pushAmount = offset * ctx.divergeWeight * fallOff;
            newTarget += pushAmount;
        }

        return newTarget;
    }
    
    /// <summary>
    /// Flip sprite to face target
    /// </summary>
    public static void UpdateFacing(BehaviorContext ctx)
    {
        bool shouldFaceRight = ctx.self.position.x < ctx.target.position.x;
        bool isFacingRight = ctx.self.localScale.x > 0;
        
        if (shouldFaceRight != isFacingRight)
        {
            Flip(ctx);
        }
    }
    
    /// <summary>
    /// Flip the sprite horizontally
    /// </summary>
    public static void Flip(BehaviorContext ctx)
    {
        Vector3 scaler = ctx.self.localScale;
        scaler.x *= -1f;
        ctx.self.localScale = scaler;
    }
    
    /// <summary>
    /// Check if within attack range
    /// </summary>
    public static bool IsInAttackRange(BehaviorContext ctx)
    {
        Collider2D[] raft = Physics2D.OverlapCircleAll(ctx.self.position, ctx.attackRange, LayerMask.GetMask("Raft"));
        if (raft.Length > 0) return true;
        return false;
    }
    
    /// <summary>
    /// Check if out of range (needs respawn)
    /// </summary>
    public static bool IsOutOfRange(BehaviorContext ctx)
    {
        return ctx.distanceToTarget >= ctx.outOfRange;
    }

    /// <summary>
    /// Move towards a position with speed calculation
    /// </summary>
    public static void MoveTowards(BehaviorContext ctx, Vector2 targetPos, float speedMultiplier = 1f)
    {
        float dist = Vector2.Distance(ctx.self.position, targetPos);
        float velocity = Mathf.Clamp(dist * ctx.speed * speedMultiplier, 0f, ctx.maxSpeed * speedMultiplier);
        ctx.self.position = Vector2.MoveTowards(ctx.self.position, targetPos, velocity * ctx.deltaTime);
        ctx.currentVelocity = velocity;
    }

    
    public static bool obstructed(BehaviorContext ctx)
    {
        RaycastHit2D[] hit = Physics2D.LinecastAll(ctx.self.position, ctx.target.position, ctx.sharkLayer);

        if (hit.Length > 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
