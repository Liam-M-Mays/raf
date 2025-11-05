using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    public CinemachineCamera vcam;  // drag your vcam here
    public float zoomOut = 6f;
    public float zoomSpeed = .5f;
    public float zoomIn = 3f;
    private Transform tf;    
    private Animator anim;
    private bool isFacingRight = true;
    private InputAction moveAction;
    private InputAction interactAction;
    private bool rowing = false;
    private Vector2 stamp;

    public Rigidbody2D raftRB;
    public float raftSpeed = 5f;
    public float raftAccel = 0.002f;
    public float raftDeAccel = 0.003f;
    private Vector2 targetRaftVelocity; // For lerping
    [SerializeField] Vector2 moveValue;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        tf = GetComponent<Transform>();
        anim = GetComponent<Animator>();
        moveAction = InputSystem.actions.FindAction("Move");
        interactAction = InputSystem.actions.FindAction("Interact");
        feetLocalOffset = transform.InverseTransformPoint(feetAnchor.position);

    }

    void Update()
    {

        moveValue = moveAction.ReadValue<Vector2>();
        // Toggle betwen player and raft control
        if (interactAction.triggered)
        {
            rowing = !rowing;
        }

        if (rowing)
        {
            vcam.Lens.OrthographicSize = Mathf.Lerp(vcam.Lens.OrthographicSize, zoomOut, zoomSpeed);
            targetRaftVelocity = moveValue * raftSpeed;
            if (targetRaftVelocity != Vector2.zero) raftRB.linearVelocity = Vector2.Lerp(raftRB.linearVelocity, targetRaftVelocity, raftAccel);
            else raftRB.linearVelocity = Vector2.Lerp(raftRB.linearVelocity, targetRaftVelocity, raftDeAccel);
            rb.linearVelocity = raftRB.linearVelocity;
            anim.SetBool("Moving", false);
        }
        else
        {
            vcam.Lens.OrthographicSize = Mathf.Lerp(vcam.Lens.OrthographicSize, zoomIn, zoomSpeed);
            raftRB.linearVelocity = Vector2.Lerp(raftRB.linearVelocity, Vector2.zero, raftDeAccel);
            rb.linearVelocity = (moveValue * moveSpeed) + raftRB.linearVelocity;
            if (moveValue != Vector2.zero)
            {
                anim.SetBool("Moving", true);
            }
            else
            {
                anim.SetBool("Moving", false);
            }

            if (moveValue.x > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (moveValue.x < 0 && isFacingRight)
            {
                Flip();
            }
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // On Player (child of raft)
    [Header("Walkable shape (collider on the raft)")]
    public Collider2D walkable;    // collider on the raft that defines the walkable area (isTrigger = true)
    public float skin = 0.02f;

    [Header("Anchors")]
    public Transform feetAnchor;    // child at the bottom of your sprite

    // cache: vector from player root to feet in the player’s local space
    private Vector3 feetLocalOffset;

    void LateUpdate()
    {
        // Proposed new root position (whatever your movement set this frame)
        Vector3 rootDesiredWorld = transform.position;

        // Where would the FEET be if we moved there?
        Vector3 feetDesiredWorld = rootDesiredWorld + transform.TransformVector(feetLocalOffset);

        // If feet are inside the walkable shape, accept
        if (walkable.OverlapPoint(feetDesiredWorld))
        {
            return;
        }

        // Otherwise snap the FEET to the nearest boundary, nudged inward by 'skin'
        Vector2 nearestWorld = walkable.ClosestPoint(feetDesiredWorld);

        // Reasonable inward direction: toward the shape's centroid
        Vector2 centroidWorld = walkable.bounds.center; // good enough; or cache a true polygon centroid
        Vector2 inwardDir = ((Vector2)centroidWorld - nearestWorld).normalized;

        Vector2 feetClampedWorld = nearestWorld + inwardDir * skin;

        // Move the ROOT so that the FEET end up at feetClampedWorld
        Vector3 rootClampedWorld = (Vector3)feetClampedWorld - transform.TransformVector(feetLocalOffset);
        transform.position = rootClampedWorld;
    }

}