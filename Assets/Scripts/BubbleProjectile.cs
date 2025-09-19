using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class BubbleProjectile : MonoBehaviour
{
    public float speed = 6f;
    public float lifeTime = 8f;

    private Rigidbody2D rb;
    private Vector2 dir = Vector2.right;
    private float timeLeft;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        timeLeft = lifeTime;

        // 建議預置體在 Inspector 設為 Kinematic + Gravity 0 + Freeze Rotation
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void Launch(Vector2 direction, float overrideSpeed = -1f)
    {
        dir = direction.normalized;
        if (overrideSpeed > 0f) speed = overrideSpeed;

        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            // Kinematic 透過 MovePosition 推進
        }
        else
        {
            // Dynamic 直接給 velocity
            rb.linearVelocity = dir * speed;
        }
    }

    private void FixedUpdate()
    {
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
        }
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
