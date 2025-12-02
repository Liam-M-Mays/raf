using UnityEngine;

/// <summary>
/// Shared context object passed between all behavior nodes
/// Contains all the data that nodes might need to execute
/// </summary>
public class BehaviorContext
{
    // Core references
    public Transform self;
    public Health health;
    public Transform target;
    public Animator anim;
    public LayerMask sharkLayer;
    
    // Movement parameters
    public float maxSpeed;
    public float speed;
    public float currentVelocity;

    // Range parameters
    public float attackRange;
    public float attackRangeMax;
    public float outOfRange;
    public float respawnRange;
    
    // Separation parameters
    public float divergeRange;
    public float divergeWeight;
    
    // Runtime data (can be modified by nodes)
    public float deltaTime;
    public Vector2 lastPosition;
    public float distanceToTarget;

    // Underwater mechanic
    public UnderwaterState underwaterState;
    
    // Tactical decision system
    public TacticalDecision tacticalDecision;
    
    // Additional data for specific behaviors
    public object customData; // Can store behavior-specific data
    public bool hittable = true;
    
    public BehaviorContext(Transform _self, Transform _target, Animator _anim)
    {
        self = _self;
        health = _self != null ? _self.GetComponent<Health>() : null;
        target = _target;
        anim = _anim;
        sharkLayer = LayerMask.GetMask("Enemy");
        deltaTime = Time.deltaTime;

        // Initialize new systems
        underwaterState = new UnderwaterState();
        tacticalDecision = new TacticalDecision();
    }

    

    public void UpdateFrame()
    {
        deltaTime = Time.deltaTime;
        distanceToTarget = Vector2.Distance(self.position, target.position);
        lastPosition = self.position;
    }
}
