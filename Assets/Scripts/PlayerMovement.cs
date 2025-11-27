using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Walking")]
    public float walkSpeed = 5f;
    public GameObject playerSprite;
    
    [Header("Raft Rowing")]
    public float paddleForce = 100f;
    public float maxSpeed = 5f;
    public float waterDrag = 3f;
    public Rigidbody2D raftRB;
    public Animator raftAnim;
    
    [Header("Camera")]
    public CinemachineCamera vcam;
    public float zoomOut = 6f;
    public float zoomIn = 3f;
    public float zoomSpeed = 0.5f;
    
    [Header("Raft Boundary")]
    public Collider2D walkable;
    public Transform feetAnchor;
    public float boundaryPadding = 0.02f;

    private Rigidbody2D rb;
    private Animator anim;
    private InputAction moveAction;
    private InputAction interactAction;
    private bool rowing = false;
    private bool isFacingRight = true;
    private Vector3 feetLocalOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        moveAction = InputSystem.actions.FindAction("Move");
        interactAction = InputSystem.actions.FindAction("Interact");
        feetLocalOffset = transform.InverseTransformPoint(feetAnchor.position);
    }

    void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        // Toggle rowing mode
        if (interactAction.triggered)
        {
            rowing = !rowing;
        }

        if (rowing)
        {
            // Rowing mode - control raft
            playerSprite.SetActive(false);
            raftAnim.SetBool("Paddle", true);
            vcam.Lens.OrthographicSize = Mathf.Lerp(vcam.Lens.OrthographicSize, zoomOut, zoomSpeed * Time.deltaTime);
            
            // Sync player to raft
            rb.linearVelocity = raftRB.linearVelocity;
            anim.SetBool("Moving", false);
            
            // Paddle animations
            if (input.x > 0) raftAnim.SetTrigger("PaddleRight");
            else if (input.x < 0) raftAnim.SetTrigger("PaddleLeft");
            else if (input.y != 0) raftAnim.SetTrigger("PaddleRight");
        }
        else
        {
            // Walking mode - move on raft
            playerSprite.SetActive(true);
            raftAnim.SetBool("Paddle", false);
            vcam.Lens.OrthographicSize = Mathf.Lerp(vcam.Lens.OrthographicSize, zoomIn, zoomSpeed * Time.deltaTime);
            
            // Walk relative to raft
            rb.linearVelocity = (input * walkSpeed) + raftRB.linearVelocity;
            
            // Animation & flip
            anim.SetBool("Moving", input != Vector2.zero);
            if (input.x > 0 && !isFacingRight) Flip();
            else if (input.x < 0 && isFacingRight) Flip();
        }
    }

    void FixedUpdate()
    {
        if (rowing)
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            
            // Apply paddle force
            if (input != Vector2.zero)
            {
                raftRB.AddForce(input.normalized * paddleForce);
            }
            
            // Water drag (smooth deceleration)
            raftRB.AddForce(-raftRB.linearVelocity * waterDrag);
            
            // Cap max speed
            if (raftRB.linearVelocity.magnitude > maxSpeed)
            {
                raftRB.linearVelocity = raftRB.linearVelocity.normalized * maxSpeed;
            }
        }
    }

    void LateUpdate()
    {
        if (!rowing)
        {
            ClampPlayerToRaft();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void ClampPlayerToRaft()
    {
        Vector3 feetWorld = transform.position + transform.TransformVector(feetLocalOffset);
        
        if (walkable.OverlapPoint(feetWorld)) return;
        
        Vector2 nearest = walkable.ClosestPoint(feetWorld);
        Vector2 toCenter = ((Vector2)walkable.bounds.center - nearest).normalized;
        Vector2 clamped = nearest + toCenter * boundaryPadding;
        
        transform.position = (Vector3)clamped - transform.TransformVector(feetLocalOffset);
    }
}