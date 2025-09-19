using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 通用搖桿：
/// - 拖曳更新 Value（-1..1）
/// - 放開重置為 0，並觸發 onReleased(lastValue)
/// - 可作為「左搖桿（移動）」或「右搖桿（瞄準）」
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("UI")] public RectTransform handle; // 搖桿頭
    public RectTransform background; // 搖桿底（若空則使用自身 RectTransform）

    [Header("參數")] public float handleRange = 80f; // 搖桿頭最大移動像素
    public float deadZone = 0.05f; // 輸入死區（歸一化後）

    public event Action<Vector2> onValueChanged;
    public event Action<Vector2> onReleased;

    public Vector2 Value { get; private set; } = Vector2.zero;

    private RectTransform _rt;
    private Canvas _canvas;
    private Camera _uiCamera;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        if (background == null) background = _rt;

        _canvas = GetComponentInParent<Canvas>();
        if (_canvas != null)
        {
            _uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceCamera ? _canvas.worldCamera : null;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, _uiCamera,
                out localPoint))
            return;

        // 以背景中心為原點
        Vector2 clamped = Vector2.ClampMagnitude(localPoint, handleRange);
        handle.anchoredPosition = clamped;

        Vector2 norm = clamped / handleRange; // -1..1
        if (norm.magnitude < deadZone) norm = Vector2.zero;

        if (norm != Value)
        {
            Value = norm;
            onValueChanged?.Invoke(Value);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Vector2 last = Value;
        Value = Vector2.zero;

        if (handle != null) handle.anchoredPosition = Vector2.zero;

        onValueChanged?.Invoke(Value);
        onReleased?.Invoke(last);
    }
}