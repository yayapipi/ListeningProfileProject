using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OrbitMotion : MonoBehaviour
{
    // --- 軌道設定 ---
    [Header("Orbit Settings")]
    public Transform orbitCenter;

    // [新增] 軌道中心點的偏移量
    [Tooltip("相對於環繞中心的偏移量 (X, Y)")]
    public Vector2 orbitOffset = Vector2.zero;

    [Tooltip("軌道的水平半徑 (寬度)")]
    public float orbitRadiusX = 6f;
    [Tooltip("軌道的垂直半徑 (高度)")]
    public float orbitRadiusY = 3f;

    [Tooltip("環繞速度 (角度/秒)")]
    public float orbitSpeed = 50f;

    // --- 縮放設定 ---
    [Header("Scaling Settings")]
    [Tooltip("在軌道最高點(最遠)時的尺寸")]
    public float minScale = 0.8f;
    [Tooltip("在軌道最低點(最近)時的尺寸")]
    public float maxScale = 1.2f;

    // --- (可選) 參考中心點的渲染器 ---
    [Header("Renderer Settings")]
    [Tooltip("將島嶼的 Sprite Renderer 拖到這裡，來自動處理前後關係")]
    public SpriteRenderer centerSpriteRenderer;

    private float angle = 0f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (orbitCenter == null)
        {
            Debug.LogError("Orbit Center is not assigned!", this.gameObject);
            return;
        }

        // [修改] 計算包含偏移量的實際中心點
        Vector3 actualCenter = orbitCenter.position + (Vector3)orbitOffset;

        // --- 1. 更新角度與位置 ---
        angle += orbitSpeed * Time.deltaTime;

        // [修改] 使用 actualCenter 來計算座標
        float x = actualCenter.x + orbitRadiusX * Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = actualCenter.y + orbitRadiusY * Mathf.Sin(angle * Mathf.Deg2Rad);
        transform.position = new Vector2(x, y);

        // --- 2. 根據 Y 座標計算並更新大小 ---
        // [修改] 最高點和最低點的計算現在基於 actualCenter
        float topY = actualCenter.y + orbitRadiusY;
        float bottomY = actualCenter.y - orbitRadiusY;
        float t = Mathf.InverseLerp(bottomY, topY, transform.position.y);
        float scale = Mathf.Lerp(maxScale, minScale, t);
        transform.localScale = new Vector3(scale, scale, 1f);

        // --- 3. 根據 Y 座標更新渲染順序 ---
        if (centerSpriteRenderer != null)
        {
            // [修改] 比較基準現在是 actualCenter 的 Y 值
            if (transform.position.y > actualCenter.y)
            {
                spriteRenderer.sortingOrder = centerSpriteRenderer.sortingOrder - 1;
            }
            else
            {
                spriteRenderer.sortingOrder = centerSpriteRenderer.sortingOrder + 1;
            }
        }
    }
}