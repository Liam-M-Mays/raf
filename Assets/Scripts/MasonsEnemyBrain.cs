using UnityEngine;
using System;
public class MasonsEnemyBrain : MonoBehaviour
{
    // Brain that takes the scriptable object and does stuff with it
    private Animator anim;
    // State machine is used to swap states during runtime
    private StateMachine sm = new StateMachine();
    
    public State moveState;


    // As soon as its instantiated
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Configure(EnemySO runtimeState)
    {
        // called from the spawn manager
        // happens before start
        moveState = runtimeState.movementState.CreateRuntimeInstance();
    }
    
    void Start()
    {
        // the enemy state machine assigns the state based on the selected state on the enemies scriptable object
        sm.ChangeState(moveState, transform, anim);
    }

    void Update()
    {
        // an function of the state machine that calls the update for the current state
        // when the brain updates, the states "psuedo-update" OnUpdate method is called
        sm.Tick();
    }

}