using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class WeaponSO : ScriptableObject
{
    [Header("Shop Info")]
    public string weaponName;
    public int cost;
    public Sprite weaponSprite;
    public string description;
    public bool isMelee;

    [Header("Prefabs")]
    public GameObject weaponPrefab;
    public GameObject projectilePrefab;

    public enum WeaponType
    {
        Ranged,
        Melee
    }

    [Header("Display")]
    public WeaponType weaponType = WeaponType.Ranged;
    
    [Header("Audio")]
    public AudioClip primarySound; // Fire sound for ranged, swing sound for melee
    public AudioClip secondarySound; // Reload for ranged, hit sound for melee
    
    // === RANGED WEAPON PROPERTIES ===
    [Header("Ranged Properties")]
    public float fireRate;
    public bool autoFire = false;
    public float projectileDamage = 10f;
    public float projectileSpeed = 12f;
    public float projectileRange = 15f;
    public int bulletsPerShot = 1;
    public float spreadAngle = 10f;
    
    [Header("Ranged Aiming Settings")]
    public float recoilDistance = 0.2f;
    public float recoilRecoverySpeed = 8f;

    [Header("Ammo Settings")]
    public bool usesAmmo = false;
}
