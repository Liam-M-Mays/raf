using UnityEngine;
using System;
using System.Collections.Generic;
// State interface and machine
// Manages a single active behavior
public class BehaviorManager
{
    public IBehavior Current { get; private set; }

    public void ChangeBehavior(IBehavior next, Transform self, Animator anim)
    {
        if (Current == next) return;
        Current?.OnExit();
        Current = next;
        Current?.OnEnter(self, anim);
    }

    public void Tick()     => Current?.OnUpdate();
    public void LateTick() => Current?.OnLateUpdate();

    public void Exit() => Current?.OnExit();
}

public interface IBehavior
{
    void OnEnter(Transform self, Animator anim);
    void OnUpdate();
    void OnLateUpdate();
    void OnExit();
}


[Serializable]
public abstract class BehaviorCfg
{
    //public string label = "Behavior";
    public abstract IBehavior CreateRuntimeBehavior();
}

[Serializable]
public class BehaviorOption
{
    public bool enabled = false;
    
     public float weight = 1f;

    // Inline-polymorphic config
    [SerializeReference] public BehaviorCfg config;
}
