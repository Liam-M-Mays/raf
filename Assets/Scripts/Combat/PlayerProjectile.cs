using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float maxRange = 15f;
    
    [Header("Visual")]
    [SerializeField] private GameObject hitEffectPrefab;
    
    private Vector2 direction;
    private Vector3 startPosition;
    private float timer = 0f;
    private float knockbackForce = 0f; // Applied to enemies on hit
    private bool initialized = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (!initialized)
        {
            Debug.LogWarning("Bullet not initialized yet!");
            return;
        }
        
        // Move projectile
        Vector3 movement = (Vector3)direction * speed * Time.deltaTime;
        transform.position += movement;
        
        // Check if exceeded max range
        if (Vector3.Distance(startPosition, transform.position) >= maxRange)
        {
            Destroy(gameObject);
            return;
        }
        
        // Destroy after lifetime
        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(Vector2 dir, float projSpeed, float projDamage, float projRange, float projKnockback = 0f)
    {
        direction = dir.normalized;
        speed = projSpeed;
        damage = projDamage;
        maxRange = projRange;
        knockbackForce = projKnockback;
        initialized = true;
        
        // Rotate to face direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        Debug.Log($"Bullet INITIALIZED! Direction: {direction}, Speed: {speed}");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore raft and player collisions
        if (collision.CompareTag("Raft") || collision.CompareTag("Player"))
        {
            return;
        }
        
        if (collision.CompareTag("Enemy"))
        {
            LiamEnemyBrain brain = collision.GetComponent<LiamEnemyBrain>();
            bool hittable = false;
            if (brain != null && brain.manager != null && brain.manager.Current != null)
            {
                var bctx = brain.manager.Current.CTX();
                hittable = (bctx != null) ? bctx.hittable : false;
            }
            if (hittable)
            {
                // Enemy is HITTABLE - deal damage and destroy projectile
                Debug.Log("Bullet hit HITTABLE enemy!");
            
                Health enemyHealth = collision.GetComponent<Health>();
                if (enemyHealth != null && !enemyHealth.IsDead())
                {
                    enemyHealth.TakeDamage(damage, transform.position);
                }
                
                // Apply knockback to enemy if force is set
                if (knockbackForce > 0f)
                {
                    PlayerMovement.ApplyEnemyKnockback(collision.transform, direction, knockbackForce);
                }
                
                SpawnHitEffect();
                Destroy(gameObject);
            }
            // If NOT hittable (underwater/non-hittable), projectile PASSES THROUGH (no destroy)
        }
    }

    void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    // Getters for ProjectileTypeConfig
    public float GetDamage() => damage;
    public float GetSpeed() => speed;
    public float GetRange() => maxRange;
}
