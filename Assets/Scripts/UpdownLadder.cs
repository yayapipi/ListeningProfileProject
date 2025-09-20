using UnityEngine;
using System.Collections;

namespace DefaultNamespace
{
    public class UpdownLadder : MonoBehaviour
    {
        [Header("升降設定")] [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float upperLimit = 5f;
        [SerializeField] private float lowerLimit = 0f;

        [Header("啟動延遲設定")] [SerializeField] private float startDelay = 2f;
        [SerializeField] private bool enableStartDelay = true;

        [Header("觸發設定")] [SerializeField] private bool requirePlayerTrigger = false;
        [SerializeField] private string playerTag = "Player";

        [Header("移動模式")] [SerializeField] private MovementMode movementMode = MovementMode.AutoLoop;

        [Header("Debug 設定")] [SerializeField] private bool enableDebug = true;
        [SerializeField] private bool showDetailedDebug = false;
        [SerializeField] private bool showGizmosAlways = true;
        [SerializeField] private Color debugLineColor = Color.cyan;

        private Vector3 startPosition;
        private Vector3 upperPosition;
        private Vector3 lowerPosition;
        private Vector3 targetPosition;
        private bool isMovingUp = true;
        private bool playerOnPlatform = false;
        private bool hasPlayer = false;
        private float distanceToTarget;
        private float currentMovementProgress;

        // 延遲啟動相關
        private bool isInitialized = false;
        private bool isDelayActive = false;
        private float delayTimer = 0f;

        // Debug 相關變數
        private float debugUpdateInterval = 1f;
        private float lastDebugTime;
        private Vector3 lastPosition;
        private float actualSpeed;

        public enum MovementMode
        {
            AutoLoop, // 自動循環升降
            PlayerTriggerUp, // 玩家上來時上升
            PlayerTriggerDown, // 玩家上來時下降
            PlayerTriggerToggle // 玩家上來時切換方向
        }

        private void Start()
        {
            InitializePlatform();

            if (enableStartDelay && startDelay > 0f)
            {
                StartDelayedActivation();
            }
            else
            {
                ActivatePlatform();
            }
        }

        private void InitializePlatform()
        {
            startPosition = transform.position;
            upperPosition = new Vector3(startPosition.x, startPosition.y + upperLimit, startPosition.z);
            lowerPosition = new Vector3(startPosition.x, startPosition.y + lowerLimit, startPosition.z);

            // 設定初始目標位置
            targetPosition = isMovingUp ? upperPosition : lowerPosition;

            LogDebug($"平台初始化：起始位置={startPosition}, 上限={upperPosition}, 下限={lowerPosition}");
            LogDebug($"初始目標位置={targetPosition}, 初始方向={(isMovingUp ? "上升" : "下降")}");
            LogDebug($"移動模式={movementMode}, 需要玩家觸發={requirePlayerTrigger}");
            LogDebug($"啟動延遲設定：{(enableStartDelay ? $"啟用 ({startDelay}秒)" : "禁用")}");

            // 初始化 Debug 變數
            lastPosition = transform.position;
            lastDebugTime = Time.time;
        }

        private void StartDelayedActivation()
        {
            isDelayActive = true;
            delayTimer = startDelay;
            LogDebug($"開始延遲啟動倒數：{startDelay} 秒");

            // 使用協程進行倒數提示
            StartCoroutine(DelayCountdownCoroutine());
        }

        private IEnumerator DelayCountdownCoroutine()
        {
            float remainingTime = startDelay;

            while (remainingTime > 0f)
            {
                if (enableDebug)
                {
                    // 每秒顯示倒數
                    if (Mathf.Ceil(remainingTime) != Mathf.Ceil(remainingTime - Time.deltaTime))
                    {
                        LogDebug($"平台啟動倒數：{Mathf.Ceil(remainingTime)} 秒");
                    }
                }

                remainingTime -= Time.deltaTime;
                yield return null;
            }

            ActivatePlatform();
        }

        private void ActivatePlatform()
        {
            isDelayActive = false;
            isInitialized = true;
            delayTimer = 0f;

            LogDebug("🚀 升降平台正式啟動！");
            LogDebug("升降平台初始化完成並開始運作");
        }

        private void Update()
        {
            // 如果還在延遲階段，不執行移動邏輯
            if (isDelayActive)
            {
                UpdateDelayTimer();
                return;
            }

            // 如果尚未初始化完成，不執行移動邏輯
            if (!isInitialized)
            {
                return;
            }

            UpdateDebugInfo();
            HandleMovement();

            if (showDetailedDebug)
            {
                ShowDetailedDebugInfo();
            }
        }

        private void UpdateDelayTimer()
        {
            delayTimer -= Time.deltaTime;

            if (delayTimer <= 0f)
            {
                ActivatePlatform();
            }

            // 在延遲期間也更新基本資訊
            UpdateDebugInfo();
        }

        private void UpdateDebugInfo()
        {
            // 計算到目標的距離
            distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            // 計算移動進度 (0-1)
            float totalDistance = Vector3.Distance(upperPosition, lowerPosition);
            if (totalDistance > 0)
            {
                float currentDistanceFromLower = Vector3.Distance(transform.position, lowerPosition);
                currentMovementProgress = currentDistanceFromLower / totalDistance;
            }

            // 計算實際移動速度
            if (Time.time - lastDebugTime >= debugUpdateInterval)
            {
                float deltaTime = Time.time - lastDebugTime;
                float deltaDistance = Vector3.Distance(transform.position, lastPosition);
                actualSpeed = deltaDistance / deltaTime;

                lastPosition = transform.position;
                lastDebugTime = Time.time;

                if (showDetailedDebug && isInitialized)
                {
                    LogDebug($"實際移動速度: {actualSpeed:F2} units/sec (設定速度: {moveSpeed})");
                }
            }
        }

        private void HandleMovement()
        {
            // 只有在初始化完成後才處理移動
            if (!isInitialized) return;

            if (requirePlayerTrigger && movementMode != MovementMode.AutoLoop)
            {
                HandlePlayerTriggeredMovement();
            }
            else if (!requirePlayerTrigger || movementMode == MovementMode.AutoLoop)
            {
                HandleAutoMovement();
            }
        }

        private void HandleAutoMovement()
        {
            Vector3 previousPosition = transform.position;

            // 移動平台
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Debug 移動資訊
            if (showDetailedDebug && Vector3.Distance(previousPosition, transform.position) > 0.001f)
            {
                LogDebug($"自動移動中：當前位置={transform.position:F2}, 目標={targetPosition:F2}, 距離={distanceToTarget:F2}");
            }

            // 檢查是否到達目標位置
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                LogDebug($"到達目標位置：{targetPosition:F2}");
                SwitchDirection();
            }
        }

        private void HandlePlayerTriggeredMovement()
        {
            if (!hasPlayer)
            {
                if (showDetailedDebug)
                {
                    LogDebug("等待玩家觸發...");
                }

                return;
            }

            Vector3 previousTarget = targetPosition;

            switch (movementMode)
            {
                case MovementMode.PlayerTriggerUp:
                    targetPosition = upperPosition;
                    break;
                case MovementMode.PlayerTriggerDown:
                    targetPosition = lowerPosition;
                    break;
                case MovementMode.PlayerTriggerToggle:
                    // 只有當平台停止時才能切換方向
                    if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
                    {
                        LogDebug("玩家觸發：切換移動方向");
                        SwitchDirection();
                    }

                    break;
            }

            if (previousTarget != targetPosition)
            {
                LogDebug($"玩家觸發模式 {movementMode}：目標更新為 {targetPosition:F2}");
            }

            // 移動平台
            Vector3 previousPosition = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (showDetailedDebug && Vector3.Distance(previousPosition, transform.position) > 0.001f)
            {
                LogDebug($"玩家觸發移動中：當前位置={transform.position:F2}, 目標={targetPosition:F2}");
            }
        }

        private void SwitchDirection()
        {
            bool previousDirection = isMovingUp;
            isMovingUp = !isMovingUp;
            targetPosition = isMovingUp ? upperPosition : lowerPosition;

            LogDebug($"方向切換：{(previousDirection ? "上升" : "下降")} → {(isMovingUp ? "上升" : "下降")}");
            LogDebug($"新目標位置：{targetPosition:F2}");
        }

        // 玩家進入平台觸發區域
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                playerOnPlatform = true;
                hasPlayer = true;

                LogDebug($"玩家進入平台：{other.name}");
                LogDebug($"設定玩家跟隨平台移動");

                // 讓玩家跟隨平台移動
                other.transform.SetParent(transform);

                // 如果是觸發模式，記錄觸發事件
                if (requirePlayerTrigger)
                {
                    LogDebug($"觸發模式啟動：{movementMode}");
                }

                // 如果平台尚未啟動且設定為玩家觸發時立即啟動
                if (isDelayActive && requirePlayerTrigger)
                {
                    LogDebug("玩家觸發：立即啟動平台（跳過剩餘延遲時間）");
                    StopAllCoroutines();
                    ActivatePlatform();
                }
            }
            else
            {
                if (showDetailedDebug)
                {
                    LogDebug($"非玩家物件進入觸發區域：{other.name} (標籤: {other.tag})");
                }
            }
        }

        // 玩家離開平台觸發區域
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                playerOnPlatform = false;
                hasPlayer = false;

                LogDebug($"玩家離開平台：{other.name}");
                LogDebug($"取消玩家跟隨");

                // 取消玩家跟隨
                other.transform.SetParent(null);

                if (requirePlayerTrigger)
                {
                    LogDebug($"觸發模式停用");
                }
            }
            else
            {
                if (showDetailedDebug)
                {
                    LogDebug($"非玩家物件離開觸發區域：{other.name} (標籤: {other.tag})");
                }
            }
        }

        private void ShowDetailedDebugInfo()
        {
            // 每秒顯示一次詳細資訊
            if (Time.frameCount % 60 == 0) // 假設 60 FPS
            {
                LogDebug("=== 升降平台狀態 ===");

                if (isDelayActive)
                {
                    LogDebug($"⏳ 啟動延遲中，剩餘時間: {delayTimer:F1} 秒");
                }
                else if (!isInitialized)
                {
                    LogDebug($"⚠️ 平台尚未初始化");
                }
                else
                {
                    LogDebug($"✅ 平台運作中");
                }

                LogDebug($"當前位置: {transform.position:F2}");
                LogDebug($"目標位置: {targetPosition:F2}");
                LogDebug($"移動方向: {(isMovingUp ? "上升" : "下降")}");
                LogDebug($"到目標距離: {distanceToTarget:F3}");
                LogDebug($"移動進度: {(currentMovementProgress * 100):F1}%");
                LogDebug($"玩家在平台上: {playerOnPlatform}");
                LogDebug($"移動模式: {movementMode}");
                LogDebug($"需要玩家觸發: {requirePlayerTrigger}");
                LogDebug($"實際移動速度: {actualSpeed:F2} units/sec");
                LogDebug("===================");
            }
        }

        // 在編輯器中繪製輔助線條
        private void OnDrawGizmos()
        {
            if (showGizmosAlways)
            {
                DrawGizmos();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmosAlways)
            {
                DrawGizmos();
            }
        }

        private void DrawGizmos()
        {
            Vector3 basePos = Application.isPlaying ? startPosition : transform.position;

            // 繪製上限位置
            Gizmos.color = Color.green;
            Vector3 upperPos = new Vector3(basePos.x, basePos.y + upperLimit, basePos.z);
            Gizmos.DrawWireCube(upperPos, transform.localScale);
            Gizmos.DrawLine(basePos, upperPos);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(upperPos + Vector3.up * 0.5f, $"上限 (+{upperLimit})");
#endif

            // 繪製下限位置
            Gizmos.color = Color.red;
            Vector3 lowerPos = new Vector3(basePos.x, basePos.y + lowerLimit, basePos.z);
            Gizmos.DrawWireCube(lowerPos, transform.localScale);
            Gizmos.DrawLine(basePos, lowerPos);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(lowerPos + Vector3.down * 0.5f, $"下限 ({lowerLimit:+0.0;-0.0;0})");
#endif

            // 繪製當前目標
            if (Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(targetPosition, 0.2f);

                // 繪製移動軌跡
                Gizmos.color = debugLineColor;
                Gizmos.DrawLine(transform.position, targetPosition);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(targetPosition + Vector3.right * 0.5f, "目標");

                // 顯示距離資訊
                Vector3 midPoint = (transform.position + targetPosition) * 0.5f;
                UnityEditor.Handles.Label(midPoint, $"距離: {distanceToTarget:F2}");

                // 顯示延遲狀態
                if (isDelayActive)
                {
                    UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f,
                        $"⏳ 延遲啟動: {delayTimer:F1}秒");
                }
                else if (isInitialized)
                {
                    UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, "✅ 運作中");
                }
#endif
            }

            // 繪製移動範圍
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Vector3 rangeCenter = new Vector3(basePos.x, basePos.y + (upperLimit + lowerLimit) * 0.5f, basePos.z);
            Vector3 rangeSize = new Vector3(transform.localScale.x, upperLimit - lowerLimit, transform.localScale.z);
            Gizmos.DrawCube(rangeCenter, rangeSize);
        }

        // Debug 日誌方法
        private void LogDebug(string message)
        {
            if (enableDebug)
            {
                Debug.Log($"[升降平台-{gameObject.name}] {message}", this);
            }
        }

        private void LogWarning(string message)
        {
            if (enableDebug)
            {
                Debug.LogWarning($"[升降平台-{gameObject.name}] {message}", this);
            }
        }

        private void LogError(string message)
        {
            if (enableDebug)
            {
                Debug.LogError($"[升降平台-{gameObject.name}] {message}", this);
            }
        }

        // 公開方法供外部調用
        public void SetMovementSpeed(float speed)
        {
            float oldSpeed = moveSpeed;
            moveSpeed = speed;
            LogDebug($"移動速度更新：{oldSpeed} → {speed}");
        }

        public void SetLimits(float upper, float lower)
        {
            LogDebug($"限制範圍更新：上限 {upperLimit} → {upper}, 下限 {lowerLimit} → {lower}");
            upperLimit = upper;
            lowerLimit = lower;
            if (Application.isPlaying)
            {
                InitializePlatform();
            }
        }

        public void StartMoving()
        {
            enabled = true;
            LogDebug("平台開始移動");
        }

        public void StopMoving()
        {
            enabled = false;
            LogDebug("平台停止移動");
        }

        public void ForceDirection(bool moveUp)
        {
            bool previousDirection = isMovingUp;
            isMovingUp = moveUp;
            targetPosition = isMovingUp ? upperPosition : lowerPosition;
            LogDebug($"強制設定移動方向：{(previousDirection ? "上升" : "下降")} → {(isMovingUp ? "上升" : "下降")}");
        }

        public void SetMovementMode(MovementMode mode)
        {
            MovementMode previousMode = movementMode;
            movementMode = mode;
            LogDebug($"移動模式更新：{previousMode} → {mode}");
        }

        public void TogglePlayerTrigger(bool enable)
        {
            requirePlayerTrigger = enable;
            LogDebug($"玩家觸發功能：{(enable ? "開啟" : "關閉")}");
        }

        public void SetStartDelay(float delay)
        {
            startDelay = delay;
            LogDebug($"啟動延遲時間設為：{delay} 秒");
        }

        public void SkipDelay()
        {
            if (isDelayActive)
            {
                LogDebug("手動跳過啟動延遲");
                StopAllCoroutines();
                ActivatePlatform();
            }
        }

        public void RestartWithDelay()
        {
            LogDebug("重新啟動平台（含延遲）");
            StopAllCoroutines();
            isInitialized = false;
            isDelayActive = false;

            if (enableStartDelay && startDelay > 0f)
            {
                StartDelayedActivation();
            }
            else
            {
                ActivatePlatform();
            }
        }

        // 編輯器測試方法
        [ContextMenu("測試-向上移動")]
        private void TestMoveUp()
        {
            ForceDirection(true);
        }

        [ContextMenu("測試-向下移動")]
        private void TestMoveDown()
        {
            ForceDirection(false);
        }

        [ContextMenu("測試-切換方向")]
        private void TestSwitchDirection()
        {
            SwitchDirection();
        }

        [ContextMenu("測試-跳過延遲")]
        private void TestSkipDelay()
        {
            SkipDelay();
        }

        [ContextMenu("測試-重新啟動")]
        private void TestRestart()
        {
            RestartWithDelay();
        }

        [ContextMenu("顯示當前狀態")]
        private void TestShowCurrentState()
        {
            LogDebug("=== 當前狀態 ===");
            LogDebug($"是否初始化: {isInitialized}");
            LogDebug($"是否延遲中: {isDelayActive}");
            LogDebug($"剩餘延遲時間: {delayTimer:F1}秒");
            LogDebug($"位置: {transform.position}");
            LogDebug($"目標: {targetPosition}");
            LogDebug($"方向: {(isMovingUp ? "上升" : "下降")}");
            LogDebug($"模式: {movementMode}");
            LogDebug($"玩家觸發: {requirePlayerTrigger}");
            LogDebug($"玩家在平台: {playerOnPlatform}");
            LogDebug($"到目標距離: {distanceToTarget:F3}");
            LogDebug("===============");
        }

        // 公開屬性供外部訪問
        public float DistanceToTarget => distanceToTarget;
        public float MovementProgress => currentMovementProgress;
        public bool IsMovingUp => isMovingUp;
        public bool HasPlayerOnPlatform => playerOnPlatform;
        public Vector3 CurrentTarget => targetPosition;
        public MovementMode CurrentMovementMode => movementMode;
        public float ActualSpeed => actualSpeed;
        public bool IsInitialized => isInitialized;
        public bool IsDelayActive => isDelayActive;
        public float RemainingDelayTime => isDelayActive ? delayTimer : 0f;
        public float StartDelay => startDelay;
    }
}