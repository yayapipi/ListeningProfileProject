using TMPro;
using UnityEngine;

public class DialogueBubbleUI : MonoBehaviour
{
    [Header("Follow Target")] public GameObject targetObject;
    public float verticalOffset = 0.2f; // 世界坐标向上偏移（单位：米），可微调到头顶

    [Header("Canvas & Transforms")] public Canvas canvas; // 所属 Canvas（不填则自动向上查找）
    public RectTransform bubbleRoot; // 氣泡根節點（通常就是掛腳本的 RectTransform）
    public RectTransform backgroundRect; // 背景圖 RectTransform（Image）
    public TextMeshProUGUI text; // TextMeshProUGUI

    [Header("Sizing")] public Vector2 padding = new Vector2(24f, 16f); // 背景相對文本的內邊距（左右、上下）
    public float maxWidth = 420f; // 最大寬度（超過會換行）
    public float minWidth = 80f; // 最小寬度（背景不會更窄）
    public bool clampToCanvas = true; // 是否把氣泡限制在畫布可見範圍內

    public string contentDemo;
    private Camera worldCam;

    // 佈局狀態守衛，避免重入
    private bool isRefreshingLayout = false;
    private bool deferredScheduled = false;

    // 由佈局引發、用於抵銷高度變化的 Y 偏移（Canvas 本地座標）
    private float layoutYOffset = 0f;

    private void Reset()
    {
        bubbleRoot = transform as RectTransform;
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        layoutYOffset = 0f;
    }

    private void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        worldCam = ResolveCamera();
    }

    private void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        layoutYOffset = 0f;
        RefreshLayout();
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
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

    private void OnTextChanged(Object obj)
    {
        if (obj == (Object)text)
        {
            if (!isRefreshingLayout)
                RefreshLayout();
        }
    }

    public void SetText()
    {
        if (!text) return;

        // 1) 設字並強制標髒
        text.SetText(contentDemo, true);
        text.ForceMeshUpdate(false, false);

        // 2) 立即做一次保守的刷新（不做重建）
        RefreshLayout();

        // 3) 本幀結束再補一次（只安排一次）
        if (!deferredScheduled && isActiveAndEnabled)
        {
            deferredScheduled = true;
            StartCoroutine(DeferredRefreshLayout());
        }
    }

    private System.Collections.IEnumerator DeferredRefreshLayout()
    {
        yield return new WaitForEndOfFrame();
        deferredScheduled = false;

        if (!isActiveAndEnabled || !text) yield break;

        text.ForceMeshUpdate(false, false);
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
            text.enableWordWrapping = true;

            // 計算首選尺寸
            float availableMaxTextWidth = Mathf.Max(0f, maxWidth - padding.x * 2f);
            float minTextWidth = Mathf.Max(0f, minWidth - padding.x * 2f);

            Vector2 pref1 = text.GetPreferredValues(
                text.text,
                availableMaxTextWidth > 0 ? availableMaxTextWidth : 0f,
                0f
            );

            float targetTextWidth = Mathf.Clamp(
                pref1.x,
                minTextWidth,
                availableMaxTextWidth > 0 ? availableMaxTextWidth : pref1.x
            );

            Vector2 pref2 = text.GetPreferredValues(text.text, targetTextWidth, 0f);
            float targetTextHeight = Mathf.Max(1f, pref2.y);

            // 套用尺寸
            RectTransform textRect = text.rectTransform;
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetTextWidth);
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetTextHeight);

            float bgW = targetTextWidth + padding.x * 2f;
            float bgH = targetTextHeight + padding.y * 2f;

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
}