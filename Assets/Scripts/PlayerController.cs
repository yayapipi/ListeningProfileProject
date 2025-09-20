using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("地面檢測設定")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("輸入與武器")]
    public BubbleWeapon bubbleWeapon;
    public Joystick moveJoystick;       // 左搖桿（移動）
    public Joystick aimJoystick;        // 右搖桿（瞄準 + 放開發射）
    public float joystickDeadZone = 0.1f;
    public Camera worldCamera;          // 用於滑鼠座標轉世界位置（未指定則自動抓主攝影機）

    [Header("動畫參數名稱")]
    [Tooltip("待機動畫參數名稱")]
    public string idleAnimationName = "idle";
    [Tooltip("攻擊動畫觸發器名稱")]
    public string attackTriggerName = "attack";
    [Tooltip("跳躍動畫觸發器名稱")]
    public string jumpTriggerName = "jump";
    [Tooltip("行走動畫參數名稱")]
    public string walkAnimationName = "walk";

    // 內部變數
    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool isFacingRight = true;
    private float inputX = 0f;

    // 對話系統相關
    private bool isInDialogue = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (worldCamera == null) worldCamera = Camera.main;

        // 確保動畫參數名稱有預設值
        ValidateAnimationParameters();

        // 訂閱右搖桿事件
        if (aimJoystick != null)
        {
            aimJoystick.onValueChanged += OnAimJoystickChanged;
            aimJoystick.onReleased += OnAimJoystickReleased;
        }
    }

    /// <summary>
    /// 驗證並設定動畫參數的預設值
    /// </summary>
    private void ValidateAnimationParameters()
    {
        if (string.IsNullOrEmpty(idleAnimationName))
            idleAnimationName = "idle";
        
        if (string.IsNullOrEmpty(attackTriggerName))
            attackTriggerName = "attack";
        
        if (string.IsNullOrEmpty(jumpTriggerName))
            jumpTriggerName = "jump";
        
        if (string.IsNullOrEmpty(walkAnimationName))
            walkAnimationName = "walk";
    }

    private void OnDestroy()
    {
        if (aimJoystick != null)
        {
            aimJoystick.onValueChanged -= OnAimJoystickChanged;
            aimJoystick.onReleased -= OnAimJoystickReleased;
        }
    }

    private void Update()
    {
        // 檢查是否在對話中
        CheckDialogueState();

        // 如果在對話中，禁用大部分操作
        if (isInDialogue)
        {
            inputX = 0f; // 停止移動
            return; // 跳過其他輸入處理
        }

        // 1) 鍵盤 A/D 或 左右方向鍵
        float kbX = Input.GetAxisRaw("Horizontal"); // Unity 預設會同時支援 WASD 與 方向鍵

        // 2) 左搖桿優先（若有明顯輸入）
        if (moveJoystick != null && moveJoystick.Value.sqrMagnitude > joystickDeadZone * joystickDeadZone)
            inputX = Mathf.Clamp(moveJoystick.Value.x, -1f, 1f);
        else
            inputX = Mathf.Clamp(kbX, -1f, 1f);

        // Space 跳躍
        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();

        // 滑鼠右鍵發射（朝滑鼠方向）
        if (Input.GetMouseButtonDown(1))
            FireTowardsMouse();

        UpdateAnimationParameters();
        Flip();
    }

    private void FixedUpdate()
    {
        // 水平移動
        rb.linearVelocity = new Vector2(inputX * moveSpeed, rb.linearVelocity.y);

        // 地面檢測
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // -------------------------
    // 跳躍
    // -------------------------
    private void TryJump()
    {
        if (!isGrounded) return;

        if (animator != null && !string.IsNullOrEmpty(jumpTriggerName))
            animator.SetTrigger(jumpTriggerName);
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    // 提供 UI Button 綁定
    public void UIJump()
    {
        TryJump();
    }

    // -------------------------
    // 發射（滑鼠右鍵）
    // -------------------------
    private void FireTowardsMouse()
    {
        if (bubbleWeapon == null || worldCamera == null) return;

        Vector3 mouseWorld = worldCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 origin = bubbleWeapon.firePoint != null ? bubbleWeapon.firePoint.position : transform.position;
        Vector2 dir = (mouseWorld - origin);

        bubbleWeapon.SetAim(dir);
        if (bubbleWeapon.TryFire())
        {
            if (animator != null && !string.IsNullOrEmpty(attackTriggerName))
                animator.SetTrigger(attackTriggerName);
        }
    }

    // -------------------------
    // 右搖桿事件：瞄準/放開發射
    // -------------------------
    private void OnAimJoystickChanged(Vector2 v)
    {
        if (bubbleWeapon == null) return;
        bubbleWeapon.SetAim(v);
    }

    private void OnAimJoystickReleased(Vector2 lastValue)
    {
        if (bubbleWeapon == null) return;

        // 放開時若有有效方向則發射
        if (lastValue.sqrMagnitude > joystickDeadZone * joystickDeadZone)
        {
            if (bubbleWeapon.TryFire())
            {
                if (animator != null && !string.IsNullOrEmpty(attackTriggerName))
                    animator.SetTrigger(attackTriggerName);
            }
        }
    }

    // -------------------------
    // 動畫與翻轉
    // -------------------------
    private void UpdateAnimationParameters()
    {
        if (animator == null) return;

        // 使用可自定義的參數名稱
        float speed = Mathf.Abs(inputX);
        
        // 設定速度參數 (用於區分 idle 和 walk)
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsGround", isGrounded);

        // 根據速度播放對應動畫
        if (speed > 0.1f && isGrounded)
        {
            // 行走動畫
            if (!string.IsNullOrEmpty(walkAnimationName))
                animator.Play(walkAnimationName);
        }
        else if (speed <= 0.1f && isGrounded)
        {
            // 待機動畫
            if (!string.IsNullOrEmpty(idleAnimationName))
                animator.Play(idleAnimationName);
        }
    }

    private void Flip()
    {
        if ((inputX < 0 && isFacingRight) || (inputX > 0 && !isFacingRight))
        {
            isFacingRight = !isFacingRight;
            GetComponent<SpriteRenderer>().flipX = !isFacingRight;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    /// <summary>
    /// 檢查對話狀態
    /// </summary>
    private void CheckDialogueState()
    {
        // 檢查是否有任何 NPC 正在對話
        NPCDialogue[] npcs = FindObjectsOfType<NPCDialogue>();
        isInDialogue = false;

        foreach (NPCDialogue npc in npcs)
        {
            if (npc.IsInDialogue)
            {
                isInDialogue = true;
                break;
            }
        }

        // 也可以通過 ChatUIManager 來檢查
        if (ChatUIManager.Instance != null && ChatUIManager.Instance.CurrentNPC != null)
        {
            isInDialogue = ChatUIManager.Instance.CurrentNPC.IsInDialogue;
        }
    }

    /// <summary>
    /// 公開方法：設置對話狀態
    /// </summary>
    public void SetDialogueState(bool inDialogue)
    {
        isInDialogue = inDialogue;
    }

    /// <summary>
    /// 公開屬性：是否在對話中
    /// </summary>
    public bool IsInDialogue => isInDialogue;
}