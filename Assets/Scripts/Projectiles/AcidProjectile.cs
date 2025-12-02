using UnityEngine;

/// <summary>
/// Acid/Glue projectile: slows and debuffs on hit.
/// Strength: disables/slows raft. Weakness: single-target, short range.
/// </summary>
public class AcidProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float slowDuration = 3f;
    [SerializeField] private float slowFactor = 0.5f;

    private Vector2 direction;
    private float lifetime = 10f;
    private Health targetHealth;

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
        lifetime -= Time.deltaTime;
        if (lifetime <= 0) Destroy(gameObject);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
    }

    public void SetDamage(float dmg) => damage = dmg;
    public void SetSpeed(float spd) => speed = spd;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Raft"))
        {
            targetHealth = col.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage, transform.position);
                
                // Apply slow status effect if StatusEffectManager exists
                var raft = GameServices.GetRaft();
                if (raft != null)
                {
                    var statusMgr = raft.GetComponent<StatusEffectManager>();
                    if (statusMgr != null)
                    {
                        statusMgr.ApplySlowEffect(slowFactor, slowDuration);
                    }
                }
            }
            Destroy(gameObject);
        }
    }
}
