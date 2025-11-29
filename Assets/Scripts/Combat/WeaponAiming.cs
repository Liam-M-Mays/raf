using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAiming : MonoBehaviour
{
    [Header("Weapon Type")]
    [Tooltip("Select weapon type: Ranged for guns, Melee for swords/axes")]
    public WeaponType weaponType = WeaponType.Ranged;
    
    [Header("Weapon Switching")]
    [SerializeField] private KeyCode switchWeaponKey = KeyCode.Q;
    [SerializeField] private bool allowWeaponSwitching = true;
    
    [Header("References")]
    [Tooltip("The weapon sprite/object to rotate")]
    public Transform weaponTransform;
    
    [Tooltip("Reference to your PlayerMovement script")]
    public PlayerMovement playerMovement;
    
    [Header("Position Settings")]
    [Tooltip("Offset the weapon from the player center")]
    public Vector2 weaponOffset = new Vector2(0.5f, 0f);
    
    [Tooltip("Distance from player (mainly for melee)")]
    public float idleDistance = 0.5f;
    
    [Header("Flip Settings")]
    [Tooltip("Flip the weapon sprite when aiming left")]
    public bool flipWeaponSprite = true;
    
    [Header("Melee Settings")]
    [Tooltip("How far the weapon extends during a swing")]
    public float swingDistance = 1.2f;
    
    [Tooltip("Speed of the swing animation")]
    public float swingSpeed = 10f;
    
    [Header("Ranged Settings")]
    [Tooltip("Recoil distance when firing")]
    public float recoilDistance = 0.2f;
    
    [Tooltip("How fast the gun returns to normal position")]
    public float recoilRecoverySpeed = 8f;
    
    [Header("Rowing Mode Behavior")]
    [Tooltip("Hide weapon when rowing the raft")]
    public bool hideWhileRowing = true;
    
    private Camera mainCamera;
    private SpriteRenderer weaponSpriteRenderer;
    private InputAction attackAction;
    private bool isSwinging = false;
    private bool isRecoiling = false;
    private bool isFacingRight = true;
    
    private PlayerRangedWeapon rangedWeapon;
    private PlayerMeleeWeapon meleeWeapon;
    
    [Header("Debug Info")]
    [SerializeField] private float debugAngle;
    [SerializeField] private Vector2 debugMouseWorld;
    [SerializeField] private Vector2 debugPlayerPos;
    [SerializeField] private Vector2 debugDirection;
    
    private float currentDistance;
    private float targetDistance;

    void Start()
    {
        mainCamera = Camera.main;
        currentDistance = idleDistance;
        targetDistance = idleDistance;
        
        if (weaponTransform != null)
        {
            weaponSpriteRenderer = weaponTransform.GetComponent<SpriteRenderer>();
        }
        
        // Setup attack input
        attackAction = InputSystem.actions.FindAction("Fire");
        
        // Auto-find PlayerMovement if not assigned
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
        
        // Get weapon components
        rangedWeapon = GetComponent<PlayerRangedWeapon>();
        meleeWeapon = GetComponent<PlayerMeleeWeapon>();
        
        // Disable the weapon that's not being used
        UpdateWeaponState();
    }

    void Update()
    {
        // Handle weapon switching
        if (allowWeaponSwitching && Input.GetKeyDown(switchWeaponKey))
        {
            SwitchWeapon();
        }
        
        // Check if player is rowing - only show weapon when walking
        bool isRowing = IsPlayerRowing();
        
        if (hideWhileRowing && isRowing)
        {
            // Hide weapon while rowing
            if (weaponSpriteRenderer != null)
            {
                weaponSpriteRenderer.enabled = false;
            }
            return; // Don't process aiming while rowing
        }
        else
        {
            // Show weapon while walking
            if (weaponSpriteRenderer != null)
            {
                weaponSpriteRenderer.enabled = true;
            }
        }
        
        // Update player facing direction based on mouse
        UpdatePlayerFacing();
        
        AimWeaponAtMouse();
        
        // Handle attack input
        if (attackAction != null && attackAction.triggered)
        {
            if (weaponType == WeaponType.Melee && !isSwinging)
            {
                StartSwing();
            }
            else if (weaponType == WeaponType.Ranged && !isRecoiling)
            {
                Fire();
            }
        }
        
        // Update animations
        if (weaponType == WeaponType.Melee && isSwinging)
        {
            UpdateSwing();
        }
        else if (weaponType == WeaponType.Ranged && isRecoiling)
        {
            UpdateRecoil();
        }
    }

    void SwitchWeapon()
    {
        // Toggle between weapon types
        if (weaponType == WeaponType.Ranged)
        {
            weaponType = WeaponType.Melee;
            Debug.Log("Switched to MELEE weapon");
        }
        else
        {
            weaponType = WeaponType.Ranged;
            Debug.Log("Switched to RANGED weapon");
        }
        
        // Reset distances based on weapon type
        currentDistance = (weaponType == WeaponType.Ranged) ? 0f : idleDistance;
        targetDistance = currentDistance;
        
        // Update which weapon script can fire
        UpdateWeaponState();
    }

    void UpdateWeaponState()
    {
        // Enable/disable weapon scripts based on current weapon type
        if (rangedWeapon != null)
        {
            rangedWeapon.enabled = (weaponType == WeaponType.Ranged);
        }
        
        if (meleeWeapon != null)
        {
            meleeWeapon.enabled = (weaponType == WeaponType.Melee);
        }
    }

    void UpdatePlayerFacing()
    {
        // Get mouse position in world space
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0));
        mousePos.z = 0f;
        
        // Calculate direction from player to mouse
        Vector2 directionToMouse = (mousePos - transform.position).normalized;
        
        // Flip player based on horizontal mouse position
        if (directionToMouse.x > 0 && !isFacingRight)
        {
            FlipPlayer();
        }
        else if (directionToMouse.x < 0 && isFacingRight)
        {
            FlipPlayer();
        }
    }

    void FlipPlayer()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void AimWeaponAtMouse()
    {
        if (weaponTransform == null) return;

        // Get mouse position in world space
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0));
        mousePos.z = 0f;

        // Calculate direction from player to mouse
        Vector2 baseDirection = (mousePos - transform.position).normalized;
        Vector2 direction;

        // Debug values
        debugMouseWorld = mousePos;
        debugPlayerPos = transform.position;

        // FIXED: Flip direction based on weapon type
        if (weaponType == WeaponType.Ranged)
        {
            direction = baseDirection; // Ranged points AT mouse (toward cursor)
        }
        else // Melee
        {
            direction = -baseDirection; // Melee points AWAY from mouse (behind player)
        }
        
        debugDirection = direction;

        // Calculate angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        debugAngle = angle;

        // Apply rotation to weapon
        weaponTransform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Position weapon based on type
        if (weaponType == WeaponType.Ranged)
        {
            // Guns stay at fixed offset with recoil
            Vector2 recoilOffset = direction * -currentDistance;
            weaponTransform.position = (Vector2)transform.position + weaponOffset + recoilOffset;
        }
        else // Melee
        {
            // Melee extends in the direction (away from mouse)
            Vector2 offset = direction * currentDistance;
            weaponTransform.position = (Vector2)transform.position + offset;
        }

        // Flip weapon sprite when aiming left
        if (flipWeaponSprite && weaponSpriteRenderer != null)
        {
            if (angle > 90f || angle < -90f)
            {
                weaponSpriteRenderer.flipY = true;
            }
            else
            {
                weaponSpriteRenderer.flipY = false;
            }
        }
    }

    // === MELEE FUNCTIONS ===
    
    public void StartSwing()
    {
        isSwinging = true;
        targetDistance = swingDistance;
        
        // Call PlayerMeleeWeapon to do damage
        PlayerMeleeWeapon meleeWeapon = GetComponent<PlayerMeleeWeapon>();
        if (meleeWeapon != null && meleeWeapon.enabled)
        {
            Debug.Log("WeaponAiming calling PlayerMeleeWeapon.Attack()");
            meleeWeapon.Attack();
        }
        else
        {
            Debug.LogError("PlayerMeleeWeapon not found or disabled!");
        }
    }

    void UpdateSwing()
    {
        // Extend weapon outward during swing
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * swingSpeed);
        
        // When reached max distance, retract
        if (currentDistance >= swingDistance - 0.1f)
        {
            targetDistance = idleDistance;
            
            // End swing when back to idle distance
            if (currentDistance <= idleDistance + 0.1f)
            {
                currentDistance = idleDistance;
                isSwinging = false;
            }
        }
    }

    // === RANGED FUNCTIONS ===
    
    public void Fire()
    {
        isRecoiling = true;
        currentDistance = recoilDistance;
        
        // Call PlayerRangedWeapon to spawn bullet
        PlayerRangedWeapon rangedWeapon = GetComponent<PlayerRangedWeapon>();
        if (rangedWeapon != null && rangedWeapon.enabled)
        {
            Debug.Log("WeaponAiming calling PlayerRangedWeapon.Fire()");
            rangedWeapon.Fire();
        }
        else
        {
            Debug.LogError("PlayerRangedWeapon not found or disabled!");
        }
    }

    void UpdateRecoil()
    {
        // Smoothly return to idle position
        currentDistance = Mathf.Lerp(currentDistance, 0f, Time.deltaTime * recoilRecoverySpeed);
        
        if (currentDistance <= 0.05f)
        {
            currentDistance = 0f;
            isRecoiling = false;
        }
    }

    // === UTILITY FUNCTIONS ===
    
    bool IsPlayerRowing()
    {
        // Check if playerMovement exists and access its rowing state
        if (playerMovement != null && playerMovement.playerSprite != null)
        {
            return !playerMovement.playerSprite.activeSelf;
        }
        return false;
    }
    
    public Vector2 GetAimDirection()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0));
        mousePos.z = 0f;
        return (mousePos - transform.position).normalized;
    }

    public float GetAimAngle()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0));
        mousePos.z = 0f;
        Vector2 direction = (mousePos - transform.position).normalized;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
    
    public bool IsAttacking()
    {
        return isSwinging || isRecoiling;
    }
    
    // Change weapon type at runtime if needed
    public void SetWeaponType(WeaponType type)
    {
        weaponType = type;
        currentDistance = (type == WeaponType.Ranged) ? 0f : idleDistance;
        UpdateWeaponState();
    }
    
    public WeaponType GetCurrentWeaponType()
    {
        return weaponType;
    }
}

public enum WeaponType
{
    Ranged,
    Melee
}