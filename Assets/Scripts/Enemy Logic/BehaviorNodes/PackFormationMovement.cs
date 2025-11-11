using UnityEngine;

/// <summary>
/// Pack formation movement - coordinate with other entities to hunt as a group
/// </summary>
public static class PackFormationMovement
{
    public class PackData
    {
        public Vector2 packCenter;
        public float angleInPack;
        public bool isFormed;
    }
    
    /// <summary>
    /// Form up with other pack members
    /// </summary>
    public static bool FormUp(BehaviorContext ctx, float formationSpacing, bool setAnimation = true)
    {
        if (setAnimation)
        {
            ActionNodes.SetMoving(ctx, true);
        }
        
        // Get or create pack data
        PackData data = ctx.customData as PackData;
        if (data == null)
        {
            data = new PackData();
            // Assign position in pack based on instance ID
            data.angleInPack = (ctx.self.GetInstanceID() % 8) * (Mathf.PI * 2f / 8f);
            ctx.customData = data;
        }
        
        // Calculate pack center
        CalculatePackCenter(ctx, data);
        
        // Calculate formation position
        Vector2 formationPos = data.packCenter + new Vector2(
            Mathf.Cos(data.angleInPack) * formationSpacing,
            Mathf.Sin(data.angleInPack) * formationSpacing
        );
        
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, formationPos);
        float dist = Vector2.Distance(ctx.self.position, targetPos);
        UtilityNodes.MoveTowards(ctx, targetPos, 0.7f); // Slower for formation
        
        UtilityNodes.UpdateFacing(ctx);
        
        // Check if in formation
        if (dist < 1f)
        {
            data.isFormed = true;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Chase as a coordinated pack
    /// </summary>
    public static void PackChase(BehaviorContext ctx, float formationSpacing, bool setAnimation = true)
    {
        if (setAnimation)
        {
            ActionNodes.SetMoving(ctx, true);
        }
        
        // Get pack data
        PackData data = ctx.customData as PackData;
        if (data == null)
        {
            data = new PackData();
            data.angleInPack = (ctx.self.GetInstanceID() % 8) * (Mathf.PI * 2f / 8f);
            ctx.customData = data;
        }
        
        // Update pack center
        CalculatePackCenter(ctx, data);
        
        // Calculate pack formation position
        Vector2 packFormationPos = data.packCenter + new Vector2(
            Mathf.Cos(data.angleInPack) * formationSpacing,
            Mathf.Sin(data.angleInPack) * formationSpacing
        );
        
        // Blend between formation position and target
        Vector2 chaseTarget = Vector2.Lerp(packFormationPos, (Vector2)ctx.target.position, 0.3f);
        
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, chaseTarget);
        UtilityNodes.MoveTowards(ctx, targetPos);
        
        // Slowly rotate pack formation for dynamic movement
        data.angleInPack += ctx.deltaTime * 0.5f;
        
        UtilityNodes.UpdateFacing(ctx);
    }
    
    private static void CalculatePackCenter(BehaviorContext ctx, PackData data)
    {
        // Find center point of all pack members
        var hits = Physics2D.OverlapCircleAll(ctx.self.position, 16f, ctx.sharkLayer);
        if (hits.Length <= 1)
        {
            data.packCenter = (Vector2)ctx.self.position;
            return;
        }
        
        Vector2 sum = Vector2.zero;
        int count = 0;
        foreach (var h in hits)
        {
            sum += (Vector2)h.transform.position;
            count++;
        }
        
        data.packCenter = sum / count;
    }
}
