// 檔案名稱：PlayerHealth.cs
// 功能：管理玩家的生命值，並提供受傷和死亡的邏輯

using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("生命值設定")]
    public int maxHealth = 10; // 玩家的最大生命值
    private int currentHealth; // 玩家目前的生命值

    // Start is called before the first frame update
    void Start()
    {
        // 遊戲開始時，讓玩家擁有最大生命值
        currentHealth = maxHealth;
    }

    // 這是一個公開 (public) 的函式，可以被其他腳本（例如 EnemyAI）呼叫
    public void TakeDamage(int damage)
    {
        // 如果已經死亡，就不再執行任何扣血邏輯
        if (currentHealth <= 0) return;

        // 扣除傳入的傷害值
        currentHealth -= damage;
        
        // 在 Console 印出除錯訊息，方便我們確認
        Debug.Log("玩家受到 " + damage + " 點傷害，剩餘血量: " + currentHealth);

        // 通知角色管理器
        CharacterManager characterManager = GetComponent<CharacterManager>();
        if (characterManager != null)
            characterManager.OnPlayerTookDamage(damage);

        // 在這裡可以觸發玩家受擊動畫或音效
        // GetComponent<Animator>().SetTrigger("Hurt");

        // 檢查生命值是否小於等於 0
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 新增：治療方法
    public void Heal(int healAmount)
    {
        if (currentHealth <= 0) return; // 死亡狀態不能治療
        
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        Debug.Log("玩家恢復 " + healAmount + " 點血量，目前血量: " + currentHealth);
    }

    // 新增：獲取當前血量
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // 新增：獲取最大血量
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    // 新增：檢查是否存活
    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    private void Die()
    {
        Debug.Log("玩家死亡！遊戲結束！");
        
        // 在這裡加入玩家死亡後要執行的邏輯，例如：
        // 1. 播放死亡動畫
        // 2. 顯示「遊戲結束」的 UI 畫面
        // 3. 幾秒後重新載入場景
        
        // 作為一個簡單的範例，我們先讓玩家物件在場景中消失
        gameObject.SetActive(false);
    }
}