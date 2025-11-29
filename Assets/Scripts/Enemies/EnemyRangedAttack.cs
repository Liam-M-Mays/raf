using UnityEngine;

/// <summary>
/// Add this to enemies that shoot projectiles at the raft
/// </summary>
public class EnemyRangedAttack : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float projectileDamage = 15f;
    [SerializeField] private float projectileSpeed = 8f;
    
    [Header("Targeting")]
    [SerializeField] private float attackRange = 10f;
    
    private Transform raftTransform;
    private float lastFireTime = -999f;
    private bool canShoot = true;

    void Start()
    {
        GameObject raft = GameObject.FindGameObjectWithTag("Raft");
        if (raft != null)
        {
            raftTransform = raft.transform;
        }
        
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    void Update()
    {
        if (!canShoot) return;
        if (raftTransform == null) return;
        
        // Check if raft is in range
        float distanceToRaft = Vector2.Distance(transform.position, raftTransform.position);
        
        if (distanceToRaft <= attackRange && Time.time >= lastFireTime + fireRate)
        {
            FireProjectile();
        }
    }

    void FireProjectile()
    {
        if (projectilePrefab == null) return;
        
        // Calculate direction to raft
        Vector2 direction = (raftTransform.position - firePoint.position).normalized;
        
        // Spawn projectile
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        EnemyProjectile projScript = proj.GetComponent<EnemyProjectile>();
        
        if (projScript != null)
        {
            projScript.SetDirection(direction);
            projScript.SetDamage(projectileDamage);
            projScript.SetSpeed(projectileSpeed);
        }
        
        lastFireTime = Time.time;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void SetCanShoot(bool value) => canShoot = value;
}