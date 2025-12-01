using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAiming : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The weapon sprite/object to rotate")]
    public Transform weaponTransform;
    
    [Tooltip("Reference to your PlayerMovement script")]
    public PlayerMovement playerMovement;
    
    [Header("Position Settings")]
    [Tooltip("Offset the weapon from the player center")]
    public Vector2 weaponOffset = new Vector2(0.5f, 0f);
    
    [Header("Flip Settings")]
    [Tooltip("Flip the weapon sprite when aiming left")]
    public bool flipWeaponSprite = true;
    
    [Header("Ranged Settings")]
    [Tooltip("Recoil distance when firing")]
    public float recoilDistance = 0.2f;
    
    [Tooltip("How fast the gun returns to normal position")]
    public float recoilRecoverySpeed = 8f;
    
    [Header("Rowing Mode Behavior")]
    [Tooltip("Hide weapon when rowing the raft")]
    public bool hideWhileRowing = true;
    
    private Camera mainCamera;
    public SpriteRenderer weaponSpriteRenderer;
    private InputAction attackAction;
    private bool isRecoiling = false;
    private bool isFacingRight = true;
    
    private PlayerRangedWeapon rangedWeapon;
    
    [Header("Debug Info")]
    [SerializeField] private float debugAngle;
    [SerializeField] private Vector2 debugMouseWorld;
    [SerializeField] private Vector2 debugPlayerPos;
    [SerializeField] private Vector2 debugDirection;
    
    private float currentDistance;

    void Start()
    {
        mainCamera = Camera.main;
        currentDistance = 0f;
        
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
        
        // Get ranged weapon component
        rangedWeapon = GetComponent<PlayerRangedWeapon>();
        if (rangedWeapon != null)
        {
            rangedWeapon.enabled = true;
        }
    }

    void Update()
    {

        
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
        
        
        
        // Update recoil animation
        if (isRecoiling)
        {
            UpdateRecoil();
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

        // Calculate direction from weapon position to mouse (not player position)
        Vector2 direction = (mousePos - weaponTransform.position).normalized;

        // Debug values
        debugMouseWorld = mousePos;
        debugPlayerPos = transform.position;
        debugDirection = direction;

        // Calculate angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        debugAngle = angle;

        // Apply rotation to weapon - use world rotation to ignore parent scaling
        weaponTransform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Calculate weapon offset accounting for player flip
        Vector2 adjustedOffset = weaponOffset;
        if (!isFacingRight)
        {
            adjustedOffset.x = -adjustedOffset.x;
        }

        // Position weapon with recoil offset
        Vector2 recoilOffset = direction * -currentDistance;
        weaponTransform.position = (Vector2)transform.position + adjustedOffset + recoilOffset;

        // Flip weapon transform scale so it points correctly
        if (angle > 90f || angle < -90f)
        {
            // Aiming left → weapon faces left
            weaponTransform.localScale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            // Aiming right → weapon faces right (-1,1,1)
            weaponTransform.localScale = new Vector3(-1f, 1f, 1f);
        }

        // Flip weapon sprite when aiming left
        if (flipWeaponSprite)
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

    public void Fire()
    {
        isRecoiling = true;
        currentDistance = recoilDistance;
        
        // Call PlayerRangedWeapon to spawn bullet
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
        return isRecoiling;
    }
}