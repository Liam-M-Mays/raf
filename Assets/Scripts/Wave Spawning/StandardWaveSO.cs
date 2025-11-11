using UnityEngine;

[CreateAssetMenu(menuName = "Waves/StandardWave")]
public class StandardWaveSO : ScriptableObject
{
    public EnemySO[] enemyPool;
    public float spawnRate;
    public float maxEnemies;
}

