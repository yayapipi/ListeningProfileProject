using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class IslandVideoController : MonoBehaviour
{
    public string videoURL;
    public VideoPlayer videoPlayer;

    private bool hasInteracted = false;

    void Start()
    {
        Debug.Log("IslandVideoController 腳本已啟動！等待使用者互動...");
    }

    void Update()
    {
        // 偵測滑鼠點擊或鍵盤按鍵
        if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
        {
            if (!hasInteracted)
            {
                hasInteracted = true;
                StartCoroutine(DelayedVideoStart());
            }
        }
    }

    IEnumerator DelayedVideoStart()
    {
        // 延遲0.5秒，確保瀏覽器準備就緒
        yield return new WaitForSeconds(0.5f);

        Debug.Log("偵測到使用者互動！正在播放影片...");
        if (videoPlayer != null)
        {
            videoPlayer.url = videoURL;
            videoPlayer.Prepare();
            videoPlayer.Play();
            Debug.Log("影片播放程式碼已執行！");
        }
        else
        {
            Debug.LogError("VideoPlayer 元件未連結！");
        }
    }
}