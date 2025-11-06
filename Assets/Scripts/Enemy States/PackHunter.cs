using UnityEngine;
using System;

[Serializable] public class PackHunter : State
{
    private Transform self, target;
    public float maxSpeed = 2; public float speed  = 1;
    public float attackRange = 1; public float outOfRange = 10; public float respawnRange = 8;
    public float divergeRange = 2; public float divergeWeight = 1;    public float packDistance = 8f; // Preferred distance from pack center
    public float formationSpacing = 3f; // Space between pack members
    private Animator anim;
    private LayerMask Shark;
    private MovementType currentMovementType;
    private Vector2 packCenter;
    private float angleInPack;
    
    private enum MovementType
    {
        FormUp,     // Get into formation
        PackChase,  // Chase as a coordinated group
        Attack,     // Attack the player
    }

    private float v;

    public PackHunter(){}
    public override void OnEnter(Transform _self, Animator _anim)
    {
        Shark = LayerMask.GetMask("Shark");
        currentMovementType = MovementType.FormUp;
        self = _self;
        target = GameObject.FindGameObjectWithTag("Raft").transform;
        anim = _anim;
        v = maxSpeed;
        
        // Assign a position in the pack based on instance ID
        angleInPack = (self.GetInstanceID() % 8) * (Mathf.PI * 2f / 8f);
    }
    
    public override void OnExit()
    {
    }
    
    public override void OnUpdate()
    {
        Vector2 targ;
        float dist;
        
        CalculatePackCenter();

        switch (currentMovementType)
        {
            // ─────────────────────────── Form Up ─────────────────────────
            case MovementType.FormUp:
                anim.SetBool("Moving", true);
                
                // Calculate formation position
                Vector2 formationPos = packCenter + new Vector2(
                    Mathf.Cos(angleInPack) * formationSpacing,
                    Mathf.Sin(angleInPack) * formationSpacing
                );
                
                targ = applyDivergence(formationPos);
                dist = Vector2.Distance(self.position, targ);
                v = Mathf.Clamp(dist * speed, 0f, maxSpeed * 0.7f);
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < target.position.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > target.position.x && self.localScale.x > 0) { Flip(); }

                // Once in formation, start pack chase
                if (dist < 1f)
                {
                    currentMovementType = MovementType.PackChase;
                }
                break;

            // ─────────────────────────── Pack Chase ──────────────────────
            case MovementType.PackChase:
                anim.SetBool("Moving", true);
                
                // Move toward player while maintaining formation
                Vector2 dirToTarget = ((Vector2)target.position - packCenter).normalized;
                Vector2 packFormationPos = packCenter + new Vector2(
                    Mathf.Cos(angleInPack) * formationSpacing,
                    Mathf.Sin(angleInPack) * formationSpacing
                );
                
                // Blend between formation position and moving toward target
                Vector2 chaseTarget = Vector2.Lerp(packFormationPos, (Vector2)target.position, 0.3f);
                
                targ = applyDivergence(chaseTarget);
                dist = Vector2.Distance(self.position, targ);
                v = Mathf.Clamp(dist * speed, 0f, maxSpeed);
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                if (self.position.x < target.position.x && self.localScale.x < 0) { Flip(); }
                else if (self.position.x > target.position.x && self.localScale.x > 0) { Flip(); }

                // Slowly rotate pack formation
                angleInPack += Time.deltaTime * 0.5f;

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
                    currentMovementType = MovementType.PackChase;
                    anim.SetBool("Attacking", false);
                }
                break;
        }
    }
    
    public override void OnLateUpdate()
    {
    }

    private void CalculatePackCenter()
    {
        // Find center point of all pack members
        var hits = Physics2D.OverlapCircleAll(self.position, packDistance * 2f, Shark);
        if (hits.Length <= 1)
        {
            packCenter = (Vector2)self.position;
            return;
        }
        
        Vector2 sum = Vector2.zero;
        int count = 0;
        foreach (var h in hits)
        {
            sum += (Vector2)h.transform.position;
            count++;
        }
        
        packCenter = sum / count;
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