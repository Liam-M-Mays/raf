using UnityEngine;
using System;

[Serializable] public class ZigzagApproach : State
{
    private Transform self, target;
    public float maxSpeed = 2; public float speed  = 1;
    public float attackRange = 1; public float outOfRange = 10; public float respawnRange = 8;
    public float divergeRange = 2; public float divergeWeight = 1;    public float zigzagAmplitude = 5f; // How wide the zigzag
    public float zigzagFrequency = 2f; // How fast it zigzags
    private Animator anim;
    private LayerMask Shark;
    private MovementType currentMovementType;
    private float zigzagTimer;
    private int zigzagDirection = 1;
    
    private enum MovementType
    {
        Zigzag,     // Zigzag toward player
        Attack,     // Attack the player
    }

    private float v;

    public ZigzagApproach(){}
    public override void OnEnter(Transform _self, Animator _anim)
    {
        Shark = LayerMask.GetMask("Shark");
        currentMovementType = MovementType.Zigzag;
        self = _self;
        target = GameObject.FindGameObjectWithTag("Raft").transform;
        anim = _anim;
        v = maxSpeed;
        zigzagTimer = 0f;
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
            // ─────────────────────────── Zigzag ──────────────────────────
            case MovementType.Zigzag:
                anim.SetBool("Moving", true);
                
                zigzagTimer += Time.deltaTime * zigzagFrequency;
                
                // Calculate direction to target
                Vector2 directionToTarget = ((Vector2)target.position - (Vector2)self.position).normalized;
                
                // Calculate perpendicular direction for zigzag
                Vector2 perpendicular = new Vector2(-directionToTarget.y, directionToTarget.x);
                
                // Apply zigzag offset using sine wave
                float zigzagOffset = Mathf.Sin(zigzagTimer) * zigzagAmplitude;
                Vector2 zigzagTarget = (Vector2)target.position + perpendicular * zigzagOffset;
                
                targ = applyDivergence(zigzagTarget);
                dist = Vector2.Distance(self.position, targ);
                v = Mathf.Clamp(dist * speed, 0f, maxSpeed);
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
                    currentMovementType = MovementType.Zigzag;
                    anim.SetBool("Attacking", false);
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