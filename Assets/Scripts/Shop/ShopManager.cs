using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    public WeaponSO[] weaponPool;
    public List<WeaponSO> availableWeaponPool; // Starter weapon items

    public ItemSO[] itemPool;
    public List<ItemSO> availableItemPool; // Starter shop items
    
    public Transform gunSpot1, gunSpot2, gunSpot3;
    public Transform itemSpot1, itemSpot2;
    public GameObject shopWeaponPrefab;
    public GameObject shopItemPrefab;

    private WaveManager waveManager;
    public PlayerMovement playerMovement;
    public WeaponManager weaponManager;

    public GameObject shopUI;

    public int weaponIndex = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waveManager = GetComponent<WaveManager>();
    }


    // Update is called once per frame
    void Update()
    {



    }

    public void PopulateShop() {
        shopUI.SetActive(true);
        UpdateWeaponPool();
        DestroyChildren(gunSpot1);
        DestroyChildren(gunSpot2);
        DestroyChildren(gunSpot3);
        DestroyChildren(itemSpot1);
        DestroyChildren(itemSpot2);
        ShuffleWeapons(availableWeaponPool);
        ShuffleItems(availableItemPool);

        if (availableWeaponPool.Count < 1) {
            Debug.LogWarning("Not enough weapons in the available pool to populate the shop.");
        } else if (availableWeaponPool.Count < 2) {
            CreateShopWeapon(availableWeaponPool[0], gunSpot1);
        } else if (availableWeaponPool.Count < 3) {
            CreateShopWeapon(availableWeaponPool[0], gunSpot1);
            CreateShopWeapon(availableWeaponPool[1], gunSpot2);
        } else {
            CreateShopWeapon(availableWeaponPool[0], gunSpot1);
            CreateShopWeapon(availableWeaponPool[1], gunSpot2);
            CreateShopWeapon(availableWeaponPool[2], gunSpot3);
        }
        

        if (availableItemPool.Count < 1) {
            Debug.LogWarning("Not enough items in the available pool to populate the shop.");
        } else if (availableItemPool.Count < 2) {
            CreateShopItem(availableItemPool[0], itemSpot1);
        } else {
            CreateShopItem(availableItemPool[0], itemSpot1);
            CreateShopItem(availableItemPool[1], itemSpot2);
        }
    }

    public void CreateShopWeapon(WeaponSO weapon, Transform spot) {
        GameObject item = Instantiate(shopWeaponPrefab, spot);
        ShopWeaponUI shopItemUI = item.GetComponent<ShopWeaponUI>();
        shopItemUI.Setup(weapon, this);
    }

    public void CreateShopItem(ItemSO item, Transform spot) {
        GameObject itemObj = Instantiate(shopItemPrefab, spot); 
        ShopItemUI shopItemUI = itemObj.GetComponent<ShopItemUI>();
        shopItemUI.Setup(item, this);
    }

    public void PurchaseWeapon(WeaponSO weapon, bool isMelee)
    {
        //weaponManager.availableWeapons.Add(weapon);
        if (playerMovement.PlayerCurrency >= weapon.cost)
        {
            playerMovement.PlayerCurrency -= weapon.cost;
             //playerInventory.Add(weapon);
             weaponManager.availableWeapons.Add(weapon);
             availableWeaponPool.Remove(weapon);
            // mark as purchased
            // remove from weaponPool
        }
        //availableWeaponPool.Remove(weapon);
    }

    public void PurchaseItem(ItemSO item)
    {
        if (playerMovement.PlayerCurrency < item.cost) return;
        playerMovement.PlayerCurrency -= item.cost;
        if (item.hasUpgrade) {
            availableItemPool.Add(item.upgradedItem);
        }
        if (!item.permanentShopItem) {
            availableItemPool.Remove(item);
        }
        if(item.Sheets) {
            playerMovement.Sheets = item;
        } else if(item.Frame) {
            playerMovement.Frame = item;
        } else if(item.Barbed) {
            playerMovement.Barbed = item;
        }
        else if(item.Paddle) {
            playerMovement.Paddle = item;
        }
        else if (item.plusHealth > 0) {
            playerMovement.raftRB.GetComponent<Health>().Heal(item.plusHealth);
        }
        //else if (item.plusAmmo > 0) {
        //    weaponManager.ReplenishAmmo(item.plusAmmo);
        //}
        //if (PlayerCurrency >= item.cost)
        //{
        //    PlayerCurrency -= item.cost;
            // playerInventory.Add(item);
            // mark as purchased
            // remove from itemPool
        //}
    }

    void ShuffleWeapons(List<WeaponSO> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            WeaponSO temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void ShuffleItems(List<ItemSO> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            ItemSO temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void DestroyChildren(Transform parent) {
        foreach (Transform child in parent) {
            GameObject.Destroy(child.gameObject);
        }
    }

    void UpdateWeaponPool() {
        if (waveManager == null) return;
        int currentWave = waveManager.GetWaveNumber();
            if (weaponIndex < weaponPool.Length) {
                availableWeaponPool.Add(weaponPool[weaponIndex]);
                weaponIndex++;
            }
    }

    public void ClosShop() {
        shopUI.SetActive(false);
    }

    public int GetPlayerCurrency() {
        return playerMovement.PlayerCurrency;
    }
}
