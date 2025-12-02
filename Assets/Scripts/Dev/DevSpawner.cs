using UnityEngine;

/// <summary>
/// Simple runtime spawner for development. Assign an `EnemySO` and press `Insert` to spawn.
/// Optionally set `spawnCount` to spawn multiple.
/// </summary>
public class DevSpawner : MonoBehaviour
{
    public KeyCode spawnKey = KeyCode.Insert;
    public EnemySO enemyToSpawn;
    public int spawnCount = 1;

    private SpawnManager spawnManager;

    void Start()
    {
        // Use FindAnyObjectByType where available to avoid deprecation warnings
        spawnManager = UnityEngine.Object.FindAnyObjectByType<SpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogWarning("DevSpawner: No SpawnManager found in scene.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            DoSpawn();
        }
    }

    public void DoSpawn()
    {
        if (enemyToSpawn == null)
        {
            Debug.LogWarning("DevSpawner: No EnemySO assigned.");
            return;
        }

        if (spawnManager == null)
        {
            spawnManager = UnityEngine.Object.FindAnyObjectByType<SpawnManager>();
            if (spawnManager == null)
            {
                Debug.LogError("DevSpawner: No SpawnManager available to spawn enemies.");
                return;
            }
        }

        for (int i = 0; i < Mathf.Max(1, spawnCount); i++)
        {
            spawnManager.Spawn(enemyToSpawn);
        }
    }
}
