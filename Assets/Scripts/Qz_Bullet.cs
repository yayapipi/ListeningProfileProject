using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace DefaultNamespace
{
    public class Qz_Bullet : MonoBehaviour
    {
        [Header("Tag 設置")] public string bulletTag = "bubble"; // 普通子彈標籤
        public string megaTag = "mega"; // mega子彈標籤
        public string pigBossTag = "pigboss"; // pigboss標籤

        [Header("動畫設置")] public Animator bulletAnimator; // 子彈的動畫控制器
        public string megaAnimationTrigger = "mega"; // mega動畫觸發器名稱

        [Header("血量UI")] public Slider healthSlider; // 血量滑塊

        [Header("遊戲結束")] public GameObject gameOverObject; // 遊戲結束時要顯示的物件

        [Header("Boss設置")] public GameObject pigBossObject; // PigBoss 物件引用（可選）

        [Header("傷害設置")] public float damageAmount = 3f; // 每次傷害量

        [Header("銷毀設置")] public bool destroyOnCollision = true; // 碰撞時是否銷毀子彈
        public float destroyDelay = 0f; // 銷毀延遲時間（秒）
        public bool destroyOnPigBossHit = true; // 攻擊 pigboss 後是否銷毀
        public bool destroyOnUpgrade = false; // 升級為 mega 後是否銷毀

        [Header("視覺效果")] public float flashDuration = 0.1f; // 閃紅持續時間
        public Color flashColor = Color.red; // 閃爍顏色

        [Header("調試")] public bool enableDebugLog = true; // 是否啟用調試日誌

        private bool isMegaBullet = false; // 是否已經是mega子彈
        public float currentHealth = 10; // 當前血量



        void OnCollisionEnter2D(Collision2D collision)
        {
            if (enableDebugLog)
            {
                Debug.Log(
                    $"[Qz_Bullet] 碰撞檢測 - 碰撞物件: {collision.gameObject.name}, 標籤: {collision.gameObject.tag}, 當前是否為Mega: {isMegaBullet}");
            }

            bool shouldDestroy = false;

            // 第一階段：普通子彈碰到 bullet tag
            if (!isMegaBullet && collision.gameObject.CompareTag(bulletTag))
            {
                UpgradeToMega();
                shouldDestroy = destroyOnUpgrade;
            }
            // 第二階段：mega子彈碰到 pigboss
            else if (isMegaBullet && collision.gameObject.CompareTag(pigBossTag))
            {
                AttackPigBoss(collision.gameObject);
                shouldDestroy = destroyOnPigBossHit;
            }
            // 其他任何碰撞
            else if (destroyOnCollision)
            {
                shouldDestroy = true;
                if (enableDebugLog)
                {
                    Debug.Log($"[Qz_Bullet] 碰撞到其他物體: {collision.gameObject.name}，準備銷毀");
                }
            }

            // 執行銷毀邏輯
            if (shouldDestroy)
            {
                DestroyBullet();
            }
        }

        /// <summary>
        /// 升級為 Mega 子彈
        /// </summary>
        private void UpgradeToMega()
        {
            // 播放mega動畫
            if (bulletAnimator != null)
            {
                bulletAnimator.SetTrigger(megaAnimationTrigger);

                if (enableDebugLog)
                {
                    Debug.Log($"[Qz_Bullet] 播放動畫觸發器: {megaAnimationTrigger}");
                }
            }
            else if (enableDebugLog)
            {
                Debug.LogWarning("[Qz_Bullet] bulletAnimator 未設置，無法播放動畫");
            }

            // 切換tag為mega
            gameObject.tag = megaTag;
            isMegaBullet = true;

            if (enableDebugLog)
            {
                Debug.Log($"[Qz_Bullet] 子彈升級為 Mega 子彈！標籤已變更為: {megaTag}");
            }
        }

        /// <summary>
        /// 攻擊 PigBoss
        /// </summary>
        private void AttackPigBoss(GameObject pigBoss)
        {
            // 扣血
            DealDamage();

            // 讓pigboss閃紅色
            StartCoroutine(FlashRed(pigBoss));


        }

        /// <summary>
        /// 造成傷害的方法
        /// </summary>
        void DealDamage()
        {
            currentHealth -= damageAmount;

            // 確保血量不會小於0
            if (currentHealth < 0)
            {
                currentHealth = 0;
            }

            // 更新血量UI
            if (healthSlider != null)
            {
                healthSlider.value = currentHealth;

                if (enableDebugLog)
                {
                    Debug.Log($"[Qz_Bullet] 血量UI已更新 - 當前值: {healthSlider.value}, 最大值: {healthSlider.maxValue}");
                }
            }
            else if (enableDebugLog)
            {
                Debug.LogWarning("[Qz_Bullet] healthSlider 未設置，無法更新血量UI");
            }

            // 檢查是否血量歸零
            if (currentHealth <= 0)
            {
                GameOver();
            }
        }

        /// <summary>
        /// 遊戲結束方法
        /// </summary>
        void GameOver()
        {
            if (gameOverObject != null)
            {
                gameOverObject.SetActive(true);

                if (enableDebugLog)
                {
                    Debug.Log($"[Qz_Bullet] 遊戲結束物件 '{gameOverObject.name}' 已激活");
                }
            }
            else if (enableDebugLog)
            {
                Debug.LogWarning("[Qz_Bullet] gameOverObject 未設置，無法顯示遊戲結束畫面");
            }

            if (enableDebugLog)
            {
                Debug.Log("[Qz_Bullet] 🎮 遊戲結束！PigBoss被擊敗！");
            }

            // 可以在這裡添加其他遊戲結束的邏輯
            // 例如：暫停遊戲、播放音效、顯示結算畫面等
            OnGameOver();
        }

        /// <summary>
        /// 遊戲結束時的額外邏輯（可以被覆寫或擴展）
        /// </summary>
        protected virtual void OnGameOver()
        {
            // 暫停遊戲時間（可選）
            // Time.timeScale = 0f;

            // 可以在這裡添加音效播放、統計記錄等邏輯
        }

        /// <summary>
        /// 銷毀子彈
        /// </summary>
        private void DestroyBullet()
        {
            if (enableDebugLog)
            {
                Debug.Log($"[Qz_Bullet] 準備銷毀子彈，延遲: {destroyDelay}秒");
            }

            if (destroyDelay <= 0f)
            {
                // 立即銷毀
                Destroy(gameObject);
            }
            else
            {
                // 延遲銷毀
                StartCoroutine(DestroyAfterDelay());
            }
        }

        /// <summary>
        /// 延遲銷毀協程
        /// </summary>
        private IEnumerator DestroyAfterDelay()
        {
            if (enableDebugLog)
            {
                Debug.Log($"[Qz_Bullet] 開始延遲銷毀計時: {destroyDelay}秒");
            }

            yield return new WaitForSeconds(destroyDelay);

            if (enableDebugLog)
            {
                Debug.Log($"[Qz_Bullet] 延遲時間結束，銷毀子彈");
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// 手動銷毀子彈（公開方法）
        /// </summary>
        public void ManualDestroy()
        {
            DestroyBullet();
        }

        /// <summary>
        /// 閃紅色效果的協程
        /// </summary>
        IEnumerator FlashRed(GameObject target)
        {
            if (target == null)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning("[Qz_Bullet] FlashRed: 目標物件為空");
                }

                yield break;
            }

            Renderer renderer = target.GetComponent<Renderer>();
            SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();

            if (renderer == null && spriteRenderer == null)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning($"[Qz_Bullet] FlashRed: 目標物件 '{target.name}' 沒有 Renderer 或 SpriteRenderer 組件");
                }

                yield break;
            }

            Color originalColor = Color.white;

            // 獲取原始顏色
            if (renderer != null)
            {
                originalColor = renderer.material.color;
            }
            else if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            // 變成指定的閃爍顏色
            if (renderer != null)
            {
                renderer.material.color = flashColor;
            }
            else if (spriteRenderer != null)
            {
                spriteRenderer.color = flashColor;
            }

            if (enableDebugLog)
            {
                Debug.Log($"[Qz_Bullet] {target.name} 開始閃爍效果，持續時間: {flashDuration}秒");
            }

            // 等待指定時間
            yield return new WaitForSeconds(flashDuration);

            // 恢復原始顏色
            if (target != null) // 確保物件仍然存在
            {
                if (renderer != null)
                {
                    renderer.material.color = originalColor;
                }
                else if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }

                if (enableDebugLog)
                {
                    Debug.Log($"[Qz_Bullet] {target.name} 閃爍效果結束，顏色已恢復");
                }
            }
        }

        /// <summary>
        /// 重置遊戲的方法（可選，用於重新開始遊戲）
        /// </summary>
        public void ResetGame()
        {
            isMegaBullet = false;
            gameObject.tag = "Untagged"; // 或者設置為初始tag

            if (healthSlider != null)
            {
                healthSlider.value = currentHealth;
            }

            if (gameOverObject != null)
            {
                gameOverObject.SetActive(false);
            }

            // 恢復遊戲時間（如果之前暫停了）
            Time.timeScale = 1f;

            if (enableDebugLog)
            {
                Debug.Log("[Qz_Bullet] 遊戲已重置");
            }
        }

        /// <summary>
        /// 手動設置為 Mega 子彈（測試用）
        /// </summary>
        [ContextMenu("設置為 Mega 子彈")]
        public void SetAsMegaBullet()
        {
            UpgradeToMega();
        }

        /// <summary>
        /// 手動造成傷害（測試用）
        /// </summary>
        [ContextMenu("造成傷害")]
        public void TestDealDamage()
        {
            DealDamage();
        }

        /// <summary>
        /// 顯示當前狀態（調試用）
        /// </summary>
        [ContextMenu("顯示當前狀態")]
        public void ShowCurrentState()
        {
            Debug.Log($"=== Qz_Bullet 當前狀態 ===");
            Debug.Log($"是否為 Mega 子彈: {isMegaBullet}");
            Debug.Log($"當前標籤: {gameObject.tag}");
            Debug.Log($"設定的標籤 - 子彈: {bulletTag}, Mega: {megaTag}, Boss: {pigBossTag}");
            Debug.Log($"銷毀設置 - 碰撞銷毀: {destroyOnCollision}, 攻擊Boss後銷毀: {destroyOnPigBossHit}, 升級後銷毀: {destroyOnUpgrade}");
            Debug.Log($"銷毀延遲: {destroyDelay}秒");
            Debug.Log($"=======================");
        }

        /// <summary>
        /// 測試銷毀子彈（測試用）
        /// </summary>
        [ContextMenu("測試銷毀子彈")]
        public void TestDestroyBullet()
        {
            DestroyBullet();
        }

        // 公開屬性供其他腳本訪問
        public bool IsMegaBullet => isMegaBullet;
        public float CurrentHealth => currentHealth;
        public bool IsGameOver => currentHealth <= 0;
    }
}