using UnityEngine;
using System;

[CreateAssetMenu(menuName = "AI/States/DirectChase")]
public class DirectChase : State
{
    private Transform self, target;
    public float maxSpeed = 2; public float speed = 1;
    public float attackRange = 1; public float outOfRange = 10; public float respawnRange = 8;
    public float divergeRange = 2; public float divergeWeight = 1; private Animator anim;
    private LayerMask Shark;
    private MovementType currentMovementType;
    private enum MovementType
    {
        ChasePlayer, // Chase the player
        Attack, // Attack the player
        OutOfRange, // State when the player is out of range (Despawn and respawn closer to the player)
    }

    private float v;

    public DirectChase() { }
    public override void OnEnter(Transform _self, Animator _anim)
    {
        Shark = LayerMask.GetMask("Shark");
        currentMovementType = MovementType.ChasePlayer;
        self = _self;
        target = GameObject.FindGameObjectWithTag("Raft").transform;
        anim = _anim;
        v = maxSpeed;
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
            // ─────────────────────────── Chase ───────────────────────────
            case MovementType.ChasePlayer:
                anim.SetBool("Moving", true);

                targ = applyDivergence((Vector2)target.position);
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

                /*if (Vector2.Distance(self.position, target.position) > outOfRange)
                {
                    currentMovementType = MovementType.OutOfRange;
                    anim.SetBool("Moving", false);
                }*/
                break;

            // ─────────────────────────── Attack ──────────────────────────
            case MovementType.Attack:
                anim.SetBool("Attacking", true);

                if (Vector2.Distance(self.position, target.position) > attackRange)
                {
                    currentMovementType = MovementType.ChasePlayer;
                    anim.SetBool("Attacking", false);
                }
                break;

            // ───────────────────────── Out Of Range ──────────────────────
            case MovementType.OutOfRange:
                anim.SetTrigger("Respawn");
                self.position = new Vector2(
                    UnityEngine.Random.Range(target.position.x - respawnRange, target.position.x + respawnRange),
                    UnityEngine.Random.Range(target.position.y - respawnRange, target.position.y + respawnRange)
                );
                currentMovementType = MovementType.ChasePlayer;
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
