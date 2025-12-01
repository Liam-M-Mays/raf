using UnityEngine;
//using UnityEngine.Random;
// Instaniate Mason will figure it out


public class SharkBossLogic : MonoBehaviour
{
    public float Health = 100f;

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Transform tf;
    private float currentAngle;
    private Vector2 target;

    private Vector2 lastKnown;
    private Transform player;
    private float side;
    [Header("Orbiting")]
    [SerializeField] private float orbitChance = 0.7f;
    [SerializeField] private float radius = 4f;
    [SerializeField] private float speedMax = 8f;
    [SerializeField] private float speedMin = 4f;
    private float speedTimer;
    private float speedCurrent;
    private float speedTarget;
    private float orbitTimer;
    [SerializeField] private float speedChangeIntervalMin = 2f;
    [SerializeField] private float speedChangeIntervalMax = 5f;
    [SerializeField] private float speedLerpSpeed = 2f;
    [SerializeField] private float orbitIntervalMin = 2f;
    [SerializeField] private float orbitIntervalMax = 5f;
    [Header("Transit")]
    private bool toPlayer = true;
    [SerializeField] private float tranSpeed = 2f;

    [Header("Bite")]
    [SerializeField] private float biteSpeed = 3f;
    private bool bite = false;
    private float biteTimer;
    [Header("Chomp")]
    [SerializeField] private int chompMin = 1;
    [SerializeField] private int chompMax = 5;
    private float chomps;
    private int chompCount = 0;
    private bool locked = false;
    //private bool chomping = false;
    private bool chomp = true;

    [SerializeField] private float chompSpeed = 7f;


    enum State {
        Under,
        Circle,
        Transit,
        Bite,
        Chomp
    };

    [SerializeField] private State currentState;


    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        tf = GetComponent<Transform>();
        currentState = State.Under;
        anim.SetBool("Surface", false);
        anim.SetBool("Submerge", true);
        anim.SetTrigger("Sub");
        player = GameObject.FindGameObjectWithTag("Raft").transform;
        lastKnown = (Vector2)player.position;
        speedTimer = Random.Range(speedChangeIntervalMin, speedChangeIntervalMax);
        speedTarget = Random.Range(speedMin, speedMax);
        speedCurrent = speedMin;
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Under: // respawn. meant for respawn and init state
            if (Random.Range(0f, 1f) < orbitChance)
            {
                lastKnown = (Vector2)player.position;
                Vector2 fromCenter = (Vector2)tf.position - lastKnown;
                if (fromCenter.sqrMagnitude < 0.0001f)
                {
                    // If we’re exactly on the center, just pick an angle
                    currentAngle = 0f;
                }
                else
                {
                    currentAngle = Mathf.Atan2(fromCenter.y, fromCenter.x);
                }
                target = lastKnown + new Vector2(
                Mathf.Cos(currentAngle) * radius,
                Mathf.Sin(currentAngle) * radius
                );
                tf.position = (Vector3)target;
                orbitTimer = Random.Range(orbitIntervalMin, orbitIntervalMax);
                anim.SetBool("Surface", true);
                anim.SetBool("Submerge", false);
                currentState = State.Circle;
            }
            else
            {
                anim.SetBool("Surface", true);
                anim.SetBool("Submerge", false);
                side = Random.Range(0f, 1f) < 0.5f ? -1f : 1f;
                toPlayer = Random.Range(0f, 1f) < 0.5f ? false : true;
                currentState = State.Transit;
            }
                    

                break;
            case State.Circle:
                // 1) Advance angle based on speed and dt
                speedTimer -= Time.deltaTime;
                orbitTimer -= Time.deltaTime;
                if (orbitTimer <= 0f)
                {
                    currentState = State.Under;
                    break;
                }
                if (speedTimer <= 0f)
                {
                    speedTimer = Random.Range(speedChangeIntervalMin, speedChangeIntervalMax);
                    speedTarget = Random.Range(speedMin, speedMax);
                }

                // smooth speed
                speedCurrent = Mathf.Lerp(speedCurrent, speedTarget, Time.deltaTime * speedLerpSpeed);

                // use this instead of constant orbitSpeed
                //rb.linearVelocity = desiredDir * speedCurrent;
                float angularSpeed = speedCurrent / radius;  // radians per second
                currentAngle += angularSpeed * Time.deltaTime;

                // 2) Compute the target point on the circle
                Vector2 offset = new Vector2(
                    Mathf.Cos(currentAngle),
                    Mathf.Sin(currentAngle)
                ) * radius;

                target = lastKnown + offset;
                // 3) Move toward that point
                Vector2 desiredVel = (target - (Vector2)tf.position).normalized * speedCurrent;
                rb.linearVelocity = desiredVel;
                Debug.DrawLine((Vector2)tf.position, (Vector2)tf.position + rb.linearVelocity, Color.red);
                break;


            case State.Transit: 
                if (toPlayer)
                {
                    target = player.position;
                    rb.linearVelocity = (target - (Vector2)tf.position).normalized * tranSpeed;
                    Debug.DrawLine((Vector2)tf.position, (Vector2)tf.position + rb.linearVelocity, Color.red);
                    if(Vector3.Distance(tf.position, player.position) < 5f && !bite)
                    {
                        biteTimer = biteSpeed;
                        rb.linearVelocity = Vector2.zero;
                        bite = true;
                        anim.SetBool("Surface", false);
                        anim.SetBool("Submerge", true);
                        anim.SetTrigger("Sub");
                    }
                }
                else
                {
                    
                    tf.position = player.position + new Vector3(21*side, 0,0);
                    rb.linearVelocity = Vector2.zero;
                    //Debug.DrawLine((Vector2)tf.position, (Vector2)tf.position + rb.linearVelocity, Color.red);
                    if(Mathf.Abs(tf.position.x - player.position.x) >= 20)
                    {
                        anim.SetBool("Surface", false);
                        anim.SetTrigger("Chomp");
                        chomps = Random.Range(chompMin, chompMax);
                        chompCount = 0;
                        currentState = State.Chomp;
                    }
                }
                break;

            case State.Bite:
                AnimatorStateInfo info;
                rb.linearVelocity = Vector2.zero;
                if(biteTimer>0f){
                    if (bite) 
                    {
                        anim.SetTrigger("Bite");
                        bite = false;
                    }
                    info = anim.GetCurrentAnimatorStateInfo(0);
                    biteTimer -= Time.deltaTime;
                    if (info.IsName("BossShark_Teeth") && info.normalizedTime >= 1f)
                    {
                        anim.speed = 0f;
                    }
                }
                else
                {
                    anim.speed = 1f;
                    if(!bite)
                    {
                        anim.SetTrigger("Attack");
                        bite = true;
                    }
                    info = anim.GetCurrentAnimatorStateInfo(0);

                    if (info.IsName("BossShark_Bite") && info.normalizedTime >= 1f)
                    {
                        tf.position = player.position + new Vector3(20, 20, 0);
                        bite = false;
                        anim.SetBool("Surface", false);
                        anim.SetBool("Submerge", true);
                        anim.SetTrigger("Sub");
                        currentState = State.Under;
                    }
                }
                break;

            case State.Chomp: // side charges
                if(chompCount < chomps)
                {
                    if (!locked)
                    {
                        target = player.position + new Vector3(20f * -side, 0.3f, 0f);
                        side *= -1f;
                        transform.position = player.position + new Vector3(21f * side, 0.3f, 0f);
                        locked = true;
                    }
                    rb.linearVelocity = (target-(Vector2)tf.position).normalized * chompSpeed;
                    Debug.DrawLine((Vector2)tf.position, (Vector2)tf.position + rb.linearVelocity, Color.red);
                    if(Vector2.Distance((Vector2)tf.position, target) < 5 && chomp)
                    {
                        chompCount += 1;
                        Debug.Log("chomped " + chompCount);
                        locked = false;
                        chomp = false;
                    }
                    else if (Vector2.Distance((Vector2)tf.position, target) >= 5 && !chomp)
                    {
                        chomp = true;
                    }
                }
                else 
                {
                    anim.SetBool("Surface", false);
                    anim.SetBool("Submerge", true);
                    anim.SetTrigger("Sub");
                    currentState = State.Under;
                }
                break;
        }
    }

    private Vector2 Noise()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
    }


    void FixedUpdate()
    {
        // Only flip if there's significant horizontal movement
        //if(Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        //{
            // Check if velocity direction matches current facing direction
            if(Mathf.Sign(rb.linearVelocity.x) == Mathf.Sign(transform.localScale.x))
            {
                Vector3 scaler = transform.localScale;
                scaler.x *= -1;
                transform.localScale = scaler;
            }
        //}
    }

    private void Decide()
    {
        
    }

    //Make event from animations for handling visability
    public void makeVisable()
    {
        spriteRenderer.enabled = true;
    }

    public void makeInvisable()
    {
        spriteRenderer.enabled = false;
        if (bite)
        {
            tf.position = player.position + new Vector3(0f, 0.3f, 0f);
            spriteRenderer.enabled = true;
            currentState = State.Bite;
        }
    }
}
