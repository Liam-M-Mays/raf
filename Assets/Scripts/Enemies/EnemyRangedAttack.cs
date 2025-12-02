using UnityEngine;

/// <summary>
/// Add this to enemies that shoot projectiles at the raft
/// </summary>
public class EnemyRangedAttack : MonoBehaviour
{
    [SerializeField] private Animator leftArmAnimator;
    [SerializeField] private Animator rightArmAnimator;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [Tooltip("Optional list of fire points. If empty the component's transform is used.")]
    [SerializeField] private Transform[] firePoints;
    private int nextFireIndex = 0;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float projectileDamage = 15f;
    [SerializeField] private float projectileSpeed = 8f;
    
    [Header("Targeting")]
    [SerializeField] private float attackRange = 10f;
    
    private Transform raftTransform;
    private float lastFireTime = -999f;
    private bool canShoot = true;
    // Used to store the selected fire point when an animation will trigger the actual fire
    private Transform pendingFirePoint;

    void Start()
    {
        raftTransform = GameServices.GetRaft();

        // ensure there's at least one valid fire point
        if (firePoints == null || firePoints.Length == 0)
        {
            firePoints = new Transform[] { transform };
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
            // Prefer using arm animators if they exist, otherwise pick a firePoint
            Transform selected = null;
            if (leftArmAnimator != null && rightArmAnimator != null)
            {
                float dl = Vector2.Distance(leftArmAnimator.transform.position, raftTransform.position);
                float dr = Vector2.Distance(rightArmAnimator.transform.position, raftTransform.position);
                if (dl <= dr)
                {
                    leftArmAnimator.SetTrigger("Shoot");
                    selected = leftArmAnimator.transform;
                }
                else
                {
                    rightArmAnimator.SetTrigger("Shoot");
                    selected = rightArmAnimator.transform;
                }
            }
            else
            {
                // choose nearest firePoint to raft if available
                float bestDist = float.MaxValue;
                for (int i = 0; i < firePoints.Length; i++)
                {
                    if (firePoints[i] == null) continue;
                    float d = Vector2.Distance(firePoints[i].position, raftTransform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        selected = firePoints[i];
                    }
                }

                // fallback to cycling through points
                if (selected == null && firePoints.Length > 0)
                {
                    selected = firePoints[nextFireIndex % firePoints.Length];
                    nextFireIndex++;
                }
            }

            // set pending fire point and trigger fire via animation or directly
            pendingFirePoint = selected;
            if (selected != null)
            {
                // try to trigger animator on selected if present
                Animator a = selected.GetComponent<Animator>();
                if (a != null) a.SetTrigger("Shoot");
                else FireProjectile();
            }
            else
            {
                FireProjectile();
            }
        }
    }

    void LateUpdate()
    {
        if (raftTransform != null && raftTransform.position.x < transform.position.x)
        {
            // Face left
            if (transform.localScale.x > 0)
            {
                Vector3 scaler = transform.localScale;
                scaler.x *= -1f;
                transform.localScale = scaler;
            }
        }
        else if (raftTransform != null && raftTransform.position.x > transform.position.x)
        {
            // Face right
            if (transform.localScale.x < 0)
            {
                Vector3 scaler = transform.localScale;
                scaler.x *= -1f;
                transform.localScale = scaler;
            }
        }
    }

    public void FireProjectile()
    {
        if (projectilePrefab == null) return;
        if (raftTransform == null) return;

        Transform fp = (pendingFirePoint != null) ? pendingFirePoint : (firePoints != null && firePoints.Length > 0 ? firePoints[0] : transform);

        // Calculate direction to raft
        Vector2 direction = (raftTransform.position - fp.position).normalized;

        // Spawn projectile
        GameObject proj = Instantiate(projectilePrefab, fp.position, Quaternion.identity);
        EnemyProjectile projScript = proj.GetComponent<EnemyProjectile>();
        
        if (projScript != null)
        {
            projScript.SetDirection(direction);
            projScript.SetDamage(projectileDamage);
            projScript.SetSpeed(projectileSpeed);
        }
        
        lastFireTime = Time.time;
        // clear pending pointer after firing
        pendingFirePoint = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void SetCanShoot(bool value) => canShoot = value;
    public void SetDamage(float newDamage) => projectileDamage = newDamage;
}