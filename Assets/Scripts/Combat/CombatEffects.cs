using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Handles combat visual feedback: hit flashes, screen shake, improved damage numbers
/// </summary>
public static class CombatEffects
{
    private static CoroutineRunner _runner;

    private static CoroutineRunner GetRunner()
    {
        if (_runner == null)
        {
            GameObject obj = new GameObject("CombatEffectsRunner");
            _runner = obj.AddComponent<CoroutineRunner>();
            Object.DontDestroyOnLoad(obj);
        }
        return _runner;
    }

    /// <summary>Flash an entity yellow when hit for visual feedback</summary>
    public static void HitFlash(SpriteRenderer sprite, float duration = 0.1f)
    {
        if (sprite == null) return;
        GetRunner().StartCoroutine(FadeColorCoroutine(sprite, duration));
    }

    private static IEnumerator FadeColorCoroutine(SpriteRenderer sprite, float duration)
    {
        if (sprite == null) yield break;
        
        // Store original color BEFORE changing it
        Color originalColor = sprite.color;
        float elapsed = 0f;
        
        // Flash yellow
        sprite.color = Color.yellow;
        
        // Fade back to original over duration
        while (elapsed < duration && sprite != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (sprite != null)
            {
                sprite.color = Color.Lerp(Color.yellow, originalColor, t);
            }
            yield return null;
        }
        if (sprite != null) sprite.color = originalColor;
    }

    /// <summary>Apply screen shake effect (simple camera jitter)</summary>
    public static void ScreenShake(Camera cam, float intensity = 0.2f, float duration = 0.1f)
    {
        if (cam == null) return;
        GetRunner().StartCoroutine(ScreenShakeCoroutine(cam, intensity, duration));
    }

    private static IEnumerator ScreenShakeCoroutine(Camera cam, float intensity, float duration)
    {
        if (cam == null) yield break;
        
        Vector3 originalPos = cam.transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-intensity, intensity);
            float y = Random.Range(-intensity, intensity);
            cam.transform.position = originalPos + new Vector3(x, y, 0);
            yield return null;
        }
        cam.transform.position = originalPos;
    }

    /// <summary>Create an enhanced damage number with better visibility</summary>
    public static GameObject CreateDamageNumber(GameObject prefab, Vector3 position, float damage, Color color)
    {
        if (prefab == null) return null;
        GameObject dmgObj = Object.Instantiate(prefab, position, Quaternion.identity);
        DamageNumber dmgScript = dmgObj.GetComponent<DamageNumber>();
        if (dmgScript != null)
        {
            dmgScript.SetDamage(damage);
            // Set color for better feedback via TextMeshPro
            TextMeshPro textMesh = dmgObj.GetComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.color = color;
            }
        }
        return dmgObj;
    }
}

/// <summary>
/// Enhanced damage number display with color and knockback
/// </summary>
public class DamageNumberEnhanced : MonoBehaviour
{
    private TextMesh textMesh;
    private Color startColor;
    private Vector3 startPos;
    private float lifetime = 1.2f;
    private float elapsed = 0f;
    private float floatSpeed = 2f;

    void Start()
    {
        textMesh = GetComponent<TextMesh>();
        startPos = transform.position;
        startColor = textMesh != null ? textMesh.color : Color.white;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float progress = elapsed / lifetime;

        // Float upward
        transform.position = startPos + Vector3.up * (floatSpeed * elapsed);

        // Fade out
        if (textMesh != null)
        {
            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, progress);
            textMesh.color = c;
        }

        // Scale up for emphasis
        float scale = Mathf.Lerp(1f, 1.3f, progress * 0.5f);
        transform.localScale = Vector3.one * scale;

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetDamage(float dmg)
    {
        if (textMesh != null)
            textMesh.text = Mathf.RoundToInt(dmg).ToString();
    }

    public void SetColor(Color col)
    {
        startColor = col;
        if (textMesh != null)
            textMesh.color = col;
    }
}

/// <summary>
/// Simple helper to run coroutines from static class
/// </summary>
public class CoroutineRunner : MonoBehaviour
{
    // This component just allows static coroutine management
}
