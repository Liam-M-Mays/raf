using UnityEngine;
using System;

[Serializable]
public class BossCfg : BehaviorCfg
{
    [Header("Boss Health")]
    [Min(100f)] public float maxHealth = 500f;

    [Header("Movement")]
    [Min(0f)] public float phase1Speed = 1.5f;
    [Min(0f)] public float phase2Speed = 2.0f;
    [Min(0f)] public float phase3Speed = 2.5f;
    [Min(0f)] public float attackRange = 2f;

    [Header("Phase 1: Chomp")]
    [Min(0f)] public float chompCooldown = 3f;
    [Min(0f)] public float chompDamage = 30f;
    [Min(0f)] public float chompWindup = 0.5f;

    [Header("Phase 2: Bite")]
    [Min(0f)] public float biteCooldown = 4f;
    [Min(0f)] public float biteDamage = 50f;
    [Min(0f)] public float biteWindup = 1.0f;
    [Min(0f)] public float biteJumpHeight = 5f;

    [Header("Phase 3: Berserk")]
    [Min(0f)] public float berserkAttackSpeedMultiplier = 2f;
    [Min(0f)] public float berserkDamageMultiplier = 1.5f;

    public override IBehavior CreateRuntimeBehavior() => new Boss(this);
}

public class Boss : IBehavior
{
    private BossCfg cfg;
    private BehaviorContext ctx;
    private Health health;

    private enum BossPhase { Phase1, Phase2, Phase3 }
    private BossPhase currentPhase = BossPhase.Phase1;

    private float attackCooldown = 0f;
    private float windupTimer = 0f;
    private bool isWinding = false;

    public Boss(BossCfg cfg)
    {
        this.cfg = cfg;
    }

    public BehaviorContext CTX() => ctx;

    public void OnEnter(Transform self, Animator anim)
    {
        var raft = GameServices.GetRaft();
        if (raft == null)
        {
            Debug.LogError("Boss: Raft not found.");
            return;
        }
        ctx = new BehaviorContext(self, raft, anim);
        health = self.GetComponent<Health>();
        if (health != null) health.SetHealth(cfg.maxHealth);
        // Boss variation (small speed variance)
        EnemySpeedVariance.ApplyVarianceToConfig(ref cfg.phase1Speed, ref cfg.phase1Speed, 0.05f);
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        if (ctx == null) return;
        ctx.UpdateFrame();

        // Update phase based on health
        UpdatePhase();

        Vector3 dir = (ctx.target.position - ctx.self.position);
        float dist = dir.magnitude;
        Vector3 moveDir = dir.normalized;

        // Get current speed based on phase
        float currentSpeed = GetPhaseSpeed();

        // Approach raft
        if (dist > cfg.attackRange)
        {
            ctx.self.position += moveDir * currentSpeed * Time.deltaTime;
            ctx.anim.SetBool("Moving", true);
        }
        else
        {
            ctx.anim.SetBool("Moving", false);
            attackCooldown -= Time.deltaTime;

            if (attackCooldown <= 0f && !isWinding)
            {
                // Start attack windup
                isWinding = true;
                windupTimer = GetWindupTime();
                ctx.anim.SetBool("Charging", true);
            }
        }

        if (isWinding)
        {
            windupTimer -= Time.deltaTime;
            if (windupTimer <= 0f)
            {
                isWinding = false;
                ctx.anim.SetBool("Charging", false);
                PerformAttack(moveDir);
                attackCooldown = GetAttackCooldown();
            }
        }
    }

    public void OnLateUpdate() { }

    private void UpdatePhase()
    {
        if (health == null) return;
        float healthPercent = health.GetHealthPercentage();

        if (healthPercent <= 0.33f)
            currentPhase = BossPhase.Phase3;
        else if (healthPercent <= 0.66f)
            currentPhase = BossPhase.Phase2;
        else
            currentPhase = BossPhase.Phase1;
    }

    private float GetPhaseSpeed()
    {
        return currentPhase switch
        {
            BossPhase.Phase1 => cfg.phase1Speed,
            BossPhase.Phase2 => cfg.phase2Speed,
            BossPhase.Phase3 => cfg.phase3Speed,
            _ => cfg.phase1Speed
        };
    }

    private float GetAttackCooldown()
    {
        float baseCooldown = currentPhase switch
        {
            BossPhase.Phase1 => cfg.chompCooldown,
            BossPhase.Phase2 => cfg.biteCooldown,
            BossPhase.Phase3 => cfg.chompCooldown * 0.5f,
            _ => cfg.chompCooldown
        };

        if (currentPhase == BossPhase.Phase3)
            baseCooldown /= cfg.berserkAttackSpeedMultiplier;
        
        return baseCooldown;
    }

    private float GetWindupTime()
    {
        return currentPhase switch
        {
            BossPhase.Phase1 => cfg.chompWindup,
            BossPhase.Phase2 => cfg.biteWindup,
            BossPhase.Phase3 => cfg.biteWindup * 0.6f,
            _ => cfg.chompWindup
        };
    }

    private void PerformAttack(Vector3 direction)
    {
        if (currentPhase == BossPhase.Phase1)
        {
            ChompAttack(direction);
        }
        else if (currentPhase == BossPhase.Phase2 || currentPhase == BossPhase.Phase3)
        {
            BiteAttack(direction);
        }
    }

    private void ChompAttack(Vector3 direction)
    {
        // Left-right chomp
        float damage = cfg.chompDamage;
        if (currentPhase == BossPhase.Phase3)
            damage *= cfg.berserkDamageMultiplier;

        ctx.anim.SetTrigger("Attack");
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(ctx.self.position, cfg.attackRange);
        foreach (var col in hits)
        {
            if (col.CompareTag("Raft"))
            {
                var health = col.GetComponent<Health>();
                if (health != null) health.TakeDamage(damage, col.transform.position);
            }
        }
    }

    private void BiteAttack(Vector3 direction)
    {
        // Jump and bite
        float damage = cfg.biteDamage;
        if (currentPhase == BossPhase.Phase3)
            damage *= cfg.berserkDamageMultiplier;

        ctx.anim.SetTrigger("Bite");
        
        // Apply jump-like effect via velocity or animation
        Collider2D[] hits = Physics2D.OverlapCircleAll(ctx.self.position, cfg.attackRange * 1.5f);
        foreach (var col in hits)
        {
            if (col.CompareTag("Raft"))
            {
                var health = col.GetComponent<Health>();
                if (health != null) health.TakeDamage(damage, col.transform.position);
            }
        }
    }
}
