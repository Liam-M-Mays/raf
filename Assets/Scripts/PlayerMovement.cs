using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck; // Assign an empty GameObject as ground checker
    public LayerMask groundLayer; // Assign your ground layer

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isFacingRight = true;
    private InputAction moveAction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveAction = InputSystem.actions.FindAction("Move");
    }

    void Update()
    {
        // Horizontal Movement Input
        
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        rb.linearVelocity = moveValue * moveSpeed;

        // Check if grounded
        //isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // Jumping Input
        //if (Input.GetButtonDown("Jump") && isGrounded)
        //{
          //  rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        //}

        // Flip sprite based on movement direction
        if (moveValue.x > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveValue.x < 0 && isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
}