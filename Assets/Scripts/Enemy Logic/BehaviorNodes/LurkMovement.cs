using UnityEngine;

/// <summary>
/// Lurk movement - maintain distance while slowly repositioning
/// </summary>
public static class LurkMovement
{
    public class LurkData
    {
        public Vector2 lurkPosition;
        public float repositionTimer;
    }
    
    /// <summary>
    /// Execute lurk movement - stay at distance from target
    /// </summary>
    public static void Execute(BehaviorContext ctx, float lurkDistance, bool setAnimation = true)
    {
        if (setAnimation)
        {
            ActionNodes.SetMoving(ctx, true);
        }
        
        // Get or create lurk data
        LurkData data = ctx.customData as LurkData;
        if (data == null)
        {
            data = new LurkData();
            PickLurkPosition(ctx, data, lurkDistance);
            ctx.customData = data;
        }
        
        // Move to lurk position slowly
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, data.lurkPosition);
        float dist = Vector2.Distance(ctx.self.position, targetPos);
        float velocity = Mathf.Clamp(dist * ctx.speed * 0.5f, 0f, ctx.maxSpeed * 0.4f); // Slower lurking
        ctx.self.position = Vector2.MoveTowards(ctx.self.position, targetPos, velocity * ctx.deltaTime);
        
        // Pick new lurk position occasionally
        if (dist < 2f || Random.value < 0.01f)
        {
            PickLurkPosition(ctx, data, lurkDistance);
        }
        
        UtilityNodes.UpdateFacing(ctx, Vector2.zero);
    }
    
    /// <summary>
    /// Pick a new lurk position around the target
    /// </summary>
    private static void PickLurkPosition(BehaviorContext ctx, LurkData data, float lurkDistance)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        data.lurkPosition = (Vector2)ctx.target.position + new Vector2(
            Mathf.Cos(angle) * lurkDistance,
            Mathf.Sin(angle) * lurkDistance
        );
    }
    
    /// <summary>
    /// Retreat away from target after an action
    /// </summary>
    public static void Retreat(BehaviorContext ctx, float retreatDistance, bool setAnimation = true)
    {
        if (setAnimation)
        {
            ActionNodes.SetMoving(ctx, true);
        }
        
        // Move away from target
        Vector2 retreatDir = ((Vector2)ctx.self.position - (Vector2)ctx.target.position).normalized;
        Vector2 retreatTarget = (Vector2)ctx.target.position + retreatDir * retreatDistance;
        
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, retreatTarget);
        UtilityNodes.MoveTowards(ctx, targetPos);
        
        UtilityNodes.UpdateFacing(ctx, Vector2.zero);
    }
}
