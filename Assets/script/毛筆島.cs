using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    // === 公開變數，可以在 Unity 編輯器中設定 ===
    public Transform pointA; // 飛船的起點
    public Transform pointB; // 飛船的終點
    public float speed = 2.0f; // 飛船的移動速度

    // === 私有變數 ===
    private Vector3 targetPosition; // 當前的目標位置
    private Rigidbody2D rb;
    private Vector3 lastPosition; // 記錄平台上一幀的位置
    private GameObject playerOnPlatform; // 記錄在平台上的玩家

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 遊戲開始時，先將飛船位置設為 A 點，並設定目標為 B 點
        transform.position = pointA.position;
        targetPosition = pointB.position;
        lastPosition = transform.position; // 記錄初始位置
    }

    void FixedUpdate()
    {
        // 記錄移動前的位置
        Vector3 previousPosition = transform.position;
        
        // 因為我們操作的是 Rigidbody2D (物理物件)，所以建議在 FixedUpdate 中處理移動
        // 使用 MovePosition 可以平滑地移動 Kinematic Rigidbody
        rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, speed * Time.fixedDeltaTime));

        // 如果有玩家在平台上，直接移動玩家
        if (playerOnPlatform != null)
        {
            // 計算平台移動的距離
            Vector3 platformMovement = transform.position - previousPosition;
            
            // Debug 信息
            if (platformMovement.magnitude > 0.001f) // 只在真的有移動時才顯示
            {
                Debug.Log($"平台移動距離: {platformMovement}, 玩家當前位置: {playerOnPlatform.transform.position}");
            }
            
            // 將玩家也移動相同的距離
            playerOnPlatform.transform.position += platformMovement;
            
            if (platformMovement.magnitude > 0.001f)
            {
                Debug.Log($"玩家移動後位置: {playerOnPlatform.transform.position}");
            }
        }

        // 檢查是否已到達目標點
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // 如果當前目標是 B 點，就切換到 A 點
            if (targetPosition == pointB.position)
            {
                targetPosition = pointA.position;
            }
            // 如果當前目標是 A 點，就切換到 B 點
            else
            {
                targetPosition = pointB.position;
            }
        }
    }

    // 當有其他物件進入我們的碰撞體時觸發
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 請務必加入這幾行來測試！
        Debug.Log("OnCollisionEnter2D 觸發了！");
        Debug.Log("碰撞到的物件是：" + collision.gameObject.name + ", Tag 是: " + collision.gameObject.tag);

        // 檢查撞進來的物件是不是玩家 (通常會為玩家設定 "Player" 標籤)
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("確認是 Player，記錄玩家物件！");
            // 記錄玩家物件，讓 FixedUpdate 可以直接移動它
            playerOnPlatform = collision.gameObject;
        }
    }

    // 當有其他物件離開我們的碰撞體時觸發
    private void OnCollisionExit2D(Collision2D collision)
    {
        // 檢查離開的物件是不是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player 離開平台！");
            // 清除玩家物件記錄
            playerOnPlatform = null;
        }
    }
}