using UnityEngine;
using System;
// State interface and machine
public class StateMachine
{
    public State current { get; private set; }
    public void ChangeState(State next, Transform self, Animator anim)
    {
        if (current == next) return;
        current?.OnExit();
        current = next;
        current?.OnEnter(self, anim);
    }
    public void Tick() => current?.OnUpdate(); 
    public void LateTick() => current?.OnLateUpdate(); 
}


[Serializable]
public abstract class State
{
    public abstract void OnEnter(Transform _self, Animator _anim);
    public abstract void OnUpdate();
    public abstract void OnLateUpdate();
    public abstract void OnExit();
}

public class SubclassSelectorAttribute : PropertyAttribute {}
