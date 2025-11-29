using UnityEngine;
using UnityEngine.InputSystem;
<<<<<<< HEAD
=======
using System.Collections.Generic;
>>>>>>> fdf3668fa4387896625b63e1da8b72f57f5347f7
using Unity.Cinemachine;

public class PlayerMovement : MonoBehaviour
{
<<<<<<< HEAD
    #region Serialized Fields

    [Header("Debug - Runtime Info")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private Vector2 currentVelocity;
    [SerializeField] private float currentDragForce;

    [Header("Player")]
    public float moveSpeed = 5f;
    public GameObject playerSprite;

=======
    [Header("Player Walking")]
    public float walkSpeed = 5f;
    public GameObject playerSprite;
    
    [Header("Raft Rowing")]
    public float paddleForce = 100f;
    private float extraForce = 0f;
    public float maxSpeed = 5f;
    public float waterDrag = 3f;
    private float weight = 1f;
    public Rigidbody2D raftRB;
    public Animator raftAnim;
    
>>>>>>> fdf3668fa4387896625b63e1da8b72f57f5347f7
    [Header("Camera")]
    public CinemachineCamera vcam;
    public float zoomOut = 6f;
    public float zoomIn = 3f;
    public float zoomSpeed = 0.5f;
<<<<<<< HEAD

    [Header("Raft")]
    public Rigidbody2D raftRB;
    public Animator raftAnim;
    public PaddleStats paddleStats;
    public RaftStats raftStats;
    public EnvironmentStats environmentStats;

    [Header("Raft Boundary")]
    [Tooltip("Collider on the raft that defines walkable area (isTrigger = true)")]
    public Collider2D walkable;
    public Transform feetAnchor;
    public float skin = 0.02f;

    [Header("Debug")]
    [SerializeField] private Vector2 moveValue;

    #endregion

    #region Private Variables

    private Rigidbody2D rb;
    private Transform tf;
    private Animator anim;
    private InputAction moveAction;
    private InputAction interactAction;

=======
    
    [Header("Raft Boundary")]
    public Collider2D walkable;
    public Transform feetAnchor;
    public float boundaryPadding = 0.02f;

    [Header("Upgrades")]
    public GameObject parentGameObject;
    public ItemSO Sheets = null;
    public ItemSO Frame = null;
    public ItemSO Barbed = null;
    private SpriteRenderer Base = null;
    private SpriteRenderer frame = null;
    private SpriteRenderer barbed = null;

    private Rigidbody2D rb;
    private Animator anim;
    private InputAction moveAction;
    private InputAction interactAction;
>>>>>>> fdf3668fa4387896625b63e1da8b72f57f5347f7
    private bool rowing = false;
    private bool isFacingRight = true;
    private Vector3 feetLocalOffset;

<<<<<<< HEAD
    #endregion

    #region Stats Classes

    [System.Serializable]
    public class PaddleStats
    {
        [Header("Force & Speed")]
        [Tooltip("How hard each paddle stroke pushes")]
        public float paddleForce = 100f;

        [Tooltip("Maximum speed this paddle can achieve")]
        public float maxSpeed = 5f;

        [Header("Feel")]
        [Tooltip("How quickly paddle responds - 0 = sluggish, 1 = instant")]
        [Range(0f, 1f)]
        public float responsiveness = 0.8f;
    }

    [System.Serializable]
    public class RaftStats
    {
        [Header("Physical Properties")]
        [Tooltip("Mass of the raft - affects acceleration and momentum")]
        public float mass = 50f;

        [Tooltip("Water resistance coefficient")]
        public float dragCoefficient = 3f;

        [Tooltip("Surface area in contact with water")]
        public float surfaceArea = 4f;

        [Header("Feel")]
        [Tooltip("How well raft maintains speed - higher = more glide")]
        [Range(0f, 1f)]
        public float momentumRetention = 0.3f;
    }

    [System.Serializable]
    public class EnvironmentStats
    {
        [Header("Wind")]
        [Tooltip("Direction wind is blowing")]
        public Vector2 windDirection = Vector2.right;

        [Tooltip("Wind speed in units/second")]
        public float windSpeed = 1f;

        [Tooltip("How much wind affects this raft")]
        [Range(0f, 1f)]
        public float windInfluence = 0.3f;

        [Header("Water Current")]
        [Tooltip("Current direction and speed combined")]
        public Vector2 waterCurrent = Vector2.zero;
    }

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        tf = GetComponent<Transform>();
        anim = GetComponent<Animator>();

        // Setup input actions
        moveAction = InputSystem.actions.FindAction("Move");
        interactAction = InputSystem.actions.FindAction("Interact");

        // Cache feet offset for boundary clamping
        feetLocalOffset = transform.InverseTransformPoint(feetAnchor.position);
=======
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        moveAction = InputSystem.actions.FindAction("Move");
        interactAction = InputSystem.actions.FindAction("Interact");
        feetLocalOffset = transform.InverseTransformPoint(feetAnchor.position);
        Base = parentGameObject.transform.Find("Base Sprite").gameObject.GetComponent<SpriteRenderer>();
        frame = parentGameObject.transform.Find("Frame Sprite").gameObject.GetComponent<SpriteRenderer>();
        barbed = parentGameObject.transform.Find("Barb Sprite").gameObject.GetComponent<SpriteRenderer>();
>>>>>>> fdf3668fa4387896625b63e1da8b72f57f5347f7
    }

    void Update()
    {
<<<<<<< HEAD
        // Read input
        moveValue = moveAction.ReadValue<Vector2>();

        // Toggle between player and raft control
=======
        if (Sheets != null)
        {
            Base.sprite = Sheets.upgradeSprite;
            extraForce = Sheets.speedEffect;
            Base.enabled = true;
        }
        else
        {
            Base.enabled = false;
            extraForce = 0f;
        }
        if (Frame != null)
        {
            frame.sprite = Frame.upgradeSprite;
            raftRB.mass = Frame.weightEffect;
            frame.enabled = true;
        }
        else
        {
            frame.enabled = false;
            raftRB.mass = 1f;
        }
        if (Barbed != null)
        {
            barbed.sprite = Barbed.upgradeSprite;
            barbed.enabled = true;
        }
        else
        {
            barbed.enabled = false;
        }

        Vector2 input = moveAction.ReadValue<Vector2>();

        // Toggle rowing mode
>>>>>>> fdf3668fa4387896625b63e1da8b72f57f5347f7
        if (interactAction.triggered)
        {
            rowing = !rowing;
        }

<<<<<<< HEAD
        // Handle rowing vs walking mode
        if (rowing)
        {
            HandleRowingMode();
            switch (moveValue.x)
            {
                case 1:
                    raftAnim.SetTrigger("PaddleRight");
                    break;
                case -1:
                    raftAnim.SetTrigger("PaddleLeft");
                    break;
                case 0:
                    if (moveValue.y != 0) {
                        raftAnim.SetTrigger("PaddleRight");
                    }
                    break;
            }
        }
        else
        {
            HandleWalkingMode();
=======
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
>>>>>>> fdf3668fa4387896625b63e1da8b72f57f5347f7
        }
    }

    void FixedUpdate()
    {
<<<<<<< HEAD
        // Calculate all forces acting on the raft
        Vector2 playerForce;
        if (!rowing) playerForce = Vector2.zero;
        else playerForce = CalculatePlayerForce(moveValue); ;
        Vector2 dragForce = CalculateDrag();
        Vector2 environmentForce = CalculateEnvironment();

        // Sum and apply
        Vector2 totalForce = playerForce + dragForce + environmentForce;
        ApplyForceToRaft(totalForce);
        // UPDATE DEBUG INFO
        UpdateDebugInfo(dragForce);
    }

    void UpdateDebugInfo(Vector2 dragForce)
    {
        currentVelocity = raftRB.linearVelocity;
        currentSpeed = raftRB.linearVelocity.magnitude;
        currentDragForce = dragForce.magnitude;
=======
        if (rowing)
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            
            // Apply paddle force
            if (input != Vector2.zero)
            {
                raftRB.AddForce(input.normalized * (paddleForce + extraForce));
            }
            
            // Water drag (smooth deceleration)
            raftRB.AddForce(-raftRB.linearVelocity  * waterDrag);
            
            // Cap max speed
            if (raftRB.linearVelocity.magnitude > maxSpeed)
            {
                raftRB.linearVelocity = raftRB.linearVelocity.normalized * maxSpeed;
            }
        }
>>>>>>> fdf3668fa4387896625b63e1da8b72f57f5347f7
    }

    void LateUpdate()
    {
<<<<<<< HEAD
        // Keep player within raft boundaries when walking
        if (!rowing)
        {
            ClampPlayerToRaftBounds();
        }
    }

    #endregion

    #region Movement Modes

    void HandleRowingMode()
    {
        // turn off the player sprite
        playerSprite.SetActive(false);

        raftAnim.SetBool("Paddle", true);

        // Zoom out camera
        vcam.Lens.OrthographicSize = Mathf.Lerp(
            vcam.Lens.OrthographicSize,
            zoomOut,
            zoomSpeed * Time.deltaTime
        );

        // Physics handles raft movement in FixedUpdate
        // Sync player velocity to raft
        rb.linearVelocity = raftRB.linearVelocity;

        // Stop walking animation
        anim.SetBool("Moving", false);
    }

    void HandleWalkingMode()
{
    // turn on the player sprite
    playerSprite.SetActive(true);
    raftAnim.SetBool("Paddle", false);

    // Zoom in camera
    vcam.Lens.OrthographicSize = Mathf.Lerp(
        vcam.Lens.OrthographicSize,
        zoomIn,
        zoomSpeed * Time.deltaTime
    );

    // Player walks relative to raft
    rb.linearVelocity = (moveValue * moveSpeed) + raftRB.linearVelocity;

    // Handle animations
    bool isMoving = moveValue != Vector2.zero;
    anim.SetBool("Moving", isMoving);

    // Handle sprite flipping - DISABLED, now controlled by WeaponAiming
    /*
    if (moveValue.x > 0 && !isFacingRight)
    {
        Flip();
    }
    else if (moveValue.x < 0 && isFacingRight)
    {
        Flip();
    }
    */
}

    #endregion

    #region Physics Calculations

    Vector2 CalculatePlayerForce(Vector2 moveInput)
    {
        // No input = no force
        if (moveInput == Vector2.zero)
            return Vector2.zero;

        // Normalize input so diagonal isn't faster
        Vector2 inputDirection = moveInput.normalized;

        // Scale paddle force by responsiveness
        float actualForce = paddleStats.paddleForce * paddleStats.responsiveness;

        return inputDirection * actualForce;
    }

    Vector2 CalculateDrag()
    {
        // No drag when stationary
        if (raftRB.linearVelocity.magnitude < 0.01f)
            return Vector2.zero;

        float currentSpeed = raftRB.linearVelocity.magnitude;
        Vector2 velocityDirection = raftRB.linearVelocity.normalized;

        // Linear drag (proportional to speed)
        float linearDrag = raftStats.dragCoefficient *
                          raftStats.surfaceArea *
                          currentSpeed;

        // Quadratic drag (proportional to speed²) - realistic water resistance
        float quadraticDrag = 0.05f * currentSpeed * currentSpeed;

        float totalDrag = linearDrag + quadraticDrag;

        // Apply momentum retention (higher = less drag = more glide)
        totalDrag *= (1f - raftStats.momentumRetention);

        // Drag opposes movement direction
        return -velocityDirection * totalDrag;
    }

    Vector2 CalculateEnvironment()
    {
        Vector2 windForce = Vector2.zero;
        Vector2 currentForce = Vector2.zero;

        // Wind - constant push in one direction
        if (environmentStats.windSpeed > 0)
        {
            Vector2 windPush = environmentStats.windDirection.normalized *
                              environmentStats.windSpeed;

            windForce = windPush *
                       environmentStats.windInfluence *
                       raftStats.surfaceArea;
        }

        // Water current - pulls velocity toward current velocity
        if (environmentStats.waterCurrent != Vector2.zero)
        {
            Vector2 velocityDifference = environmentStats.waterCurrent - raftRB.linearVelocity;
            currentForce = velocityDifference * 0.5f;
        }

        return windForce + currentForce;
    }

    void ApplyForceToRaft(Vector2 totalForce)
    {
        // F = ma, therefore a = F/m
        Vector2 acceleration = totalForce / raftStats.mass;

        // Integrate acceleration into velocity
        raftRB.linearVelocity += acceleration * Time.fixedDeltaTime;

        // Clamp to paddle's maximum speed
        if (raftRB.linearVelocity.magnitude > paddleStats.maxSpeed)
        {
            raftRB.linearVelocity = raftRB.linearVelocity.normalized * paddleStats.maxSpeed;
        }

    }

    #endregion

    #region Helper Methods

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void ClampPlayerToRaftBounds()
    {
        // Proposed root position (from movement this frame)
        Vector3 rootDesiredWorld = transform.position;

        // Where would feet be at this position?
        Vector3 feetDesiredWorld = rootDesiredWorld + transform.TransformVector(feetLocalOffset);

        // If feet are inside walkable area, we're good
        if (walkable.OverlapPoint(feetDesiredWorld))
        {
            return;
        }

        // Otherwise, snap feet to nearest boundary point (nudged inward)
        Vector2 nearestWorld = walkable.ClosestPoint(feetDesiredWorld);

        // Calculate inward direction (toward center of walkable area)
        Vector2 centroidWorld = walkable.bounds.center;
        Vector2 inwardDir = ((Vector2)centroidWorld - nearestWorld).normalized;

        // Apply skin distance to prevent edge clipping
        Vector2 feetClampedWorld = nearestWorld + inwardDir * skin;

        // Move root so feet end up at clamped position
        Vector3 rootClampedWorld = (Vector3)feetClampedWorld - transform.TransformVector(feetLocalOffset);
        transform.position = rootClampedWorld;
    }

    #endregion
}

/*


PaddleStats
        paddleForce
        What it affects: How hard you push the raft with each input
        Higher = faster acceleration
        Lower = sluggish, weak paddling

        maxSpeed
        What it affects: Top speed cap
        The raft velocity will never exceed this value

        responsiveness
        What it affects: How "instant" the paddle feels
        1.0 = Full force applied immediately (snappy)
        0.5 = Only 50% of paddle force applied (delayed, heavy feeling)
        0.0 = Almost no response (barely moves)


RaftStats
        mass
        What it affects: How heavy the raft is
        Higher = slower to accelerate, slower to stop, stable in wind
        Lower = quick to accelerate, quick to stop, pushed around by wind
        Physics formula: acceleration = force / mass

        dragCoefficient
        What it affects: Base water resistance
        Higher = more resistance, stops faster when you stop paddling
        Lower = less resistance, glides more

        surfaceArea
        What it affects: How much raft is touching water
        Larger surface = more drag (both from water and wind)
        Smaller surface = cuts through water easier
        Multiplies with dragCoefficient to calculate total drag
        Big flat raft = high surface area, narrow kayak = low surface area

        momentumRetention
        What it affects: How much the raft "remembers" its velocity
        1.0 = no drag applied, slides forever (ice physics)
        0.5 = half drag applied, moderate glide
        0.0 = full drag applied, stops quickly


EnvironmentStats
        windDirection
        What it affects: Which way wind pushes
        Vector2.right = east (positive X)
        Vector2.up = north (positive Y)
        Vector2.left = west (negative X)
        Vector2.down = south (negative Y)
        Can be diagonal: new Vector2(1, 1) = northeast

        windSpeed
        What it affects: How strong the wind is
        Higher = stronger push
        0 = no wind
        Combines with surfaceArea to determine actual force
        windInfluence = 0.3f
        What it affects: How much wind affects THIS raft
        0.0 = wind has no effect (heavy, aerodynamic)
        1.0 = wind has maximum effect (light, sail-like)
        Tuning knob to balance wind without making it overpowered

        waterCurrent
        What it affects: River flow that pulls you along
        Both direction AND speed in one vector
        new Vector2(2, 0) = current flowing east at 2 units/sec
        new Vector2(0, -3) = current flowing south at 3 units/sec
        Creates a "moving walkway" effect


*/
=======
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
>>>>>>> fdf3668fa4387896625b63e1da8b72f57f5347f7
