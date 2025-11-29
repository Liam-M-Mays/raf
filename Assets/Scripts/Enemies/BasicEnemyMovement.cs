using UnityEngine;

public class BasicEnemyMovement : MonoBehaviour
{

    [SerializeField] private MovementType currentMovementType;

    private Transform playerTransform;
    private Animator anim;
    private EnemyMeleeAttack meleeAttack; // NEW: Reference to melee attack component

    [SerializeField] private float respawnRange = 5f;
    [SerializeField] private float outOfRange = 10f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float speed = 5f;

    public enum MovementType
    {
        ChasePlayer, // Chase the player
        Attack, // Attack the player
        OutOfRange, // State when the player is out of range (Despawn and respawn closer to the player)
    }


    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        anim = GetComponent<Animator>();
        meleeAttack = GetComponent<EnemyMeleeAttack>(); // NEW: Get melee attack component
    }


    

    void Update()
    {
        switch (currentMovementType)
        {
            case MovementType.ChasePlayer:
                // Chase logic
                anim.SetBool("Moving", true);
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, speed * Time.deltaTime);
                if (transform.position.x < playerTransform.position.x && transform.localScale.x < 0)
                {
                    Flip();
                }
                else if (transform.position.x > playerTransform.position.x && transform.localScale.x > 0)
                {
                    Flip();
                }
                if (Vector2.Distance(transform.position, playerTransform.position) < attackRange)
                {
                    currentMovementType = MovementType.Attack;
                    anim.SetBool("Moving", false);
                }
                if (Vector2.Distance(transform.position, playerTransform.position) > outOfRange)
                {
                    currentMovementType = MovementType.OutOfRange;
                    anim.SetBool("Moving", false);
                }
                break;
            case MovementType.Attack:
                // Attack logic
                anim.SetBool("Attacking", true);
                
                // NEW: Perform melee attack if component exists
                if (meleeAttack != null)
                {
                    meleeAttack.PerformAttack();
                }
                
                if (Vector2.Distance(transform.position, playerTransform.position) > attackRange)
                {
                    currentMovementType = MovementType.ChasePlayer;
                    anim.SetBool("Attacking", false);
                }
                break;
            case MovementType.OutOfRange:
                // Respawn logic
                anim.SetTrigger("Respawn");
                transform.position = new Vector2(Random.Range(playerTransform.position.x - respawnRange, playerTransform.position.x + respawnRange), Random.Range(playerTransform.position.y - respawnRange, playerTransform.position.y + respawnRange));
                currentMovementType = MovementType.ChasePlayer;
                break;
        }
    }


    void Flip()
    {
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void OnDrawGizmosSelected()
    {
        // Draw out of range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, outOfRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}