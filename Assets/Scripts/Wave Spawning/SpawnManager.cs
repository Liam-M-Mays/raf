using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private Transform raft;

    void Start()
    {
        raft = GameServices.GetRaft();
        if (raft == null)
        {
            Debug.LogWarning("SpawnManager: Raft not found via GameServices. Spawns will use Vector2.zero as fallback.");
        }
    }

    public void Spawn(EnemySO enemy)
    {
        Vector2 center = raft != null ? (Vector2)raft.position : Vector2.zero;
        Vector2 offset = Random.insideUnitCircle.normalized * enemy.spawnRadius;
        Vector3 spawnPos = (Vector3)(center + offset);

        if (enemy.prefab != null)
        {
            var go = Instantiate(enemy.prefab, spawnPos, Quaternion.identity);
            var enemyBrain = go.GetComponent<EnemyController>();
            if (enemyBrain == null)
            {
                // Try legacy name
                enemyBrain = go.GetComponent<LiamEnemyBrain>();
            }

            if (enemyBrain != null)
            {
                enemyBrain.Configure(enemy);
            }
            else
            {
                Debug.LogWarning("SpawnManager: Spawned prefab does not contain an EnemyController. Creating minimal enemy.");
                CreateMinimalEnemy(spawnPos, enemy);
            }
        }
        else
        {
            CreateMinimalEnemy(spawnPos, enemy);
        }
    }

    private void CreateMinimalEnemy(Vector3 spawnPos, EnemySO enemy)
    {
        GameObject go = new GameObject("Enemy_Minimal");
        go.transform.position = spawnPos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = enemy.color;
        var ec = go.AddComponent<EnemyController>();
        var health = go.AddComponent<Health>();
        health.SetHealth(enemy.health);
        // Simple visual - a default square
        // Configure through EnemyController using a temporary SO
        ec.Configure(enemy);
    }
}
