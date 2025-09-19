using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueBubbleUI : MonoBehaviour
{
    [Header("Follow Target")] 
    public GameObject targetObject;
    public GameObject playerObject; // 玩家物件，用於玩家對話時跟隨
    public float verticalOffset = 0.2f; // 世界坐标向上偏移（单位：米），可微调到头顶

    [Header("Canvas & Transforms")] public Canvas canvas; // 所属 Canvas（不填则自动向上查找）
    public RectTransform bubbleRoot; // 氣泡根節點（通常就是掛腳本的 RectTransform）
    public RectTransform backgroundRect; // 背景圖 RectTransform（Image）
    public Text text; // UI Text

    [Header("Sizing")] public Vector2 padding = new Vector2(24f, 16f); // 背景相對文本的內邊距（左右、上下）
    public float maxWidth = 420f; // 最大寬度（超過會換行）
    public float minWidth = 80f; // 最小寬度（背景不會更窄）
    public bool clampToCanvas = true; // 是否把氣泡限制在畫布可見範圍內

    [Header("打字機效果")]
    public float typewriterSpeed = 0.05f; // 每個字符顯示的間隔時間
    public bool enableTypewriter = true; // 是否啟用打字機效果

    [Header("對話類型")]
    public bool isPlayerSpeaking = true; // true=玩家對話, false=NPC對話

    public string contentDemo;
    private Camera worldCam;

    // 佈局狀態守衛，避免重入
    private bool isRefreshingLayout = false;
    private bool deferredScheduled = false;

    // 由佈局引發、用於抵銷高度變化的 Y 偏移（Canvas 本地座標）
    private float layoutYOffset = 0f;

    // 打字機效果相關
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;
    private string currentFullText = "";

    public static DialogueBubbleUI Instance { get; private set; }

    private void Reset()
    {
        bubbleRoot = transform as RectTransform;
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        layoutYOffset = 0f;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (!canvas) canvas = GetComponentInParent<Canvas>();
        worldCam = ResolveCamera();

        // 自動尋找玩家物件
        if (playerObject == null)
        {
            playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                Debug.Log($"[DialogueBubbleUI] 自動找到玩家物件: {playerObject.name}");
            }
            else
            {
                Debug.LogWarning("[DialogueBubbleUI] 找不到標籤為 'Player' 的物件");
            }
        }
    }

    private void Start()
    {
        HideDialogue();
    }

    private void OnEnable()
    {
        layoutYOffset = 0f;
        RefreshLayout();
    }

    private void OnDisable()
    {
        deferredScheduled = false;
        isRefreshingLayout = false;
    }

    private Camera ResolveCamera()
    {
        if (!canvas) return Camera.main;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null; // Overlay 用 null
        return canvas.worldCamera ? canvas.worldCamera : Camera.main;
    }

    private void LateUpdate()
    {
        UpdateFollow();
    }


    public void SetText()
    {
        if (!text) return;

        // 1) 設置文字
        text.text = contentDemo;

        // 2) 立即做一次保守的刷新
        RefreshLayout();

        // 3) 本幀結束再補一次（只安排一次）
        if (!deferredScheduled && isActiveAndEnabled)
        {
            deferredScheduled = true;
            StartCoroutine(DeferredRefreshLayout());
        }
    }

    // 新增：設置對話文字（帶打字機效果）
    public void SetDialogueText(string dialogueText, GameObject speaker = null)
    {
        if (!text) return;

        // 設置跟隨目標
        if (speaker != null)
        {
            SetTargetObject(speaker);
        }

        currentFullText = dialogueText;

        if (enableTypewriter)
        {
            StartTypewriter(dialogueText);
        }
        else
        {
            text.text = dialogueText;
            RefreshLayout();
        }
    }

    // 新增：設置對話文字（帶說話者類型）
    public void SetDialogueText(string dialogueText, GameObject speaker, SpeakerType speakerType)
    {
        if (!text) return;

        Debug.Log($"[DialogueBubbleUI] 設置對話: [{speakerType}] {dialogueText}");

        // 根據說話者類型設置跟隨目標
        GameObject followTarget = speakerType == SpeakerType.Player ? 
            (playerObject ?? speaker) : speaker;

        if (followTarget != null)
        {
            SetTargetObject(followTarget);
            Debug.Log($"[DialogueBubbleUI] 跟隨目標: {followTarget.name} (說話者類型: {speakerType})");
        }
        else
        {
            Debug.LogWarning($"[DialogueBubbleUI] 跟隨目標為空，說話者類型: {speakerType}");
        }

        // 更新對話類型標誌
        isPlayerSpeaking = (speakerType == SpeakerType.Player);

        currentFullText = dialogueText;

        if (enableTypewriter)
        {
            StartTypewriter(dialogueText);
        }
        else
        {
            text.text = dialogueText;
            RefreshLayout();
        }
    }

    // 新增：設置跟隨目標
    public void SetTargetObject(GameObject target)
    {
        targetObject = target;

        // 根據目標類型設置對話類型
        if (target != null)
        {
            isPlayerSpeaking = target.CompareTag("Player");
        }
    }

    // 新增：開始打字機效果
    private void StartTypewriter(string fullText)
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        typewriterCoroutine = StartCoroutine(TypewriterEffect(fullText));
    }

    // 新增：打字機效果協程
    private IEnumerator TypewriterEffect(string fullText)
    {
        isTyping = true;
        text.text = "";

        for (int i = 0; i <= fullText.Length; i++)
        {
            text.text = fullText.Substring(0, i);
            RefreshLayout();

            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        typewriterCoroutine = null;
    }

    // 新增：跳過打字機效果
    public void SkipTypewriter()
    {
        if (isTyping && typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            text.text = currentFullText;
            RefreshLayout();
            isTyping = false;
            typewriterCoroutine = null;
        }
    }

    // 新增：檢查是否正在打字
    public bool IsTyping()
    {
        return isTyping;
    }

    // 新增：顯示對話泡泡
    public void ShowDialogue()
    {
        gameObject.SetActive(true);
    }

    // 新增：隱藏對話泡泡
    public void HideDialogue()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;
        gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator DeferredRefreshLayout()
    {
        yield return new WaitForEndOfFrame();
        deferredScheduled = false;

        if (!isActiveAndEnabled || !text) yield break;

        Canvas.ForceUpdateCanvases();
        RefreshLayout();
    }

    private void UpdateFollow()
    {
        if (!targetObject || !canvas || !bubbleRoot) return;

        // 以目標的世界座標（含向上偏移）轉成螢幕座標
        Vector3 worldPos = targetObject.transform.position + Vector3.up * verticalOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // 佈局造成的 Y 補償
        screenPos.y += layoutYOffset;

        // 設置 UI 位置（直接用螢幕座標）
        bubbleRoot.position = screenPos;
    }

    public void RefreshLayout()
    {
        if (isRefreshingLayout) return;
        if (!text || !backgroundRect || !bubbleRoot) return;

        isRefreshingLayout = true;
        try
        {
            float prevBgH = backgroundRect.rect.height;

            // 確保啟用換行
            text.horizontalOverflow = HorizontalWrapMode.Wrap;

            // 計算首選尺寸
            float availableMaxTextWidth = Mathf.Max(0f, maxWidth - padding.x * 2f);
            float minTextWidth = Mathf.Max(0f, minWidth - padding.x * 2f);

            // 使用 UI Text 的首選尺寸計算
            TextGenerator textGen = text.cachedTextGenerator;
            TextGenerationSettings settings = text.GetGenerationSettings(Vector2.zero);
            settings.generationExtents = new Vector2(availableMaxTextWidth, 0f);

            float preferredWidth = textGen.GetPreferredWidth(text.text, settings);
            preferredWidth = Mathf.Clamp(preferredWidth, minTextWidth, availableMaxTextWidth);

            settings.generationExtents = new Vector2(preferredWidth, 0f);
            float preferredHeight = textGen.GetPreferredHeight(text.text, settings);
            preferredHeight = Mathf.Max(1f, preferredHeight);

            // 套用尺寸
            RectTransform textRect = text.rectTransform;
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);

            float bgW = preferredWidth + padding.x * 2f;
            float bgH = preferredHeight + padding.y * 2f;

            backgroundRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bgW);
            backgroundRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bgH);

            // 高度變化補償位置
            float deltaH = bgH - prevBgH;
            if (Mathf.Abs(deltaH) > Mathf.Epsilon)
            {
                float pivotY = bubbleRoot.pivot.y;
                layoutYOffset += deltaH * pivotY;
            }

            // 保守推進一次（避免頻繁重建）
            Canvas.ForceUpdateCanvases();
        }
        finally
        {
            isRefreshingLayout = false;
        }
    }

    /// <summary>
    /// 獲取玩家物件
    /// </summary>
    public GameObject GetPlayerObject()
    {
        // 如果 playerObject 為空，嘗試重新尋找
        if (playerObject == null)
        {
            playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                Debug.Log($"[DialogueBubbleUI] 重新找到玩家物件: {playerObject.name}");
            }
        }

        return playerObject;
    }

    /// <summary>
    /// 手動設置玩家物件
    /// </summary>
    public void SetPlayerObject(GameObject player)
    {
        playerObject = player;
        Debug.Log($"[DialogueBubbleUI] 手動設置玩家物件: {(player != null ? player.name : "null")}");
    }

    /// <summary>
    /// 獲取當前跟隨的目標物件
    /// </summary>
    public GameObject GetCurrentTarget()
    {
        return targetObject;
    }

    /// <summary>
    /// 檢查是否為玩家正在說話
    /// </summary>
    public bool IsPlayerCurrentlySpeaking()
    {
        return isPlayerSpeaking;
    }
}