using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public Health matressHealth;
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI shopCurrencyText;
    [Header("Debug - Runtime Info")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private Vector2 currentVelocity;
    [SerializeField] private float currentDragForce;
    [SerializeField] public int PlayerCurrency = 0;

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
    public ItemSO Paddle = null;
    private float barbDamage = 0f;

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
    private float paddleExtraForce = 0f;
    private int sheetsHealthEffect = 0;
    private float sheetsSpeedEffect = 0f;
    private float paddleWeightEffect = 0f;
    private float frameHealthEffect = 0f;
    private float frameWeightEffect = 0f;
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
        shopCurrencyText.text = $"${PlayerCurrency}";
        currencyText.text = $"${PlayerCurrency}";
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
            sheetsHealthEffect = Sheets.maxHealthBonus;
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
            frameWeightEffect =  Frame.weightEffect;
            frameHealthEffect = Frame.maxHealthBonus;
            frameSprite.enabled = true;
        }
        else
        {
            frameSprite.enabled = false;
            frameHealthEffect = 0f;
            frameWeightEffect = 0f;
            //raftRB.mass = 1f;
        }

        // Barbed upgrade
        if (Barbed != null)
        {
            barbedSprite.sprite = Barbed.upgradeSprite;
            barbedSprite.enabled = true;
            barbDamage = Barbed.barbedDamageBonus;
        }
        else
        {
            barbedSprite.enabled = false;
        }
        if (Paddle != null)
        {
            paddleExtraForce = Paddle.speedEffect;
            paddleWeightEffect = Paddle.weightEffect;
        }
        else
        {
            paddleExtraForce = 0f;
            paddleWeightEffect = 0f;
        }
        raftRB.mass = 1f + frameWeightEffect + paddleWeightEffect;
        extraForce = sheetsSpeedEffect + paddleExtraForce;
        raftRB.GetComponent<Health>().SetMaxHealth(frameHealthEffect);
    }

    public void Heal() {
        matressHealth.Heal(sheetsHealthEffect);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore raft and player collisions
        if (collision.CompareTag("Raft") || collision.CompareTag("Player"))
        {
            return;
        }
        
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("raft hit enemy!");
            
            Health enemyHealth = collision.GetComponent<Health>();
            if (enemyHealth != null && !enemyHealth.IsDead())
            {
                enemyHealth.TakeDamage(barbDamage, transform.position);
            }
            
            //SpawnHitEffect();
            //Destroy(gameObject);
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
        //playerSprite.SetActive(false);
        //raftAnim.SetBool("Paddle", true);
        if (input == Vector2.zero)
        {
            anim.SetTrigger("StopRow");
        }
        else
        {
            anim.SetTrigger("Row");
            anim.speed = Mathf.Max(0.5f, currentSpeed);
        }
        transform.position = raftRB.transform.position + new Vector3(0f, 0.005f, 0f);
        //vcam.Lens.OrthographicSize = Mathf.Lerp(vcam.Lens.OrthographicSize, zoomOut, zoomSpeed * Time.deltaTime);
        
        // Sync player to raft
        rb.linearVelocity = raftRB.linearVelocity;
        anim.SetBool("Moving", false);
        
        // Paddle animations
        if (input.x > 0) 
            //raftAnim.SetTrigger("PaddleRight");
            anim.SetBool("Rowing", true);
        else if (input.x < 0) 
            //raftAnim.SetTrigger("PaddleLeft");
            anim.SetBool("Rowing", true);
        else if (input.y != 0) 
            //raftAnim.SetTrigger("PaddleRight");
            anim.SetBool("Rowing", true);
    }

    void HandleWalkingMode(Vector2 input)
    {
        anim.speed = 1f;
        playerSprite.SetActive(true);
        raftAnim.SetBool("Paddle", false);
        anim.SetTrigger("StopROW");
        //vcam.Lens.OrthographicSize = Mathf.Lerp(vcam.Lens.OrthographicSize, zoomIn, zoomSpeed * Time.deltaTime);
        
        // Walk relative to raft
        rb.linearVelocity = (input * walkSpeed) + raftRB.linearVelocity;
        
        // Animation & flip
        anim.SetBool("Moving", input != Vector2.zero);
        /*if (input.x > 0 && !isFacingRight) 
            Flip();
        else if (input.x < 0 && isFacingRight) 
            Flip();*/
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

    public bool IsPlayerRowing()
    {
        return rowing;
    }
}