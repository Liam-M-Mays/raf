using UnityEngine;

public class ShopIndicator : MonoBehaviour
{
    private bool useIndicator = false;
    public GameObject indicator;   
    private Transform target; 
    public float hideDistance = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (useIndicator) {
            Vector3 dir = target.position - transform.position;
            if (dir.magnitude < hideDistance) {
                indicator.SetActive(false);
            } else {
                indicator.SetActive(true);
                float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
                indicator.transform.rotation = Quaternion.AngleAxis(-angle, Vector3.forward);
            }
        }
    }

    public void ShowIndicator() {
        useIndicator = true;
        target = GameObject.FindGameObjectWithTag("Shop")?.transform;
    }

    public void HideIndicator() {
        useIndicator = false;
        indicator.SetActive(false);
        target = null;
    }
}
