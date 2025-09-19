using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(DialogueManager))]
public class CharacterManager : MonoBehaviour
{
    [Header("角色系統引用")]
    public PlayerController playerController;
    public PlayerHealth playerHealth;
    public DialogueManager dialogueManager;
    
    [Header("角色狀態")]
    public bool isAlive = true;
    public bool isInDialogue = false;
    
    [Header("統計數據")]
    [SerializeField] private int totalDamageTaken = 0;
    [SerializeField] private int enemiesKilled = 0;
    [SerializeField] private float playTime = 0f;
    
    void Awake()
    {
        InitializeComponents();
        SetupEventHandlers();
    }
    
    void Start()
    {
        ValidateSetup();
    }
    
    void Update()
    {
        if (isAlive)
        {
            playTime += Time.deltaTime;
            UpdateCharacterState();
        }
    }
    
    private void InitializeComponents()
    {
        // 自動獲取所需組件
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
            
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
            
        if (dialogueManager == null)
            dialogueManager = GetComponent<DialogueManager>();
    }
    
    private void SetupEventHandlers()
    {
        // 可以在這裡設置事件處理器，當其他腳本有事件系統時
    }
    
    private void ValidateSetup()
    {
        if (playerController == null)
            Debug.LogError("CharacterManager: PlayerController 組件遺失！");
            
        if (playerHealth == null)
            Debug.LogError("CharacterManager: PlayerHealth 組件遺失！");
            
        if (dialogueManager == null)
            Debug.LogError("CharacterManager: DialogueManager 組件遺失！");
    }
    
    private void UpdateCharacterState()
    {
        // 檢查角色狀態
        isAlive = playerHealth != null && playerHealth.GetCurrentHealth() > 0;
        isInDialogue = dialogueManager != null && dialogueManager.IsDialogueActive();
        
        // 根據狀態調整其他系統
        if (isInDialogue && playerController != null)
        {
            // 對話時禁用玩家控制
            playerController.enabled = false;
        }
        else if (!isInDialogue && playerController != null)
        {
            // 非對話時啟用玩家控制
            playerController.enabled = true;
        }
    }
    
    // 公開方法給外部系統使用
    public void OnPlayerTookDamage(int damage)
    {
        totalDamageTaken += damage;
        Debug.Log($"玩家總共受到 {totalDamageTaken} 點傷害");
    }
    
    public void OnEnemyKilled()
    {
        enemiesKilled++;
        Debug.Log($"已擊敗 {enemiesKilled} 個敵人");
    }
    
    public void StartDialogue()
    {
        if (dialogueManager != null)
            dialogueManager.StartDialogue();
    }
    
    public void EndDialogue()
    {
        if (dialogueManager != null)
            dialogueManager.EndDialogue();
    }
    
    public void RestoreFullHealth()
    {
        if (playerHealth != null && playerHealth.GetMaxHealth() > 0)
        {
            int healAmount = playerHealth.GetMaxHealth() - playerHealth.GetCurrentHealth();
            playerHealth.Heal(healAmount);
        }
    }
    
    // 獲取角色統計信息
    public CharacterStats GetCharacterStats()
    {
        return new CharacterStats
        {
            currentHealth = playerHealth != null ? playerHealth.GetCurrentHealth() : 0,
            maxHealth = playerHealth != null ? playerHealth.GetMaxHealth() : 0,
            totalDamageTaken = this.totalDamageTaken,
            enemiesKilled = this.enemiesKilled,
            playTime = this.playTime,
            isAlive = this.isAlive
        };
    }
}

[System.Serializable]
public class CharacterStats
{
    public int currentHealth;
    public int maxHealth;
    public int totalDamageTaken;
    public int enemiesKilled;
    public float playTime;
    public bool isAlive;
}