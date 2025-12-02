using UnityEngine;
using System;

[Serializable]
public class PackFormationCfg : BehaviorCfg
{
    [Min(0f)] public float radius = 3f;
    [Min(0f)] public float speed = 1.5f;
    [Range(0f,1f)] public float cohesion = 0.9f;
    [Min(0f)] public float rotationSpeed = 0.2f;

    [Header("Flocking")]
    [Min(0f)] public float neighborRadius = 2.5f;
    [Min(0f)] public float separationDistance = 0.5f;
    [Min(0f)] public float separationWeight = 1.2f;
    [Min(0f)] public float alignmentWeight = 0.5f;
    [Min(0f)] public float cohesionWeight = 0.6f;
    [Min(0f)] public float orbitWeight = 1.0f;

    [Header("Formation")]
    public bool useDeterministicSlots = true;
    [Min(0f)] public float slotArrivalDistance = 0.3f;

    [Header("Attack")]
    [Min(0f)] public float attackRange = 1.5f;
    [Min(0f)] public float attackCooldown = 2f;
    [Min(0f)] public float attackDamage = 5f; // Damage per attack (pack members do moderate damage)

    public override IBehavior CreateRuntimeBehavior() => new PackFormation(this);
}

public class PackFormation : IBehavior
{
    private PackFormationCfg cfg;
    private BehaviorContext ctx;
    private float angleOffset;
    private float attackCooldownTimer = 0f;

    public PackFormation(PackFormationCfg cfg)
    {
        this.cfg = cfg;
    }

    public BehaviorContext CTX() => ctx;

    public void OnEnter(Transform self, Animator anim)
    {
        var raft = GameServices.GetRaft();
        if (raft == null)
        {
            Debug.LogError("PackFormation: Raft not found.");
            return;
        }
        ctx = new BehaviorContext(self, raft, anim);

        // Unique offset per-instance for distribution
        angleOffset = (self.GetInstanceID() % 360) * Mathf.Deg2Rad;
        attackCooldownTimer = cfg.attackCooldown;
        // Speed variance for variety
        EnemySpeedVariance.ApplySpeedVariance(ctx, 0.10f);
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        if (ctx == null) return;
        ctx.UpdateFrame();
        // Update underwater state
        UnderwaterManager.UpdateUnderwaterState(ctx, ctx.underwaterState);
        // Tactical update
        var pm = GameServices.GetPlayerMovement();
        if (pm != null && pm.matressHealth != null)
        {
            ctx.tacticalDecision.UpdateTactic(ctx, pm.matressHealth.GetCurrentHealth(), pm.matressHealth.GetMaxHealth());
        }

        // Check if in attack range
        if (ctx.distanceToTarget <= cfg.attackRange)
        {
            // Try to attack if off cooldown
            attackCooldownTimer -= ctx.deltaTime;
            if (attackCooldownTimer <= 0f)
            {
                if (RaftTracker.addSelf(this))
                {
                    ctx.anim.SetTrigger("Attack");
                    DealDamageToRaft();
                    attackCooldownTimer = cfg.attackCooldown;
                }
            }
        }
        else
        {
            RaftTracker.removeSelf(this);
            attackCooldownTimer = cfg.attackCooldown;
        }

        // Boids-like flocking + orbiting around raft
        Vector3 pos = ctx.self.position;
        Vector3 raftPos = ctx.target.position;

        // Desired orbit position around raft with a small rotation over time
        float time = Time.time;
        float angle = angleOffset + time * cfg.rotationSpeed;
        Vector3 orbitPos = raftPos + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * cfg.radius;

        // Flocking: find neighbors within neighborRadius
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, cfg.neighborRadius);
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int neighborCount = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            var other = hits[i];
            if (other == null) continue;
            var ec = other.GetComponent<EnemyController>();
            if (ec == null || other.transform == ctx.self) continue;

            Vector3 toNeighbor = pos - other.transform.position;
            float dist = toNeighbor.magnitude;
            if (dist < 0.001f) continue;

            // Separation (avoid crowding)
            if (dist < cfg.separationDistance)
            {
                separation += (toNeighbor / dist) / dist; // weighted away
            }

            // Alignment & cohesion
            neighborCount++;
            alignment += other.transform.up; // approximate heading
            cohesion += other.transform.position;
        }

        if (neighborCount > 0)
        {
            alignment /= neighborCount;
            cohesion = (cohesion / neighborCount - pos);
        }

        // Combine orbit desire with flocking behaviors
        Vector3 toOrbit = (orbitPos - pos);
        Vector3 desiredDir = toOrbit.normalized * cfg.orbitWeight
                             + separation * cfg.separationWeight
                             + alignment * cfg.alignmentWeight
                             + cohesion * cfg.cohesionWeight;

        if (desiredDir.sqrMagnitude < 0.0001f) desiredDir = toOrbit.normalized;

        // Move with smoothing to avoid teleporting
        Vector3 velocity = desiredDir.normalized * cfg.speed;
        ctx.self.position += (Vector3)(velocity * Time.deltaTime);

        // Animation flags
        bool moving = velocity.magnitude > 0.01f;
        if (ctx.anim != null) ctx.anim.SetBool("Moving", moving);
    }

    public void OnLateUpdate() { }

    private void DealDamageToRaft()
    {
        var raftHealth = ctx.target.GetComponent<Health>();
        if (raftHealth != null && !raftHealth.IsDead())
        {
            raftHealth.TakeDamage(cfg.attackDamage, ctx.self.position);
        }
    }
}
