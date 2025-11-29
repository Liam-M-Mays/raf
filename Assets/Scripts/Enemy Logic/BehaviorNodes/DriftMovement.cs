using UnityEngine;

/// <summary>
/// Drift movement - slow drift with occasional pulses (jellyfish-like)
/// </summary>
public static class DriftMovement
{
    public class DriftData
    {
        public Vector2 driftDirection;
        public float pulseTimer;
        public bool isPulsing;
    }
    
    /// <summary>
    /// Execute drift movement with occasional pulses
    /// </summary>
    public static void Execute(BehaviorContext ctx, float driftSpeed, float pulseInterval, float pulseStrength, bool setAnimation = true)
    {
        if (setAnimation)
        {
            ActionNodes.SetMoving(ctx, true);
        }
        
        // Get or create drift data
        DriftData data = ctx.customData as DriftData;
        if (data == null)
        {
            data = new DriftData();
            data.pulseTimer = pulseInterval;
            PickNewDriftDirection(ctx, data);
            ctx.customData = data;
        }
        
        data.pulseTimer -= ctx.deltaTime;
        
        if (data.isPulsing)
        {
            // Quick pulse toward player
            Vector2 dirToPlayer = ((Vector2)ctx.target.position - (Vector2)ctx.self.position).normalized;
            Vector2 pulseTarget = (Vector2)ctx.self.position + dirToPlayer * pulseStrength;
            
            Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, pulseTarget);
            float dist = Vector2.Distance(ctx.self.position, targetPos);
            ctx.self.position = Vector2.MoveTowards(ctx.self.position, targetPos, ctx.maxSpeed * ctx.deltaTime);
            
            // End pulse after short duration
            if (dist < 0.5f || Time.frameCount % 30 == 0)
            {
                data.isPulsing = false;
                PickNewDriftDirection(ctx, data);
            }
        }
        else
        {
            // Slow drift
            Vector2 driftTarget = (Vector2)ctx.self.position + data.driftDirection * 10f;
            Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, driftTarget);
            ctx.self.position = Vector2.MoveTowards(ctx.self.position, targetPos, driftSpeed * ctx.deltaTime);
            
            // Occasionally change drift direction
            if (Random.value < 0.01f)
            {
                PickNewDriftDirection(ctx, data);
            }
            
            // Trigger pulse
            if (data.pulseTimer <= 0f)
            {
                data.isPulsing = true;
                data.pulseTimer = pulseInterval;
            }
        }
    }
    
    private static void PickNewDriftDirection(BehaviorContext ctx, DriftData data)
    {
        // Bias drift direction toward player
        Vector2 toPlayer = ((Vector2)ctx.target.position - (Vector2)ctx.self.position).normalized;
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        data.driftDirection = (toPlayer * 0.4f + randomDir * 0.6f).normalized;
    }
}
