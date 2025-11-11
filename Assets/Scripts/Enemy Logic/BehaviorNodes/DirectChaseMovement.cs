using UnityEngine;

/// <summary>
/// Direct chase movement - moves straight toward target
/// </summary>
public static class DirectChaseMovement
{
    public static void Execute(BehaviorContext ctx, bool setAnimation = true)
    {
        if (setAnimation)
        {
            ActionNodes.SetMoving(ctx, true);
        }
        
        // Apply divergence to avoid other sharks
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, ctx.target.position);
        
        // Move towards target with distance-based speed
        float dist = Vector2.Distance(ctx.self.position, targetPos);
        float velocity = Mathf.Clamp(dist * ctx.speed + 1f, 0f, ctx.maxSpeed);
        ctx.self.position = Vector2.MoveTowards(ctx.self.position, targetPos, velocity * ctx.deltaTime);
        ctx.currentVelocity = velocity;
        
        // Update facing
        UtilityNodes.UpdateFacing(ctx);
    }
}
