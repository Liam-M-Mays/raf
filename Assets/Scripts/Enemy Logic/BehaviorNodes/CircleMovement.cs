using UnityEngine;

/// <summary>
/// Circle movement - orbit around target at specified radius
/// </summary>
public static class CircleMovement
{
    public class CircleData
    {
        public float circleRadius;
        public float currentAngle;
        public float direction; // 1 or -1 for clockwise/counter-clockwise
    }
    
    public static void Execute(BehaviorContext ctx, float minRadius, float maxRadius, float direction, bool setAnimation = true)
    {
        
        
        // Get or create circle data
        CircleData data = ctx.customData as CircleData;
        if (data == null)
        {
            data = new CircleData();
            data.direction = direction;
            ctx.customData = data;
        }
        
        // Calculate current offset and clamp orbit radius
        Vector2 currentOffset = (Vector2)ctx.self.position - (Vector2)ctx.target.position;
        data.circleRadius = Mathf.Clamp(currentOffset.magnitude, minRadius, maxRadius);
        data.circleRadius = Mathf.Max(data.circleRadius - 0.1f, minRadius);
        
        // Get current angle on circle
        data.currentAngle = Mathf.Atan2(currentOffset.y, currentOffset.x);
        
        // Calculate rotation speed based on radius
        float rotationSpeed = ctx.maxSpeed / data.circleRadius;
        float targetAngle = data.currentAngle + (data.direction * rotationSpeed * 0.5f);
        
        // Calculate target position on circle
        Vector2 circlePos = (Vector2)ctx.target.position + new Vector2(
            Mathf.Cos(targetAngle) * data.circleRadius,
            Mathf.Sin(targetAngle) * data.circleRadius
        );
        
        // Apply divergence and move
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, circlePos);
        UtilityNodes.UpdateFacing(ctx, targetPos);
        UtilityNodes.MoveTowards(ctx, targetPos);
        
        // Update facing
        
    }
}
