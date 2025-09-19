using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("對話UI元件")]
    public GameObject dialoguePanel;
    public Text dialogueText;
    public Text nameText;
    public Button[] responseButtons;
    public Text[] responseTexts;

    [Header("AI對話設定")]
    public float textSpeed = 0.05f;
    public KeyCode interactKey = KeyCode.E;
    
    [Header("AI角色設定")]
    public string aiCharacterName = "AI助手";
    public List<DialogueNode> dialogueNodes = new List<DialogueNode>();

    private Queue<string> sentences;
    private bool isDialogueActive = false;
    private bool isPlayerNearby = false;
    private GameObject currentNPC;
    private int currentNodeIndex = 0;
    
    void Start()
    {
        sentences = new Queue<string>();
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        InitializeDefaultDialogues();
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(interactKey) && !isDialogueActive)
        {
            StartDialogue();
        }
    }

    private void InitializeDefaultDialogues()
    {
        // 初始化預設AI對話節點
        DialogueNode greeting = new DialogueNode();
        greeting.dialogueText = "你好！我是AI助手，很高興見到你！";
        greeting.responses = new string[] { "你好！", "你是誰？", "再見" };
        greeting.responseActions = new int[] { 1, 2, -1 };
        
        DialogueNode friendly = new DialogueNode();
        friendly.dialogueText = "我很高興和你交談！有什麼我可以幫助你的嗎？";
        friendly.responses = new string[] { "告訴我遊戲技巧", "沒事，謝謝", "你的功能是什麼？" };
        friendly.responseActions = new int[] { 3, -1, 4 };
        
        DialogueNode whoAmI = new DialogueNode();
        whoAmI.dialogueText = "我是這個世界的AI嚮導，負責幫助冒險者們！";
        whoAmI.responses = new string[] { "很棒！", "能幫我什麼？", "再見" };
        whoAmI.responseActions = new int[] { 1, 3, -1 };
        
        DialogueNode tips = new DialogueNode();
        tips.dialogueText = "記住：使用WASD移動，空格跳躍，滑鼠左鍵攻擊。小心敵人！";
        tips.responses = new string[] { "謝謝你的建議！", "還有其他技巧嗎？", "我知道了" };
        tips.responseActions = new int[] { 1, 5, -1 };
        
        DialogueNode features = new DialogueNode();
        features.dialogueText = "我可以提供遊戲指導、回答問題，還能和你聊天！";
        features.responses = new string[] { "太好了！", "能給我一些建議嗎？", "明白了" };
        features.responseActions = new int[] { 1, 3, -1 };
        
        DialogueNode moreTips = new DialogueNode();
        moreTips.dialogueText = "收集道具能增強實力，探索隱藏區域能找到寶藏！";
        moreTips.responses = new string[] { "非常有用！", "謝謝！", "我會記住的" };
        moreTips.responseActions = new int[] { 1, -1, -1 };

        dialogueNodes.AddRange(new DialogueNode[] { greeting, friendly, whoAmI, tips, features, moreTips });
    }

    public void StartDialogue()
    {
        if (isDialogueActive) return;
        
        isDialogueActive = true;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
            
        currentNodeIndex = 0;
        DisplayCurrentNode();
        
        // 暫停玩家移動
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.enabled = false;
    }

    private void DisplayCurrentNode()
    {
        if (currentNodeIndex >= dialogueNodes.Count) 
        {
            EndDialogue();
            return;
        }
        
        DialogueNode currentNode = dialogueNodes[currentNodeIndex];
        
        if (nameText != null)
            nameText.text = aiCharacterName;
            
        StartCoroutine(TypeSentence(currentNode.dialogueText));
        SetupResponseButtons(currentNode);
    }

    IEnumerator TypeSentence(string sentence)
    {
        if (dialogueText == null) yield break;
        
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    private void SetupResponseButtons(DialogueNode node)
    {
        for (int i = 0; i < responseButtons.Length; i++)
        {
            if (i < node.responses.Length && responseButtons[i] != null)
            {
                responseButtons[i].gameObject.SetActive(true);
                if (responseTexts[i] != null)
                    responseTexts[i].text = node.responses[i];
                
                int responseIndex = i;
                responseButtons[i].onClick.RemoveAllListeners();
                responseButtons[i].onClick.AddListener(() => HandleResponse(node.responseActions[responseIndex]));
            }
            else if (responseButtons[i] != null)
            {
                responseButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void HandleResponse(int nextNodeIndex)
    {
        if (nextNodeIndex == -1)
        {
            EndDialogue();
            return;
        }
        
        currentNodeIndex = nextNodeIndex;
        DisplayCurrentNode();
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        // 恢復玩家移動
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.enabled = true;
    }

    // 新增：檢查對話是否活動中
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    // 新增：檢查玩家是否在對話範圍內
    public bool IsPlayerNearby()
    {
        return isPlayerNearby;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            currentNPC = gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (isDialogueActive)
                EndDialogue();
        }
    }
}

[System.Serializable]
public class DialogueNode
{
    public string dialogueText;
    public string[] responses;
    public int[] responseActions; // -1 表示結束對話，其他數字表示下一個節點的索引
}
