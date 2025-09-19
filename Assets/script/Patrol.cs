using UnityEngine;

public class Patrol : MonoBehaviour
{
    public float moveSpeed = 3f; 
    public Transform groundDetection; 

    private Rigidbody2D rb;
    private bool movingRight = false;

    // 這會告訴腳本要偵測什麼圖層
    public LayerMask whatIsGround;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.eulerAngles = new Vector2(0, 180);

        // 設定 whatIsGround 為除了 Player 圖層之外的所有圖層
        whatIsGround = ~LayerMask.GetMask("Player");
    }

    void Update()
    {
        // 讓角色向當前方向移動
        if (movingRight)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
        }

        // 發射射線，並指定只偵測 whatIsGround 圖層
        RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, 1f, whatIsGround);

        // 如果腳下沒有東西，就回頭
        if (groundInfo.collider == null)
        {
            TurnAround();
        }
    }

    void TurnAround()
    {
        movingRight = !movingRight;
        
        if (movingRight)
        {
            transform.eulerAngles = new Vector2(0, 0);
        }
        else
        {
            transform.eulerAngles = new Vector2(0, 180);
        }
    }
}