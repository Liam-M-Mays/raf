using UnityEngine;

/// <summary>
/// Interfaces for better separation of concerns in behavior animation management.
/// These can be implemented by external components instead of coupling to BehaviorContext.
/// </summary>

/// <summary>
/// Handles animation state management for behaviors
/// </summary>
public interface IAnimationController
{
    void SetBool(string paramName, bool value);
    void SetTrigger(string paramName);
    void ResetTrigger(string paramName);
    Animator GetAnimator();
}

/// <summary>
/// Handles health and hittable state management
/// </summary>
public interface IHittableTarget
{
    bool IsHittable { get; set; }
    Health GetHealth();
}

/// <summary>
/// Default implementation of IAnimationController using Animator
/// </summary>
public class AnimatorController : IAnimationController
{
    private Animator animator;

    public AnimatorController(Animator anim)
    {
        animator = anim;
    }

    public void SetBool(string paramName, bool value)
    {
        if (animator != null)
            animator.SetBool(paramName, value);
    }

    public void SetTrigger(string paramName)
    {
        if (animator != null)
            animator.SetTrigger(paramName);
    }

    public void ResetTrigger(string paramName)
    {
        if (animator != null)
            animator.ResetTrigger(paramName);
    }

    public Animator GetAnimator()
    {
        return animator;
    }
}

/// <summary>
/// Implementation of IHittableTarget using a MonoBehaviour component
/// </summary>
public class HittableComponent : IHittableTarget
{
    private bool isHittable = true;
    private Health health;

    public bool IsHittable
    {
        get => isHittable;
        set => isHittable = value;
    }

    public HittableComponent(Health healthComponent)
    {
        health = healthComponent;
    }

    public Health GetHealth()
    {
        return health;
    }
}
