using UnityEngine;

namespace Akashic.Scripts.Utility.UI
{
    [ExecuteInEditMode] // 允許在「編輯模式」下執行Update

    public class UIFollowObj : MonoBehaviour
    {

        public Transform target; // 要跟隨的 Sprite 物件（通常為物件的 Transform）
        public RectTransform uiElement; // 要移動的 UI 元素（RectTransform）
        public Camera mainCamera; // 主攝影機
        public Vector2 offset; // 偏移量 (相對於 UI)

        private void Update()
        {
            // 在編輯模式和播放模式下都同步 UI
            SyncUIPosition();
        }

        /// <summary>
        /// 同步UI的位置。
        /// </summary>
        public void SyncUIPosition()
        {
            if (target == null || uiElement == null || mainCamera == null)
                return;

            // 1. 獲取世界座標並轉換為螢幕座標
            Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);

            // 2. 加上偏移量
            Vector3 finalPosition = screenPos + new Vector3(offset.x, offset.y, 0);

            // 3. 設置UI元素位置
            uiElement.position = finalPosition;
        }

        /// <summary>
        /// 新增一個Context Menu，用於手動同步位置。
        /// 在編輯模式下，右鍵此腳本可觸發。
        /// </summary>
        [ContextMenu("Sync UI Position")]
        public void SyncUIPositionInEditor()
        {
            SyncUIPosition();
            Debug.Log("UI同步成功！", this);
        }
    }
    
}