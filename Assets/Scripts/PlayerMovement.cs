using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Debug - Runtime Info")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private Vector2 currentVelocity;
    [SerializeField] private float currentDragForce;

    [Header("Player Walking")]
    public float walkSpeed = 5f;
    public GameObject playerSprite;

    [Header("Raft Physics")]
    public Rigidbody2D raftRB;
    public Animator raftAnim;
    public float paddleForce = 10f;
    public float maxSpeed = 8f;
    public float acceleration = 15f;
    public float deceleration = 8f;
    public float waterDrag = 2f;

    [Header("Camera")]
    public CinemachineCamera vcam;
    public float zoomOut = 6f;
    public float zoomIn = 3f;
    public float zoomSpeed = 0.5f;
    
    [Header("Raft Boundary")]
    public Collider2D walkable;
    public Transform feetAnchor;
    public float boundaryPadding = 0.02f;

    [Header("Upgrades")]
    public GameObject parentGameObject;
    public ItemSO Sheets = null;
    public ItemSO Frame = null;
    public ItemSO Barbed = null;

    // Components
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer baseSprite;
    private SpriteRenderer frameSprite;
    private SpriteRenderer barbedSprite;

    // Input
    private InputAction moveAction;
    private InputAction interactAction;

    // State
    private bool rowing = false;
    private bool isFacingRight = true;
    private Vector3 feetLocalOffset;
    private float extraForce = 0f;

    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        moveAction = InputSystem.actions.FindAction("Move");
        interactAction = InputSystem.actions.FindAction("Interact");
        feetLocalOffset = transform.InverseTransformPoint(feetAnchor.position);

        // Get upgrade sprites
        baseSprite = parentGameObject.transform.Find("Base Sprite").GetComponent<SpriteRenderer>();
        frameSprite = parentGameObject.transform.Find("Frame Sprite").GetComponent<SpriteRenderer>();
        barbedSprite = parentGameObject.transform.Find("Barb Sprite").GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        UpdateUpgrades();
        HandleInput();
        UpdateDebugInfo();
    }

    void FixedUpdate()
    {
        if (rowing)
        {
            HandleRaftPhysics();
        }
    }

    void LateUpdate()
    {
        if (!rowing)
        {
            ClampPlayerToRaft();
        }
    }

    // === UPGRADE MANAGEMENT ===
    void UpdateUpgrades()
    {
        // Sheets upgrade (speed boost)
        if (Sheets != null)
        {
            baseSprite.sprite = Sheets.upgradeSprite;
            extraForce = Sheets.speedEffect;
            baseSprite.enabled = true;
        }
        else
        {
            baseSprite.enabled = false;
            extraForce = 0f;
        }

        // Frame upgrade (weight/mass)
        if (Frame != null)
        {
            frameSprite.sprite = Frame.upgradeSprite;
            raftRB.mass = Frame.weightEffect;
            frameSprite.enabled = true;
        }
        else
        {
            frameSprite.enabled = false;
            raftRB.mass = 1f;
        }

        // Barbed upgrade
        if (Barbed != null)
        {
            barbedSprite.sprite = Barbed.upgradeSprite;
            barbedSprite.enabled = true;
        }
        else
        {
            barbedSprite.enabled = false;
        }
    }

    // === INPUT HANDLING ===
    void HandleInput()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        // Toggle rowing mode
        if (interactAction.triggered)
        {
            rowing = !rowing;
        }

        if (rowing)
        {
            HandleRowingMode(input);
        }
        else
        {
            HandleWalkingMode(input);
        }
    }

    void HandleRowingMode(Vector2 input)
    {
        playerSprite.SetActive(false);
        raftAnim.SetBool("Paddle", true);
        vcam.Lens.OrthographicSize = Mathf.Lerp(vcam.Lens.OrthographicSize, zoomOut, zoomSpeed * Time.deltaTime);
        
        // Sync player to raft
        rb.linearVelocity = raftRB.linearVelocity;
        anim.SetBool("Moving", false);
        
        // Paddle animations
        if (input.x > 0) 
            raftAnim.SetTrigger("PaddleRight");
        else if (input.x < 0) 
            raftAnim.SetTrigger("PaddleLeft");
        else if (input.y != 0) 
            raftAnim.SetTrigger("PaddleRight");
    }

    void HandleWalkingMode(Vector2 input)
    {
        playerSprite.SetActive(true);
        raftAnim.SetBool("Paddle", false);
        vcam.Lens.OrthographicSize = Mathf.Lerp(vcam.Lens.OrthographicSize, zoomIn, zoomSpeed * Time.deltaTime);
        
        // Walk relative to raft
        rb.linearVelocity = (input * walkSpeed) + raftRB.linearVelocity;
        
        // Animation & flip
        anim.SetBool("Moving", input != Vector2.zero);
        if (input.x > 0 && !isFacingRight) 
            Flip();
        else if (input.x < 0 && isFacingRight) 
            Flip();
    }

    // === RAFT PHYSICS ===
    void HandleRaftPhysics()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        
        if (input != Vector2.zero)
        {
            // Acceleration phase
            Vector2 targetDirection = input.normalized;
            float currentSpeedInDirection = Vector2.Dot(raftRB.linearVelocity, targetDirection);
            
            // Apply acceleration force
            raftRB.AddForce(targetDirection * (paddleForce + extraForce) * acceleration * Time.fixedDeltaTime);
            
            // Counter-force for tighter control
            raftRB.AddForce(-raftRB.linearVelocity * (waterDrag * 0.5f) * Time.fixedDeltaTime);
        }
        else
        {
            // Deceleration phase (natural water drag)
            raftRB.AddForce(-raftRB.linearVelocity * waterDrag * deceleration * Time.fixedDeltaTime);
        }
        
        // Cap max speed
        if (raftRB.linearVelocity.magnitude > maxSpeed)
        {
            raftRB.linearVelocity = raftRB.linearVelocity.normalized * maxSpeed;
        }
    }

    // === UTILITY ===
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

    void UpdateDebugInfo()
    {
        currentSpeed = raftRB.linearVelocity.magnitude;
        currentVelocity = raftRB.linearVelocity;
        currentDragForce = waterDrag * deceleration;
    }
}