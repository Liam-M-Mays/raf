using UnityEngine;

public class WeaponUpgradeExample : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRangedWeapon rangedWeapon;
    [SerializeField] private PlayerMeleeWeapon meleeWeapon;
    
    [Header("Ranged Upgrades")]
    [SerializeField] private KeyCode increaseDamageKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode increaseFireRateKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode addBulletKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode increaseSpreadKey = KeyCode.Alpha4;
    
    [Header("Melee Upgrades")]
    [SerializeField] private KeyCode increaseMeleeDamageKey = KeyCode.Alpha5;
    [SerializeField] private KeyCode increaseMeleeRangeKey = KeyCode.Alpha6;
    [SerializeField] private KeyCode increaseMeleeSpeedKey = KeyCode.Alpha7;

    void Update()
    {
        if (rangedWeapon != null)
        {
            if (Input.GetKeyDown(increaseDamageKey))
            {
                Debug.Log("Ranged Damage Increased!");
            }
            
            if (Input.GetKeyDown(increaseFireRateKey))
            {
                rangedWeapon.SetFireRate(0.3f);
                Debug.Log("Fire Rate Increased!");
            }
            
            if (Input.GetKeyDown(addBulletKey))
            {
                rangedWeapon.SetBulletsPerShot(3);
                rangedWeapon.SetSpreadAngle(20f);
                Debug.Log("Multi-shot Enabled!");
            }
            
            if (Input.GetKeyDown(increaseSpreadKey))
            {
                rangedWeapon.SetSpreadAngle(30f);
                Debug.Log("Spread Increased!");
            }
        }
        
        if (meleeWeapon != null)
        {
            if (Input.GetKeyDown(increaseMeleeDamageKey))
            {
                meleeWeapon.SetDamage(50f);
                Debug.Log("Melee Damage Increased!");
            }
            
            if (Input.GetKeyDown(increaseMeleeRangeKey))
            {
                meleeWeapon.SetRange(3f);
                Debug.Log("Melee Range Increased!");
            }
            
            if (Input.GetKeyDown(increaseMeleeSpeedKey))
            {
                meleeWeapon.SetAttackSpeed(0.4f);
                Debug.Log("Melee Speed Increased!");
            }
        }
    }
}
