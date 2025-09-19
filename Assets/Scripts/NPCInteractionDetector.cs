using UnityEngine;

public class NPCInteractionDetector : MonoBehaviour
{
    [Header("互動檢測設定")]
    public float detectionRadius = 2f; // 檢測半徑
    public LayerMask playerLayer = 1; // 玩家圖層
    public bool useColliderDetection = true; // 是否使用碰撞檢測（否則使用距離檢測）

    [Header("NPC 引用")]
    public NPCDialogue npcDialogue; // NPC 對話組件

    private bool playerInRange = false;
    private GameObject currentPlayer;

    // 事件
    public System.Action<NPCDialogue> OnPlayerEnterRange;
    public System.Action<NPCDialogue> OnPlayerExitRange;

    private void Awake()
    {
        Debug.Log($"[NPCInteractionDetector] 初始化 NPC: {gameObject.name}", this);

        // 自動獲取 NPC 對話組件
        if (npcDialogue == null)
        {
            npcDialogue = GetComponent<NPCDialogue>();
            if (npcDialogue != null)
            {
                Debug.Log($"[NPCInteractionDetector] 自動找到 NPCDialogue 組件: {npcDialogue.NPCName}", this);
            }
            else
            {
                Debug.LogWarning($"[NPCInteractionDetector] 沒有找到 NPCDialogue 組件在 {gameObject.name}", this);
            }
        }
        else
        {
            Debug.Log($"[NPCInteractionDetector] 使用指定的 NPCDialogue 組件: {npcDialogue.NPCName}", this);
        }

        // 確保有 Collider 組件用於觸發檢測
        if (useColliderDetection)
        {
            Debug.Log($"[NPCInteractionDetector] 使用碰撞檢測模式，檢測半徑: {detectionRadius}", this);

            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                // 自動添加圓形碰撞器
                CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
                circleCol.isTrigger = true;
                circleCol.radius = detectionRadius;
                Debug.Log($"[NPCInteractionDetector] 自動添加 CircleCollider2D，半徑: {detectionRadius}", this);
            }
            else
            {
                col.isTrigger = true;
                Debug.Log($"[NPCInteractionDetector] 使用現有的 {col.GetType().Name}，已設為 Trigger", this);
            }
        }
        else
        {
            Debug.Log($"[NPCInteractionDetector] 使用距離檢測模式，檢測半徑: {detectionRadius}", this);
        }
    }

    private void Update()
    {
        // 如果不使用碰撞檢測，則使用距離檢測
        if (!useColliderDetection)
        {
            CheckPlayerDistance();
        }
    }

    /// <summary>
    /// 距離檢測方式
    /// </summary>
    private void CheckPlayerDistance()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) 
        {
            if (Time.frameCount % 300 == 0) // 每 5 秒警告一次（假設 60 FPS）
            {
                Debug.LogWarning($"[NPCInteractionDetector] 找不到標籤為 'Player' 的物件", this);
            }
            return;
        }

        float distance = Vector2.Distance(transform.position, player.transform.position);

        // 每 60 幀（約 1 秒）記錄一次距離，僅在接近時
        if (Time.frameCount % 60 == 0 && distance <= detectionRadius + 1f)
        {
            Debug.Log($"[NPCInteractionDetector] 玩家距離: {distance:F2}, 檢測半徑: {detectionRadius}, 玩家在範圍內: {playerInRange}", this);
        }

        if (distance <= detectionRadius && !playerInRange)
        {
            Debug.Log($"[NPCInteractionDetector] 距離檢測觸發：玩家進入範圍 (距離: {distance:F2})", this);
            PlayerEnterRange(player);
        }
        else if (distance > detectionRadius && playerInRange)
        {
            Debug.Log($"[NPCInteractionDetector] 距離檢測觸發：玩家離開範圍 (距離: {distance:F2})", this);
            PlayerExitRange();
        }
    }

    /// <summary>
    /// 玩家進入範圍
    /// </summary>
    private void PlayerEnterRange(GameObject player)
    {
        if (playerInRange) 
        {
            Debug.Log($"[NPCInteractionDetector] 玩家已在範圍內，忽略重複進入事件", this);
            return;
        }

        playerInRange = true;
        currentPlayer = player;

        Debug.Log($"[NPCInteractionDetector] ✅ 玩家進入 {npcDialogue?.NPCName ?? "未知NPC"} 的互動範圍", this);
        Debug.Log($"[NPCInteractionDetector] 玩家物件: {player.name}, NPC位置: {transform.position}, 玩家位置: {player.transform.position}", this);

        // 檢查 ChatUIManager
        if (ChatUIManager.Instance == null)
        {
            Debug.LogError($"[NPCInteractionDetector] ChatUIManager.Instance 為 null，無法顯示 Chat UI", this);
        }
        else if (npcDialogue == null)
        {
            Debug.LogError($"[NPCInteractionDetector] npcDialogue 為 null，無法顯示 Chat UI", this);
        }
        else
        {
            Debug.Log($"[NPCInteractionDetector] 正在顯示 {npcDialogue.NPCName} 的 Chat UI", this);
            ChatUIManager.Instance.ShowChatUI(npcDialogue);
        }

        // 觸發事件
        if (OnPlayerEnterRange != null)
        {
            Debug.Log($"[NPCInteractionDetector] 觸發 OnPlayerEnterRange 事件，訂閱者數量: {OnPlayerEnterRange.GetInvocationList().Length}", this);
            OnPlayerEnterRange.Invoke(npcDialogue);
        }
        else
        {
            Debug.Log($"[NPCInteractionDetector] OnPlayerEnterRange 事件沒有訂閱者", this);
        }
    }

    /// <summary>
    /// 玩家離開範圍
    /// </summary>
    private void PlayerExitRange()
    {
        if (!playerInRange) 
        {
            Debug.Log($"[NPCInteractionDetector] 玩家不在範圍內，忽略離開事件", this);
            return;
        }

        string playerName = currentPlayer != null ? currentPlayer.name : "未知玩家";
        bool wasInDialogue = npcDialogue != null && npcDialogue.IsInDialogue;

        playerInRange = false;
        currentPlayer = null;

        Debug.Log($"[NPCInteractionDetector] ❌ 玩家離開 {npcDialogue?.NPCName ?? "未知NPC"} 的互動範圍", this);
        Debug.Log($"[NPCInteractionDetector] 離開的玩家: {playerName}, 是否正在對話: {wasInDialogue}", this);

        // 隱藏 Chat UI
        if (ChatUIManager.Instance != null)
        {
            Debug.Log($"[NPCInteractionDetector] 隱藏 Chat UI", this);
            ChatUIManager.Instance.HideChatUI();
        }
        else
        {
            Debug.LogWarning($"[NPCInteractionDetector] ChatUIManager.Instance 為 null，無法隱藏 Chat UI", this);
        }

        // 如果正在對話，強制結束對話
        if (npcDialogue != null && npcDialogue.IsInDialogue)
        {
            Debug.Log($"[NPCInteractionDetector] 強制結束與 {npcDialogue.NPCName} 的對話", this);
            npcDialogue.ForceEndDialogue();
        }

        // 觸發事件
        if (OnPlayerExitRange != null)
        {
            Debug.Log($"[NPCInteractionDetector] 觸發 OnPlayerExitRange 事件，訂閱者數量: {OnPlayerExitRange.GetInvocationList().Length}", this);
            OnPlayerExitRange.Invoke(npcDialogue);
        }
        else
        {
            Debug.Log($"[NPCInteractionDetector] OnPlayerExitRange 事件沒有訂閱者", this);
        }
    }

    /// <summary>
    /// 碰撞檢測 - 進入觸發器
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useColliderDetection) 
        {
            Debug.Log($"[NPCInteractionDetector] 碰撞檢測已禁用，忽略觸發器進入事件", this);
            return;
        }

        Debug.Log($"[NPCInteractionDetector] 觸發器進入: {other.gameObject.name}, 標籤: {other.tag}", this);

        if (other.CompareTag("Player"))
        {
            Debug.Log($"[NPCInteractionDetector] 碰撞檢測確認：玩家進入觸發器", this);
            PlayerEnterRange(other.gameObject);
        }
        else
        {
            Debug.Log($"[NPCInteractionDetector] 非玩家物件進入觸發器，忽略", this);
        }
    }

    /// <summary>
    /// 碰撞檢測 - 離開觸發器
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!useColliderDetection) 
        {
            Debug.Log($"[NPCInteractionDetector] 碰撞檢測已禁用，忽略觸發器離開事件", this);
            return;
        }

        Debug.Log($"[NPCInteractionDetector] 觸發器離開: {other.gameObject.name}, 標籤: {other.tag}", this);

        if (other.CompareTag("Player"))
        {
            Debug.Log($"[NPCInteractionDetector] 碰撞檢測確認：玩家離開觸發器", this);
            PlayerExitRange();
        }
        else
        {
            Debug.Log($"[NPCInteractionDetector] 非玩家物件離開觸發器，忽略", this);
        }
    }

    /// <summary>
    /// 在場景視圖中繪製檢測範圍
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // 公開屬性
    public bool IsPlayerInRange => playerInRange;
    public GameObject CurrentPlayer => currentPlayer;

    /// <summary>
    /// 手動測試玩家進入範圍（編輯器用）
    /// </summary>
    [ContextMenu("測試玩家進入範圍")]
    private void TestPlayerEnter()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Debug.Log($"[NPCInteractionDetector] 手動測試玩家進入範圍", this);
            PlayerEnterRange(player);
        }
        else
        {
            Debug.LogError($"[NPCInteractionDetector] 測試失敗：找不到玩家", this);
        }
    }

    /// <summary>
    /// 手動測試玩家離開範圍（編輯器用）
    /// </summary>
    [ContextMenu("測試玩家離開範圍")]
    private void TestPlayerExit()
    {
        Debug.Log($"[NPCInteractionDetector] 手動測試玩家離開範圍", this);
        PlayerExitRange();
    }

    /// <summary>
    /// 顯示當前狀態（編輯器用）
    /// </summary>
    [ContextMenu("顯示當前狀態")]
    private void ShowCurrentState()
    {
        Debug.Log($"[NPCInteractionDetector] === 當前狀態 ===", this);
        Debug.Log($"NPC名稱: {npcDialogue?.NPCName ?? "無"}", this);
        Debug.Log($"檢測模式: {(useColliderDetection ? "碰撞檢測" : "距離檢測")}", this);
        Debug.Log($"檢測半徑: {detectionRadius}", this);
        Debug.Log($"玩家在範圍內: {playerInRange}", this);
        Debug.Log($"當前玩家: {(currentPlayer != null ? currentPlayer.name : "無")}", this);
        Debug.Log($"NPC是否在對話中: {(npcDialogue != null ? npcDialogue.IsInDialogue.ToString() : "無NPC對話組件")}", this);
        Debug.Log($"ChatUIManager實例: {(ChatUIManager.Instance != null ? "存在" : "不存在")}", this);
        Debug.Log($"=================", this);
    }

    private void Start()
    {
        // 在開始時顯示一次狀態
        Debug.Log($"[NPCInteractionDetector] NPC '{npcDialogue?.NPCName ?? gameObject.name}' 互動檢測器已就緒", this);
    }
}
