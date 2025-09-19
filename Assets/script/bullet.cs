// 檔案名稱：bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("子彈屬性")]
    public int damage = 1; // 這顆子彈的傷害值

    void Start()
    {
        // 確保子彈即使沒打中任何東西，也會在3秒後消失
        Destroy(gameObject, 3f);
    }

    // 當這個物件的碰撞體 (Is Trigger = false 的 Collider) 與另一個碰撞體接觸時被呼叫
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // === 核心修正處 ===
        // 嘗試從我們撞到的物件上獲取 EnemyAI 腳本
        EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();

        // 檢查是否真的有 EnemyAI 腳本 (代表我們撞到的是敵人)
        if (enemy != null)
        {
            // 如果有，就呼叫他的 TakeDamage 函式，並傳入這顆子彈的傷害值
            enemy.TakeDamage(damage);
        }
        
        // 不論撞到的是不是敵人（也可能是牆壁），都立刻銷毀子彈自己
        Destroy(gameObject);
    }
}