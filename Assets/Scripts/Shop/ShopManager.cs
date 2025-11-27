using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public WeaponSO[] weaponPool;
    public WeaponSO[] availableWeaponPool; // Stater shop weapons

    public ItemSO[] itemPool;
    public ItemSO[] availableItemPool; // Starter shop items
    
    public Transform gunSpot1, gunSpot2, gunSpot3;
    public Transform itemSpot1, itemSpot2;
    public GameObject shopItemPrefab;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PopulateShop();
    }


    // Update is called once per frame
    void Update()
    {
        // add weapons to available wepons pool every few waves or if pool is less than 3
        if (availableWeaponPool.Length < 3)
        {
            // Add more weapons to the available pool
        }
    }

    public void PopulateShop() {
        Debug.Log("Populate");
        Shuffle(availableWeaponPool);
        Debug.Log("Shuffled");

        if (availableWeaponPool.Length < 1) {
            Debug.LogWarning("Not enough weapons in the available pool to populate the shop.");
            return;
        } else if (availableWeaponPool.Length < 2) {
            CreateShopWeapon(availableWeaponPool[0], gunSpot1);
            return;
        } else if (availableWeaponPool.Length < 3) {
            CreateShopWeapon(availableWeaponPool[0], gunSpot1);
            CreateShopWeapon(availableWeaponPool[1], gunSpot2);
            return;
        }
        CreateShopWeapon(availableWeaponPool[0], gunSpot1);
        CreateShopWeapon(availableWeaponPool[1], gunSpot2);
        CreateShopWeapon(availableWeaponPool[2], gunSpot3);

        if (availableItemPool.Length < 1) {
            Debug.LogWarning("Not enough items in the available pool to populate the shop.");
            return;
        } else if (availableItemPool.Length < 2) {
            CreateShopItem(availableItemPool[0], itemSpot1);
            return;
        }

        CreateShopItem(availableItemPool[0], itemSpot1);
        CreateShopItem(availableItemPool[1], itemSpot2);
    }

    public void CreateShopWeapon(WeaponSO weapon, Transform spot) {
        GameObject item = Instantiate(shopItemPrefab, spot);
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
        //if (PlayerCurrency >= weapon.cost)
        //{
        //    PlayerCurrency -= weapon.cost;
            // playerInventory.Add(weapon);
            // mark as purchased
            // remove from weaponPool
        //}
    }

    public void PurchaseItem(ItemSO item)
    {
        //if (PlayerCurrency >= item.cost)
        //{
        //    PlayerCurrency -= item.cost;
            // playerInventory.Add(item);
            // mark as purchased
            // remove from itemPool
        //}
    }

    void Shuffle(WeaponSO[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            WeaponSO temp = list[i];
            int randomIndex = Random.Range(i, list.Length);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
