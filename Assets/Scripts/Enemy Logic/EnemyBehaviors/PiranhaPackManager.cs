using UnityEngine;
using System;
using System.Collections.Generic;

#region Pack Manager - Singleton to coordinate all packs

public class PiranhaPackManager : MonoBehaviour
{
    public static PiranhaPackManager Instance { get; private set; }
    
    private Dictionary<int, PiranhaPackCoordinator> activePacks = new Dictionary<int, PiranhaPackCoordinator>();
    private int nextPackId = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public int RegisterPack(PiranhaPackCoordinator pack)
    {
        int id = nextPackId++;
        activePacks[id] = pack;
        return id;
    }
    
    public void UnregisterPack(int packId)
    {
        activePacks.Remove(packId);
    }
    
    public PiranhaPackCoordinator GetPack(int packId)
    {
        return activePacks.ContainsKey(packId) ? activePacks[packId] : null;
    }
}

#endregion
