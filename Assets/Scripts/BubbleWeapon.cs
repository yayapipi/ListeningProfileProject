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

    [Header("瞄準偏移")]
    [Tooltip("瞄準角度偏移（度）")]
    public float aimAngleOffset = 0f;
    [Tooltip("瞄準向量偏移")]
    public Vector2 aimVectorOffset = Vector2.zero;
    [Tooltip("是否使用角度偏移（否則使用向量偏移）")]
    public bool useAngleOffset = true;

    private float cdTimer = 0f;
    private Vector2 currentAim = Vector2.right;

    public void SetAim(Vector2 aimWorldDirection)
    {
        if (aimWorldDirection.sqrMagnitude >= aimDeadZone * aimDeadZone)
        {
            Vector2 baseAim = aimWorldDirection.normalized;

            // 應用偏移
            Vector2 offsetAim = ApplyAimOffset(baseAim);

            currentAim = offsetAim.normalized;

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

    /// <summary>
    /// 應用瞄準偏移
    /// </summary>
    private Vector2 ApplyAimOffset(Vector2 originalAim)
    {
        Vector2 resultAim = originalAim;

        if (useAngleOffset)
        {
            // 使用角度偏移
            if (Mathf.Abs(aimAngleOffset) > 0.01f)
            {
                float angleRad = aimAngleOffset * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angleRad);
                float sin = Mathf.Sin(angleRad);

                // 旋轉矩陣
                resultAim = new Vector2(
                    originalAim.x * cos - originalAim.y * sin,
                    originalAim.x * sin + originalAim.y * cos
                );
            }
        }
        else
        {
            // 使用向量偏移
            if (aimVectorOffset.sqrMagnitude > 0.01f)
            {
                resultAim = originalAim + aimVectorOffset;
            }
        }

        return resultAim;
    }

    /// <summary>
    /// 設置角度偏移
    /// </summary>
    public void SetAimAngleOffset(float angleOffset)
    {
        aimAngleOffset = angleOffset;
        useAngleOffset = true;
    }

    /// <summary>
    /// 設置向量偏移
    /// </summary>
    public void SetAimVectorOffset(Vector2 vectorOffset)
    {
        aimVectorOffset = vectorOffset;
        useAngleOffset = false;
    }

    /// <summary>
    /// 獲取應用偏移後的瞄準方向
    /// </summary>
    public Vector2 GetAimWithOffset(Vector2 originalAim)
    {
        return ApplyAimOffset(originalAim.normalized).normalized;
    }

    /// <summary>
    /// 重置所有偏移
    /// </summary>
    public void ResetAimOffset()
    {
        aimAngleOffset = 0f;
        aimVectorOffset = Vector2.zero;
    }
}
