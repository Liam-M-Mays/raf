using UnityEngine;

/// <summary>
/// Zigzag movement - approach target in a zigzag pattern
/// </summary>
public static class ZigzagMovement
{
    public class ZigzagData
    {
        public float zigzagTimer;
    }

    public static void Execute(BehaviorContext ctx, float amplitude, float frequency, bool setAnimation = true)
    {

        // Get or create zigzag data
        ZigzagData data = ctx.customData as ZigzagData;
        if (data == null)
        {
            data = new ZigzagData();
            ctx.customData = data;
        }

        // Update timer
        data.zigzagTimer += ctx.deltaTime * frequency;

        // Calculate direction to target
        Vector2 dirToTarget = ((Vector2)ctx.target.position - (Vector2)ctx.self.position).normalized;

        // Calculate perpendicular direction for zigzag
        Vector2 perpendicular = new Vector2(-dirToTarget.y, dirToTarget.x);

        // Apply sine-based offset
        float offset = Mathf.Sin(data.zigzagTimer) * amplitude;
        Vector2 zigzagTarget = (Vector2)ctx.target.position + perpendicular * offset;

        // Apply divergence and move
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, zigzagTarget);
        UtilityNodes.MoveTowards(ctx, targetPos);

        // Update facing
        UtilityNodes.UpdateFacing(ctx, Vector2.zero);
    }
    
    public static void ExecuteTarget(BehaviorContext ctx, Vector2 target, float amplitude, float frequency, bool setAnimation = true)
    {
        
        // Get or create zigzag data
        ZigzagData data = ctx.customData as ZigzagData;
        if (data == null)
        {
            data = new ZigzagData();
            ctx.customData = data;
        }
        
        // Update timer
        data.zigzagTimer += ctx.deltaTime * frequency;
        
        // Calculate direction to target
        Vector2 dirToTarget = (target - (Vector2)ctx.self.position).normalized;
        
        // Calculate perpendicular direction for zigzag
        Vector2 perpendicular = new Vector2(-dirToTarget.y, dirToTarget.x);
        
        // Apply sine-based offset
        float offset = Mathf.Sin(data.zigzagTimer) * amplitude;
        Vector2 zigzagTarget = target + perpendicular * offset;
        
        // Apply divergence and move
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, zigzagTarget);
        UtilityNodes.UpdateFacing(ctx, targetPos);
        UtilityNodes.MoveTowards(ctx, targetPos);
    }
}
