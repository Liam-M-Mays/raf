using UnityEngine;
using System;

[CreateAssetMenu(menuName = "AI/States/TerritorialGuard")]
public class TerritorialGuard : State
{
    private Transform self, target;
    public float maxSpeed = 2; public float speed = 1;
    public float attackRange = 1; public float outOfRange = 10; public float respawnRange = 8;
    public float divergeRange = 2; public float divergeWeight = 1; public float territoryRadius = 12f; // Size of territory
    public float patrolSpeed = 0.3f; // Speed while patrolling
    private Animator anim;
    private LayerMask Shark;
    private MovementType currentMovementType;
    private Vector2 territoryCenter;
    private Vector2 patrolTarget;
    private bool playerInTerritory;

    private enum MovementType
    {
        Patrol,     // Patrol around territory
        Chase,      // Chase player if they enter territory
        Attack,     // Attack the player
        Return,     // Return to territory after player leaves
    }

    private float v;

    public TerritorialGuard() { }
    public override void OnEnter(Transform _self, Animator _anim)
    {
        Shark = LayerMask.GetMask("Shark");
        currentMovementType = MovementType.Patrol;
        self = _self;
        target = GameObject.FindGameObjectWithTag("Raft").transform;
        anim = _anim;
        v = maxSpeed;

        // Territory center is starting position
        territoryCenter = self.position;
        PickNewPatrolTarget();
    }

    public override void OnExit()
    {
    }

    public override void OnUpdate()
    {
        Vector2 targ;
        float dist;

        // Check if player is in territory
        playerInTerritory = Vector2.Distance(target.position, territoryCenter) < territoryRadius;

        switch (currentMovementType)
        {
            // ─────────────────────────── Patrol ──────────────────────────
            case MovementType.Patrol:
                anim.SetBool("Moving", true);

                // Slowly patrol within territory
                targ = applyDivergence(patrolTarget);
                dist = Vector2.Distance(self.position, targ);
                v = maxSpeed * patrolSpeed;
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < patrolTarget.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > patrolTarget.x && self.localScale.x > 0) { Flip(); }

                // Pick new patrol point when reached
                if (dist < 1f)
                {
                    PickNewPatrolTarget();
                }

                // Chase if player enters territory
                if (playerInTerritory)
                {
                    currentMovementType = MovementType.Chase;
                }
                break;

            // ─────────────────────────── Chase ───────────────────────────
            case MovementType.Chase:
                anim.SetBool("Moving", true);

                // Aggressively chase player
                targ = applyDivergence((Vector2)target.position);
                dist = Vector2.Distance(self.position, targ);
                v = Mathf.Clamp(dist * speed, 0f, maxSpeed * 1.2f); // Faster when defending
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < target.position.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > target.position.x && self.localScale.x > 0) { Flip(); }

                if (Vector2.Distance(self.position, target.position) < attackRange)
                {
                    currentMovementType = MovementType.Attack;
                    anim.SetBool("Moving", false);
                }

                // Return to patrol if player leaves territory
                if (!playerInTerritory)
                {
                    currentMovementType = MovementType.Return;
                }
                break;

            // ─────────────────────────── Attack ──────────────────────────
            case MovementType.Attack:
                anim.SetBool("Attacking", true);

                if (Vector2.Distance(self.position, target.position) > attackRange)
                {
                    if (playerInTerritory)
                    {
                        currentMovementType = MovementType.Chase;
                    }
                    else
                    {
                        currentMovementType = MovementType.Return;
                    }
                    anim.SetBool("Attacking", false);
                }
                break;

            // ─────────────────────────── Return ──────────────────────────
            case MovementType.Return:
                anim.SetBool("Moving", true);

                // Return to territory center
                targ = applyDivergence(territoryCenter);
                dist = Vector2.Distance(self.position, targ);
                v = Mathf.Clamp(dist * speed, 0f, maxSpeed);
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < territoryCenter.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > territoryCenter.x && self.localScale.x > 0) { Flip(); }

                // Resume patrol when back in territory
                if (dist < 3f)
                {
                    currentMovementType = MovementType.Patrol;
                    PickNewPatrolTarget();
                }

                // Chase again if player re-enters
                if (playerInTerritory)
                {
                    currentMovementType = MovementType.Chase;
                }
                break;
        }
    }

    public override void OnLateUpdate()
    {
    }

    private void PickNewPatrolTarget()
    {
        // Pick random point within territory
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float radius = UnityEngine.Random.Range(territoryRadius * 0.3f, territoryRadius * 0.7f);
        patrolTarget = territoryCenter + new Vector2(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius
        );
    }

    public void Flip()
    {
        Vector3 scaler = self.localScale;
        scaler.x *= -1f;
        self.localScale = scaler;
    }

    Vector2 applyDivergence(Vector2 original)
    {
        Vector2 newTarget = original;

        var hits = Physics2D.OverlapCircleAll(self.position, divergeRange, Shark);
        foreach (var h in hits)
        {
            if (h.transform == self) continue;

            Vector2 offset = (Vector2)(self.position - h.transform.position);
            float dist = offset.magnitude;

            if (offset.magnitude <= 0f || offset.magnitude >= divergeRange) continue;

            float fallOff = 1f - (offset.magnitude / divergeRange);
            fallOff *= fallOff;

            Vector2 pushAmount = offset * divergeWeight * fallOff;
            newTarget += pushAmount;
        }

        return newTarget;
    }
}