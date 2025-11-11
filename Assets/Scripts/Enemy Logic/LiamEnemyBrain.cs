using UnityEngine;
using System;
public class LiamEnemyBrain : MonoBehaviour //TODO change name, add decision matrix, dynamic weight
{
    public BehaviorManager manager = new BehaviorManager();

    public void Configure(EnemySO logic)
    {
        var behavior = logic.getBehavior().CreateRuntimeBehavior();
        manager.ChangeBehavior(behavior, transform, GetComponent<Animator>());
    }
    void Update() => manager.Tick();
    void LateUpdate() => manager.LateTick();

    void OnDestroy() => manager.Exit();
}
