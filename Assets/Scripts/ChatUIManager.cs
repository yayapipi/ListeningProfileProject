using UnityEngine;
using UnityEngine.UI;

public class ChatUIManager : MonoBehaviour
{
    [Header("Chat UI 元件")]
    public GameObject chatButton; // Chat 按鈕物件
    public Button chatButtonComponent; // Chat 按鈕組件
    public Canvas chatCanvas; // Chat UI 的 Canvas

    [Header("UI 設定")]
    public float uiOffsetY = 100f; // Chat 按鈕相對於玩家的 Y 軸偏移
    public bool followPlayer = true; // 是否跟隨玩家位置

    private NPCDialogue currentNPC;
    private bool isShowingChatUI = false;
    private Camera uiCamera;

    public static ChatUIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeUI();
    }

    private void Start()
    {
        uiCamera = Camera.main;

        // 訂閱 NPC 對話事件
        NPCDialogue.OnNPCDialogueStart += OnDialogueStart;
        NPCDialogue.OnNPCDialogueEnd += OnDialogueEnd;
    }

    private void OnDestroy()
    {
        // 取消訂閱事件
        NPCDialogue.OnNPCDialogueStart -= OnDialogueStart;
        NPCDialogue.OnNPCDialogueEnd -= OnDialogueEnd;
    }

    private void InitializeUI()
    {
        // 初始化時隱藏 Chat 按鈕
        if (chatButton != null)
        {
            chatButton.SetActive(false);
        }

        // 設置按鈕點擊事件
        if (chatButtonComponent != null)
        {
            chatButtonComponent.onClick.RemoveAllListeners();
            chatButtonComponent.onClick.AddListener(OnChatButtonClicked);
        }

        // 獲取或設置 Canvas
        if (chatCanvas == null)
        {
            chatCanvas = GetComponentInParent<Canvas>();
        }
    }

    private void Update()
    {
        // 如果正在顯示 Chat UI 且設置為跟隨玩家
        if (isShowingChatUI && followPlayer && chatButton != null)
        {
            UpdateChatButtonPosition();
        }
    }

    /// <summary>
    /// 顯示 Chat UI 按鈕
    /// </summary>
    public void ShowChatUI(NPCDialogue npc)
    {
        if (npc == null || chatButton == null) return;

        currentNPC = npc;
        isShowingChatUI = true;

        chatButton.SetActive(true);
        UpdateChatButtonPosition();

        Debug.Log($"顯示與 {npc.NPCName} 的 Chat UI");
    }

    /// <summary>
    /// 隱藏 Chat UI 按鈕
    /// </summary>
    public void HideChatUI()
    {
        isShowingChatUI = false;
        currentNPC = null;

        if (chatButton != null)
        {
            chatButton.SetActive(false);
        }

        Debug.Log("隱藏 Chat UI");
    }

    /// <summary>
    /// 更新 Chat 按鈕位置
    /// </summary>
    private void UpdateChatButtonPosition()
    {
        if (!followPlayer || currentNPC == null || chatButton == null || uiCamera == null) return;

        // 獲取玩家位置
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        // 計算螢幕位置
        Vector3 playerWorldPos = player.transform.position + Vector3.up * (uiOffsetY / 100f);
        Vector3 screenPos = uiCamera.WorldToScreenPoint(playerWorldPos);

        // 設置 UI 位置
        RectTransform buttonRect = chatButton.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.position = screenPos;
        }
    }

    /// <summary>
    /// Chat 按鈕點擊事件
    /// </summary>
    private void OnChatButtonClicked()
    {
        if (currentNPC == null) return;

        if (!currentNPC.IsInDialogue)
        {
            // 開始對話
            currentNPC.StartDialogue();
        }
        else
        {
            // 推進對話或跳過打字機效果
            if (DialogueBubbleUI.Instance != null && DialogueBubbleUI.Instance.IsTyping())
            {
                DialogueBubbleUI.Instance.SkipTypewriter();
            }
            else
            {
                currentNPC.AdvanceDialogue();
            }
        }
    }

    /// <summary>
    /// 對話開始時的回調
    /// </summary>
    private void OnDialogueStart(NPCDialogue npc)
    {
        if (currentNPC == npc)
        {
            // 對話開始時可以保持 Chat 按鈕顯示，讓玩家可以點擊推進對話
            Debug.Log("對話開始，Chat 按鈕保持顯示用於推進對話");
        }
    }

    /// <summary>
    /// 對話結束時的回調
    /// </summary>
    private void OnDialogueEnd(NPCDialogue npc)
    {
        if (currentNPC == npc)
        {
            // 對話結束後隱藏 Chat 按鈕
            HideChatUI();
        }
    }

    // 公開屬性
    public bool IsShowingChatUI => isShowingChatUI;
    public NPCDialogue CurrentNPC => currentNPC;
}
