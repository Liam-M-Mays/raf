using UnityEngine;
using System;
public class LiamEnemyBrain : MonoBehaviour //TODO change name, add decision matrix, dynamic weight
{
    public BehaviorManager manager = new BehaviorManager();
    private Transform target;
    private SpriteRenderer spriteRenderer;

    public void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        target = GameObject.FindGameObjectWithTag("Raft").transform;
    }

    public void Configure(EnemySO logic)
    {
        var behavior = logic.getBehavior().CreateRuntimeBehavior();
        manager.ChangeBehavior(behavior, transform, GetComponent<Animator>());
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

    void OnDestroy() => manager.Exit();
}
