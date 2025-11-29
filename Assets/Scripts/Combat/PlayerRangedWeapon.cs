 using UnityEngine;

public class PlayerRangedWeapon : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    
    [Header("Fire Rate")]
    [SerializeField] private float fireRate = 0.5f;
    
    [Header("Projectile Properties")]
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileRange = 15f;
    
    [Header("Multi-Shot")]
    [SerializeField] private int bulletsPerShot = 1;
    [SerializeField] private float spreadAngle = 10f;
    
    [Header("Input")]
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField] private bool useOldInputSystem = true;
    
    [Header("Audio")]
    [SerializeField] private AudioClip fireSound;
    
    private float lastFireTime = -999f;
    private Camera mainCam;
    private AudioSource audioSource;
    private AmmoSystem ammoSystem;
    private WeaponAiming weaponAiming;
    private bool canFire = true;

    void Start()
    {
        mainCam = Camera.main;
        
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        audioSource = GetComponent<AudioSource>();
        ammoSystem = GetComponent<AmmoSystem>();
        weaponAiming = GetComponent<WeaponAiming>();
    }

    void Update()
    {
        // Only check input if this weapon script is enabled
        if (useOldInputSystem && enabled)
        {
            if (Input.GetKeyDown(fireKey) && CanFire())
            {
                Fire();
            }
        }
    }

    bool CanFire()
    {
        if (!canFire) return false;
        if (!enabled) return false;
        return Time.time >= lastFireTime + fireRate;
    }

    public void Fire()
    {
        if (!CanFire()) return;
        if (projectilePrefab == null) return;
        
        if (ammoSystem != null)
        {
            if (!ammoSystem.TryConsumeAmmo())
            {
                return;
            }
        }
        
        Vector2 direction;
        if (weaponAiming != null)
        {
            direction = weaponAiming.GetAimDirection();
        }
        else
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            direction = (mousePos - firePoint.position).normalized;
        }
        
        Debug.Log($"PlayerRangedWeapon.Fire() called! Direction: {direction}");
        
        float startAngle = -spreadAngle / 2f;
        float angleStep = bulletsPerShot > 1 ? spreadAngle / (bulletsPerShot - 1) : 0f;
        
        for (int i = 0; i < bulletsPerShot; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 spreadDirection = RotateVector(direction, currentAngle);
            
            SpawnProjectile(spreadDirection);
        }
        
        lastFireTime = Time.time;
        
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }

    void SpawnProjectile(Vector2 direction)
    {
        Debug.Log($"SpawnProjectile called! Direction: {direction}");
        
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Debug.Log($"Bullet spawned at: {firePoint.position}");
        
        PlayerProjectile projScript = proj.GetComponent<PlayerProjectile>();
        
        if (projScript != null)
        {
            Debug.Log("PlayerProjectile script FOUND on bullet");
            projScript.Initialize(direction, projectileSpeed, projectileDamage, projectileRange);
        }
        else
        {
            Debug.LogError("PlayerProjectile script NOT FOUND on bullet prefab!");
        }
    }

    Vector2 RotateVector(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    public void SetFireRate(float rate) => fireRate = Mathf.Max(0.01f, rate);
    public void SetDamage(float dmg) => projectileDamage = Mathf.Max(0f, dmg);
    public void SetSpeed(float spd) => projectileSpeed = Mathf.Max(0.1f, spd);
    public void SetRange(float range) => projectileRange = Mathf.Max(0.1f, range);
    public void SetBulletsPerShot(int bullets) => bulletsPerShot = Mathf.Max(1, bullets);
    public void SetSpreadAngle(float angle) => spreadAngle = Mathf.Clamp(angle, 0f, 180f);
    public void SetCanFire(bool value) => canFire = value;
}