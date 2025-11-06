using UnityEngine;

public class LiamsBasicEnemyMovement : MonoBehaviour
{
    // ───────────────────────────────── Inspector ─────────────────────────────────
    [Header("State")]
    [SerializeField] private MovementType currentMovementType;

    [Header("References")]
    public Transform target;
    public LayerMask Shark;
    private Animator anim;

    [Header("Ranges")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float outOfRange = 10f;
    [SerializeField] private float respawnRange = 5f;

    [Header("Orbit (Circle)")]
    [SerializeField] private float circleMin = 5f;
    [SerializeField] private float circleMax = 10f;
    private float circleRange = 3f;

    [Header("Divergence (Separation)")]
    [SerializeField] private float divergeRange = 1f;
    [SerializeField] private float divergeWeight = 0.5f;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float chase = 0.5f;   // distance → speed factor
    [SerializeField] private float v = 5f;         // current frame speed (debug/inspect)

    // ───────────────────────────────── Types ────────────────────────────────────
    public enum MovementType
    {
        ChasePlayer, // Chase the player
        Attack,      // Attack the player
        OutOfRange,  // Despawn/respawn closer to the player
        Circle,      // Circle the player
    }

    // ─────────────────────────────── Unity Events ───────────────────────────────
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        Vector2 targ;
        float dist;

        switch (currentMovementType)
        {
            // ─────────────────────────── Chase ───────────────────────────
            case MovementType.ChasePlayer:
                anim.SetBool("Moving", true);

                targ = applyDivergence((Vector2)target.position);
                dist = Vector2.Distance(transform.position, targ);
                v = Mathf.Clamp(dist * chase, 0f, speed);
                transform.position = Vector2.MoveTowards(transform.position, targ, v * Time.deltaTime);

                if (transform.position.x < target.position.x && transform.localScale.x < 0) { Flip(); }
                else if (transform.position.x > target.position.x && transform.localScale.x > 0) { Flip(); }

                if (Vector2.Distance(transform.position, target.position) < attackRange)
                {
                    currentMovementType = MovementType.Attack;
                    anim.SetBool("Moving", false);
                }

                if (Vector2.Distance(transform.position, target.position) > outOfRange)
                {
                    currentMovementType = MovementType.OutOfRange;
                    anim.SetBool("Moving", false);
                }
                break;

            // ─────────────────────────── Attack ──────────────────────────
            case MovementType.Attack:
                anim.SetBool("Attacking", true);

                if (Vector2.Distance(transform.position, target.position) > attackRange)
                {
                    currentMovementType = MovementType.ChasePlayer;
                    anim.SetBool("Attacking", false);
                }
                break;

            // ───────────────────────── Out Of Range ──────────────────────
            case MovementType.OutOfRange:
                anim.SetTrigger("Respawn");
                transform.position = new Vector2(
                    Random.Range(target.position.x - respawnRange, target.position.x + respawnRange),
                    Random.Range(target.position.y - respawnRange, target.position.y + respawnRange)
                );
                currentMovementType = MovementType.ChasePlayer;
                break;

            // ─────────────────────────── Circle ──────────────────────────
            case MovementType.Circle:
                anim.SetBool("Moving", true);

                // Current offset from target + clamp orbit radius to [circleMin, circleMax]
                Vector2 currentOffset = (Vector2)transform.position - (Vector2)target.position;
                circleRange = Mathf.Min(Mathf.Max(circleMin, currentOffset.magnitude), circleMax);
                circleRange = Mathf.Max(circleMin, circleRange - 0.05f);

                // Where am I on the circle right now?
                float currentAngle = Mathf.Atan2(currentOffset.y, currentOffset.x);

                // Where should I be going (advance angle at speed/circleRange)?
                float rotationSpeed = speed / circleRange;
                float targetAngle = currentAngle + (rotationSpeed * 0.5f);

                // Compute target point on the circle
                Vector2 circlePos = (Vector2)target.position + new Vector2(
                    Mathf.Cos(targetAngle) * circleRange,
                    Mathf.Sin(targetAngle) * circleRange
                );

                // Apply divergence to that target
                targ = applyDivergence(circlePos);

                // Move toward the diverged target with distance-based speed
                dist = Vector2.Distance(transform.position, targ);
                v = Mathf.Clamp(dist * chase, 0f, speed);
                transform.position = Vector2.MoveTowards(transform.position, targ, v * Time.deltaTime);

                // Flip sprite to face target horizontally
                if (transform.position.x < target.position.x && transform.localScale.x < 0) { Flip(); }
                else if (transform.position.x > target.position.x && transform.localScale.x > 0) { Flip(); }

                if (Vector2.Distance(transform.position, target.position) < attackRange)
                {
                    currentMovementType = MovementType.Attack;
                    anim.SetBool("Moving", false);
                }
                break;
        }
    }

    // ──────────────────────────────── Helpers ──────────────────────────────────
    Vector2 applyDivergence(Vector2 original)
    {
        Vector2 newTarget = original;

        var hits = Physics2D.OverlapCircleAll(transform.position, divergeRange, Shark);
        foreach (var h in hits)
        {
            if (h.transform == transform) continue;

            Vector2 offset = (Vector2)(transform.position - h.transform.position);
            float dist = offset.magnitude;

            if (offset.magnitude <= 0f || offset.magnitude >= divergeRange) continue;

            float fallOff = 1f - (offset.magnitude / divergeRange);
            fallOff *= fallOff;

            Vector2 pushAmount = offset * divergeWeight * fallOff;
            newTarget += pushAmount;
        }

        return newTarget;
    }

    void Flip()
    {
        Vector3 scaler = transform.localScale;
        scaler.x *= -1f;
        transform.localScale = scaler;
    }

    void OnDrawGizmosSelected()
    {
        // Out-of-range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, outOfRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Divergence range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, divergeRange);

        // Circle range (current value)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, circleRange);
    }
}
