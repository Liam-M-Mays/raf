using UnityEngine;
using TMPro;

/// Displays floating damage numbers when entities take damage
public class DamageNumber : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float fadeDelay = 0.5f;
    
    [Header("Randomization")]
    [SerializeField] private float randomXRange = 0.5f;
    [SerializeField] private float randomYRange = 0.3f;
    
    private TextMeshPro textMesh;
    private Color originalColor;
    private float timer = 0f;
    private Vector3 moveDirection;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            originalColor = textMesh.color;
        }
        
        // Randomize movement direction slightly
        moveDirection = new Vector3(
            Random.Range(-randomXRange, randomXRange),
            floatSpeed + Random.Range(-randomYRange, randomYRange),
            0
        );
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Float upward with slight randomization
        transform.position += moveDirection * Time.deltaTime;

        // Fade out after delay
        if (timer > fadeDelay && textMesh != null)
        {
            float fadeProgress = (timer - fadeDelay) / (lifetime - fadeDelay);
            Color c = originalColor;
            c.a = Mathf.Lerp(1f, 0f, fadeProgress);
            textMesh.color = c;
        }

        // Destroy after lifetime
        if (timer > lifetime)
        {
            Destroy(gameObject);
        }
    }

    /// Set the damage value to display
    public void SetDamage(float damage)
    {
        if (textMesh != null)
        {
            textMesh.text = Mathf.RoundToInt(damage).ToString();
        }
    }
}