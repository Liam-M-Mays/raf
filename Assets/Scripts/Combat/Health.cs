using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// Universal health component for raft, enemies, and any damageable object
public class Health : MonoBehaviour
{
    public TextMeshProUGUI healthText; // Optional UI element to display health
    public GameObject gameOver; // Reference to Game Over UI for raft death

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Damage Numbers")]
    [SerializeField] private bool showDamageNumbers = true;
    [SerializeField] private GameObject damageNumberPrefab;
    
    [Header("Death Settings")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 0.5f;
    [SerializeField] private GameObject deathEffectPrefab;
    
    [Header("Events")]
    public UnityEvent<float> OnDamaged;
    public UnityEvent<float> OnHealed;
    public UnityEvent OnDeath;
    
    private bool isDead = false;
    private float realMaxHealth;

    void Start()
    {
        currentHealth = maxHealth;
        realMaxHealth = maxHealth;
    }

    /// Deal damage to this entity
    public void TakeDamage(float damage, Vector3 hitPosition)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Show damage number
        if (showDamageNumbers && damageNumberPrefab != null)
        {
            GameObject dmgNum = Instantiate(damageNumberPrefab, hitPosition, Quaternion.identity);
            DamageNumber dmgScript = dmgNum.GetComponent<DamageNumber>();
            if (dmgScript != null)
            {
                dmgScript.SetDamage(damage);
            }
        }

        // Hit flash visual feedback
        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
        {
            CombatEffects.HitFlash(sprite, 0.3f);
        }

        // Screen shake on significant damage (if this is the raft)
        if (CompareTag("Raft") && damage > 5f)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                CombatEffects.ScreenShake(mainCam, 0.15f, 0.1f);
            }
        }

        // Trigger damage event
        OnDamaged?.Invoke(damage);

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// Heal this entity
    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        OnHealed?.Invoke(amount);
    }

    /// Handle death
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        OnDeath?.Invoke();

        // Spawn death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Check if this is the raft
        if (gameObject.CompareTag("Raft"))
        {
            if (gameOver != null)
            {
                gameOver.SetActive(true);
            }
        }

        // Destroy object after delay
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    // Getters
    public float GetCurrentHealth() => currentHealth;
    public void SetHealth(float newHealth)
    {
        currentHealth = newHealth;
        maxHealth = newHealth;
    }
    public float GetMaxHealth() => maxHealth;
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = realMaxHealth + newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsDead() => isDead;

    // Optional: Take damage without position (uses object's position)
    public void TakeDamage(float damage)
    {
        TakeDamage(damage, transform.position);
    }

    void Update() {
        if (healthText != null) {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }
}