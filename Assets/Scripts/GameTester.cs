using UnityEngine;
using System.Runtime.InteropServices;

public class GameTester : MonoBehaviour
{
    // [DllImport("__Internal")] 是 C# 用來呼叫網頁 JavaScript 函式的關鍵屬性。
    [DllImport("__Internal")]
    private static extern void SendGameResultToWeb(string gameResult);

    // 這個公有函式會被 UI 按鈕的 OnClick() 事件調用。
    public void OnClickEndGame()
    {
        // 準備一組固定的假數據。
        string fakeData = "{\"score\": 456, \"role_code\": \"K-POP_TRAINEE\"}";
        Debug.Log("UI 按鈕或 F12 鍵被按下，準備傳送假數據到網頁...");

        // 確保這段程式碼只在 WebGL 平台上運行。
#if !UNITY_EDITOR && UNITY_WEBGL
        SendGameResultToWeb(fakeData);
#endif
    }
    
    // Update 函式每幀都會運行，用來偵測按鍵。
    void Update()
    {
        // 偵測按下 F12 鍵，然後呼叫 OnClickEndGame 函式。
        if (Input.GetKeyDown(KeyCode.F12))
        {
            OnClickEndGame();
        }
    }
}