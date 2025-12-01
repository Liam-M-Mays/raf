using UnityEngine;

public class PlayerRangedWeapon : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    
    [Header("Fire Rate")]
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private bool autoFire = false;
    
    [Header("Projectile Properties")]
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileRange = 15f;
    
    [Header("Multi-Shot")]
    [SerializeField] private int bulletsPerShot = 1;
    [SerializeField] private float spreadAngle = 10f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip fireSound;
    
    private float lastFireTime = -999f;
    private Camera mainCam;
    private AudioSource audioSource;
    private AmmoSystem ammoSystem;
    private WeaponAiming weaponAiming;
    private WeaponManager weaponManager;
    private bool canFire = true;

    void Update() {
        // Auto-fire: hold button to fire repeatedly
            if (autoFire)
            {
                if (Input.GetMouseButton(0) && CanFire())
                {
                    Fire();
                }
            }
            // Semi-auto: single shot per button press
            else
            {
                if (Input.GetMouseButtonDown(0) && CanFire())
                {
                    Fire();
                }
            }
    }

    void Start()
    {
        mainCam = Camera.main;
        
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        ammoSystem = GetComponent<AmmoSystem>();
        weaponAiming = GetComponent<WeaponAiming>();
        weaponManager = GetComponent<WeaponManager>();
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
        if (projectilePrefab == null)
        {
            Debug.LogError("PlayerRangedWeapon: No projectile prefab assigned!");
            return;
        }

        // FIXED: Removed duplicate and added null check
        if (weaponManager != null && weaponManager.animator != null)
        {
            weaponManager.animator.SetTrigger("Fire");
        }
        
        // Check ammo
        if (ammoSystem != null)
        {
            if (!ammoSystem.TryConsumeAmmo())
            {
                Debug.Log("Out of ammo!");
                return;
            }
        }
        
        // Get aim direction from WeaponAiming
        Vector2 direction;
        if (weaponAiming != null)
        {
            direction = weaponAiming.GetAimDirection();
        }
        else
        {
            // Fallback to mouse position if no WeaponAiming
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            direction = (mousePos - firePoint.position).normalized;
        }
        
        Debug.Log($"PlayerRangedWeapon.Fire() called! Direction: {direction}");
        
        // Calculate spread for multiple bullets
        float startAngle = -spreadAngle / 2f;
        float angleStep = bulletsPerShot > 1 ? spreadAngle / (bulletsPerShot - 1) : 0f;
        
        // Spawn all bullets
        for (int i = 0; i < bulletsPerShot; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 spreadDirection = RotateVector(direction, currentAngle);
            
            SpawnProjectile(spreadDirection);
        }
        
        lastFireTime = Time.time;
        
        // Play fire sound
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

    // Setter methods for WeaponManager
    public void SetProjectilePrefab(GameObject prefab) => projectilePrefab = prefab;
    public void SetFirePoint(Transform point) => firePoint = point;
    public void SetFireRate(float rate) => fireRate = Mathf.Max(0.01f, rate);
    public void SetAutoFire(bool auto) => autoFire = auto;
    public void SetDamage(float dmg) => projectileDamage = Mathf.Max(0f, dmg);
    public void SetSpeed(float spd) => projectileSpeed = Mathf.Max(0.1f, spd);
    public void SetRange(float range) => projectileRange = Mathf.Max(0.1f, range);
    public void SetBulletsPerShot(int bullets) => bulletsPerShot = Mathf.Max(1, bullets);
    public void SetSpreadAngle(float angle) => spreadAngle = Mathf.Clamp(angle, 0f, 180f);
    public void SetFireSound(AudioClip clip) => fireSound = clip;
    public void SetCanFire(bool value) => canFire = value;
}