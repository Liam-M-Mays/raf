using UnityEngine;

/// <summary>
/// Flamethrower attack: short-range sustained damage with fire cone.
/// Deals damage over time to enemies in an arc in front of the enemy.
/// </summary>
public class EnemyFlamethrowerAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRange = 3f;
    [SerializeField] private float fireDamagePerSecond = 10f;
    [SerializeField] private float fireConeAngle = 45f;
    [SerializeField] private float fireRate = 0.2f;

    private float lastFireTime = -999f;

    public void Fire(Vector2 direction)
    {
        if (Time.time < lastFireTime + fireRate) return;

        Vector2 firePos = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        
        // Find all entities in cone
        Collider2D[] hits = Physics2D.OverlapCircleAll(firePos, fireRange);
        foreach (var col in hits)
        {
            if (col.CompareTag("Raft"))
            {
                // Check if in cone
                Vector2 toTarget = ((Vector2)col.transform.position - firePos).normalized;
                float angle = Vector2.Angle(direction, toTarget);
                if (angle <= fireConeAngle)
                {
                    var health = col.GetComponent<Health>();
                    if (health != null)
                    {
                        health.TakeDamage(fireDamagePerSecond * fireRate, col.transform.position);
                    }
                }
            }
        }

        lastFireTime = Time.time;
    }

    public void SetDamage(float newDamage) => fireDamagePerSecond = newDamage;
}
