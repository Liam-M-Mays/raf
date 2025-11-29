using UnityEngine;
using UnityEngine.InputSystem;
public class EnemySpawner : MonoBehaviour
{
    public GameObject SharkPrefab; 
    public LiamSharkDefault sharkConfig;

    private Transform raft;
    public float spawnRadius = 15f;

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
        Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos = (Vector3)((Vector2)raft.position + offset);

        LiamsBasicEnemyMovement enemy = Instantiate(SharkPrefab, spawnPos, Quaternion.identity)
            .GetComponent<LiamsBasicEnemyMovement>();

        enemy.Configure(sharkConfig);
    }
}
