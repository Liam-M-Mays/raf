using UnityEngine;

public static class AnimatorUtils
{
    public static bool HasParameter(Animator animator, string paramName)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters)
        {
            if (p.name == paramName) return true;
        }
        return false;
    }

    public static void SafeSetBool(Animator animator, string param, bool value)
    {
        if (animator == null) return;
        if (HasParameter(animator, param)) animator.SetBool(param, value);
    }

    public static void SafeSetTrigger(Animator animator, string param)
    {
        if (animator == null) return;
        if (HasParameter(animator, param)) animator.SetTrigger(param);
    }

    public static void SafeSetFloat(Animator animator, string param, float value)
    {
        if (animator == null) return;
        if (HasParameter(animator, param)) animator.SetFloat(param, value);
    }
}
