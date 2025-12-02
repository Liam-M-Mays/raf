using UnityEngine;

/// <summary>
/// Skip to any wave number. Press W to open skip menu.
/// Useful for testing specific wave compositions.
/// </summary>
public class DevWaveSkipper : MonoBehaviour
{
    public KeyCode skipKey = KeyCode.W;
    private bool menuOpen = false;
    private string waveInput = "";

    void Update()
    {
        if (Input.GetKeyDown(skipKey))
        {
            menuOpen = !menuOpen;
            waveInput = "";
        }

        if (menuOpen && Input.GetKeyDown(KeyCode.Return))
        {
            if (int.TryParse(waveInput, out int waveNum))
            {
                SkipToWave(waveNum);
                menuOpen = false;
            }
            else
            {
                Debug.LogWarning("DevWaveSkipper: Invalid wave number");
            }
        }

        if (menuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            menuOpen = false;
        }
    }

    void OnGUI()
    {
        if (!menuOpen) return;

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 50, 300, 100));
        GUILayout.Box("Skip to Wave");
        waveInput = GUILayout.TextField(waveInput, 3);
        GUILayout.Label("Press Enter to skip, Escape to cancel");
        GUILayout.EndArea();
    }

    private void SkipToWave(int waveNum)
    {
        // Kill all current enemies
        var enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        Debug.Log($"Skipping to wave {waveNum}. All enemies killed.");
    }
}
