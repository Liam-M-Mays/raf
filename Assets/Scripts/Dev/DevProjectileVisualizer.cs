using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Visualize projectile trajectories and damage amounts.
/// Press P to toggle. Shows trajectory lines and damage labels for all active projectiles.
/// </summary>
public class DevProjectileVisualizer : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.P;
    private bool visualizeProjectiles = false;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            visualizeProjectiles = !visualizeProjectiles;
            Debug.Log(visualizeProjectiles ? "Projectile visualization ON" : "Projectile visualization OFF");
        }
    }

    void OnDrawGizmos()
    {
        if (!visualizeProjectiles) return;

        var projectiles = Object.FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None);
        foreach (var proj in projectiles)
        {
            if (proj == null) continue;

            // Draw trajectory line (extrapolate forward)
            Gizmos.color = Color.cyan;
            Vector3 pos = proj.transform.position;
            var rb = proj.GetComponent<Rigidbody2D>();
            float speed = rb != null ? rb.linearVelocity.magnitude : 5f;
            Vector3 predictedPos = pos + proj.transform.up * speed * Time.deltaTime * 60f;
            Gizmos.DrawLine(pos, predictedPos);

            // Draw circle at current position
            DrawCircle(pos, 0.2f, 8);
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
