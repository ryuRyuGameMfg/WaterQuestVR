using UnityEngine;

/// <summary>
/// 水を受けるもの（畑、洗濯など）
/// WaterVesselから水を注がれた時に通知を受けてタスクを実行
/// </summary>
public abstract class WaterReceiver : MonoBehaviour, IWaterReceiver
{
    [Header("トリガー設定")]
    [Tooltip("当たり判定の範囲（Colliderが必要、Is Trigger = true）")]
    [SerializeField] protected Collider triggerCollider;

    [Header("水消費設定")]
    [SerializeField] protected float staminaCost = 15f;        // 体力コスト

    [Header("1回実行設定")]
    [Tooltip("1回のみ実行するか")]
    [SerializeField] protected bool oneTimeExecution = true;

    [Header("クールダウン設定")]
    [Tooltip("タスク実行後のクールダウン時間（秒）。連続実行を防ぐために使用")]
    [Min(0f)]
    [SerializeField] protected float cooldownTime = 0.5f;

    [Header("容器フィルター")]
    [Tooltip("対象となる器具の型")]
    [SerializeField] protected AllowedVesselType allowedVesselTypeEnum = AllowedVesselType.All;

    /// <summary>
    /// 対象となる器具の型
    /// </summary>
    public enum AllowedVesselType
    {
        All = 0,        // すべて
        WaterBucket = 1, // バケツのみ
        WaterCup = 2     // コップのみ
    }

    [Header("視覚的フィードバック")]
    [Tooltip("マテリアルを変更するRenderer（自動取得も可能）")]
    [SerializeField] protected Renderer targetRenderer;
    [Tooltip("タスク実行前のマテリアル（通常の色）")]
    [SerializeField] protected Material beforeMaterial;
    [Tooltip("タスク実行後のマテリアル（完了後の色）")]
    [SerializeField] protected Material afterMaterial;

    // 内部で使用するType（enumから自動変換）
    protected System.Type allowedVesselType = null;

    protected bool hasBeenCompleted = false;
    private float lastExecutionTime = -1f;

    // 現在範囲内にいる器具
    private WaterVessel currentVessel = null;

    // IWaterReceiver実装
    public bool CanReceiveWater => !hasBeenCompleted || !oneTimeExecution;

    protected virtual void Awake()
    {
        Debug.Log($"[{gameObject.name}] WaterReceiver.Awake() が呼ばれました");

        // Trigger Colliderの設定
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider>();
            if (triggerCollider == null)
            {
                triggerCollider = GetComponentInChildren<Collider>();
            }
        }

        if (triggerCollider != null)
        {
            Debug.Log($"[{gameObject.name}] Trigger Collider: {triggerCollider.gameObject.name}, IsTrigger={triggerCollider.isTrigger}");
            if (!triggerCollider.isTrigger)
            {
                Debug.LogWarning($"[{gameObject.name}] Trigger Collider の Is Trigger が false です。true に設定してください。");
            }
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Trigger Collider が見つかりません。");
        }

        // Rigidbodyの確認（OnTriggerEnterが呼ばれるために必要）
        Rigidbody rb = GetComponent<Rigidbody>();
        Rigidbody rbChild = GetComponentInChildren<Rigidbody>();
        if (rb == null && rbChild == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Rigidbodyが見つかりません。OnTriggerEnterが呼ばれるには、少なくとも一方のオブジェクトにRigidbodyが必要です。");
        }
        else
        {
            Rigidbody foundRb = rb != null ? rb : rbChild;
            Debug.Log($"[{gameObject.name}] Rigidbody: {foundRb.gameObject.name}, IsKinematic={foundRb.isKinematic}");
        }

        // Rendererの自動取得
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }
        }

        // 初期マテリアルの設定
        if (targetRenderer != null && beforeMaterial != null)
        {
            targetRenderer.material = beforeMaterial;
        }

        // enumからTypeに変換
        ConvertVesselTypeEnumToType();
    }

    /// <summary>
    /// enumからTypeに変換
    /// </summary>
    private void ConvertVesselTypeEnumToType()
    {
        switch (allowedVesselTypeEnum)
        {
            case AllowedVesselType.All:
                allowedVesselType = null;
                break;
            case AllowedVesselType.WaterBucket:
                allowedVesselType = typeof(WaterBucket);
                break;
            case AllowedVesselType.WaterCup:
                allowedVesselType = typeof(WaterCup);
                break;
            default:
                allowedVesselType = null;
                break;
        }
    }

    /// <summary>
    /// 器具がトリガー範囲に入った時
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[{gameObject.name}] OnTriggerEnter: {other.gameObject.name}");

        WaterVessel vessel = other.GetComponent<WaterVessel>();
        if (vessel == null)
        {
            Debug.Log($"[{gameObject.name}] WaterVesselコンポーネントなし: {other.gameObject.name}");
            return;
        }

        // 器具の型チェック
        if (!IsAllowedVesselType(vessel))
        {
            Debug.Log($"[{gameObject.name}] 許可されていない器具タイプ: {vessel.GetType().Name}");
            return;
        }

        // 器具を記録（水の有無に関わらず）
        currentVessel = vessel;

        // 水がある場合は登録
        if (vessel.HasWater)
        {
            vessel.RegisterReceiver(this);
            Debug.Log($"[{gameObject.name}] 器具を登録しました（水あり）: {vessel.gameObject.name}");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] 器具が範囲内に入りました（水なし）: {vessel.gameObject.name}");
        }
    }

    /// <summary>
    /// 器具がトリガー範囲内にいる間（毎フレーム）
    /// 水を汲んだ後に範囲内で傾けるケースに対応
    /// </summary>
    protected virtual void OnTriggerStay(Collider other)
    {
        WaterVessel vessel = other.GetComponent<WaterVessel>();
        if (vessel == null) return;

        // 現在の器具でない場合は無視
        if (vessel != currentVessel) return;

        // 器具の型チェック
        if (!IsAllowedVesselType(vessel)) return;

        // 水がある場合は登録（まだ登録されていない場合）
        if (vessel.HasWater)
        {
            vessel.RegisterReceiver(this);
        }
    }

    /// <summary>
    /// 器具がトリガー範囲から出た時
    /// </summary>
    protected virtual void OnTriggerExit(Collider other)
    {
        Debug.Log($"[{gameObject.name}] OnTriggerExit: {other.gameObject.name}");

        WaterVessel vessel = other.GetComponent<WaterVessel>();
        if (vessel == null) return;

        if (vessel == currentVessel)
        {
            // WaterVesselから自分を登録解除
            vessel.UnregisterReceiver(this);
            currentVessel = null;

            Debug.Log($"[{gameObject.name}] 器具が範囲外に出ました: {vessel.gameObject.name}");
        }
    }

    /// <summary>
    /// 許可された器具の型かチェック
    /// </summary>
    private bool IsAllowedVesselType(WaterVessel vessel)
    {
        if (allowedVesselType == null) return true; // すべて許可

        System.Type vesselType = vessel.GetType();
        return vesselType == allowedVesselType || vesselType.IsSubclassOf(allowedVesselType);
    }

    /// <summary>
    /// IWaterReceiver.ReceiveWater の実装
    /// WaterVesselから水を注がれた時に呼ばれる
    /// </summary>
    public bool ReceiveWater(float amount, float quality)
    {
        // クールダウン中のチェック
        if (Time.time - lastExecutionTime < cooldownTime)
        {
            Debug.Log($"[{gameObject.name}] クールダウン中です");
            return false;
        }

        // 1回実行制限のチェック
        if (oneTimeExecution && hasBeenCompleted)
        {
            Debug.Log($"[{gameObject.name}] 既に完了済みです");
            return false;
        }

        // タスク実行
        ExecuteTask(amount, quality);

        lastExecutionTime = Time.time;

        // 視覚的フィードバック（マテリアル変更）
        if (!hasBeenCompleted)
        {
            UpdateMaterial();
            hasBeenCompleted = true;
        }

        return true;
    }

    /// <summary>
    /// タスクを実行（サブクラスで実装）
    /// </summary>
    /// <param name="amount">注がれた水量</param>
    /// <param name="quality">注がれた水質</param>
    protected abstract void ExecuteTask(float amount, float quality);

    /// <summary>
    /// マテリアルを変更して視覚的フィードバックを提供
    /// </summary>
    protected virtual void UpdateMaterial()
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Target Rendererが設定されていません。");
            return;
        }

        if (afterMaterial == null)
        {
            Debug.LogWarning($"[{gameObject.name}] After Materialが設定されていません。");
            return;
        }

        targetRenderer.material = afterMaterial;
        Debug.Log($"[{gameObject.name}] マテリアルを変更しました（タスク完了後）");
    }
}

