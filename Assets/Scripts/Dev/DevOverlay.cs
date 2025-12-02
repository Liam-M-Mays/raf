using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Lightweight runtime dev overlay. Creates its own Canvas if none exists and provides
/// buttons for toggling god mode, spawning, and slowing/speeding time. Safe to run in
/// scenes without UI.
/// </summary>
public class DevOverlay : MonoBehaviour
{
    public DevGodMode godMode;
    public DevSpawner spawner;
    public KeyCode toggleKey = KeyCode.F1;

    private GameObject canvasGO;
    private bool visible = true;

    void Start()
    {
        // Find or create a Canvas
        Canvas existing = Object.FindFirstObjectByType<Canvas>();
        if (existing != null)
        {
            canvasGO = existing.gameObject;
        }
        else
        {
            canvasGO = new GameObject("DevOverlayCanvas");
            canvasGO.layer = LayerMask.NameToLayer("UI");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Ensure it's on top
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Create panel
        CreateButton("God", new Vector2(10, -10), () => { if (godMode != null) godMode.ToggleGodMode(); else Debug.Log("No DevGodMode assigned"); });
        CreateButton("Spawn", new Vector2(10, -50), () => { if (spawner != null) spawner.DoSpawn(); else Debug.Log("No DevSpawner assigned"); });
        CreateButton("Pause", new Vector2(10, -90), () => { Time.timeScale = Mathf.Approximately(Time.timeScale, 0f) ? 1f : 0f; });
        CreateButton("+Time", new Vector2(10, -130), () => { Time.timeScale = Mathf.Clamp(Time.timeScale + 0.25f, 0f, 5f); });
        CreateButton("-Time", new Vector2(10, -170), () => { Time.timeScale = Mathf.Clamp(Time.timeScale - 0.25f, 0f, 5f); });
        
        Debug.Log("DevOverlay: Initialized successfully on " + canvasGO.name);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            visible = !visible;
            if (canvasGO != null) canvasGO.SetActive(visible);
        }
    }

    private void CreateButton(string label, Vector2 anchoredPos, UnityEngine.Events.UnityAction onClick)
    {
        if (canvasGO == null)
        {
            Debug.LogError("DevOverlay: Canvas is null, cannot create button!");
            return;
        }

        GameObject btnGO = new GameObject("DevBtn_" + label);
        btnGO.layer = LayerMask.NameToLayer("UI");
        btnGO.transform.SetParent(canvasGO.transform, false);

        RectTransform rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(80, 30);

        var image = btnGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        var button = btnGO.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        button.colors = colors;
        button.onClick.AddListener(onClick);

        GameObject textGO = new GameObject("Text");
        textGO.layer = LayerMask.NameToLayer("UI");
        textGO.transform.SetParent(btnGO.transform, false);
        var txtRT = textGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;
        var txt = textGO.AddComponent<TextMeshProUGUI>();
        txt.text = label;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        txt.fontSize = 20;
    }
}
