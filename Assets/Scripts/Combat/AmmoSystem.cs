using UnityEngine;

/// Manages ammo for ranged weapons
/// Add this component to your PlayerRangedWeapon object
public class AmmoSystem : MonoBehaviour
{
    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private int currentAmmo = 30;
    [SerializeField] private bool infiniteAmmo = false;
    
    [Header("Reload Settings")]
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private KeyCode reloadKey = KeyCode.R;
    [SerializeField] private bool autoReload = true;
    
    [Header("Reserve Ammo (Optional)")]
    [SerializeField] private bool useReserveAmmo = false;
    [SerializeField] private int maxReserveAmmo = 120;
    [SerializeField] private int currentReserveAmmo = 120;
    
    [Header("Audio")]
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptyClickSound;
    
    private bool isReloading = false;
    private float reloadTimer = 0f;
    private PlayerRangedWeapon rangedWeapon;
    private AudioSource audioSource;
    
    // Events for UI updates
    public delegate void AmmoChanged(int current, int max, int reserve);
    public event AmmoChanged OnAmmoChanged;
    
    public delegate void ReloadStarted(float reloadTime);
    public event ReloadStarted OnReloadStarted;
    
    public delegate void ReloadComplete();
    public event ReloadComplete OnReloadComplete;

    void Start()
    {
        rangedWeapon = GetComponent<PlayerRangedWeapon>();
        audioSource = GetComponent<AudioSource>();
        
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    void Update()
    {
        // Handle reload input
        if (Input.GetKeyDown(reloadKey) && !isReloading && currentAmmo < maxAmmo)
        {
            StartReload();
        }
        
        // Update reload timer
        if (isReloading)
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer >= reloadTime)
            {
                CompleteReload();
            }
        }
    }

    /// Try to consume ammo when firing. Returns true if ammo is available.
    public bool TryConsumeAmmo()
    {
        if (infiniteAmmo)
        {
            return true;
        }
        
        if (isReloading)
        {
            return false;
        }
        
        // Check if we have ammo in magazine
        if (currentAmmo > 0)
        {
            currentAmmo--;
            UpdateAmmoUI();
            
            // Auto reload when empty
            if (currentAmmo == 0 && autoReload && (!useReserveAmmo || currentReserveAmmo > 0))
            {
                StartReload();
            }
            
            return true;
        }
        else
        {
            // Play empty click sound
            if (audioSource != null && emptyClickSound != null)
            {
                audioSource.PlayOneShot(emptyClickSound);
            }
            
            // Try to auto reload if we have reserve ammo
            if (autoReload && !isReloading && (!useReserveAmmo || currentReserveAmmo > 0))
            {
                StartReload();
            }
            
            return false;
        }
    }

    void StartReload()
    {
        // Check if we have reserve ammo (if using reserve system)
        if (useReserveAmmo && currentReserveAmmo == 0)
        {
            Debug.Log("No reserve ammo!");
            return;
        }
        
        // Don't reload if already full
        if (currentAmmo >= maxAmmo)
        {
            return;
        }
        
        isReloading = true;
        reloadTimer = 0f;
        
        // Disable shooting during reload
        if (rangedWeapon != null)
        {
            rangedWeapon.SetCanFire(false);
        }
        
        // Play reload sound
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
        
        // Notify UI
        OnReloadStarted?.Invoke(reloadTime);
        
        Debug.Log("Reloading...");
    }

    void CompleteReload()
    {
        isReloading = false;
        
        if (useReserveAmmo)
        {
            // Calculate how much ammo we need
            int ammoNeeded = maxAmmo - currentAmmo;
            int ammoToReload = Mathf.Min(ammoNeeded, currentReserveAmmo);
            
            currentAmmo += ammoToReload;
            currentReserveAmmo -= ammoToReload;
        }
        else
        {
            // Simple reload - fill to max
            currentAmmo = maxAmmo;
        }
        
        // Re-enable shooting
        if (rangedWeapon != null)
        {
            rangedWeapon.SetCanFire(true);
        }
        
        UpdateAmmoUI();
        OnReloadComplete?.Invoke();
        
        Debug.Log("Reload complete!");
    }

    void UpdateAmmoUI()
    {
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo, currentReserveAmmo);
    }

    // Public methods
    public void AddAmmo(int amount)
    {
        if (useReserveAmmo)
        {
            // Add to reserve ammo pool
            currentReserveAmmo = Mathf.Min(currentReserveAmmo + amount, maxReserveAmmo);
        }
        else
        {
            // Add directly to magazine (capped at max)
            currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        }
        UpdateAmmoUI();
    }
    
    public void ReplenishAmmo()
    {
        // Fully replenish ammo (used in shops)
        currentAmmo = maxAmmo;
        currentReserveAmmo = maxReserveAmmo;
        UpdateAmmoUI();
    }

    public void SetInfiniteAmmo(bool infinite) => infiniteAmmo = infinite;
    public bool IsReloading() => isReloading;
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;
    public int GetReserveAmmo() => currentReserveAmmo;
    public float GetReloadProgress() => isReloading ? (reloadTimer / reloadTime) : 0f;
    // Setters for WeaponSO integration
    public void ConfigureFromWeapon(WeaponSO weapon)
    {
        // Set reserve ammo system flag FIRST
        useReserveAmmo = weapon.useReserveAmmo;
        
        // Configure magazine
        maxAmmo = weapon.maxAmmo;
        currentAmmo = weapon.maxAmmo;
        
        // Configure reserve ammo
        maxReserveAmmo = weapon.maxReserveAmmo;
        currentReserveAmmo = weapon.maxReserveAmmo;
        
        // Configure reload behavior
        reloadTime = weapon.reloadTime;
        autoReload = weapon.autoReload;
        
        // Configure audio
        reloadSound = weapon.reloadSound;
        emptyClickSound = weapon.emptyClickSound;
        
        UpdateAmmoUI();
        Debug.Log($"Ammo configured: Magazine={currentAmmo}/{maxAmmo}, Reserve={currentReserveAmmo}/{maxReserveAmmo}, UseReserve={useReserveAmmo}");
    }
}