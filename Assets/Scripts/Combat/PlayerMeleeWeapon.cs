using UnityEngine;

public class PlayerMeleeWeapon : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Damage Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float attackRange = 2f;
    
    [Header("Attack Speed")]
    [SerializeField] private float attackCooldown = 0.8f;
    
    [Header("Input")]
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;
    [SerializeField] private bool useOldInputSystem = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject slashEffectPrefab;
    [SerializeField] private float effectDuration = 0.3f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip hitSound;
    
    private float lastAttackTime = -999f;
    private Camera mainCam;
    private AudioSource audioSource;
    private Animator animator;
    private WeaponAiming weaponAiming;

    void Start()
    {
        mainCam = Camera.main;
        
        if (attackPoint == null)
        {
            attackPoint = transform;
        }
        
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        weaponAiming = GetComponent<WeaponAiming>();
    }

    void Update()
    {
        // Only check input if this weapon script is enabled
        if (useOldInputSystem && enabled)
        {
            if (Input.GetKeyDown(attackKey) && CanAttack())
            {
                Attack();
            }
        }
    }

    bool CanAttack()
    {
        if (!enabled) return false;
        return Time.time >= lastAttackTime + attackCooldown;
    }

    public void Attack()
    {
        if (!CanAttack()) return;
        
        lastAttackTime = Time.time;
        
        Debug.Log("Melee Attack!");
        
        // Play swing sound
        if (audioSource != null && swingSound != null)
        {
            audioSource.PlayOneShot(swingSound);
        }
        
        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        
        Debug.Log($"Melee detected {hitEnemies.Length} colliders in range");
        
        bool hitSomething = false;
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"Checking collider: {enemy.name} with tag: {enemy.tag}");
            
            if (enemy.CompareTag("Enemy"))
            {
                Health enemyHealth = enemy.GetComponent<Health>();
                if (enemyHealth != null && !enemyHealth.IsDead())
                {
                    Debug.Log($"✓ Melee hit enemy! Dealing {damage} damage");
                    enemyHealth.TakeDamage(damage, attackPoint.position);
                    hitSomething = true;
                }
                else
                {
                    Debug.LogWarning("Enemy has no Health component or is dead");
                }
            }
        }
        
        // Play hit sound if we hit something
        if (hitSomething && audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        // Spawn slash effect
        if (slashEffectPrefab != null)
        {
            Vector2 direction;
            float angle;
            
            if (weaponAiming != null)
            {
                direction = weaponAiming.GetAimDirection();
                angle = weaponAiming.GetAimAngle();
            }
            else
            {
                Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                direction = (mousePos - attackPoint.position).normalized;
                angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            }
            
            GameObject effect = Instantiate(slashEffectPrefab, attackPoint.position, Quaternion.Euler(0, 0, angle));
            Destroy(effect, effectDuration);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public void SetDamage(float dmg) => damage = Mathf.Max(0f, dmg);
    public void SetRange(float range) => attackRange = Mathf.Max(0.1f, range);
    public void SetAttackSpeed(float cooldown) => attackCooldown = Mathf.Max(0.1f, cooldown);
}