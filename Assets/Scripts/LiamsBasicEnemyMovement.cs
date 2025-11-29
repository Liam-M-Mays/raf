using UnityEngine;
using System;
public class LiamsBasicEnemyMovement : MonoBehaviour
{
    private Animator anim;
    private StateMachine sm = new StateMachine();
    public State moveState;


    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Configure(LiamSharkDefault runtimeState)
    {
        moveState = runtimeState.movementState.CreateRuntimeInstance();
    }
    
    void Start()
    {
        sm.ChangeState(moveState, transform, anim);
    }

    void Update()
    {
        sm.Tick();
    }

}