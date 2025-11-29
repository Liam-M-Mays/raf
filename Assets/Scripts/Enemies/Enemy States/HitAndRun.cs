using UnityEngine;
using System;

[CreateAssetMenu(menuName = "AI/States/HitAndRun")]
public class HitAndRun : State
{
    private Transform self, target;
    public float maxSpeed = 2; public float speed = 1;
    public float attackRange = 1; public float outOfRange = 10; public float respawnRange = 8;
    public float divergeRange = 2; public float divergeWeight = 1; public float approachDistance = 12f; // Distance to approach from
    public float retreatDistance = 15f; // Distance to retreat to
    public float strikeSpeed = 2.5f; // Speed multiplier during strike
    public float strikeCooldown = 2f; // Time between strikes
    private Animator anim;
    private LayerMask Shark;
    private MovementType currentMovementType;
    private float cooldownTimer;
    private Vector2 retreatPosition;

    private enum MovementType
    {
        Approach,   // Move to striking distance
        Strike,     // Quick dash attack
        Attack,     // Brief contact
        Retreat,    // Pull back quickly
        Wait,       // Circle at distance during cooldown
    }

    private float v;

    public HitAndRun() { }
    public override void OnEnter(Transform _self, Animator _anim)
    {
        Shark = LayerMask.GetMask("Shark");
        currentMovementType = MovementType.Approach;
        self = _self;
        target = GameObject.FindGameObjectWithTag("Raft").transform;
        anim = _anim;
        v = maxSpeed;
        cooldownTimer = 0f;
    }

    public override void OnExit()
    {
    }

    public override void OnUpdate()
    {
        Vector2 targ;
        float dist;

        switch (currentMovementType)
        {
            // ─────────────────────────── Approach ────────────────────────
            case MovementType.Approach:
                anim.SetBool("Moving", true);

                // Move to approach distance
                Vector2 dirToTarget = ((Vector2)target.position - (Vector2)self.position).normalized;
                Vector2 approachPos = (Vector2)target.position - dirToTarget * approachDistance;

                targ = applyDivergence(approachPos);
                dist = Vector2.Distance(self.position, targ);
                v = Mathf.Clamp(dist * speed, 0f, maxSpeed * 0.7f);
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < target.position.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > target.position.x && self.localScale.x > 0) { Flip(); }

                // Once in position, strike
                if (dist < 2f)
                {
                    currentMovementType = MovementType.Strike;
                }
                break;

            // ─────────────────────────── Strike ──────────────────────────
            case MovementType.Strike:
                anim.SetBool("Moving", true);

                // Fast strike toward player
                targ = applyDivergence((Vector2)target.position);
                dist = Vector2.Distance(self.position, targ);
                v = maxSpeed * strikeSpeed;
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < target.position.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > target.position.x && self.localScale.x > 0) { Flip(); }

                if (Vector2.Distance(self.position, target.position) < attackRange)
                {
                    currentMovementType = MovementType.Attack;
                    anim.SetBool("Moving", false);
                }
                break;

            // ─────────────────────────── Attack ──────────────────────────
            case MovementType.Attack:
                anim.SetBool("Attacking", true);

                // Brief attack, then immediately retreat
                if (Vector2.Distance(self.position, target.position) > attackRange * 0.8f)
                {
                    currentMovementType = MovementType.Retreat;
                    anim.SetBool("Attacking", false);
                    PickRetreatPosition();
                }
                break;

            // ─────────────────────────── Retreat ──────────────────────────
            case MovementType.Retreat:
                anim.SetBool("Moving", true);

                // Quickly move away
                targ = applyDivergence(retreatPosition);
                dist = Vector2.Distance(self.position, targ);
                v = maxSpeed * 1.2f; // Fast retreat
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < retreatPosition.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > retreatPosition.x && self.localScale.x > 0) { Flip(); }

                if (dist < 2f)
                {
                    currentMovementType = MovementType.Wait;
                    cooldownTimer = strikeCooldown;
                }
                break;

            // ─────────────────────────── Wait ────────────────────────────
            case MovementType.Wait:
                anim.SetBool("Moving", true);

                cooldownTimer -= Time.deltaTime;

                // Circle at distance while cooling down
                Vector2 currentOffset = (Vector2)self.position - (Vector2)target.position;
                float currentAngle = Mathf.Atan2(currentOffset.y, currentOffset.x);
                float circleRadius = Mathf.Max(retreatDistance, currentOffset.magnitude);

                float targetAngle = currentAngle + (maxSpeed / circleRadius) * 0.5f;
                Vector2 circlePos = (Vector2)target.position + new Vector2(
                    Mathf.Cos(targetAngle) * circleRadius,
                    Mathf.Sin(targetAngle) * circleRadius
                );

                targ = applyDivergence(circlePos);
                dist = Vector2.Distance(self.position, targ);
                v = Mathf.Clamp(dist * speed, 0f, maxSpeed * 0.5f);
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < target.position.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > target.position.x && self.localScale.x > 0) { Flip(); }

                if (cooldownTimer <= 0f)
                {
                    currentMovementType = MovementType.Approach;
                }
                break;
        }
    }

    public override void OnLateUpdate()
    {
    }

    private void PickRetreatPosition()
    {
        // Retreat to opposite side from current position
        Vector2 dirFromTarget = ((Vector2)self.position - (Vector2)target.position).normalized;
        retreatPosition = (Vector2)target.position + dirFromTarget * retreatDistance;
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