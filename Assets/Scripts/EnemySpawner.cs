/*using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;
public class EnemySpawner : MonoBehaviour
{
    public EnemySO one;
    public EnemySO two;
    public EnemySO three;
    public EnemySO four;
    public EnemySO five;

    private Transform raft;
    
    private InputAction first;
    private InputAction second;
    private InputAction third;
    private InputAction fourth;
    private InputAction fith;

    void Start()
    {
        raft = GameObject.FindGameObjectWithTag("Raft").transform;
        first = InputSystem.actions.FindAction("one");
        second = InputSystem.actions.FindAction("two");
        third = InputSystem.actions.FindAction("three");
        fourth = InputSystem.actions.FindAction("four");
        fith = InputSystem.actions.FindAction("five");
    }

    void Update()
    {
        if (first.triggered)
        {
            SpawnShark(one);
        }
        if (second.triggered)
        {
            SpawnShark(two);
        }
        if (third.triggered)
        {
            SpawnShark(three);
        }
        if (fourth.triggered)
        {
            SpawnShark(four);
        }
        if (fith.triggered)
        {
            SpawnShark(five);
        }
    }


    public void SpawnShark(EnemySO config) 
    {
        
        Vector2 offset = Random.insideUnitCircle.normalized * config.spawnRadius;
        Vector3 spawnPos = raft.position + (Vector3)offset;

        LiamEnemyBrain enemy = Instantiate(config.prefab, spawnPos, Quaternion.identity)
            .GetComponent<LiamEnemyBrain>();

        enemy.Configure(config);
    }

}*/