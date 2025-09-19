using UnityEngine;

public class Bubble : MonoBehaviour
{
    private FixedJoint2D fixedJoint;
    private Animator animator;

    // 讓玩家腳本可以設定泡泡的初始速度
    public void SetVelocity(Vector2 direction, float speed)
    {
        GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        
        // 讓泡泡在一段時間後自動銷毀
        Destroy(gameObject, 10f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 判斷碰撞到的物件是否為玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            // 檢查玩家是否從上方踩到泡泡，防止從側面黏住
            if (collision.contacts[0].normal.y < 0.9f)
            {
                PopAndDestroy();
                return; 
            }

            if (fixedJoint == null)
            {
                fixedJoint = collision.gameObject.AddComponent<FixedJoint2D>();
                fixedJoint.connectedBody = GetComponent<Rigidbody2D>();
            }
        }
        else
        {
            // 撞到任何其他東西，都觸發破裂動畫並銷毀
            PopAndDestroy();
        }
    }

    private void PopAndDestroy()
    {
        animator.SetTrigger("Pop");

        // 停止泡泡的物理運動，讓它靜止下來
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        GetComponent<Rigidbody2D>().isKinematic = true;
        
        // 移除所有碰撞器，防止玩家還能踩到
        GetComponent<Collider2D>().enabled = false;
        
        if (fixedJoint != null)
        {
            Destroy(fixedJoint);
        }

        // 讓泡泡在破裂動畫播放完畢後銷毀 (0.5秒)
        Destroy(gameObject, 0.5f);
    }
}