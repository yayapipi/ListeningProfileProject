using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 要追蹤的目標物件 (我們的玩家)
    public Transform target;

    // 相機移動的平滑度 (數值越小越滑順，但也越慢跟上)
    public float smoothSpeed = 0.125f;

    // 相機相對於目標的固定偏移量
    public Vector3 offset;

    // 在所有 Update 函式執行完畢後才執行，適合用來處理攝影機追蹤
    void LateUpdate()
    {
        // 確認 target 是否存在，避免報錯
        if (target != null)
        {
            // 計算攝影機想要的目標位置 (玩家位置 + 偏移量)
            Vector3 desiredPosition = target.position + offset;

            // 使用 Lerp (線性插值) 讓攝影機平滑地從目前位置移動到目標位置
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // 更新攝影機的實際位置
            transform.position = smoothedPosition;
        }
    }
}