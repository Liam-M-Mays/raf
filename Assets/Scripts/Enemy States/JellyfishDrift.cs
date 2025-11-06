using UnityEngine;
using System;

[Serializable] public class JellyfishDrift : State
{
    private Transform self, target;
    public float maxSpeed = 2; public float speed  = 1;
    public float attackRange = 1; public float outOfRange = 10; public float respawnRange = 8;
    public float divergeRange = 2; public float divergeWeight = 1;    public float pulseStrength = 2f; // Strength of each pulse
    public float pulseInterval = 1.5f; // Time between pulses
    public float driftSpeed = 0.5f; // Speed of passive drift
    private Animator anim;
    private LayerMask Shark;
    private MovementType currentMovementType;
    private float pulseTimer;
    private Vector2 driftDirection;
    private bool isPulsing;
    
    private enum MovementType
    {
        Drift,      // Slowly drift with occasional pulses
        Pulse,      // Quick pulse movement
        Attack,     // Contact damage
    }

    private float v;

    public JellyfishDrift(){}
    public override void OnEnter(Transform _self, Animator _anim)
    {
        Shark = LayerMask.GetMask("Shark");
        currentMovementType = MovementType.Drift;
        self = _self;
        target = GameObject.FindGameObjectWithTag("Raft").transform;
        anim = _anim;
        v = maxSpeed;
        pulseTimer = pulseInterval;
        PickNewDriftDirection();
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
            // ─────────────────────────── Drift ───────────────────────────
            case MovementType.Drift:
                anim.SetBool("Moving", true);
                
                pulseTimer -= Time.deltaTime;
                
                // Slowly drift in current direction
                Vector2 driftTarget = (Vector2)self.position + driftDirection * 10f;
                targ = applyDivergence(driftTarget);
                v = driftSpeed;
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                // Occasionally change drift direction
                if (UnityEngine.Random.value < 0.01f)
                {
                    PickNewDriftDirection();
                }

                // No flipping - jellyfish don't need to face direction
                
                // Pulse toward player occasionally
                if (pulseTimer <= 0f)
                {
                    currentMovementType = MovementType.Pulse;
                    pulseTimer = pulseInterval;
                    isPulsing = true;
                }

                if (Vector2.Distance(self.position, target.position) < attackRange)
                {
                    currentMovementType = MovementType.Attack;
                    anim.SetBool("Moving", false);
                }
                break;

            // ─────────────────────────── Pulse ───────────────────────────
            case MovementType.Pulse:
                anim.SetBool("Moving", true);
                
                // Quick pulse toward player
                Vector2 dirToPlayer = ((Vector2)target.position - (Vector2)self.position).normalized;
                Vector2 pulseTarget = (Vector2)self.position + dirToPlayer * pulseStrength;
                
                targ = applyDivergence(pulseTarget);
                dist = Vector2.Distance(self.position, targ);
                v = maxSpeed;
                self.position = Vector2.MoveTowards(self.position, targ, v * Time.deltaTime);

                // Return to drift after short pulse
                if (dist < 0.5f || isPulsing && Time.frameCount % 30 == 0)
                {
                    currentMovementType = MovementType.Drift;
                    isPulsing = false;
                    PickNewDriftDirection();
                }

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
                    currentMovementType = MovementType.Drift;
                    anim.SetBool("Attacking", false);
                    PickNewDriftDirection();
                }
                break;
        }
    }
    
    public override void OnLateUpdate()
    {
    }

    private void PickNewDriftDirection()
    {
        // Bias drift direction toward player
        Vector2 toPlayer = ((Vector2)target.position - (Vector2)self.position).normalized;
        Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
        driftDirection = (toPlayer * 0.4f + randomDir * 0.6f).normalized;
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