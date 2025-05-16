using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 12f;

    private Rigidbody2D rb;
    private bool isGrounded;
    public bool isActive = true;

    [HideInInspector]
    public bool isSwinging = false; // 👈 доступно другим скриптам
    

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isActive)
        {
            if (isSwinging) return; // ❌ если раскачка активна, отключаем управление

            float moveInput = Input.GetAxisRaw("Horizontal");
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
