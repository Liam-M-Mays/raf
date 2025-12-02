using UnityEngine;

/// <summary>
/// Toggle visual gizmos for all enemies to see ranges and debug info.
/// Press V to toggle. Shows attack range, respawn range, direction to target, and perception radius.
/// </summary>
public class DevGizmoToggle : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.V;
    private bool gizmosEnabled = false;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            gizmosEnabled = !gizmosEnabled;
            Debug.Log(gizmosEnabled ? "Gizmos ON" : "Gizmos OFF");
            
            // Enable/disable all BehaviorDebuggers in scene
            var debuggers = Object.FindObjectsByType<BehaviorDebugger>(FindObjectsSortMode.None);
            foreach (var debugger in debuggers)
            {
                var sr = debugger.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = gizmosEnabled;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!gizmosEnabled) return;

        // Draw all enemy ranges
        var enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            var behavior = enemy.currentBehavior();
            var ctx = behavior?.CTX();
            if (ctx == null) continue;

            Vector3 pos = ctx.self.position;

            // Attack range (green)
            Gizmos.color = Color.green;
            DrawCircle(pos, ctx.attackRange, 16);

            // Respawn range (yellow)
            Gizmos.color = Color.yellow;
            DrawCircle(pos, ctx.outOfRange, 24);

            // Target line (red)
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, ctx.target.position);

            // Perception radius (purple)
            Gizmos.color = new Color(1, 0, 1, 0.3f);
            DrawCircle(pos, 2.5f, 12);
        }
    }

    private void DrawCircle(Vector3 pos, float radius, int segments)
    {
        float angle = 360f / segments;
        Vector3 lastPoint = pos + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float rad = angle * i * Mathf.Deg2Rad;
            Vector3 newPoint = pos + new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0);
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        }
    }
}
