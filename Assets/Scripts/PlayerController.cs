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

    // 【新增】武器與瞄準
    private BubbleWeapon bubbleWeapon;
    private Vector2 aimDirection = Vector2.right;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        // 訂閱事件
        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Attack.performed += Attack; // 【新增】訂閱攻擊事件

        // 【新增】取得武器元件
        bubbleWeapon = GetComponent<BubbleWeapon>();
    }

    private void OnEnable() { playerInputActions.Player.Enable(); }
    private void OnDisable() { playerInputActions.Player.Disable(); }

    void Update()
    {
        moveInput = playerInputActions.Player.Move.ReadValue<Vector2>();

        // 【新增】以滑鼠位置做 360° 瞄準並同步箭頭
        UpdateAimFromMouse();

        // 【新增】滑鼠左鍵點擊也可發射泡泡
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (bubbleWeapon != null && bubbleWeapon.TryFire())
            {
                animator.SetTrigger("Attack");
            }
        }

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
            // 【新增】實際發射泡泡
            bubbleWeapon?.TryFire();
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

    // 【變更】翻面：優先依瞄準方向 X，若接近 0 則退回用移動方向
    private void Flip()
    {
        float refX = Mathf.Abs(aimDirection.x) >= 0.01f ? aimDirection.x : moveInput.x;

        if ((refX < 0 && isFacingRight) || (refX > 0 && !isFacingRight))
        {
            isFacingRight = !isFacingRight;
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }

    // 【新增】以滑鼠位置更新瞄準並同步到武器與箭頭
    private void UpdateAimFromMouse()
    {
        if (Camera.main == null || Mouse.current == null)
            return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));

        Vector3 origin = transform.position;
        if (bubbleWeapon != null && bubbleWeapon.firePoint != null)
            origin = bubbleWeapon.firePoint.position;

        aimDirection = ((Vector2)(mouseWorld - origin)).normalized;
        bubbleWeapon?.SetAim(aimDirection);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}