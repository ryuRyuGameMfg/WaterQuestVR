using UnityEngine;

/// <summary>
/// 水器具の抽象基底クラス（バケツ・コップ共通機能）
/// Colliderコンポーネントが必要です（インスペクターで手動設定してください）
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class WaterVessel : MonoBehaviour
{
    [Header("Container Settings")]
    [SerializeField] protected float maxCapacity = 80f;           // 最大容量
    [SerializeField] protected float waterQuality = 100f;         // 水質
    [SerializeField] protected float pourAngleThreshold = 45f;    // 傾き閾値

    [Header("Collider Settings")]
    [Tooltip("当たり判定のCollider（インスペクターで割り当て、または自動取得）")]
    [SerializeField] protected Collider vesselCollider;

    [Header("Visual Feedback")]
    [Tooltip("マテリアルカラーを変更するRenderer（自動取得も可能）")]
    [SerializeField] protected Renderer vesselRenderer;
    [Tooltip("水がある時の色（青）")]
    [SerializeField] protected Color waterColor = new Color(0.2f, 0.5f, 1f, 1f); // 青
    [Tooltip("空の時の色（白）")]
    [SerializeField] protected Color emptyColor = Color.white; // 白

    // 2値管理（満タン/空）
    protected bool isFull = false;
    private Material vesselMaterial;

    // プロパティ
    public bool IsFull => isFull;
    public float MaxCapacity => maxCapacity;
    public float WaterQuality => waterQuality;

    /// <summary>
    /// 傾き判定（角度ベース）
    /// </summary>
    public virtual bool IsPouringAngle()
    {
        float angleX = Mathf.Abs(transform.eulerAngles.x);
        float angleZ = Mathf.Abs(transform.eulerAngles.z);

        // 180度を超える場合は補正
        if (angleX > 180f) angleX = 360f - angleX;
        if (angleZ > 180f) angleZ = 360f - angleZ;

        return angleX > pourAngleThreshold || angleZ > pourAngleThreshold;
    }

    protected virtual void Awake()
    {
        // Colliderを自動取得（インスペクターで設定されていない場合）
        if (vesselCollider == null)
        {
            vesselCollider = GetComponent<Collider>();
            if (vesselCollider == null)
            {
                vesselCollider = GetComponentInChildren<Collider>();
            }
        }

        // Rendererを自動取得（設定されていない場合）
        if (vesselRenderer == null)
        {
            vesselRenderer = GetComponent<Renderer>();
            if (vesselRenderer == null)
            {
                vesselRenderer = GetComponentInChildren<Renderer>();
            }
        }

        // マテリアルのインスタンスを作成（共有マテリアルを変更しないように）
        if (vesselRenderer != null)
        {
            vesselMaterial = vesselRenderer.material;
            UpdateMaterialColor(); // 初期状態を反映
        }

        // Colliderの設定確認（警告のみ）
        ValidateCollider();
    }

    /// <summary>
    /// Colliderの設定を確認（警告のみ、自動修正はしない）
    /// </summary>
    private void ValidateCollider()
    {
        if (vesselCollider == null)
        {
            Debug.LogError($"[{gameObject.name}] Colliderコンポーネントが必要です。インスペクターで「Vessel Collider」にColliderを割り当てるか、GameObjectにColliderコンポーネントを追加してください。");
            return;
        }

        // Trigger Colliderではないことを確認（器具自体は通常のCollider）
        if (vesselCollider.isTrigger)
        {
            Debug.LogWarning($"[{gameObject.name}] ColliderのIs Triggerはfalseに設定してください。器具自体は通常のCollider（Is Trigger = false）である必要があります。");
        }
    }

    /// <summary>
    /// 水を満タンにする
    /// </summary>
    public virtual void FillWater(float quality)
    {
        if (isFull) return; // すでに満タンなら何もしない

        isFull = true;
        waterQuality = quality;
        UpdateMaterialColor(); // マテリアルカラーを更新

        // ログ出力
        Debug.Log($"[{gameObject.name}] 水が満タンになりました。水量: {maxCapacity:F0}L、水質: {quality:F0}");
    }

    /// <summary>
    /// 水を空にする
    /// </summary>
    public virtual void EmptyWater()
    {
        if (!isFull) return; // すでに空なら何もしない

        isFull = false;
        waterQuality = 0f;
        UpdateMaterialColor(); // マテリアルカラーを更新

        // ログ出力
        Debug.Log($"[{gameObject.name}] 水が空になりました。");
    }

    /// <summary>
    /// マテリアルカラーを更新（水の状態に応じて）
    /// </summary>
    protected virtual void UpdateMaterialColor()
    {
        if (vesselMaterial == null) return;

        Color targetColor = isFull ? waterColor : emptyColor;
        vesselMaterial.color = targetColor;
    }

    protected virtual void Update()
    {
        // タスク場所にいない時に傾けたら自動廃棄
        if (isFull && IsPouringAngle())
        {
            OnTiltedOutsideTask();
        }
    }

    /// <summary>
    /// タスク場所外で傾けられた時の処理（自動廃棄）
    /// </summary>
    protected virtual void OnTiltedOutsideTask()
    {
        // サブクラスで具体的な廃棄処理を実装
        // または、タスク場所でのみ無効化するフラグを持つ
    }
}
