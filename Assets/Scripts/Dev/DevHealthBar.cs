using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Floating health bar above entities.
/// Shows current/max HP and color gradient (green -> yellow -> red).
/// </summary>
public class DevHealthBar : MonoBehaviour
{
    private Health health;
    private Canvas canvas;
    private RectTransform rectTrans;

    void Start()
    {
        health = GetComponent<Health>();
        if (health == null) return;

        // Create WorldSpace canvas if not present
        GameObject canvasGO = new GameObject("HealthBarCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = Vector3.zero;

        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var rectCanvas = canvasGO.GetComponent<RectTransform>();
        rectCanvas.sizeDelta = new Vector2(1, 0.2f);

        // Create bar background
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform);
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.color = Color.black;
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Create bar fill
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(canvasGO.transform);
        var fillImage = fillGO.AddComponent<Image>();
        fillImage.color = Color.green;
        rectTrans = fillGO.GetComponent<RectTransform>();
        rectTrans.anchorMin = Vector2.zero;
        rectTrans.anchorMax = new Vector2(1, 1);
    }

    void Update()
    {
        if (health == null || rectTrans == null) return;

        float healthPercent = health.GetHealthPercentage();
        rectTrans.offsetMax = new Vector2(-(1 - healthPercent), 0);

        // Color gradient
        if (healthPercent > 0.66f)
            rectTrans.GetComponent<Image>().color = Color.green;
        else if (healthPercent > 0.33f)
            rectTrans.GetComponent<Image>().color = Color.yellow;
        else
            rectTrans.GetComponent<Image>().color = Color.red;
    }
}
