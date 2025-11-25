using UnityEngine;

public static class DirectChaseMovement
{
    public static void Execute(BehaviorContext ctx, bool setAnimation = true)
    {

        // Apply divergence to avoid other sharks
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, ctx.target.position);

        // Move towards target with distance-based speed
        UtilityNodes.UpdateFacing(ctx, targetPos);
        UtilityNodes.MoveTowards(ctx, targetPos);
    }
    
    public static void SlowExecute(BehaviorContext ctx, bool setAnimation = true)
    {
        
        // Apply divergence to avoid other sharks
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, ctx.target.position);
        UtilityNodes.UpdateFacing(ctx, targetPos);
        ctx.self.position = Vector2.MoveTowards(ctx.self.position, targetPos, 0.7f * ctx.deltaTime);
    }
}
