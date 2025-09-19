using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class NPCDialogueData
{
    public string npcName = "NPC";
    public string[] dialogueLines;
    public bool repeatDialogue = true; // 對話結束後是否可以重複

    [Header("對話設定")]
    public float dialogueDisplayTime = 3f; // 每句對話顯示時間
    public bool autoAdvance = false; // 是否自動推進對話
}

public class NPCDialogue : MonoBehaviour
{
    [Header("NPC 對話設定")]
    public NPCDialogueData dialogueData;

    [Header("互動設定")]
    public Transform objPoint; // NPC 的 ObjPoint，用於對話泡泡跟隨

    private int currentDialogueIndex = 0;
    private bool isInDialogue = false;
    private bool hasFinishedDialogue = false;

    public static event Action<NPCDialogue> OnNPCDialogueStart;
    public static event Action<NPCDialogue> OnNPCDialogueEnd;

    private void Awake()
    {
        // 如果沒有設置 objPoint，則使用自己的 Transform
        if (objPoint == null)
        {
            objPoint = transform;
        }

        // 初始化對話數據
        if (dialogueData.dialogueLines == null || dialogueData.dialogueLines.Length == 0)
        {
            dialogueData.dialogueLines = new string[] 
            {
                "你好！我是" + dialogueData.npcName,
                "有什麼我可以幫助你的嗎？",
                "祝你在這個世界玩得開心！"
            };
        }
    }

    /// <summary>
    /// 開始 NPC 對話
    /// </summary>
    public void StartDialogue()
    {
        if (isInDialogue) return;

        // 檢查是否可以重複對話
        if (hasFinishedDialogue && !dialogueData.repeatDialogue)
        {
            Debug.Log($"{dialogueData.npcName}: 對話已結束，無法重複對話");
            return;
        }

        isInDialogue = true;
        currentDialogueIndex = 0;

        // 通知其他系統對話開始
        OnNPCDialogueStart?.Invoke(this);

        // 顯示第一句對話
        ShowCurrentDialogue();

        Debug.Log($"開始與 {dialogueData.npcName} 的對話");
    }

    /// <summary>
    /// 推進到下一句對話
    /// </summary>
    public void AdvanceDialogue()
    {
        if (!isInDialogue) return;

        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueData.dialogueLines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowCurrentDialogue();
        }
    }

    /// <summary>
    /// 顯示當前對話
    /// </summary>
    private void ShowCurrentDialogue()
    {
        if (currentDialogueIndex >= dialogueData.dialogueLines.Length) return;

        string currentLine = dialogueData.dialogueLines[currentDialogueIndex];

        // 使用對話泡泡顯示文字
        if (DialogueBubbleUI.Instance != null)
        {
            DialogueBubbleUI.Instance.SetDialogueText(currentLine, objPoint.gameObject);
            DialogueBubbleUI.Instance.ShowDialogue();
        }

        Debug.Log($"{dialogueData.npcName}: {currentLine}");

        // 如果啟用自動推進
        if (dialogueData.autoAdvance)
        {
            Invoke(nameof(AdvanceDialogue), dialogueData.dialogueDisplayTime);
        }
    }

    /// <summary>
    /// 結束對話
    /// </summary>
    public void EndDialogue()
    {
        if (!isInDialogue) return;

        isInDialogue = false;
        hasFinishedDialogue = true;

        // 隱藏對話泡泡
        if (DialogueBubbleUI.Instance != null)
        {
            DialogueBubbleUI.Instance.HideDialogue();
        }

        // 通知其他系統對話結束
        OnNPCDialogueEnd?.Invoke(this);

        Debug.Log($"與 {dialogueData.npcName} 的對話結束");
    }

    /// <summary>
    /// 強制結束對話
    /// </summary>
    public void ForceEndDialogue()
    {
        CancelInvoke(); // 取消自動推進
        EndDialogue();
    }

    // 公開屬性
    public bool IsInDialogue => isInDialogue;
    public string NPCName => dialogueData.npcName;
    public bool CanRepeatDialogue => dialogueData.repeatDialogue || !hasFinishedDialogue;
    public Transform ObjPoint => objPoint;

    // 編輯器中的輔助方法
    [ContextMenu("測試開始對話")]
    private void TestStartDialogue()
    {
        StartDialogue();
    }

    [ContextMenu("測試推進對話")]
    private void TestAdvanceDialogue()
    {
        AdvanceDialogue();
    }

    private void OnDestroy()
    {
        CancelInvoke();
        if (isInDialogue)
        {
            ForceEndDialogue();
        }
    }
}
