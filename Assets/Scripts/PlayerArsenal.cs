using UnityEngine;
using System.Collections.Generic;

public class PlayerArsenal : MonoBehaviour
{

    [SerializeField] private List<WeaponSO> playerGuns;
    [SerializeField] private List<WeaponSO> playerMelee;

    public GameObject weaponHolder;
    public GameObject meleeHolder;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddWeapon(WeaponSO newWeapon) {
        if (!newWeapon.isMelee) {
            playerGuns.Add(newWeapon);
        } else {
            playerMelee.Add(newWeapon);
        }
    }

    public void ProcessItem(ItemSO item) {
        // Process item effects here
    }
}
