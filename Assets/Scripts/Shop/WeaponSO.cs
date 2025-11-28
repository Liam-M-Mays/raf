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

    [Header("Weapon Stats")]
    public bool autoFire;
    public float attackingSpeed;
    public int damage;
    public float fireRate;
    public float spreadAngle;
    public int bulletsPerShot;
    public float range;

    [Header("Prefabs")]
    public GameObject weaponPrefab;
    public GameObject projectilePrefab;
}
