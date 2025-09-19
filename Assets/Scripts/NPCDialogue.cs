using UnityEngine;
using System.Collections.Generic;
using System;

public enum SpeakerType
{
    Player,    // 玩家說話
    NPC       // NPC說話
}

[System.Serializable]
public class NPCDialogueData
{
    public string npcName = "NPC";
    public string[] dialogueLines;
    public SpeakerType[] speakers; // 每句對話的說話者，如果長度不匹配則默認為NPC
    public bool repeatDialogue = true; // 對話結束後是否可以重複

    [Header("對話設定")]
    public float dialogueDisplayTime = 3f; // 每句對話顯示時間
    public bool autoAdvance = false; // 是否自動推進對話

    /// <summary>
    /// 獲取指定索引的說話者類型
    /// </summary>
    public SpeakerType GetSpeaker(int index)
    {
        if (speakers == null || index >= speakers.Length || index < 0)
            return SpeakerType.NPC; // 默認為NPC說話

        return speakers[index];
    }

    /// <summary>
    /// 檢查說話者陣列長度並自動調整
    /// </summary>
    public void ValidateSpeakers()
    {
        if (dialogueLines == null) return;

        if (speakers == null || speakers.Length != dialogueLines.Length)
        {
            SpeakerType[] newSpeakers = new SpeakerType[dialogueLines.Length];

            // 複製現有的說話者設定
            if (speakers != null)
            {
                for (int i = 0; i < Mathf.Min(speakers.Length, newSpeakers.Length); i++)
                {
                    newSpeakers[i] = speakers[i];
                }
            }

            // 剩餘的設為NPC
            for (int i = speakers?.Length ?? 0; i < newSpeakers.Length; i++)
            {
                newSpeakers[i] = SpeakerType.NPC;
            }

            speakers = newSpeakers;
        }
    }
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
                "很高興見到你！",
                "有什麼我可以幫助你的嗎？",
                "謝謝你！",
                "祝你在這個世界玩得開心！"
            };

            // 設置對應的說話者（交替對話示例）
            dialogueData.speakers = new SpeakerType[]
            {
                SpeakerType.NPC,    // NPC: "你好！我是..."
                SpeakerType.Player, // 玩家: "很高興見到你！"
                SpeakerType.NPC,    // NPC: "有什麼我可以幫助..."
                SpeakerType.Player, // 玩家: "謝謝你！"
                SpeakerType.NPC     // NPC: "祝你在這個世界..."
            };
        }

        // 驗證並調整說話者陣列
        dialogueData.ValidateSpeakers();
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
        SpeakerType currentSpeaker = dialogueData.GetSpeaker(currentDialogueIndex);

        // 獲取說話者名稱用於Debug
        string speakerName = currentSpeaker == SpeakerType.Player ? "玩家" : dialogueData.npcName;

        // 使用對話泡泡顯示文字
        if (DialogueBubbleUI.Instance != null)
        {
            // 根據說話者類型決定跟隨的物件
            GameObject speakerObject = currentSpeaker == SpeakerType.Player ? 
                DialogueBubbleUI.Instance.GetPlayerObject() : objPoint.gameObject;

            DialogueBubbleUI.Instance.SetDialogueText(currentLine, speakerObject, currentSpeaker);
            DialogueBubbleUI.Instance.ShowDialogue();
        }

        Debug.Log($"[NPCDialogue] {speakerName}: {currentLine}");
        Debug.Log($"[NPCDialogue] 當前說話者: {currentSpeaker}, 對話索引: {currentDialogueIndex}/{dialogueData.dialogueLines.Length - 1}");

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

    [ContextMenu("驗證說話者陣列")]
    private void TestValidateSpeakers()
    {
        dialogueData.ValidateSpeakers();
        Debug.Log($"[NPCDialogue] 說話者陣列已驗證，對話行數: {dialogueData.dialogueLines?.Length ?? 0}, 說話者數: {dialogueData.speakers?.Length ?? 0}");

        // 顯示說話者配置
        if (dialogueData.dialogueLines != null && dialogueData.speakers != null)
        {
            for (int i = 0; i < dialogueData.dialogueLines.Length; i++)
            {
                SpeakerType speaker = dialogueData.GetSpeaker(i);
                string speakerName = speaker == SpeakerType.Player ? "玩家" : dialogueData.npcName;
                Debug.Log($"[NPCDialogue] 第{i+1}句: [{speakerName}] {dialogueData.dialogueLines[i]}");
            }
        }
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
