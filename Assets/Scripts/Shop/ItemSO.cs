using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class ItemSO : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public int cost;
    public Sprite itemSprite;
    public string description;

    [Header("Stat Bonuses")]
    public int maxHealthBonus;
    public int healthOverTimeBonus;
    public int damageBonus;
    public int barbedDamageBonus;
    public int plusAmmo;
    public int plusHealth;

    [Header("Effects")]
    public int weightEffect;
    public int speedEffect;
    public Sprite upgradeSprite;
    public bool Sheets;
    public bool Frame;
    public bool Barbed;

    public bool hasUpgrade;
    public ItemSO upgradedItem;

    public bool permanentShopItem;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
