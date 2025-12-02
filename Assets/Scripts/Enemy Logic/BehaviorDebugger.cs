using UnityEngine;

/// <summary>
/// Debug helper for visualizing enemy behavior state.
/// Shows current behavior and context info as gizmos or text.
/// </summary>
public class BehaviorDebugger : MonoBehaviour
{
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private Color debugColor = Color.yellow;
    
    private EnemyController controller;
    private BehaviorContext lastContext;

    private void Start()
    {
        controller = GetComponent<EnemyController>();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;

        controller = GetComponent<EnemyController>();
        if (controller == null) return;

        IBehavior behavior = controller.currentBehavior();
        if (behavior == null) return;

        lastContext = behavior.CTX();
        if (lastContext == null) return;

        // Draw line to target
        Gizmos.color = debugColor;
        Gizmos.DrawLine(transform.position, lastContext.target.position);

        // Draw attack range
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        DrawCircle(transform.position, lastContext.attackRange, 16);

        // Draw attack max range
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        DrawCircle(transform.position, lastContext.attackRangeMax, 16);

        // Draw out of range
        Gizmos.color = new Color(0f, 0f, 1f, 0.15f);
        DrawCircle(transform.position, lastContext.outOfRange, 24);
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;

        controller = GetComponent<EnemyController>();
        if (controller == null) return;

        IBehavior behavior = controller.currentBehavior();
        if (behavior == null) return;

        lastContext = behavior.CTX();
        if (lastContext == null) return;

        // Display behavior info
        GUI.color = Color.white;
        GUI.Label(new Rect(10, 10, 300, 200), GetBehaviorInfo());
    }

    private string GetBehaviorInfo()
    {
        if (lastContext == null)
            return "No behavior context";

        string info = $"<b>Behavior Debug</b>\n";
        info += $"Distance to target: {lastContext.distanceToTarget:F2}\n";
        info += $"Attack range: {lastContext.attackRange:F2}\n";
        info += $"Max range: {lastContext.attackRangeMax:F2}\n";
        info += $"Out of range: {lastContext.outOfRange:F2}\n";
        info += $"Hittable: {lastContext.hittable}\n";
        info += $"Speed: {lastContext.currentVelocity:F2}/{lastContext.maxSpeed:F2}\n";
        
        return info;
    }

    private void DrawCircle(Vector3 position, float radius, int segments)
    {
        float angle = 360f / segments;
        Vector3 lastPoint = position + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float rad = angle * i * Mathf.Deg2Rad;
            Vector3 newPoint = position + new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0);
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        }
    }
}
