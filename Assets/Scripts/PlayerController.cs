using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("地面檢測設定")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // 內部變數
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInputActions playerInputActions;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isFacingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        // 訂閱事件
        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Attack.performed += Attack; // 【新增】訂閱攻擊事件
    }

    private void OnEnable() { playerInputActions.Player.Enable(); }
    private void OnDisable() { playerInputActions.Player.Disable(); }

    void Update()
    {
        moveInput = playerInputActions.Player.Move.ReadValue<Vector2>();
        UpdateAnimationParameters();
        Flip();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // 【新增】攻擊事件的處理函式
    private void Attack(InputAction.CallbackContext context)
    {
        if (isGrounded) // 假設只有在地面才能攻擊
        {
            animator.SetTrigger("Attack");
            // 在這裡可以加上實際的攻擊邏輯，例如產生子彈或傷害判定
            Debug.Log("Player Attacks!");
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            animator.SetTrigger("Jump");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void UpdateAnimationParameters()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("IsGround", isGrounded);
    }

    private void Flip()
    {
        if ((moveInput.x < 0 && isFacingRight) || (moveInput.x > 0 && !isFacingRight))
        {
            isFacingRight = !isFacingRight;
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}