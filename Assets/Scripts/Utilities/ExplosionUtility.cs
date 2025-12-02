using UnityEngine;

public static class ExplosionUtility
{
    public static void ApplyExplosion(Vector3 position, float radius, float damage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);
        foreach (var c in hits)
        {
            if (c == null) continue;
            var h = c.GetComponent<Health>();
            if (h != null)
            {
                h.TakeDamage(damage, c.transform.position);
            }
        }

        // Optional: spawn explosion VFX if an Explosion prefab exists in Resources/VFX/Explosion
        var vfx = Resources.Load<GameObject>("VFX/Explosion");
        if (vfx != null)
        {
            GameObject.Instantiate(vfx, position, Quaternion.identity);
        }
    }
}
