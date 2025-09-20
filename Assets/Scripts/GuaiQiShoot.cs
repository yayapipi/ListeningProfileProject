using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class GuaiQiShoot : MonoBehaviour
    {
        [Header("射擊設置")] public GameObject bulletPrefab; // 子彈預製體
        public Transform firePoint; // 發射點
        public Transform targetPoint; // 目標點
        public float minShootInterval = 1f; // 最小射擊間隔
        public float maxShootInterval = 3f; // 最大射擊間隔
        public float bulletSpeed = 10f; // 子彈速度

        [Header("移動設置")] public float moveSpeed = 2f; // 移動速度
        public float jumpForce = 8f; // 跳躍力量
        public float minMoveInterval = 3f; // 最小移動間隔
        public float maxMoveInterval = 6f; // 最大移動間隔
        public float moveDistance = 3f; // 移動距離

        [Header("移動範圍限制")] public bool useMovementBounds = true; // 是否使用移動範圍限制
        public Vector2 movementBoundsMin = new Vector2(-5f, -5f); // 移動範圍最小值
        public Vector2 movementBoundsMax = new Vector2(5f, 5f); // 移動範圍最大值
        public bool relativeToBounds = true; // 範圍是否相對於初始位置

        [Header("檢測設置")] public LayerMask groundLayerMask = 1; // 地面圖層

        [Header("瞄準輔助")] public Transform aimArrow; // 瞄準箭頭（可選）
        public bool showAimDirection = true; // 是否顯示瞄準方向

        [Header("動畫設置")] public string jumpAnimationName = "jump"; // 跳躍動畫名稱
        public string attackAnimationName = "usb"; // 攻擊動畫名稱
        public string idleAnimationName = "idle"; // 待機動畫名稱
        public string walkAnimationName = "walk"; // 行走動畫名稱

        private Rigidbody2D rb;
        private Animator animator;
        private Vector3 initialPosition;
        private Vector2 actualBoundsMin;
        private Vector2 actualBoundsMax;
        private bool isMoving = false;
        private bool canShoot = true;
        private Vector2 currentAimDirection = Vector2.right;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }

            animator = GetComponent<Animator>();
            initialPosition = transform.position;

            // 計算實際移動邊界
            CalculateMovementBounds();

            // 如果沒有設置發射點，創建一個
            if (firePoint == null)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(transform);
                firePointObj.transform.localPosition = Vector3.zero;
                firePoint = firePointObj.transform;
            }

            // 如果沒有設置目標點，嘗試找到玩家
            if (targetPoint == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    targetPoint = player.transform;
                }
            }

            // 驗證動畫參數
            ValidateAnimationNames();

            // 開始射擊循環
            StartCoroutine(ShootingLoop());
            // 開始隨機行為循環
            StartCoroutine(RandomBehaviorLoop());
        }

        private void ValidateAnimationNames()
        {
            if (string.IsNullOrEmpty(jumpAnimationName))
                jumpAnimationName = "jump";

            if (string.IsNullOrEmpty(attackAnimationName))
                attackAnimationName = "usb";

            if (string.IsNullOrEmpty(idleAnimationName))
                idleAnimationName = "idle";

            if (string.IsNullOrEmpty(walkAnimationName))
                walkAnimationName = "walk";
        }

        private void CalculateMovementBounds()
        {
            if (relativeToBounds)
            {
                // 相對於初始位置
                actualBoundsMin = (Vector2)initialPosition + movementBoundsMin;
                actualBoundsMax = (Vector2)initialPosition + movementBoundsMax;
            }
            else
            {
                // 世界座標
                actualBoundsMin = movementBoundsMin;
                actualBoundsMax = movementBoundsMax;
            }
        }

        private void Update()
        {
            UpdateAimDirection();

            // 確保NPC在範圍內
            if (useMovementBounds)
            {
                ClampPositionToBounds();
            }
        }

        private void ClampPositionToBounds()
        {
            Vector3 currentPos = transform.position;
            Vector3 clampedPos = new Vector3(
                Mathf.Clamp(currentPos.x, actualBoundsMin.x, actualBoundsMax.x),
                Mathf.Clamp(currentPos.y, actualBoundsMin.y, actualBoundsMax.y),
                currentPos.z
            );

            if (currentPos != clampedPos)
            {
                transform.position = clampedPos;
            }
        }

        private void UpdateAimDirection()
        {
            if (targetPoint == null || firePoint == null) return;

            // 計算從發射點到目標點的方向
            Vector2 directionToTarget = (targetPoint.position - firePoint.position).normalized;
            currentAimDirection = directionToTarget;

            // 更新瞄準箭頭方向
            if (aimArrow != null)
            {
                aimArrow.right = currentAimDirection;
            }
        }

        private IEnumerator ShootingLoop()
        {
            while (true)
            {
                if (canShoot && bulletPrefab != null && targetPoint != null)
                {
                    Shoot();
                }

                // 隨機射擊間隔
                float waitTime = Random.Range(minShootInterval, maxShootInterval);
                yield return new WaitForSeconds(waitTime);
            }
        }

        private IEnumerator RandomBehaviorLoop()
        {
            while (true)
            {
                // 隨機等待時間
                float waitTime = Random.Range(minMoveInterval, maxMoveInterval);
                yield return new WaitForSeconds(waitTime);

                // 隨機選擇行為：走路或跳躍
                int randomBehavior = Random.Range(0, 3); // 0: 不動, 1: 走路, 2: 跳躍

                switch (randomBehavior)
                {
                    case 1:
                        StartCoroutine(Walk());
                        break;
                    case 2:
                        Jump();
                        break;
                }
            }
        }

        private void Shoot()
        {
            if (bulletPrefab == null || firePoint == null || targetPoint == null) return;

            // 播放攻擊動畫
            if (animator != null && !string.IsNullOrEmpty(attackAnimationName))
            {
                animator.Play(attackAnimationName);
            }

            // 創建子彈
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // 給子彈添加速度（朝向目標點）
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = currentAimDirection * bulletSpeed;
            }
            else
            {
                // 如果子彈沒有 Rigidbody2D，添加一個簡單的移動腳本
                BulletMovement bulletMovement = bullet.GetComponent<BulletMovement>();
                if (bulletMovement == null)
                {
                    bulletMovement = bullet.AddComponent<BulletMovement>();
                }

                bulletMovement.Initialize(currentAimDirection * bulletSpeed);
            }

            // 可選：添加發射音效或特效
            Debug.Log($"{name} 朝向目標發射子彈！目標方向: {currentAimDirection}");
        }

        private IEnumerator Walk()
        {
            if (isMoving) yield break;

            isMoving = true;

            // 播放行走動畫
            if (animator != null && !string.IsNullOrEmpty(walkAnimationName))
            {
                animator.Play(walkAnimationName);
            }

            // 隨機選擇移動方向（左或右）
            float direction = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
            Vector3 targetPosition = transform.position + Vector3.right * direction * moveDistance;

            // 如果使用移動範圍限制，確保目標位置在範圍內
            if (useMovementBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, actualBoundsMin.x, actualBoundsMax.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, actualBoundsMin.y, actualBoundsMax.y);
            }

            // 移動到目標位置
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // 停止移動後播放待機動畫
            if (animator != null && !string.IsNullOrEmpty(idleAnimationName))
            {
                animator.Play(idleAnimationName);
            }

            isMoving = false;
        }

        private void Jump()
        {
            if (IsGrounded())
            {
                // 播放跳躍動畫
                if (animator != null && !string.IsNullOrEmpty(jumpAnimationName))
                {
                    animator.Play(jumpAnimationName);
                }

                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                Debug.Log($"{name} 跳躍！");
            }
        }

        private bool IsGrounded()
        {
            // 簡單的地面檢測
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayerMask);
            return hit.collider != null;
        }

        /// <summary>
        /// 設置目標點
        /// </summary>
        public void SetTargetPoint(Transform target)
        {
            targetPoint = target;
        }

        /// <summary>
        /// 設置移動範圍
        /// </summary>
        public void SetMovementBounds(Vector2 min, Vector2 max, bool relative = true)
        {
            movementBoundsMin = min;
            movementBoundsMax = max;
            relativeToBounds = relative;
            CalculateMovementBounds();
        }

        /// <summary>
        /// 獲取當前瞄準方向
        /// </summary>
        public Vector2 GetAimDirection()
        {
            return currentAimDirection;
        }

        /// <summary>
        /// 手動觸發射擊（供外部調用）
        /// </summary>
        public void TriggerShoot()
        {
            if (canShoot && bulletPrefab != null && targetPoint != null)
            {
                Shoot();
            }
        }

        /// <summary>
        /// 設置射擊狀態
        /// </summary>
        public void SetCanShoot(bool enabled)
        {
            canShoot = enabled;
        }

        /// <summary>
        /// 檢查是否在移動範圍內
        /// </summary>
        public bool IsWithinBounds(Vector3 position)
        {
            if (!useMovementBounds) return true;

            return position.x >= actualBoundsMin.x && position.x <= actualBoundsMax.x &&
                   position.y >= actualBoundsMin.y && position.y <= actualBoundsMax.y;
        }

        // 在編輯器中顯示瞄準方向、目標連線和移動範圍
        private void OnDrawGizmosSelected()
        {
            // 顯示瞄準方向和目標連線
            if (firePoint != null && targetPoint != null)
            {
                // 畫出瞄準方向
                Gizmos.color = Color.red;
                Vector2 direction = (targetPoint.position - firePoint.position).normalized;
                Gizmos.DrawRay(firePoint.position, direction * 3f);

                // 畫出到目標點的連線
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(firePoint.position, targetPoint.position);
            }

            if (showAimDirection && firePoint != null)
            {
                // 畫出當前瞄準方向
                Gizmos.color = Color.green;
                Gizmos.DrawRay(firePoint.position, currentAimDirection * 2f);
            }

            // 顯示移動範圍
            if (useMovementBounds)
            {
                Gizmos.color = Color.cyan;
                Vector2 boundsMin, boundsMax;

                if (Application.isPlaying)
                {
                    boundsMin = actualBoundsMin;
                    boundsMax = actualBoundsMax;
                }
                else
                {
                    // 編輯器模式下預覽
                    if (relativeToBounds)
                    {
                        boundsMin = (Vector2)transform.position + movementBoundsMin;
                        boundsMax = (Vector2)transform.position + movementBoundsMax;
                    }
                    else
                    {
                        boundsMin = movementBoundsMin;
                        boundsMax = movementBoundsMax;
                    }
                }

                Vector3 center = (boundsMin + boundsMax) * 0.5f;
                Vector3 size = boundsMax - boundsMin;

                Gizmos.DrawWireCube(center, size);

                // 顯示範圍邊界點
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(boundsMin, 0.2f);
                Gizmos.DrawWireSphere(boundsMax, 0.2f);
            }
        }
    }

    // 簡單的子彈移動腳本
    public class BulletMovement : MonoBehaviour
    {
        private Vector2 velocity;
        public float lifetime = 5f; // 子彈生存時間

        public void Initialize(Vector2 vel)
        {
            velocity = vel;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.Translate(velocity * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 碰到其他物體時銷毀子彈（可根據需要修改）
            if (other.CompareTag("Player") || other.CompareTag("Wall"))
            {
                Destroy(gameObject);
            }
        }
    }
}