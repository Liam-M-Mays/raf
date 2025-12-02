using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// FormationManager: Assigns and tracks deterministic pack member slots.
/// Provides tight, coordinated formations where members know their position and role.
/// </summary>
public class FormationManager : MonoBehaviour
{
    private List<EnemyController> members = new List<EnemyController>();
    private Vector3 formationCenter;
    private float formationRadius = 2f;
    private bool isActive = true;

    public static FormationManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!isActive) return;

        formationCenter = GameServices.GetRaft().position;

        // Update member positions in formation slots
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i] == null)
            {
                members.RemoveAt(i);
                i--;
                continue;
            }

            float angle = (i / (float)Mathf.Max(1, members.Count)) * 360f * Mathf.Deg2Rad;
            Vector3 slotPos = formationCenter + new Vector3(
                Mathf.Cos(angle) * formationRadius,
                Mathf.Sin(angle) * formationRadius,
                0
            );

            // Store slot position in custom data for behaviors to use
            var behavior = members[i].currentBehavior();
            if (behavior?.CTX() != null)
            {
                behavior.CTX().customData = slotPos;
            }
        }
    }

    /// <summary>
    /// Register an enemy as part of the formation pack.
    /// </summary>
    public void RegisterMember(EnemyController member)
    {
        if (!members.Contains(member))
        {
            members.Add(member);
        }
    }

    /// <summary>
    /// Unregister an enemy from the formation pack.
    /// </summary>
    public void UnregisterMember(EnemyController member)
    {
        members.Remove(member);
    }

    public int GetMemberCount() => members.Count;
    public Vector3 GetFormationCenter() => formationCenter;
    public void SetFormationRadius(float radius) => formationRadius = radius;
}
