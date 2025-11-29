using UnityEngine;

/// <summary>
/// Handles melee damage for enemies. Add to enemies that attack up close.
/// Can be triggered by animation events or colliders.
/// </summary>
public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    
    [Header("Attack Detection")]
    [SerializeField] private LayerMask raftLayer;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1f;
    
    private float lastAttackTime = -999f;
    private bool canAttack = true;

    void Start()
    {
        // If no attack point specified, use this object's position
        if (attackPoint == null)
        {
            attackPoint = transform;
        }
    }

    /// <summary>
    /// Call this from animation event or Update when in attack state
    /// </summary>
    public void PerformAttack()
    {
        if (!canAttack) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        // Detect raft in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, raftLayer);
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Raft"))
            {
                Health raftHealth = hit.GetComponent<Health>();
                if (raftHealth != null)
                {
                    raftHealth.TakeDamage(damage, attackPoint.position);
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    /// <summary>
    /// Alternative: Trigger damage on collision (for enemies that damage on touch)
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Raft"))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Health raftHealth = collision.gameObject.GetComponent<Health>();
                if (raftHealth != null)
                {
                    raftHealth.TakeDamage(damage, collision.contacts[0].point);
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }

    // Public methods for external control
    public void SetCanAttack(bool value) => canAttack = value;
    public void SetDamage(float newDamage) => damage = newDamage;
}