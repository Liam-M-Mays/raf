using UnityEngine;
using System;
public class LiamsBasicEnemyMovement : MonoBehaviour
{
    private Animator anim;
    private StateMachine sm = new StateMachine();
    [SerializeReference, SubclassSelector] public State moveState;

    void Start()
    {
        anim = GetComponent<Animator>();
        sm.ChangeState(moveState, transform, anim);
    }

    void Update()
    {
        sm.Tick();
    }

}