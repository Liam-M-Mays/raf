using UnityEngine;

/// <summary>
/// Shared context object passed between all behavior nodes
/// Contains all the data that nodes might need to execute
/// </summary>
public class BehaviorContext
{
    // Core references
    public Transform self;
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

    // Additional data for specific behaviors
    public object customData; // Can store behavior-specific data
    
    
    
    public BehaviorContext(Transform _self, Transform _target, Animator _anim)
    {
        self = _self;
        target = _target;
        anim = _anim;
        sharkLayer = LayerMask.GetMask("Shark");
        deltaTime = Time.deltaTime;
    }
    

    public void UpdateFrame()
    {
        deltaTime = Time.deltaTime;
        distanceToTarget = Vector2.Distance(self.position, target.position);
        lastPosition = self.position;
    }
}
