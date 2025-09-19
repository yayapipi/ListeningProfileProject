using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class BubbleProjectile : MonoBehaviour
{
    [Header("基本設定")]
    public float speed = 6f;
    public float lifeTime = 8f;
    public float initialCollisionDelay = 0.5f;

    [Header("平台設定")]
    public LayerMask playerLayer = 1 << 0; // Player layer
    public LayerMask environmentLayer = -1; // 建築物/環境 layer

    [Header("閃爍警告設定")]
    public float warningTime = 2f; // 剩餘多少秒開始閃爍
    public float flashInterval = 0.2f; // 閃爍間隔
    public Color flashColor = Color.red; // 閃爍顏色

    private Rigidbody2D rb;
    private Collider2D bubbleCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 dir = Vector2.right;
    private float timeLeft;
    private bool collisionEnabled = false;
    private bool isPopping = false;
    private bool isFlashing = false;
    private Color originalColor;

    // 平台功能相關
    private List<GameObject> playersOnBubble = new List<GameObject>();
    private Dictionary<GameObject, Vector2> originalPlayerVelocities = new Dictionary<GameObject, Vector2>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bubbleCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        timeLeft = lifeTime;

        // 儲存原始顏色
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 建議預置體在 Inspector 設為 Kinematic + Gravity 0 + Freeze Rotation
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // 初始時禁用碰撞器
        if (bubbleCollider != null)
        {
            bubbleCollider.enabled = false;
        }

        // 播放 Idle 動畫 (BB)
        if (animator != null)
        {
            animator.Play("BB");
        }
    }

    public void Launch(Vector2 direction, float overrideSpeed = -1f)
    {
        dir = direction.normalized;
        if (overrideSpeed > 0f) speed = overrideSpeed;

        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            // Kinematic 透過 MovePosition 推進
        }
        else
        {
            // Dynamic 直接給 velocity
            rb.linearVelocity = dir * speed;
        }

        // 啟動延遲啟用碰撞器的協程
        StartCoroutine(EnableCollisionAfterDelay());
    }

    private IEnumerator EnableCollisionAfterDelay()
    {
        yield return new WaitForSeconds(initialCollisionDelay);

        if (bubbleCollider != null && !isPopping)
        {
            bubbleCollider.enabled = true;
            collisionEnabled = true;
        }
    }

    private void FixedUpdate()
    {
        if (isPopping) return;

        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            Vector2 newPosition = rb.position + dir * speed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);

            // 如果有玩家在泡泡上，讓他們跟隨移動
            UpdatePlayersOnBubble(newPosition - rb.position);
        }
    }

    private void Update()
    {
        if (isPopping) return;

        timeLeft -= Time.deltaTime;

        // 檢查是否需要開始閃爍警告
        if (!isFlashing && timeLeft <= warningTime && timeLeft > 0f)
        {
            StartFlashing();
        }

        if (timeLeft <= 0f)
        {
            PopBubble();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!collisionEnabled || isPopping) return;

        // 檢查是否是玩家
        if (IsInLayerMask(other.gameObject.layer, playerLayer))
        {
            HandlePlayerEnter(other.gameObject);
        }
        // 檢查是否是建築物/環境
        else if (IsInLayerMask(other.gameObject.layer, environmentLayer))
        {
            PopBubble();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!collisionEnabled || isPopping) return;

        // 玩家離開泡泡
        if (IsInLayerMask(other.gameObject.layer, playerLayer))
        {
            HandlePlayerExit(other.gameObject);
        }
    }

    private void HandlePlayerEnter(GameObject player)
    {
        if (!playersOnBubble.Contains(player))
        {
            playersOnBubble.Add(player);

            // 儲存玩家原來的速度
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                originalPlayerVelocities[player] = playerRb.linearVelocity;
            }
        }
    }

    private void HandlePlayerExit(GameObject player)
    {
        if (playersOnBubble.Contains(player))
        {
            playersOnBubble.Remove(player);

            // 恢復玩家原來的速度
            if (originalPlayerVelocities.ContainsKey(player))
            {
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = originalPlayerVelocities[player];
                }
                originalPlayerVelocities.Remove(player);
            }
        }
    }

    private void UpdatePlayersOnBubble(Vector2 movement)
    {
        foreach (GameObject player in playersOnBubble)
        {
            if (player != null)
            {
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    // 讓玩家跟隨泡泡移動
                    Vector2 currentVel = playerRb.linearVelocity;
                    Vector2 bubbleVel = dir * speed;

                    // 保持玩家的 Y 軸速度，但 X 軸跟隨泡泡
                    playerRb.linearVelocity = new Vector2(bubbleVel.x, currentVel.y);
                }
            }
        }
    }

    private void PopBubble()
    {
        if (isPopping) return;

        isPopping = true;

        // 讓所有在泡泡上的玩家恢復原速度
        foreach (GameObject player in playersOnBubble)
        {
            HandlePlayerExit(player);
        }

        // 停止移動
        rb.linearVelocity = Vector2.zero;

        // 播放破裂動畫
        if (animator != null)
        {
            animator.Play("BBpop");
            // 等待動畫播放完成再銷毀
            StartCoroutine(DestroyAfterAnimation());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyAfterAnimation()
    {
        // 等待動畫播放完成
        yield return new WaitForSeconds(0.5f); // 調整此時間以配合您的動畫長度

        Destroy(gameObject);
    }

    private void StartFlashing()
    {
        if (isFlashing) return;

        isFlashing = true;
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        while (!isPopping && timeLeft > 0f)
        {
            // 變成閃爍顏色
            if (spriteRenderer != null)
            {
                spriteRenderer.color = flashColor;
            }

            yield return new WaitForSeconds(flashInterval);

            // 變回原始顏色
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            yield return new WaitForSeconds(flashInterval);
        }
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}
