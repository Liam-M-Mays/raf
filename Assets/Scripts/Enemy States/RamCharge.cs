using UnityEngine;
using System;

[Serializable] public class RamCharge : State
{
    private Transform self, target;
    public float maxSpeed = 2; public float speed  = 1;
    public float attackRange = 1; public float outOfRange = 10; public float respawnRange = 8;
    public float divergeRange = 2; public float divergeWeight = 1;    public float chargeDistance = 15f; // How far back to position before charging
    public float chargeWindupTime = 1f; // Time to wait before charging
    public float chargeCooldown = 3f; // Time between charges
    private Animator anim;
    private LayerMask Shark;
    private MovementType currentMovementType;
    private float chargeTimer;
    private Vector2 chargeDirection;
    private Vector2 chargeStartPos;
    
    private enum MovementType
    {
        Position,   // Move to charging position
        Windup,     // Brief pause before charge
        Charge,     // Fast ram charge
        Attack,     // Attack on impact
        Cooldown,   // Circle while waiting for next charge
    }

    private float v;
    private float windupTimer;
    private float cooldownTimer;

    public RamCharge(){}
    public override void OnEnter(Transform _self, Animator _anim)
    {
        Shark = LayerMask.GetMask("Shark");
        currentMovementType = MovementType.Position;
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
            // ─────────────────────────── Position ────────────────────────
            case MovementType.Position:
                anim.SetBool("Moving", true);
                
                // Move to position behind current location relative to target
                Vector2 dirToTarget = ((Vector2)target.position - (Vector2)self.position).normalized;
                Vector2 positionTarget = (Vector2)target.position - dirToTarget * chargeDistance;
                
                targ = applyDivergence(positionTarget);
                dist = Vector2.Distance(self.position, targ);
                v = Mathf.Clamp(dist * speed, 0f, maxSpeed * 0.8f);
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < target.position.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > target.position.x && self.localScale.x > 0) { Flip(); }

                if (dist < 2f)
                {
                    currentMovementType = MovementType.Windup;
                    windupTimer = chargeWindupTime;
                    chargeDirection = ((Vector2)target.position - (Vector2)self.position).normalized;
                    chargeStartPos = self.position;
                    anim.SetBool("Moving", false);
                }
                break;

            // ─────────────────────────── Windup ──────────────────────────
            case MovementType.Windup:
                windupTimer -= Time.deltaTime;
                
                // Stay still during windup
                // Could add a visual tell here
                
                if (windupTimer <= 0f)
                {
                    currentMovementType = MovementType.Charge;
                    // Recalculate direction at charge time
                    chargeDirection = ((Vector2)target.position - (Vector2)self.position).normalized;
                }
                break;

            // ─────────────────────────── Charge ──────────────────────────
            case MovementType.Charge:
                anim.SetBool("Moving", true);
                
                // Charge in straight line
                Vector2 chargeTarget = (Vector2)self.position + chargeDirection * 100f;
                
                v = maxSpeed * 2.5f; // Very fast charge
                self.position = Vector2.MoveTowards(self.position, chargeTarget, v * Time.deltaTime);

                if (self.position.x < target.position.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > target.position.x && self.localScale.x > 0) { Flip(); }

                // Stop charge if hit player or traveled too far
                if (Vector2.Distance(self.position, target.position) < attackRange)
                {
                    currentMovementType = MovementType.Attack;
                    anim.SetBool("Moving", false);
                }
                else if (Vector2.Distance(self.position, chargeStartPos) > chargeDistance * 2f)
                {
                    currentMovementType = MovementType.Cooldown;
                    cooldownTimer = chargeCooldown;
                    anim.SetBool("Moving", false);
                }
                break;

            // ─────────────────────────── Attack ──────────────────────────
            case MovementType.Attack:
                anim.SetBool("Attacking", true);

                if (Vector2.Distance(self.position, target.position) > attackRange)
                {
                    currentMovementType = MovementType.Cooldown;
                    cooldownTimer = chargeCooldown;
                    anim.SetBool("Attacking", false);
                }
                break;

            // ─────────────────────────── Cooldown ────────────────────────
            case MovementType.Cooldown:
                anim.SetBool("Moving", true);
                
                cooldownTimer -= Time.deltaTime;
                
                // Circle around player during cooldown
                Vector2 currentOffset = (Vector2)self.position - (Vector2)target.position;
                float currentAngle = Mathf.Atan2(currentOffset.y, currentOffset.x);
                float circleRadius = Mathf.Max(10f, currentOffset.magnitude);
                
                float targetAngle = currentAngle + (maxSpeed / circleRadius) * 0.3f;
                Vector2 circlePos = (Vector2)target.position + new Vector2(
                    Mathf.Cos(targetAngle) * circleRadius,
                    Mathf.Sin(targetAngle) * circleRadius
                );
                
                targ = applyDivergence(circlePos);
                dist = Vector2.Distance(self.position, targ);
                v = Mathf.Clamp(dist * speed, 0f, maxSpeed * 0.6f);
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < target.position.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > target.position.x && self.localScale.x > 0) { Flip(); }

                if (cooldownTimer <= 0f)
                {
                    currentMovementType = MovementType.Position;
                }
                break;
        }
    }
    
    public override void OnLateUpdate()
    {
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