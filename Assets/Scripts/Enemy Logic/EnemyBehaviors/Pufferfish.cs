using UnityEngine;
using System;

[Serializable]
public class PufferfishCfg : BehaviorCfg
{
    [Min(0f)] public float explosionRadius = 3f;
    [Min(0f)] public float explosionDamage = 50f;
    public bool explodeOnDamage = true;
    public bool explodeOnDeath = true;

    public override IBehavior CreateRuntimeBehavior() => new Pufferfish(this);
}

public class Pufferfish : IBehavior
{
    private PufferfishCfg cfg;
    private BehaviorContext ctx;

    public Pufferfish(PufferfishCfg cfg)
    {
        this.cfg = cfg;
    }

    public BehaviorContext CTX() => ctx;

    public void OnEnter(Transform self, Animator anim)
    {
        Transform raftTarget = GameServices.GetRaft();
        if (raftTarget == null)
        {
            Debug.LogError("Pufferfish: Could not find Raft. Behavior will be disabled.");
            return;
        }

        ctx = new BehaviorContext(self, raftTarget, anim);

        // Subscribe to health events if present
        if (ctx.health != null)
        {
            if (cfg.explodeOnDamage) ctx.health.OnDamaged.AddListener(OnDamaged);
            if (cfg.explodeOnDeath) ctx.health.OnDeath.AddListener(OnDeath);
        }
    }

    public void OnExit()
    {
        if (ctx?.health != null)
        {
            ctx.health.OnDamaged.RemoveListener(OnDamaged);
            ctx.health.OnDeath.RemoveListener(OnDeath);
        }
    }

    private void OnDamaged(float amt)
    {
        Explode();
    }

    private void OnDeath()
    {
        Explode();
    }

    private void Explode()
    {
        if (ctx == null || ctx.self == null) return;
        ExplosionUtility.ApplyExplosion(ctx.self.position, cfg.explosionRadius, cfg.explosionDamage);
        GameObject.Destroy(ctx.self.gameObject);
    }

    public void OnUpdate()
    {
        // Passive; Pufferfish waits until damaged or dead to explode
    }

    public void OnLateUpdate() { }
}
