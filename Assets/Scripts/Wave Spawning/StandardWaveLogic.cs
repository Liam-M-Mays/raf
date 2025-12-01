using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StandardWaveLogic : MonoBehaviour
{

    [SerializeField] private EnemySO[] enemyPool;
    [SerializeField] private GameObject[] alive;

    [SerializeField] private bool standardWaveActive;
    [SerializeField] private int waveCurrency;
    [SerializeField] private float spawnRate;
    [SerializeField] private int maxEnemies;
    [SerializeField] private int minEnemies;

    private SpawnManager spawnManager;



    private WaveManager waveManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waveManager = GetComponent<WaveManager>();
        spawnManager = GetComponent<SpawnManager>();
    }



    public void StartWave(StandardWaveSO waveData, int waveCurrency) {
        if (standardWaveActive) {
            return;
        }
        this.waveCurrency = waveCurrency;
        enemyPool = waveData.enemyPool;
        spawnRate = waveData.spawnRate;
        maxEnemies = waveData.maxEnemies;
        minEnemies = waveData.minEnemies;

        standardWaveActive = true;
        Debug.Log(
            $"------------ Wave Configuration ------------\n" +
            $"Wave Currency: {waveCurrency}\n" +
            $"Enemy Pool: {enemyPool}\n" +
            $"Spawn Rate: {spawnRate}"
        );
        
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies() {
        Debug.Log("Start Spawning for Wave " + waveManager.waveNumber);
        while(waveCurrency > 0) {
            if (alive.Length >= maxEnemies) {
                while (alive.Length > minEnemies) {
                    yield return null;
                }
            }
            EnemySO enemy = PickEnemyToSpawn();
            waveCurrency-=enemy.cost;
            Debug.Log(
                $"------------ Enemy Spawn ------------\n" +
                $"Enemy: {enemy}\n" +
                $"Enemy Cost: {enemy.cost}\n" +
                $"Currency Left: {waveCurrency}"
            );

            spawnManager.Spawn(enemy);

            // wait before spawning the next enemy
            yield return new WaitForSeconds(spawnRate);
        }
    }

    // picks an enemy from the list
    public EnemySO PickEnemyToSpawn() {

        // add logic here to see if can afford anymore enemies
        // if no longer can pick enemy set wave currency to zero

        int num = Random.Range(0, enemyPool.Length);
        return enemyPool[num];
    }

    // Update is called once per frame
    void Update()
    {
        alive = GameObject.FindGameObjectsWithTag("Enemy");

        if (standardWaveActive && alive.Length <= 0 && waveCurrency <= 0) {
            waveManager.EndWave();
            standardWaveActive = false;
        }
    }

    
}
