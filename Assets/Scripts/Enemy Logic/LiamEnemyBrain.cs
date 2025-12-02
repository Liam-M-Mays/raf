using UnityEngine;
using System;

/// <summary>
/// Controls enemy behavior and animation for a single enemy instance.
/// Manages the active behavior and visual updates.
/// </summary>
public class EnemyController : MonoBehaviour
{
    public BehaviorManager manager = new BehaviorManager();
    private Transform raftTarget;
    private SpriteRenderer spriteRenderer;
    private int cost;
    private float lastY;
    private bool needsSortingUpdate = true;

    public void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        raftTarget = GameServices.GetRaft();
        
        if (raftTarget == null)
        {
            Debug.LogError($"EnemyController on {gameObject.name}: Could not find Raft. Ensure it has the 'Raft' tag.");
            enabled = false;
            return;
        }
        
        lastY = transform.position.y;
    }

    public void Configure(EnemySO logic)
    {
        if (logic == null)
        {
            Debug.LogError($"EnemyController on {gameObject.name}: Attempted to configure with null EnemySO.");
            return;
        }

        cost = logic.cost;
        var behavior = logic.getBehavior().CreateRuntimeBehavior();
        Animator animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"EnemyController on {gameObject.name}: Missing Animator component.");
            return;
        }

        manager.ChangeBehavior(behavior, transform, animator);
        spriteRenderer.color = logic.color;
        
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.SetHealth(logic.health);
            // Subscribe to damage events to allow underwater/dive reactions
            health.OnDamaged.AddListener((float dmg) => {
                var beh = manager.Current;
                if (beh != null)
                {
                    var ctx = beh.CTX();
                    if (ctx != null && ctx.underwaterState != null)
                    {
                        // Small chance to dive when damaged
                        UnderwaterManager.TriggerDiveOnDamage(ctx, ctx.underwaterState, 0.25f);
                    }
                }
            });
        }
        else
        {
            Debug.LogWarning($"EnemyController on {gameObject.name}: Missing Health component.");
        }

        if (TryGetComponent<EnemyMeleeAttack>(out var attack))
        {
            attack.SetDamage(logic.damage);
        }
        else if (TryGetComponent<EnemyRangedAttack>(out var rangedAttack))
        {
            rangedAttack.SetDamage(logic.damage);
        }
    }
    void Update()
    {
        if (raftTarget == null)
            return;

        manager.Tick();
        
        // Only update sorting order when enemy crosses the raft's Y position
        if (needsSortingUpdate || Mathf.Sign(transform.position.y - lastY) != Mathf.Sign(lastY - raftTarget.position.y))
        {
            UpdateSortingOrder();
            needsSortingUpdate = false;
        }
        lastY = transform.position.y;
    }

    private void UpdateSortingOrder()
    {
        if (raftTarget == null || spriteRenderer == null)
            return;

        SpriteRenderer raftRenderer = raftTarget.GetComponentInChildren<SpriteRenderer>();
        if (raftRenderer == null)
        {
            Debug.LogWarning("EnemyController: Raft does not have a SpriteRenderer.");
            return;
        }

        if (transform.position.y > raftTarget.position.y)
        {
            spriteRenderer.sortingOrder = raftRenderer.sortingOrder - 1;
        }
        else
        {
            spriteRenderer.sortingOrder = raftRenderer.sortingOrder + 1;
        }
    } 

    void LateUpdate() => manager.LateTick();

    public IBehavior currentBehavior()
    {
        return manager.Current;
    }

    void OnDestroy() 
    {
        PlayerMovement player = GameServices.GetPlayerMovement();
        if (player != null)
        {
            player.PlayerCurrency += cost * 2;
        }
        else
        {
            Debug.LogWarning("EnemyController: Could not find PlayerMovement when enemy was destroyed.");
        }
        manager.Exit();
    }
}

// Keep the old name as an alias for backward compatibility
public class LiamEnemyBrain : EnemyController { }
