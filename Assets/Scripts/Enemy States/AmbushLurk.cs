using UnityEngine;
using System;

[Serializable] public class AmbushLurk : State
{
    private Transform self, target;
    public float maxSpeed = 2; public float speed  = 1;
    public float attackRange = 1; public float outOfRange = 10; public float respawnRange = 8;
    public float divergeRange = 2; public float divergeWeight = 1;
    public float lurkDistance = 15f; // Distance to maintain while lurking
    public float lurkTime = 3f; // Time to lurk before attacking
    public float retreatDistance = 20f; // Distance to retreat after attack
    private Animator anim;
    private LayerMask Shark;
    private MovementType currentMovementType;
    private float lurkTimer;
    private Vector2 lurkPosition;
    
    private enum MovementType
    {
        Lurk,       // Stay at distance, slowly repositioning
        Charge,     // Fast direct attack
        Attack,     // Attack the player
        Retreat,    // Back away after attack
    }

    private float v;

    public AmbushLurk(){}
    public override void OnEnter(Transform _self, Animator _anim)
    {
        Shark = LayerMask.GetMask("Shark");
        currentMovementType = MovementType.Lurk;
        self = _self;
        target = GameObject.FindGameObjectWithTag("Raft").transform;
        anim = _anim;
        v = maxSpeed;
        lurkTimer = lurkTime;
        PickLurkPosition();
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
            // ─────────────────────────── Lurk ────────────────────────────
            case MovementType.Lurk:
                anim.SetBool("Moving", true);
                
                lurkTimer -= Time.deltaTime;
                
                // Move to lurk position slowly
                targ = applyDivergence(lurkPosition);
                dist = Vector2.Distance(self.position, targ);
                v = Mathf.Clamp(dist * speed * 0.5f, 0f, maxSpeed * 0.4f); // Slower while lurking
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < target.position.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > target.position.x && self.localScale.x > 0) { Flip(); }

                // Pick new lurk position occasionally
                if (dist < 2f || lurkTimer <= 0f)
                {
                    if (lurkTimer <= 0f)
                    {
                        currentMovementType = MovementType.Charge;
                        anim.SetBool("Moving", false);
                    }
                    else
                    {
                        PickLurkPosition();
                    }
                }
                break;

            // ─────────────────────────── Charge ──────────────────────────
            case MovementType.Charge:
                anim.SetBool("Moving", true);
                
                targ = applyDivergence((Vector2)target.position);
                dist = Vector2.Distance(self.position, targ);
                v = maxSpeed * 1.5f; // Extra fast charge
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
                    currentMovementType = MovementType.Retreat;
                    anim.SetBool("Attacking", false);
                }
                break;

            // ─────────────────────────── Retreat ─────────────────────────
            case MovementType.Retreat:
                anim.SetBool("Moving", true);
                
                // Move away from target
                Vector2 retreatDir = ((Vector2)self.position - (Vector2)target.position).normalized;
                Vector2 retreatTarget = (Vector2)target.position + retreatDir * retreatDistance;
                
                targ = applyDivergence(retreatTarget);
                dist = Vector2.Distance(self.position, targ);
                v = Mathf.Clamp(dist * speed, 0f, maxSpeed);
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < target.position.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > target.position.x && self.localScale.x > 0) { Flip(); }

                if (dist < 2f)
                {
                    currentMovementType = MovementType.Lurk;
                    lurkTimer = lurkTime;
                    PickLurkPosition();
                    anim.SetBool("Moving", false);
                }
                break;
        }
    }
    
    public override void OnLateUpdate()
    {
    }

    private void PickLurkPosition()
    {
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        lurkPosition = (Vector2)target.position + new Vector2(
            Mathf.Cos(angle) * lurkDistance,
            Mathf.Sin(angle) * lurkDistance
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