using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 水器具の抽象基底クラス（バケツ・コップ共通機能）
/// Colliderコンポーネントが必要です（インスペクターで手動設定してください）
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class WaterVessel : MonoBehaviour
{
    [Header("容器設定")]
    [SerializeField] protected float maxCapacity = 80f;           // 最大容量
    [SerializeField] protected float pourAngleThreshold = 45f;    // 傾き閾値

    [Header("傾き検知クールダウン設定")]
    [Tooltip("傾き検知後のクールダウン時間（秒）。連続実行を防ぐために使用")]
    [Min(0f)]
    [SerializeField] protected float tiltCooldownTime = 0.3f;

    // 水質は受け取った水によって決まるため、インスペクターで設定できない
    private float waterQuality = 0f;
    // 現在の水量（重み付き平均計算用）
    private float currentWaterAmount = 0f;

    [Header("コライダー設定")]
    [Tooltip("当たり判定のCollider（インスペクターで割り当て、または自動取得）")]
    [SerializeField] protected Collider vesselCollider;

    [Header("視覚的フィードバック")]
    [Tooltip("マテリアルを変更するRenderer（自動取得も可能）")]
    [SerializeField] protected Renderer vesselRenderer;
    [Tooltip("水を受ける前のマテリアル（空の時）")]
    [SerializeField] protected Material beforeMaterial;
    [Tooltip("水を受けた後のマテリアル（満タンの時）")]
    [SerializeField] protected Material afterMaterial;

    // 傾き検知用フラグ（前フレームの状態を記録）
    private bool wasTiltedLastFrame = false;
    private float lastTiltTime = -1f;

    // 現在範囲内にいるWaterReceiver（トリガー範囲内）
    private readonly List<IWaterReceiver> receiversInRange = new List<IWaterReceiver>();

    // プロパティ
    public bool IsFull => currentWaterAmount >= maxCapacity;
    public bool HasWater => currentWaterAmount > 0f;
    public float MaxCapacity => maxCapacity;
    public float WaterQuality => waterQuality;
    public float CurrentWaterAmount => currentWaterAmount;

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
    /// 水を減らす（部分的に空にする）
    /// </summary>
    /// <param name="amount">減らす水量</param>
    public virtual void ReduceWater(float amount)
    {
        if (amount <= 0f) return;
        if (currentWaterAmount <= 0f) return;

        currentWaterAmount = Mathf.Max(0f, currentWaterAmount - amount);

        if (currentWaterAmount <= 0f)
        {
            waterQuality = 0f;
        }

        UpdateMaterial();
    }

    /// <summary>
    /// 水を追加する（重み付き平均で水質を計算）
    /// </summary>
    /// <param name="amount">追加する水量</param>
    /// <param name="quality">追加する水の水質</param>
    /// <returns>実際に追加された水量</returns>
    public virtual float FillWater(float amount, float quality)
    {
        if (amount <= 0f) return 0f;

        // 追加可能な水量を計算
        float availableSpace = maxCapacity - currentWaterAmount;
        float actualAmount = Mathf.Min(amount, availableSpace);

        if (actualAmount <= 0f)
        {
            // 満タンの場合は何もしない
            return 0f;
        }

        // 重み付き平均で水質を計算
        if (currentWaterAmount > 0f)
        {
            // 既存の水がある場合：重み付き平均
            float totalAmount = currentWaterAmount + actualAmount;
            waterQuality = (waterQuality * currentWaterAmount + quality * actualAmount) / totalAmount;
        }
        else
        {
            // 空の場合は新しい水質をそのまま設定
            waterQuality = quality;
        }

        // 水量を更新
        currentWaterAmount += actualAmount;

        // マテリアルを更新
        UpdateMaterial();

        // ログ出力
        Debug.Log($"[{gameObject.name}] 水を追加しました。追加量: {actualAmount:F0}L、現在の水量: {currentWaterAmount:F0}L/{maxCapacity:F0}L、水質: {waterQuality:F1}");

        return actualAmount;
    }

    /// <summary>
    /// 水を満タンにする（互換性のためのメソッド、全容量で追加）
    /// </summary>
    /// <param name="quality">水の水質</param>
    public virtual void FillWater(float quality)
    {
        FillWater(maxCapacity, quality);
    }

    /// <summary>
    /// 水を空にする（全量を空にする）
    /// </summary>
    public virtual void EmptyWater()
    {
        if (currentWaterAmount <= 0f) return; // すでに空なら何もしない

        float emptiedAmount = currentWaterAmount;
        currentWaterAmount = 0f;
        waterQuality = 0f;
        UpdateMaterial(); // マテリアルを更新（Before Materialに切り替え）

        // ログ出力
        Debug.Log($"[{gameObject.name}] 水が空になりました。空にした水量: {emptiedAmount:F0}L");
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

        Material targetMaterial = (currentWaterAmount > 0f) ? afterMaterial : beforeMaterial;

        if (targetMaterial == null)
        {
            Debug.LogWarning($"[{gameObject.name}] UpdateMaterial: {(currentWaterAmount > 0f ? "After" : "Before")} Materialが設定されていません。インスペクターでマテリアルを割り当ててください。");
            return;
        }

        // マテリアルを置き換え
        vesselRenderer.material = targetMaterial;
    }

    protected virtual void Update()
    {
        // 傾き検知（水がある場合のみ）
        if (currentWaterAmount > 0f)
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
    /// WaterReceiverのトリガー範囲に入った時に呼ばれる
    /// </summary>
    public void RegisterReceiver(IWaterReceiver receiver)
    {
        if (receiver != null && !receiversInRange.Contains(receiver))
        {
            receiversInRange.Add(receiver);
            Debug.Log($"[{gameObject.name}] WaterReceiver登録: {receiver.GetType().Name}");
        }
    }

    /// <summary>
    /// WaterReceiverのトリガー範囲から出た時に呼ばれる
    /// </summary>
    public void UnregisterReceiver(IWaterReceiver receiver)
    {
        if (receiver != null && receiversInRange.Contains(receiver))
        {
            receiversInRange.Remove(receiver);
            Debug.Log($"[{gameObject.name}] WaterReceiver登録解除: {receiver.GetType().Name}");
        }
    }

    /// <summary>
    /// 傾けた瞬間の処理（水を注ぐ）
    /// </summary>
    protected virtual void OnTilted()
    {
        if (currentWaterAmount <= 0f) return; // 既に空なら何もしない

        // クールダウン中のチェック
        if (Time.time - lastTiltTime < tiltCooldownTime)
        {
            return;
        }

        // 水の情報を保存
        float amount = currentWaterAmount;
        float quality = waterQuality;

        lastTiltTime = Time.time; // クールダウンタイマーを更新

        Debug.Log($"[{gameObject.name}] 傾けました。水量: {amount:F0}L、範囲内のReceiver数: {receiversInRange.Count}");

        // 1. 範囲内のWaterReceiverに通知（タスク実行）
        bool taskExecuted = NotifyWaterReceivers(amount, quality);

        // 2. 水を空にする
        EmptyWater();

        // 3. タスク場所外だった場合は廃棄として記録
        if (!taskExecuted)
        {
            OnTiltedOutsideTask(amount, quality);
        }
    }

    /// <summary>
    /// 範囲内のWaterReceiverに水を注ぐ通知を送る
    /// </summary>
    /// <returns>いずれかのReceiverがタスクを実行したかどうか</returns>
    private bool NotifyWaterReceivers(float amount, float quality)
    {
        bool anyTaskExecuted = false;

        // 無効なReceiverを除去しながら通知
        for (int i = receiversInRange.Count - 1; i >= 0; i--)
        {
            IWaterReceiver receiver = receiversInRange[i];

            // nullチェック（Destroyされた場合など）
            if (receiver == null || (receiver is MonoBehaviour mb && mb == null))
            {
                receiversInRange.RemoveAt(i);
                continue;
            }

            // 受け取れる状態かチェック
            if (!receiver.CanReceiveWater)
            {
                Debug.Log($"[{gameObject.name}] {receiver.GetType().Name} は水を受け取れない状態です");
                continue;
            }

            // 水を注ぐ通知
            bool executed = receiver.ReceiveWater(amount, quality);
            if (executed)
            {
                Debug.Log($"[{gameObject.name}] {receiver.GetType().Name} がタスクを実行しました");
                anyTaskExecuted = true;
                break; // 1つのReceiverがタスクを実行したら終了
            }
        }

        return anyTaskExecuted;
    }

    /// <summary>
    /// タスク場所外で傾けられた時の処理（廃棄として記録）
    /// </summary>
    protected virtual void OnTiltedOutsideTask(float amount, float quality)
    {
        // デフォルトでは何もしない
        // サブクラスでオーバーライドして廃棄処理を実装
    }
}
