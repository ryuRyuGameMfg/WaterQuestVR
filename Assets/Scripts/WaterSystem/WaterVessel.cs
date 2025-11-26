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
    [Tooltip("マテリアルを変更するRenderer（自動取得も可能）")]
    [SerializeField] protected Renderer vesselRenderer;
    [Tooltip("空の時のマテリアル（水を受ける前）")]
    [SerializeField] protected Material beforeMaterial;
    [Tooltip("満タンの時のマテリアル（水を受けた後）")]
    [SerializeField] protected Material afterMaterial;

    // 2値管理（満タン/空）
    protected bool isFull = false;

    // 傾き検知用フラグ（前フレームの状態を記録）
    private bool wasTiltedLastFrame = false;

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
        Debug.Log($"[{gameObject.name}] WaterVessel.Awake() 開始");

        // Colliderを自動取得（インスペクターで設定されていない場合）
        if (vesselCollider == null)
        {
            vesselCollider = GetComponent<Collider>();
            if (vesselCollider == null)
            {
                vesselCollider = GetComponentInChildren<Collider>();
            }
        }

        if (vesselCollider != null)
        {
            Debug.Log($"[{gameObject.name}] Vessel Collider設定済み: {vesselCollider.gameObject.name}, IsTrigger={vesselCollider.isTrigger}");
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Vessel Colliderが見つかりません");
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

        // マテリアルの設定確認と初期化
        if (vesselRenderer != null)
        {
            if (beforeMaterial == null)
            {
                Debug.LogWarning($"[{gameObject.name}] ⚠️ Before Material（空の時）が設定されていません。インスペクターで「Before Material」にマテリアルを割り当ててください。");
            }
            if (afterMaterial == null)
            {
                Debug.LogWarning($"[{gameObject.name}] ⚠️ After Material（満タンの時）が設定されていません。インスペクターで「After Material」にマテリアルを割り当ててください。");
            }

            // 初期状態のマテリアルを設定（空の状態）
            UpdateMaterial();
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] ⚠️ Vessel Rendererが見つかりません。マテリアルの切り替えができません。");
        }

        // Colliderの設定確認（警告のみ）
        ValidateCollider();
        Debug.Log($"[{gameObject.name}] WaterVessel.Awake() 完了");
    }

    protected virtual void Start()
    {
        Debug.Log($"[{gameObject.name}] WaterVessel.Start() 呼ばれました。enabled={enabled}, activeInHierarchy={gameObject.activeInHierarchy}");

        // Start()でもマテリアルを確実に設定（念のため）
        if (vesselRenderer != null)
        {
            UpdateMaterial();
        }

        // Colliderの最終確認
        if (vesselCollider == null)
        {
            Debug.LogError($"[{gameObject.name}] ⚠️ Vessel Colliderがnullです！OnTriggerEnterが呼ばれません。");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] ✅ Vessel Collider確認: {vesselCollider.gameObject.name}, IsTrigger={vesselCollider.isTrigger} (falseである必要があります)");
            if (vesselCollider.isTrigger)
            {
                Debug.LogError($"[{gameObject.name}] ⚠️ Vessel ColliderのIs Triggerがtrueです！falseに設定してください。");
            }
        }

        // Rigidbodyの確認（OnTriggerEnterが呼ばれるために必要）
        Rigidbody rb = GetComponent<Rigidbody>();
        Rigidbody rbChild = GetComponentInChildren<Rigidbody>();
        if (rb == null && rbChild == null)
        {
            Debug.LogWarning($"[{gameObject.name}] ⚠️ Rigidbodyが見つかりません。OnTriggerEnterが呼ばれるには、少なくとも一方のオブジェクトにRigidbodyが必要です。");
        }
        else
        {
            Rigidbody foundRb = rb != null ? rb : rbChild;
            Debug.Log($"[{gameObject.name}] ✅ Rigidbody確認: {foundRb.gameObject.name}, IsKinematic={foundRb.isKinematic}");
        }
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
        UpdateMaterial(); // マテリアルを更新（Water Materialに切り替え）

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
        UpdateMaterial(); // マテリアルを更新（Empty Materialに切り替え）

        // ログ出力
        Debug.Log($"[{gameObject.name}] 水が空になりました。");
    }

    /// <summary>
    /// マテリアルを更新（水の状態に応じて）
    /// 満タンの時: afterMaterial（水を受けた後）
    /// 空の時: beforeMaterial（水を受ける前）
    /// </summary>
    protected virtual void UpdateMaterial()
    {
        if (vesselRenderer == null)
        {
            Debug.LogWarning($"[{gameObject.name}] UpdateMaterial: Vessel Rendererがnullです。");
            return;
        }

        Material targetMaterial = isFull ? afterMaterial : beforeMaterial;

        if (targetMaterial == null)
        {
            Debug.LogWarning($"[{gameObject.name}] UpdateMaterial: {(isFull ? "After" : "Before")} Materialが設定されていません。インスペクターでマテリアルを割り当ててください。");
            return;
        }

        // マテリアルを置き換え
        vesselRenderer.material = targetMaterial;
    }

    protected virtual void Update()
    {
        // 傾き検知（満タンの場合のみ）
        if (isFull)
        {
            bool isTiltedNow = IsPouringAngle();

            // 傾けた瞬間を検知（前フレームは傾いていなかったが、今フレームで傾いた）
            if (isTiltedNow && !wasTiltedLastFrame)
            {
                OnTilted(); // 傾けた瞬間の処理
            }

            wasTiltedLastFrame = isTiltedNow;
        }
        else
        {
            wasTiltedLastFrame = false;
        }
    }

    /// <summary>
    /// 傾けた瞬間の処理（水を空にする）
    /// </summary>
    protected virtual void OnTilted()
    {
        if (!isFull) return; // 既に空なら何もしない

        // 水を空にする
        float amount = maxCapacity;
        float quality = waterQuality;

        EmptyWater();

        Debug.Log($"[{gameObject.name}] 傾けたため、水が空になりました。水量: {amount:F0}L");

        // タスク場所外で傾けた場合は廃棄として記録
        OnTiltedOutsideTask(amount, quality);
    }

    /// <summary>
    /// タスク場所外で傾けられた時の処理（廃棄として記録）
    /// タスク場所内の場合は、WaterReceiverが処理するため、ここでは何もしない
    /// </summary>
    protected virtual void OnTiltedOutsideTask(float amount, float quality)
    {
        // デフォルトでは何もしない（タスク場所内の場合はWaterReceiverが処理）
        // タスク場所外の場合は、WaterDisposalZoneが処理する
    }
}
