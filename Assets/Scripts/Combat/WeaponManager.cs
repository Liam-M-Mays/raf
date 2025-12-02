using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon List")]
    [SerializeField] public List<WeaponSO> availableWeapons = new List<WeaponSO>();
    
    [Header("Starting Weapon")]
    [SerializeField] private WeaponSO startingWeapon; // Drag a weapon here to start with it
    
    [Header("References")]
    [SerializeField] private Transform weaponHoldPoint; // Where the weapon visual appears (for WeaponAiming.weaponTransform)
    [SerializeField] private Transform firePoint; // Where projectiles spawn (ranged)
    [SerializeField] public Animator animator;
    
    [Header("Input")]
    [SerializeField] private bool useScrollWheel = true;
    [SerializeField] private KeyCode nextWeaponKey = KeyCode.E;
    [SerializeField] private KeyCode previousWeaponKey = KeyCode.Q;
    
    private int currentWeaponIndex = 0;
    private GameObject currentWeaponInstance;
    private PlayerRangedWeapon rangedWeaponScript;
    private WeaponAiming weaponAiming;
    
    // Events for UI updates
    public delegate void WeaponChangedHandler(WeaponSO newWeapon, int index);
    public event WeaponChangedHandler OnWeaponChanged;

    void Start()
    {
        // Get or create ranged weapon script
        rangedWeaponScript = GetComponent<PlayerRangedWeapon>();
        if (rangedWeaponScript == null)
        {
            rangedWeaponScript = gameObject.AddComponent<PlayerRangedWeapon>();
        }
        
        // Get weapon aiming component
        weaponAiming = GetComponent<WeaponAiming>();
        if (weaponAiming == null)
        {
            Debug.LogWarning("WeaponManager: No WeaponAiming component found!");
        }
        
        // If a specific starting weapon is assigned, add it to availableWeapons if not already there
        if (startingWeapon != null)
        {
            if (!availableWeapons.Contains(startingWeapon))
            {
                availableWeapons.Insert(0, startingWeapon);
            }
            
            // Find the index of the starting weapon
            int startIndex = availableWeapons.IndexOf(startingWeapon);
            if (startIndex >= 0)
            {
                EquipWeapon(startIndex);
                return;
            }
        }
        
        // Fallback: equip first weapon if available
        if (availableWeapons.Count > 0)
        {
            EquipWeapon(0);
        }
        else
        {
            Debug.LogWarning("WeaponManager: No weapons available!");
        }
    }

    void Update()
    {
        HandleWeaponSwitchInput();
    }

    void HandleWeaponSwitchInput()
    {
        if (availableWeapons.Count <= 1) return;
        
        // Scroll wheel switching - cycles through ALL weapons
        if (useScrollWheel)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                if (scroll > 0f)
                {
                    SwitchToNextWeapon();
                }
                else if (scroll < 0f)
                {
                    SwitchToPreviousWeapon();
                }
            }
        }
        //if (InputSystem.actions.FindAction("Next").triggered) SwitchToNextWeapon();
        // Key switching
        if (Input.GetKeyDown(nextWeaponKey))
        {
            SwitchToNextWeapon();
        }
        
        if (Input.GetKeyDown(previousWeaponKey))
        {
            SwitchToPreviousWeapon();
        }
        
        // Number key switching (1-9 for direct weapon selection)
        for (int i = 0; i < Mathf.Min(availableWeapons.Count, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                EquipWeapon(i);
            }
        }
    }

    public void SwitchToNextWeapon()
    {
        if (availableWeapons.Count <= 1) return;
        
        currentWeaponIndex++;
        if (currentWeaponIndex >= availableWeapons.Count)
        {
            currentWeaponIndex = 0;
        }
        
        EquipWeapon(currentWeaponIndex);
    }

    public void SwitchToPreviousWeapon()
    {
        if (availableWeapons.Count <= 1) return;
        
        currentWeaponIndex--;
        if (currentWeaponIndex < 0)
        {
            currentWeaponIndex = availableWeapons.Count - 1;
        }
        
        EquipWeapon(currentWeaponIndex);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= availableWeapons.Count)
        {
            Debug.LogWarning($"WeaponManager: Invalid weapon index {index}");
            return;
        }
        
        WeaponSO weaponData = availableWeapons[index];
        
        if (weaponData == null)
        {
            Debug.LogWarning($"WeaponManager: Weapon data at index {index} is null");
            return;
        }
        
        currentWeaponIndex = index;
        
        // Destroy old weapon visual
        if (currentWeaponInstance != null)
        {
            Destroy(currentWeaponInstance);
        }
        
        // Spawn new weapon visual
        if (weaponData.weaponVisualPrefab != null && weaponHoldPoint != null)
        {
            currentWeaponInstance = Instantiate(weaponData.weaponVisualPrefab, weaponHoldPoint);
            animator = currentWeaponInstance.GetComponent<Animator>();
            weaponAiming.weaponSpriteRenderer = currentWeaponInstance.GetComponent<SpriteRenderer>();
            currentWeaponInstance.transform.localPosition = Vector3.zero;
            currentWeaponInstance.transform.localRotation = Quaternion.identity;
            
            // Update WeaponAiming reference to the new weapon transform
            if (weaponAiming != null)
            {
                weaponAiming.weaponTransform = currentWeaponInstance.transform;
            }
        }
        
        // Apply ranged weapon data
        ApplyRangedWeaponData(weaponData);
        rangedWeaponScript.enabled = true;
        
        // Update WeaponAiming settings
        if (weaponAiming != null)
        {
            weaponAiming.recoilDistance = weaponData.recoilDistance;
            weaponAiming.recoilRecoverySpeed = weaponData.recoilRecoverySpeed;
        }
        
        Debug.Log($"Equipped weapon: {weaponData.weaponName}");
        
        // Notify listeners (for UI updates)
        OnWeaponChanged?.Invoke(weaponData, index);
    }

    void ApplyRangedWeaponData(WeaponSO data)
    {
        if (rangedWeaponScript == null) return;
        
        // Set all ranged weapon properties
        rangedWeaponScript.SetProjectilePrefab(data.projectilePrefab);
        rangedWeaponScript.SetFirePoint(firePoint);
        rangedWeaponScript.SetFireRate(data.fireRate);
        rangedWeaponScript.SetAutoFire(data.autoFire);
        rangedWeaponScript.SetDamage(data.projectileDamage);
        rangedWeaponScript.SetSpeed(data.projectileSpeed);
        rangedWeaponScript.SetRange(data.projectileRange);
        rangedWeaponScript.SetBulletsPerShot(data.bulletsPerShot);
        rangedWeaponScript.SetSpreadAngle(data.spreadAngle);
        rangedWeaponScript.SetFireSound(data.primarySound);
        rangedWeaponScript.SetWeaponKickbackForce(data.weaponKickbackForce);
        rangedWeaponScript.SetEnemyKnockbackForce(data.enemyKnockbackForce);
        
        // Update ammo system if present
        AmmoSystem ammoSystem = GetComponent<AmmoSystem>();
        if (ammoSystem != null)
        {
            ammoSystem.SetInfiniteAmmo(!data.usesAmmo);
            ammoSystem.ConfigureFromWeapon(data);
        }
    }

    // Public getters
    public WeaponSO GetCurrentWeapon()
    {
        if (currentWeaponIndex >= 0 && currentWeaponIndex < availableWeapons.Count)
        {
            return availableWeapons[currentWeaponIndex];
        }
        return null;
    }

    public int GetCurrentWeaponIndex() => currentWeaponIndex;
    
    public int GetWeaponCount() => availableWeapons.Count;
    
    public List<WeaponSO> GetAllWeapons() => availableWeapons;

    // Add/remove weapons at runtime
    public void AddWeapon(WeaponSO weapon)
    {
        if (!availableWeapons.Contains(weapon))
        {
            availableWeapons.Add(weapon);
        }
    }

    public void RemoveWeapon(WeaponSO weapon)
    {
        if (availableWeapons.Contains(weapon))
        {
            int index = availableWeapons.IndexOf(weapon);
            availableWeapons.Remove(weapon);
            
            // If we removed the current weapon, switch to another
            if (index == currentWeaponIndex && availableWeapons.Count > 0)
            {
                EquipWeapon(Mathf.Min(currentWeaponIndex, availableWeapons.Count - 1));
            }
        }
    }
}