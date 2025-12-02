using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class WaveManager : MonoBehaviour
{
    private bool winScreenActive = false;
    public GameObject winScreen;
    [Header("Wave Settings")]
    public int currencyMultiplier = 5;
    public int bossFrequency = 10;
    //public float timeBetweenSpawns = 1f;
    
    public StandardWaveSO[] standardWavePool;
    //public BossWaveSO[] bossWavePool;


    //public EnemySO[] enemies;
    //public GameObject[] alive;

    [Header("Wave Updates")]
    public bool waveActive = false;
    public int waveNumber = 0;
    public int waveCurrency = 0;
    public Wave waveType;


    private StandardWaveLogic standardWaveLogic;
    private MasonManager masonManager;
    //private BossWaveLogic bossWaveLogic;
    

    void Start() {
        standardWaveLogic = GetComponent<StandardWaveLogic>();
        masonManager = GetComponent<MasonManager>();
        //bossWaveLogic = GetComponent<BossWaveLogic>();
    }

    public void StartWave() {
        if (waveActive) {
            return;
        }
        Debug.Log("Wave Manager -> Start Wave " + waveNumber);
        waveActive = true;
        waveCurrency = waveNumber*currencyMultiplier + currencyMultiplier;

        waveType = DetermineWave();
        switch(waveType) {

            case Wave.standard:
                // pick a wave from wavePool
                // call the standard wave spawner
                Debug.Log("Wave " + waveNumber + " is Standard");
                if (waveNumber < standardWavePool.Length) {
                    standardWaveLogic.StartWave(standardWavePool[waveNumber], waveCurrency);
                } else {
                    standardWaveLogic.StartWave(standardWavePool[15], waveCurrency);
                }
                break;

            case Wave.boss:
                // pick a wave
                // call the boss wave spawner
                Debug.Log("Wave " + waveNumber + " is A Boss");
                EndWave();
                break;
            
        }

    }

    public enum Wave {
        standard,
        boss,
        special
    }

    public Wave DetermineWave() {
            return Wave.standard;
    }

    public void EndWave() {
        if (!waveActive) {
            return;
        }
        waveActive = false;
        Debug.Log("Wave Manager -> End Wave " + waveNumber);
        waveNumber++;
        // Heal the player between waves (25% of max health)
        var player = GameServices.GetPlayerMovement();
        if (player != null) player.HealBetweenWaves(0.25f);

        masonManager.StartShop();
        
    }

    void Update() {
        if (InputSystem.actions.FindAction("Jump").triggered) {
            StartWave();
        }

        if (waveNumber == 15 && !winScreenActive) {
            winScreen.SetActive(true);
            winScreenActive = true;
        }
    }

    public int GetWaveNumber() {
        return waveNumber;
    }

    public bool IsWaveActive() {
        return waveActive;
    }

    public void CloseWin() {
        winScreen.SetActive(false);
    }
}
