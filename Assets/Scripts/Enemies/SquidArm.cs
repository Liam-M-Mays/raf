using UnityEngine;

public class SquidArm : MonoBehaviour
{
    private EnemyRangedAttack parentScript; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parentScript = GetComponentInParent<EnemyRangedAttack>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FireProjectileFromArm()
    {
        parentScript.FireProjectile();
    }


}
