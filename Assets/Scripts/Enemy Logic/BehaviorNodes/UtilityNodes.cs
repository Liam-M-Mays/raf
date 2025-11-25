using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Common utility nodes used across all behaviors
/// </summary>
public static class UtilityNodes
{
    
    public static Vector2 ApplyDivergence(BehaviorContext ctx, Vector2 targetPosition)
    {
        Vector2 selfPos = ctx.self.position;
        Vector2 totalPush = Vector2.zero;
        
        Collider2D[] sharks = Physics2D.OverlapCircleAll(selfPos, ctx.divergeRange, ctx.sharkLayer);

        foreach (Collider2D shark in sharks)
        {
            if (shark.transform == ctx.self) continue;

            Vector2 offset = selfPos - (Vector2)shark.transform.position;
            float distSquared = offset.sqrMagnitude;

            float dist = Mathf.Sqrt(distSquared);
            
            // Inverse distance weighting - closer sharks push MUCH harder
            float fallOff = 1f - (dist / ctx.divergeRange);
            fallOff = fallOff * fallOff * fallOff; // Cubic for aggressive close-range push
            
            // Normalize direction and apply weighted push
            totalPush += (offset / dist) * fallOff;
        }
        Debug.DrawLine(selfPos, targetPosition, Color.red);
        Debug.DrawLine(selfPos, targetPosition + (totalPush * ctx.divergeWeight), Color.green);
        Debug.Log($"TotalPush magnitude: {totalPush.magnitude}, Weight: {ctx.divergeWeight}");
        Debug.Log($"Found {sharks.Length} sharks within range");
        return targetPosition + (totalPush * ctx.divergeWeight);
    }
    
    /// <summary>
    /// Flip sprite to face target
    /// </summary>
    public static void UpdateFacing(BehaviorContext ctx, Vector2 target)
    {
        bool shouldFaceRight = ctx.self.position.x <= target.x;
        bool isFacingRight = ctx.self.localScale.x > 0;
        
        if (shouldFaceRight != isFacingRight )//&& Vector2.Distance(target, (Vector2)ctx.self.position) > 0.75)
        {
            Flip(ctx);
        }
    }

    public static void UpdateFacingReverse(BehaviorContext ctx, Vector2 target)
    {
        bool shouldFaceRight = ctx.self.position.x >= target.x;
        bool isFacingRight = ctx.self.localScale.x > 0;
        
        if (shouldFaceRight != isFacingRight )//&& Vector2.Distance(target, (Vector2)ctx.self.position) > 0.75)
        {
            Flip(ctx);
        }
    }

    public static void Flip(BehaviorContext ctx)
    {
        Vector3 scaler = ctx.self.localScale;
        scaler.x *= -1f;
        ctx.self.localScale = scaler;
    }

    public static bool IsInAttackRange(BehaviorContext ctx)
    {
        Collider2D[] raft = Physics2D.OverlapCircleAll(ctx.self.position, ctx.attackRange, LayerMask.GetMask("Raft"));
        if (raft.Length > 0) return true;
        return false;
    }
    public static bool IsInAttackMax(BehaviorContext ctx)
    {
        Collider2D[] raft = Physics2D.OverlapCircleAll(ctx.self.position, ctx.attackRangeMax, LayerMask.GetMask("Raft"));
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


    public static void MoveTowards(BehaviorContext ctx, Vector2 targetPos, float speedMultiplier = 1f)
    {
        float dist = Vector2.Distance(ctx.self.position, targetPos);
        float velocity = Mathf.Clamp(dist * ctx.speed * speedMultiplier, 0f, ctx.maxSpeed * speedMultiplier);
        ctx.self.position = Vector2.MoveTowards(ctx.self.position, targetPos, velocity * ctx.deltaTime);
        ctx.currentVelocity = velocity;
    }

    public static Vector2 TargetOnOrbit(BehaviorContext ctx, float distance)
    {
        Vector2 currentOffset = (Vector2)ctx.self.position - (Vector2)ctx.target.position;
        var currentAngle = Mathf.Atan2(currentOffset.y, currentOffset.x);
        
        // Calculate target position on circle
        return (Vector2)ctx.target.position + new Vector2(
            Mathf.Cos(currentAngle) * distance,
            Mathf.Sin(currentAngle) * distance
        );
    }

    
    public static bool Obstructed(BehaviorContext ctx)
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
