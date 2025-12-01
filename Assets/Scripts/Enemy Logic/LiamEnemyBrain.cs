using UnityEngine;
using System;
public class LiamEnemyBrain : MonoBehaviour //TODO change name, add decision matrix, dynamic weight
{
    public BehaviorManager manager = new BehaviorManager();
    private Transform target;
    private SpriteRenderer spriteRenderer;
    private int cost;

    public void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        target = GameObject.FindGameObjectWithTag("Raft").transform;
    }

    public void Configure(EnemySO logic)
    {
        cost = logic.cost;
        var behavior = logic.getBehavior().CreateRuntimeBehavior();
        manager.ChangeBehavior(behavior, transform, GetComponent<Animator>());
        spriteRenderer.color = logic.color;
        GetComponent<Health>().SetHealth(logic.health);
        if(TryGetComponent<EnemyMeleeAttack>(out var attack))
        {
            attack.SetDamage(logic.damage);
        }
        else if (TryGetComponent<EnemyRangedAttack>(out var rangedAttack))
        {
            rangedAttack.SetDamage(logic.damage);
        }
    }
    void Update()
    {
        manager.Tick();
        if (transform.position.y > target.position.y)
        {
            spriteRenderer.sortingOrder = target.GetComponentInChildren<SpriteRenderer>().sortingOrder - 1;
        }
        else
        {
            spriteRenderer.sortingOrder = target.GetComponentInChildren<SpriteRenderer>().sortingOrder + 1;
        }
    } 

    void LateUpdate() => manager.LateTick();

    public IBehavior currentBehavior()
    {
        return manager.Current;
    }

    void OnDestroy() 
    {
        manager.Exit();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().PlayerCurrency += cost;
    }
}
