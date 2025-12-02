using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Shared perception system for enemies to be aware of allies and threats.
/// Allows soft hive-mind behavior where enemies coordinate and support each other.
/// </summary>
public static class EnemyPerception
{
    [System.Serializable]
    public class AllyInfo
    {
        public EnemyController enemy;
        public float distance;
        public bool isUnderAttack;
    }

    /// <summary>
    /// Find nearby allies and their status. Used for hive-mind awareness.
    /// </summary>
    public static List<AllyInfo> GetNearbyAllies(Transform self, float perceptionRadius, LayerMask allyLayer)
    {
        var allies = new List<AllyInfo>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(self.position, perceptionRadius, allyLayer);

        foreach (var col in hits)
        {
            if (col.transform == self) continue;
            var enemy = col.GetComponent<EnemyController>();
            if (enemy != null)
            {
                var health = col.GetComponent<Health>();
                allies.Add(new AllyInfo
                {
                    enemy = enemy,
                    distance = Vector2.Distance(self.position, col.transform.position),
                    isUnderAttack = health != null && health.GetHealthPercentage() < 0.9f
                });
            }
        }

        return allies;
    }

    /// <summary>
    /// Compute a shared threat position from nearby allies being attacked.
    /// This allows enemies to reinforce locations where allies are struggling.
    /// </summary>
    public static Vector2 GetSharedThreatPosition(List<AllyInfo> allies, Transform primaryTarget)
    {
        Vector2 threat = (Vector2)primaryTarget.position;
        float underAttackCount = 0f;

        foreach (var ally in allies)
        {
            if (ally.isUnderAttack)
            {
                threat += (Vector2)ally.enemy.transform.position;
                underAttackCount++;
            }
        }

        if (underAttackCount > 0)
        {
            threat /= (underAttackCount + 1f);
        }

        return threat;
    }

    /// <summary>
    /// Soft hive-mind sync: coordinate speeds and headings among nearby allies.
    /// </summary>
    public static Vector2 GetHiveMindInfluence(List<AllyInfo> allies, Vector2 myHeading, float syncWeight = 0.3f)
    {
        if (allies.Count == 0) return myHeading;

        Vector2 avgHeading = myHeading;
        foreach (var ally in allies)
        {
            var behavior = ally.enemy.currentBehavior();
            var ctx = behavior?.CTX();
            if (ctx != null)
            {
                avgHeading += (Vector2)(ctx.self.position - ally.enemy.transform.position).normalized;
            }
        }

        avgHeading /= (allies.Count + 1f);
        return Vector2.Lerp(myHeading, avgHeading, syncWeight);
    }
}
