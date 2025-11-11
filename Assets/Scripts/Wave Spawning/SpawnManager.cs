using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private Transform raft;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        raft = GameObject.FindGameObjectWithTag("Raft").transform;
    }

    public void Spawn(EnemySO enemy) {
        Vector2 offset = Random.insideUnitCircle.normalized * enemy.spawnRadius;
        Vector3 spawnPos = (Vector3)((Vector2)raft.position + offset);

        MasonsEnemyBrain enemyBrain = Instantiate(enemy.prefab, spawnPos, Quaternion.identity)
            .GetComponent<MasonsEnemyBrain>();

        enemyBrain.Configure(enemy);
    }
}
