using UnityEngine;

/// <summary>
/// Charge movement - fast straight-line charge with windup and cooldown
/// </summary>
public static class ChargeMovement
{
    public class ChargeData
    {
        public Vector2 chargeDirection;
        public Vector2 chargeStartPos;
        public float windupTimer;
        public float cooldownTimer;
        public bool isCharging;
    }
    
    /// <summary>
    /// Position behind target for charge windup
    /// </summary>
    public static void PositionForCharge(BehaviorContext ctx, float chargeDistance)
    {
        
        // Move to a point behind the target
        Vector2 dirToTarget = ((Vector2)ctx.target.position - (Vector2)ctx.self.position).normalized;
        Vector2 positionTarget = (Vector2)ctx.target.position - dirToTarget * chargeDistance;
        
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, positionTarget);
        UtilityNodes.UpdateFacing(ctx, targetPos);
        UtilityNodes.MoveTowards(ctx, targetPos, 0.8f); // Slower positioning
    }
    
    public static bool Windup(BehaviorContext ctx, float windupTime)
    {
        ChargeData data = ctx.customData as ChargeData;
        if (data == null)
        {
            data = new ChargeData();
            data.windupTimer = windupTime;
            data.chargeStartPos = ctx.self.position;
            ctx.customData = data;
        }
        

        data.windupTimer -= ctx.deltaTime;
        
        if (data.windupTimer <= 0f)
        {
            // Set charge direction at the moment of charge
            data.chargeDirection = ((Vector2)ctx.target.position - (Vector2)ctx.self.position).normalized;
            return true; // Windup complete
        }
        
        return false; // Still winding up
    }
    

    public static bool ExecuteCharge(BehaviorContext ctx, float chargeSpeed, float maxChargeDistance)
    {
        ChargeData data = ctx.customData as ChargeData;
        if (data == null) return true; // No charge data, end charge
        
        
        // Charge straight ahead at high speed
        Vector2 chargeTarget = (Vector2)ctx.self.position + data.chargeDirection * 100f;
        float velocity = ctx.maxSpeed * chargeSpeed;
        UtilityNodes.UpdateFacing(ctx, chargeTarget);
        ctx.self.position = Vector2.MoveTowards(ctx.self.position, chargeTarget, velocity * ctx.deltaTime);
        
        
        // Check if charge should end
        float distanceTraveled = Vector2.Distance(ctx.self.position, data.chargeStartPos);
        if (distanceTraveled > maxChargeDistance)
        {
            return true; // Charge complete
        }
        
        return false; // Still charging
    }
    
    /// <summary>
    /// Cooldown movement after charge (circles target)
    /// </summary>
    public static bool Cooldown(BehaviorContext ctx, float cooldownTime)
    {
        ChargeData data = ctx.customData as ChargeData;
        if (data == null)
        {
            data = new ChargeData();
            data.cooldownTimer = cooldownTime;
            ctx.customData = data;
        }
        
        ActionNodes.SetMoving(ctx, true);
        
        data.cooldownTimer -= ctx.deltaTime;
        
        // Simple circle during cooldown
        Vector2 currentOffset = (Vector2)ctx.self.position - (Vector2)ctx.target.position;
        float currentAngle = Mathf.Atan2(currentOffset.y, currentOffset.x);
        float circleRadius = Mathf.Max(10f, currentOffset.magnitude);
        
        float targetAngle = currentAngle + (ctx.maxSpeed / circleRadius) * 0.3f;
        Vector2 circlePos = (Vector2)ctx.target.position + new Vector2(
            Mathf.Cos(targetAngle) * circleRadius,
            Mathf.Sin(targetAngle) * circleRadius
        );
        
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, circlePos);
        UtilityNodes.UpdateFacing(ctx, targetPos);
        UtilityNodes.MoveTowards(ctx, targetPos, 0.6f); // Slower during cooldown
        
        
        return data.cooldownTimer <= 0f; // Return true when cooldown is complete
    }
}
