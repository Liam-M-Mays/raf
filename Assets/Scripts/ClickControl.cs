using UnityEngine;
using UnityEngine.InputSystem;

public class ClickControl : MonoBehaviour
{
    public Camera cam;
    private InputAction click;

    void Start()
    {
        click = InputSystem.actions.FindAction("Cik");
    }
    void Update()
    {
        if(click.triggered) OnCik();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnCik()
    {
        Debug.Log("click");

        if (cam == null) cam = Camera.main;

        // Raycast from mouse position
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null)
        {
            Destroy(hit.collider.gameObject);
        }

    }
}
