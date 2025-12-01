using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class ScrollZoomController : MonoBehaviour
{
    [Header("Zoom Settings")]
    public CinemachineCamera vcam;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 10f;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float smoothSpeed = 5f;
    
    private float currentZoom = 5f;
    private float targetZoom = 5f;
    
    // Reference to your Input Actions
    //private PlayerInput playerInput;
    private InputAction scrollAction;
    
    void Awake()
    {
        // Get the PlayerInput component
        //playerInput = GetComponent<PlayerInput>();
        
        // Get the scroll action from your Input Actions asset
        // Replace "Player/Scroll" with your actual action map and action name
        scrollAction = InputSystem.actions.FindAction("Scroll");
    }
    
    void OnEnable()
    {
        scrollAction.Enable();
    }
    
    void OnDisable()
    {
        scrollAction.Disable();
    }
    
    void Update()
    {
        // Read scroll input
        float scrollInput = scrollAction.ReadValue<Vector2>().y;
        
        // Update target zoom based on scroll input
        if (scrollInput != 0)
        {
            targetZoom -= scrollInput * zoomSpeed * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
        
        // Smoothly interpolate to target zoom
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, smoothSpeed * Time.deltaTime);
        
        // Apply zoom (example: orthographic camera)
        vcam.Lens.OrthographicSize  = currentZoom;
        
        // For perspective camera, you'd modify field of view instead:
        // Camera.main.fieldOfView = currentZoom;
    }
    
    // Alternative: Direct scroll method without smoothing
    public void OnScroll(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            float scrollValue = context.ReadValue<Vector2>().y;
            targetZoom -= scrollValue * zoomSpeed * 0.1f;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
    }
}