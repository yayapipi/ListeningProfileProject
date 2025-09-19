using UnityEngine;

[DisallowMultipleComponent]
public class BubbleWeapon : MonoBehaviour
{
    [Header("參考")]
    public Transform firePoint;       // 發射起點
    public Transform aimArrow;        // 箭頭物件（其 local +X 方向指向目標）
    public GameObject bubblePrefab;   // 泡泡預置體（需含 Rigidbody2D + Collider2D）

    [Header("參數")]
    public float projectileSpeed = 6f;
    public float fireCooldown = 0.15f;
    [Tooltip("忽略太小的瞄準變化")]
    public float aimDeadZone = 0.01f;

    private float cdTimer = 0f;
    private Vector2 currentAim = Vector2.right;

    public void SetAim(Vector2 aimWorldDirection)
    {
        if (aimWorldDirection.sqrMagnitude >= aimDeadZone * aimDeadZone)
        {
            currentAim = aimWorldDirection.normalized;
            if (aimArrow != null)
            {
                // 讓箭頭 +X 方向指向 currentAim
                aimArrow.right = currentAim;
            }
        }
    }

    public bool TryFire()
    {
        if (cdTimer > 0f) return false;
        if (bubblePrefab == null || firePoint == null) return false;

        cdTimer = fireCooldown;

        var go = Object.Instantiate(bubblePrefab, firePoint.position, Quaternion.identity);

        // 若預置體有 BubbleProjectile，優先使用
        var proj = go.GetComponent<BubbleProjectile>();
        if (proj != null)
        {
            proj.Launch(currentAim, projectileSpeed);
        }
        else
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 動態剛體：以速度推進
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.linearVelocity = currentAim * projectileSpeed;
            }
        }
        return true;
    }

    // UI 按鈕可直接綁這個函式
    public void UIClickFire()
    {
        TryFire();
    }

    public Vector2 CurrentAim => currentAim;

    private void Update()
    {
        if (cdTimer > 0f) cdTimer -= Time.deltaTime;
    }
}
