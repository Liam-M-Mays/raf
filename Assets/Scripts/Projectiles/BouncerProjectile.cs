using UnityEngine;

/// <summary>
/// Bouncer/Ricochet projectile: bounces off surfaces and entities.
/// Tracks bounces and reduces damage per bounce. Strength: multi-hit potential, clever angles. Weakness: unpredictable, easy to dodge.
/// </summary>
public class BouncerProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private float damage = 12f;
    [SerializeField] private int maxBounces = 5;
    [SerializeField] private float bounceDamageReduction = 0.2f;

    private Vector2 velocity;
    private int bounceCount = 0;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }
        else
        {
            transform.position += (Vector3)velocity * Time.deltaTime;
        }

        if (velocity.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, velocity);
        }
    }

    public void SetDirection(Vector2 dir)
    {
        velocity = dir.normalized * speed;
    }

    public void SetDamage(float dmg) => damage = dmg;
    public void SetSpeed(float spd) => speed = spd;

    void OnTriggerEnter2D(Collider2D col)
    {
        bool shouldBounce = !col.CompareTag("Raft");
        
        if (col.CompareTag("Raft"))
        {
            var health = col.GetComponent<Health>();
            if (health != null)
            {
                float dmg = damage * (1f - (bounceDamageReduction * bounceCount));
                health.TakeDamage(dmg, transform.position);
            }
        }

        if (shouldBounce && bounceCount < maxBounces)
        {
            // Reflect velocity
            Vector2 normal = (Vector2)(col.bounds.center - transform.position).normalized;
            velocity = Vector2.Reflect(velocity, normal);
            bounceCount++;
        }
        else if (!shouldBounce || bounceCount >= maxBounces)
        {
            Destroy(gameObject);
        }
    }
}
