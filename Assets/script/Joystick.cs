using UnityEngine;
using UnityEngine.EventSystems; // 導入 UI 事件系統的命名空間

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // 搖桿把手 (在 Inspector 中拖曳 Joystick_Handle 物件到這裡)
    public RectTransform handle;

    // 搖桿活動範圍的半徑 (可自由調整，單位為像素)
    public float joystickRadius = 100f;

    // 外部腳本可以讀取的方向向量，這是搖桿的核心輸出
    public Vector2 direction { get; private set; }

    private RectTransform joystickBG;
    private Vector2 initialHandlePosition;

    void Start()
    {
        // 取得搖桿背景的 RectTransform 元件
        joystickBG = GetComponent<RectTransform>();

        // 記錄把手的初始位置，用來放開手指時歸位
        initialHandlePosition = handle.anchoredPosition;
    }

    // 當手指在搖桿區域上按下時
    public void OnPointerDown(PointerEventData eventData)
    {
        // 直接呼叫 OnDrag，讓搖桿在點擊瞬間就開始計算位置
        OnDrag(eventData);
    }

    // 當手指在搖桿區域上拖曳時
    public void OnDrag(PointerEventData eventData)
    {
        // 將滑鼠或手指在螢幕上的位置，轉換為在搖桿 UI 上的局部座標
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBG, eventData.position, eventData.pressEventCamera, out localPosition);

        // 將把手的位置限制在搖桿半徑範圍內
        Vector2 clampedPosition = Vector2.ClampMagnitude(localPosition, joystickRadius);
        
        // 更新把手的位置
        handle.anchoredPosition = clampedPosition;

        // 計算並正規化方向向量，以便外部腳本使用
        direction = clampedPosition.normalized;
    }

    // 當手指放開時
    public void OnPointerUp(PointerEventData eventData)
    {
        // 將把手回到初始位置
        handle.anchoredPosition = initialHandlePosition;
        
        // 方向向量歸零，角色停止移動
        direction = Vector2.zero;
    }
}