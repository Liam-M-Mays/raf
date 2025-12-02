using UnityEngine;

/// <summary>
/// Simple runtime dev tool: toggles god mode for the raft (negates incoming damage).
/// Press `G` to toggle when this component is active.
/// </summary>
public class DevGodMode : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.G;
    private bool enabledGod = false;
    private Health raftHealth;

    void Start()
    {
        raftHealth = GameServices.GetRaftHealth();
        if (raftHealth == null)
        {
            Debug.LogWarning("DevGodMode: Raft health not found. God mode will be unavailable.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleGodMode();
        }
    }

    public void ToggleGodMode()
    {
        if (raftHealth == null) raftHealth = GameServices.GetRaftHealth();
        if (raftHealth == null) return;

        enabledGod = !enabledGod;
        if (enabledGod)
        {
            raftHealth.OnDamaged.AddListener(OnRaftDamaged);
            Debug.Log("DevGodMode: GOD MODE ENABLED");
        }
        else
        {
            raftHealth.OnDamaged.RemoveListener(OnRaftDamaged);
            Debug.Log("DevGodMode: God mode disabled");
        }
    }

    private void OnRaftDamaged(float dmg)
    {
        // Heal back the damage immediately to negate it
        if (raftHealth != null)
        {
            raftHealth.Heal(dmg);
        }
    }
}
