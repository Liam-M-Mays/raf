using UnityEngine;
using System;

[Serializable]
public class TankCfg : BehaviorCfg
{
    [Min(0f)] public float maxSpeed = 1.5f;
    [Min(0f)] public float speed = 1f;
    [Min(0f)] public float attackRange = 1.2f;
    [Min(0f)] public float attackDamage = 8f;
    [Min(0f)] public float attackCooldown = 1.5f; // Time between attacks
    public bool canCharge = true;
    [Min(0f)] public float chargeSpeed = 3f;
    [Min(0f)] public float chargeWindup = 0.8f;
    [Min(0f)] public float chargeCooldown = 4f;
    [Min(0f)] public float chargeImpactForce = 5f; // Force applied to raft on impact

    public override IBehavior CreateRuntimeBehavior() => new Tank(this);
}

public class Tank : IBehavior
{
    private TankCfg cfg;
    private BehaviorContext ctx;

    private enum State { Approach, Windup, Charging, Recover }
    private State state = State.Approach;
    private float cooldownTimer = 0f;
    private float windupTimer = 0f;
    private float attackCooldownTimer = 0f;

    public Tank(TankCfg cfg)
    {
        this.cfg = cfg;
    }

    public BehaviorContext CTX() => ctx;

    public void OnEnter(Transform self, Animator anim)
    {
        var raftTarget = GameServices.GetRaft();
        if (raftTarget == null)
        {
            Debug.LogError("Tank: Raft not found.");
            return;
        }
        ctx = new BehaviorContext(self, raftTarget, anim);
        ctx.maxSpeed = cfg.maxSpeed;
        ctx.speed = cfg.speed;
        ctx.attackRange = cfg.attackRange;
        cooldownTimer = cfg.chargeCooldown;
        // Per-instance speed variance
        EnemySpeedVariance.ApplySpeedVariance(ctx, 0.12f);
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        if (ctx == null) return;
        ctx.UpdateFrame();
        // Update underwater cycle if applicable
        UnderwaterManager.UpdateUnderwaterState(ctx, ctx.underwaterState);
        // Tactical update
        var pm = GameServices.GetPlayerMovement();
        if (pm != null && pm.matressHealth != null)
        {
            ctx.tacticalDecision.UpdateTactic(ctx, pm.matressHealth.GetCurrentHealth(), pm.matressHealth.GetMaxHealth());
        }

        Vector3 dir = (ctx.target.position - ctx.self.position);
        float dist = dir.magnitude;
        Vector3 moveDir = dir.normalized;

        // Decrement attack cooldown
        attackCooldownTimer -= ctx.deltaTime;

        switch (state)
        {
            case State.Approach:
                // Move toward target slowly
                ctx.self.position += (Vector3)(moveDir * cfg.speed * Time.deltaTime);
                ctx.anim.SetBool("Moving", true);
                
                // Attack if in range AND cooldown is ready
                if (dist <= cfg.attackRange && attackCooldownTimer <= 0f)
                {
                    ctx.anim.SetTrigger("Attack");
                    DealDamageToRaft();
                    attackCooldownTimer = cfg.attackCooldown;
                }

                // Charge decision: only if not attacking frequently
                if (cfg.canCharge && cooldownTimer <= 0f && dist < cfg.attackRange * 4f && attackCooldownTimer < cfg.attackCooldown * 0.5f)
                {
                    state = State.Windup;
                    windupTimer = cfg.chargeWindup;
                    AnimatorUtils.SafeSetBool(ctx.anim, "Moving", false);
                    AnimatorUtils.SafeSetBool(ctx.anim, "Lurk", true);
                }
                break;

            case State.Windup:
                windupTimer -= ctx.deltaTime;
                if (windupTimer <= 0f)
                {
                    state = State.Charging;
                    AnimatorUtils.SafeSetBool(ctx.anim, "Lurk", false);
                    AnimatorUtils.SafeSetBool(ctx.anim, "Moving", true);
                }
                break;

            case State.Charging:
                // Fast dash toward player
                ctx.self.position += (Vector3)(moveDir * cfg.chargeSpeed * Time.deltaTime);
                // End charge if close or overshot
                if (dist <= 1f)
                {
                    state = State.Recover;
                    cooldownTimer = cfg.chargeCooldown;
                    ctx.anim.SetBool("Moving", false);
                    ctx.anim.SetBool("Dazed", true);
                    
                    // Apply impact force to raft
                    var raftMovement = ctx.target.GetComponent<PlayerMovement>();
                    if (raftMovement != null)
                    {
                        raftMovement.ApplyRaftImpact(moveDir, cfg.chargeImpactForce);
                    }
                    
                    // Deal extra damage on charge
                    DealDamageToRaft(cfg.attackDamage * 1.5f);
                    attackCooldownTimer = cfg.attackCooldown;
                }
                break;

            case State.Recover:
                cooldownTimer -= ctx.deltaTime;
                if (cooldownTimer <= 0f)
                {
                    state = State.Approach;
                    ctx.anim.SetBool("Dazed", false);
                }
                break;
        }
    }

    private void DealDamageToRaft(float damageOverride = -1f)
    {
        float damage = damageOverride >= 0f ? damageOverride : cfg.attackDamage;
        var raftHealth = ctx.target.GetComponent<Health>();
        if (raftHealth != null && !raftHealth.IsDead())
        {
            raftHealth.TakeDamage(damage, ctx.self.position);
        }
    }

    public void OnLateUpdate() { }
}
