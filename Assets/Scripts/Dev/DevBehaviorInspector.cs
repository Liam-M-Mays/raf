using UnityEngine;

/// <summary>
/// Lightweight inspector that shows info about the closest enemy under the mouse (press Tab to toggle).
/// Uses OnGUI to avoid requiring a Canvas.
/// </summary>
public class DevBehaviorInspector : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.Tab;
    private bool visible = false;
    private EnemyController lastSelected;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) visible = !visible;
        if (!visible) return;

        // Raycast from mouse into world
        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(wp);
        if (hit != null)
        {
            var ec = hit.GetComponent<EnemyController>();
            if (ec != null) lastSelected = ec;
        }
    }

    void OnGUI()
    {
        if (!visible) return;
        if (lastSelected == null) return;

        IBehavior behavior = lastSelected.currentBehavior();
        BehaviorContext ctx = behavior?.CTX();

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Box($"Enemy: {lastSelected.gameObject.name}");
        if (behavior != null) GUILayout.Label($"Behavior: {behavior.GetType().Name}");
        if (ctx != null)
        {
            GUILayout.Label($"Dist to raft: {ctx.distanceToTarget:F2}");
            GUILayout.Label($"HP: {(ctx.health != null ? ctx.health.GetCurrentHealth().ToString("F0") : "N/A")}");
        }
        GUILayout.EndArea();
    }
}
