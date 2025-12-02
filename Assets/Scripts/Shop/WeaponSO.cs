using UnityEngine;
using System;

/// <summary>
/// Comprehensive weapon configuration. ALL weapon settings in one SO.
/// Handles: firing, projectiles, ammo, physics, visuals, audio, etc.
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class WeaponSO : ScriptableObject
{
    [Header("=== SHOP INFO ===")]
    public string weaponName = "New Weapon";
    public int cost = 100;
    public Sprite weaponSprite;
    public string description = "A weapon";
    public bool isMelee = false; // Is this a melee weapon?
    
    [Header("=== PREFABS ===")]
    public GameObject weaponVisualPrefab; // Just the visual - can be any prefab
    public GameObject projectilePrefab;   // Projectile to spawn

    public enum WeaponType { Ranged, Melee }
    public enum ProjectileType
    {
        Standard,      // Straight line
        Homing,        // Seeks target
        Bouncing,      // Bounces off walls
        Explosive,     // Explodes on impact
        Piercing,      // Goes through enemies
        Spray,         // Shotgun spread
        Slow,          // Slows on hit
        Splitting      // Splits into more projectiles
    }

    [Header("=== FIRING MECHANICS ===")]
    public WeaponType weaponType = WeaponType.Ranged;
    [Min(0.01f)] public float fireRate = 0.3f;
    public bool autoFire = false;
    public int bulletsPerShot = 1;
    [Min(0f)] public float spreadAngle = 0f;

    [Header("=== PROJECTILE CONFIG ===")]
    public ProjectileType projectileType = ProjectileType.Standard;
    [Min(0.1f)] public float projectileSpeed = 12f;
    [Min(1f)] public float projectileRange = 15f;
    [Min(1f)] public float projectileDamage = 10f;
    
    // Homing
    [Min(0f)] public float homingTurnSpeed = 5f;
    [Min(0f)] public float homingRange = 10f;
    
    // Bouncing
    public int maxBounces = 3;
    [Min(0f)] public float bounceDampenDamage = 0.1f;
    
    // Explosive
    [Min(0f)] public float explosionRadius = 3f;
    [Min(0f)] public float explosionDamage = 15f;
    public GameObject explosionEffectPrefab;
    
    // Spray/Shotgun
    // (uses bulletsPerShot and spreadAngle above)
    
    // Slow
    [Min(0f)] public float slowDuration = 2f;
    [Min(0f)] public float slowAmount = 0.5f;
    
    // Splitting
    public int splitCount = 2;
    public GameObject splitProjectilePrefab;
    [Min(0f)] public float splitSpeedReduction = 0.7f;

    [Header("=== AIMING & RECOIL ===")]
    [Min(0f)] public float recoilDistance = 0.2f;
    [Min(0f)] public float recoilRecoverySpeed = 8f;

    [Header("=== AMMO SYSTEM ===")]
    public bool usesAmmo = false;
    [Min(1)] public int maxAmmo = 30;
    [Min(0.1f)] public float reloadTime = 2f;
    public bool autoReload = true;
    public bool useReserveAmmo = false;
    [Min(1)] public int maxReserveAmmo = 120;

    [Header("=== PHYSICS IMPACT ===")]
    [Min(0f)] public float weaponKickbackForce = 0f;  // Pushes raft backward on fire
    [Min(0f)] public float enemyKnockbackForce = 0f;  // Knocks enemies back on hit

    [Header("=== AUDIO ===")]
    public AudioClip primarySound;    // Alias for fireSound (for backward compatibility)
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptyClickSound;

    [Header("=== DEBUG ===")]
    [TextArea(2, 4)] public string notes = "Weapon notes and strategy";
}
