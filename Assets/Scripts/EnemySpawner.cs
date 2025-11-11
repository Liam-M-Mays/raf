using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;
public class EnemySpawner : MonoBehaviour
{
    public EnemySO config;
    private Transform raft;
    private InputAction spawn;

    void Start()
    {
        raft = GameObject.FindGameObjectWithTag("Raft").transform;
        spawn = InputSystem.actions.FindAction("Jump");
    }

    void Update()
    {
        if (spawn.triggered)
        {
            SpawnShark();
        }
    }

   

    public void SpawnShark() 
    {
        
        Vector2 offset = Random.insideUnitCircle.normalized * config.spawnRadius;
        Vector3 spawnPos = raft.position + (Vector3)offset;

        LiamEnemyBrain enemy = Instantiate(config.prefab, spawnPos, Quaternion.identity)
            .GetComponent<LiamEnemyBrain>();

        enemy.Configure(config);
    }

}