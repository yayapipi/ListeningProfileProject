// 檔案名稱：EnemyAI.cs
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 2.0f;

    [Header("AI 行為設定")]
    public float attackRange = 1.0f;
    public float detectionRange = 8f;

    [Header("生命值設定")]
    public int maxHealth = 3;

    [Header("攻擊設定")]
    public float attackCooldown = 2f;
    public int attackDamage = 1;

    [Header("必要組件 (請在 Inspector 中指派)")]
    public Transform edgeCheckPoint;
    public LayerMask groundLayer;

    [Header("動畫設定")]
    public float deathAnimationTime = 2.5f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private int currentHealth;
    private bool isDead = false;
    private float nextAttackTime = 0f;
    private float moveDirection = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        moveDirection = transform.localScale.x > 0 ? 1 : -1;
        
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("場景中找不到 Tag 為 'Player' 的物件，敵人將會自主巡邏。");
        }
    }

    void Update()
    {
        if (isDead) return;

        if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            if (Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                StopWalking();
                FacePlayer();
                if (Time.time >= nextAttackTime)
                {
                    Attack();
                }
            }
            else
            {
                Chase();
            }
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        animator.SetBool("IsWalking", true);
        bool isAtEdge = !Physics2D.OverlapCircle(edgeCheckPoint.position, 0.1f, groundLayer);
        if (isAtEdge)
        {
            Flip();
        }
        rb.linearVelocity = new Vector2(moveDirection * speed, rb.linearVelocity.y);
    }

    private void Chase()
    {
        FacePlayer();
        animator.SetBool("IsWalking", true);
        rb.linearVelocity = new Vector2(moveDirection * speed, rb.linearVelocity.y);
    }

    private void StopWalking()
    {
        animator.SetBool("IsWalking", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    
    private void Attack()
    {
        nextAttackTime = Time.time + attackCooldown;
        animator.SetTrigger("Attack");
    }

    private void FacePlayer()
    {
        float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
        if (directionToPlayer != moveDirection)
        {
            Flip();
        }
    }

    private void Flip()
    {
        moveDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }
    
    public void DealDamageToPlayer()
    {
        if (isDead) return;
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange * 1.2f)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        Debug.Log(gameObject.name + " 受到 " + damage + " 點傷害，剩餘血量: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log(gameObject.name + " 已死亡！");
        
        // 通知玩家的CharacterManager
        if (player != null)
        {
            CharacterManager characterManager = player.GetComponent<CharacterManager>();
            if (characterManager != null)
                characterManager.OnEnemyKilled();
        }
        
        animator.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero; 
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false; 
        this.enabled = false;
        Destroy(gameObject, deathAnimationTime);
    }
}