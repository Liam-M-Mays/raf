using UnityEngine;

/// <summary>
/// Laser beam attack: continuous directional beam that deals continuous damage.
/// Uses LineRenderer for visualization and raycasts for detection.
/// Strength: consistent, precise damage. Weakness: can be blocked/reflected.
/// </summary>
public class EnemyLaserAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private float laserRange = 10f;
    [SerializeField] private float laserDamagePerSecond = 15f;
    [SerializeField] private float fireRate = 0.1f;

    private float lastDamageTime = -999f;

    void Start()
    {
        if (laserLine == null)
        {
            laserLine = GetComponent<LineRenderer>();
        }
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }
    }

    public void FireLaser(Vector2 direction)
    {
        Vector2 firePos = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        
        // Raycast in direction
        RaycastHit2D hit = Physics2D.Raycast(firePos, direction, laserRange);

        if (laserLine != null)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, firePos);
            if (hit.collider != null)
            {
                laserLine.SetPosition(1, hit.point);
                
                // Apply damage
                if (Time.time >= lastDamageTime + fireRate && hit.collider.CompareTag("Raft"))
                {
                    var health = hit.collider.GetComponent<Health>();
                    if (health != null)
                    {
                        health.TakeDamage(laserDamagePerSecond * fireRate, hit.point);
                    }
                    lastDamageTime = Time.time;
                }
            }
            else
            {
                laserLine.SetPosition(1, firePos + direction * laserRange);
            }
        }
    }

    public void StopLaser()
    {
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }
    }

    public void SetDamage(float newDamage) => laserDamagePerSecond = newDamage;
}
