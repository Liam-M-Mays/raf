using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 5f;
    
    [Header("Visual")]
    [SerializeField] private GameObject hitEffectPrefab;
    
    private Vector2 direction;
    private float timer = 0f;

    void Update()
    {
        // Move projectile
        transform.position += (Vector3)direction * speed * Time.deltaTime;
        
        // Destroy after lifetime
        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        
        // Rotate sprite to face direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Hit the raft
        if (collision.CompareTag("Raft"))
        {
            Health raftHealth = collision.GetComponent<Health>();
            if (raftHealth != null)
            {
                raftHealth.TakeDamage(damage, transform.position);
            }
            
            SpawnHitEffect();
            Destroy(gameObject);
        }
        // If you want bullets to be destroyed by ANY collision (not just raft):
        // Uncomment the lines below:
        /*
        else
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
        */
    }

    void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    public void SetDamage(float newDamage) => damage = newDamage;
    public void SetSpeed(float newSpeed) => speed = newSpeed;
}