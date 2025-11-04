using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isFacingRight = true;
    private InputAction moveAction;
    [SerializeField] Vector2 moveValue;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        moveAction = InputSystem.actions.FindAction("Move");
    }

    void Update()
    {
        moveValue = moveAction.ReadValue<Vector2>();
        rb.linearVelocity = moveValue * moveSpeed;
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

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
}