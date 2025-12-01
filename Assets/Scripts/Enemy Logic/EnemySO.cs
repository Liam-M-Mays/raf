using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[System.Serializable]
public class BehaviorOptions
{
    [SerializeReference] public BehaviorOption SharkDefault = new BehaviorOption { config = new SharkDefaultCfg() };
    [SerializeReference] public BehaviorOption Orbit = new BehaviorOption { config = new RangeOrbitCfg() };
    [SerializeReference] public BehaviorOption HammerDefault = new BehaviorOption { config = new HammerDefaultCfg() };
    [SerializeReference] public BehaviorOption Direct = new BehaviorOption { config = new DirectCfg() };
    //[SerializeReference] public  BehaviorOption PiranaDefault = new BehaviorOption { config = new PiranhaPackCfg() };


    public IEnumerable<BehaviorOption> All()
    {
        return this.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(BehaviorOption))
            .Select(f => (BehaviorOption)f.GetValue(this));
    }
}

[CreateAssetMenu(menuName = "Enemies")]
public class EnemySO : ScriptableObject //TODO change name, add prefab, 
{
    [SerializeField] public BehaviorOptions Behaviors = new BehaviorOptions();
    public int cost;
    public int health;
    public int damage;
    public GameObject prefab;
    public float spawnRadius;
    public Color color = Color.white;


    public BehaviorCfg getBehavior()
    {
        List<(BehaviorCfg config, float weight)> output = new();
        foreach (var option in Behaviors.All())
        {
            if (option.enabled)
            {
                output.Add((option.config, option.weight));
            }
        }

        if (output == null || output.Count == 0) return null;
        BehaviorCfg movement = null;
        float totalWeight = output.Sum(x => x.weight);
        float rand = UnityEngine.Random.Range(0f, 1f);
        float chance = 0f;
        foreach (var i in output)
        {
            chance += i.weight / totalWeight;
            if (rand <= chance)
            {
                movement = i.config;
                Debug.Log("Choose " +  movement + " with a "+i.weight + " weight");
                break;
            }
        }
        if (movement == null) movement = output[0].config;

        return movement;
    }

}
