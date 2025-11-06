using UnityEngine;
using System;

[Serializable] public class SpiralDive : State
{
    private Transform self, target;
    public float maxSpeed = 2; public float speed  = 1;
    public float attackRange = 1; public float outOfRange = 10; public float respawnRange = 8;
    public float divergeRange = 2; public float divergeWeight = 1;    public float spiralRadius = 8f;
    public float spiralSpeed = 3f;
    public float diveSpeed = 1.5f;
    private Animator anim;
    private LayerMask Shark;
    private MovementType currentMovementType;
    private float spiralAngle;
    private float currentRadius;
    
    private enum MovementType
    {
        Spiral,     // Spiral inward toward player
        Dive,       // Final dive attack
        Attack,     // Attack the player
        Reset,      // Move back out to reset spiral
    }

    private float v;

    public SpiralDive(){}
    public override void OnEnter(Transform _self, Animator _anim)
    {
        Shark = LayerMask.GetMask("Shark");
        currentMovementType = MovementType.Spiral;
        self = _self;
        target = GameObject.FindGameObjectWithTag("Raft").transform;
        anim = _anim;
        v = maxSpeed;
        
        // Start at current angle relative to target
        Vector2 offset = (Vector2)self.position - (Vector2)target.position;
        spiralAngle = Mathf.Atan2(offset.y, offset.x);
        currentRadius = Mathf.Max(spiralRadius, offset.magnitude);
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
            // ─────────────────────────── Spiral ──────────────────────────
            case MovementType.Spiral:
                anim.SetBool("Moving", true);
                
                // Increment angle to spiral
                spiralAngle += spiralSpeed * Time.deltaTime;
                
                // Gradually decrease radius to spiral inward
                currentRadius = Mathf.Max(attackRange + 1f, currentRadius - diveSpeed * Time.deltaTime);
                
                // Calculate position on spiral
                Vector2 spiralPos = (Vector2)target.position + new Vector2(
                    Mathf.Cos(spiralAngle) * currentRadius,
                    Mathf.Sin(spiralAngle) * currentRadius
                );
                
                targ = applyDivergence(spiralPos);
                dist = Vector2.Distance(self.position, targ);
                v = Mathf.Clamp(dist * speed, 0f, maxSpeed);
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < target.position.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > target.position.x && self.localScale.x > 0) { Flip(); }

                // When close enough, dive
                if (currentRadius <= attackRange + 1f)
                {
                    currentMovementType = MovementType.Dive;
                }
                break;

            // ─────────────────────────── Dive ────────────────────────────
            case MovementType.Dive:
                anim.SetBool("Moving", true);
                
                // Fast dive directly at player
                targ = applyDivergence((Vector2)target.position);
                dist = Vector2.Distance(self.position, targ);
                v = maxSpeed * 2f; // Very fast dive
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

                if (Vector2.Distance(self.position, target.position) > attackRange)
                {
                    currentMovementType = MovementType.Reset;
                    anim.SetBool("Attacking", false);
                }
                break;

            // ─────────────────────────── Reset ───────────────────────────
            case MovementType.Reset:
                anim.SetBool("Moving", true);
                
                // Move back out to spiral starting distance
                Vector2 resetOffset = ((Vector2)self.position - (Vector2)target.position).normalized;
                Vector2 resetPos = (Vector2)target.position + resetOffset * spiralRadius;
                
                targ = applyDivergence(resetPos);
                dist = Vector2.Distance(self.position, targ);
                v = Mathf.Clamp(dist * speed, 0f, maxSpeed);
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < target.position.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > target.position.x && self.localScale.x > 0) { Flip(); }

                if (dist < 2f)
                {
                    currentMovementType = MovementType.Spiral;
                    currentRadius = spiralRadius;
                    Vector2 newOffset = (Vector2)self.position - (Vector2)target.position;
                    spiralAngle = Mathf.Atan2(newOffset.y, newOffset.x);
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