using UnityEngine;

namespace DefaultNamespace
{
    public class finish : MonoBehaviour
    {
        [Header("滾動設定")] [SerializeField] private float scrollSpeed = 5f; // 基礎滾動速度
        [SerializeField] private float targetY = 10f; // 目標Y位置

        [Header("加速設定")] [SerializeField] private float boostMultiplier = 2f; // 加速倍數
        [SerializeField] private float boostDuration = 0.5f; // 加速持續時間

        [Header("Gizmos設定")] [SerializeField] private Color gizmosColor = Color.yellow; // Gizmos顏色
        [SerializeField] private float gizmosSize = 0.5f; // Gizmos大小

        private Vector3 startPosition;
        private bool isScrolling = true;
        private bool isBoosting = false;
        private float boostTimer = 0f;
        private float currentSpeed;

        void Start()
        {
            // 記錄起始位置
            startPosition = transform.position;
            currentSpeed = scrollSpeed;
        }

        void Update()
        {
            HandleInput();
            HandleBoosting();
            HandleScrolling();
        }

        private void HandleInput()
        {
            // 檢測點擊或觸摸
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                if (isScrolling && !isBoosting)
                {
                    StartBoost();
                }
            }
        }

        private void HandleBoosting()
        {
            if (isBoosting)
            {
                boostTimer -= Time.deltaTime;
                if (boostTimer <= 0f)
                {
                    EndBoost();
                }
            }
        }

        private void HandleScrolling()
        {
            if (isScrolling)
            {
                // 向上移動
                transform.Translate(Vector3.up * currentSpeed * Time.deltaTime);

                // 檢查是否到達目標位置
                if (transform.position.y >= startPosition.y + targetY)
                {
                    isScrolling = false;
                    isBoosting = false;
                    Debug.Log("滾動完成！");
                }
            }
        }

        private void StartBoost()
        {
            isBoosting = true;
            boostTimer = boostDuration;
            currentSpeed = scrollSpeed * boostMultiplier;
            Debug.Log("加速開始！");
        }

        private void EndBoost()
        {
            isBoosting = false;
            currentSpeed = scrollSpeed;
            Debug.Log("加速結束！");
        }

        // 重新開始滾動的方法（可選）
        [ContextMenu("重新開始滾動")]
        public void RestartScroll()
        {
            transform.position = startPosition;
            isScrolling = true;
            isBoosting = false;
            boostTimer = 0f;
            currentSpeed = scrollSpeed;
        }

        // 繪製Gizmos
        private void OnDrawGizmos()
        {
            Vector3 currentPos = Application.isPlaying ? startPosition : transform.position;

            // 設置Gizmos顏色
            Gizmos.color = gizmosColor;

            // 繪製起始位置
            Gizmos.DrawWireSphere(currentPos, gizmosSize);

            // 繪製目標位置
            Vector3 targetPos = currentPos + Vector3.up * targetY;
            Gizmos.DrawWireSphere(targetPos, gizmosSize);

            // 繪製移動路徑
            Gizmos.DrawLine(currentPos, targetPos);

            // 繪製當前位置（運行時）
            if (Application.isPlaying)
            {
                Gizmos.color = isBoosting ? Color.red : Color.green;
                Gizmos.DrawSphere(transform.position, gizmosSize * 0.7f);

                // 顯示速度指示器
                if (isScrolling)
                {
                    Vector3 velocityIndicator = transform.position + Vector3.up * (currentSpeed * 0.2f);
                    Gizmos.DrawLine(transform.position, velocityIndicator);
                    Gizmos.DrawSphere(velocityIndicator, gizmosSize * 0.3f);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 選中時顯示更詳細的資訊
            OnDrawGizmos();

            // 添加文字標籤（如果需要）
            Vector3 currentPos = Application.isPlaying ? startPosition : transform.position;
            Vector3 targetPos = currentPos + Vector3.up * targetY;

            // 可以在這裡使用 Handles.Label 來顯示文字，但需要引入 UnityEditor
#if UNITY_EDITOR
            UnityEditor.Handles.Label(currentPos, "起點");
            UnityEditor.Handles.Label(targetPos, "終點");
#endif
        }

        // 公共屬性用於外部訪問
        public bool IsScrolling => isScrolling;
        public bool IsBoosting => isBoosting;
        public float CurrentSpeed => currentSpeed;
        public float Progress => isScrolling ? (transform.position.y - startPosition.y) / targetY : 1f;
    }
}