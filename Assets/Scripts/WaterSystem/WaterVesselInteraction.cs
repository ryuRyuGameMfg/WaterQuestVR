using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// WaterVesselに水の出し入れ機能を追加するコンポーネント
/// コップやバケツ自体が水を出す側・受ける側の両方になれる
/// </summary>
[RequireComponent(typeof(WaterVessel))]
public class WaterVesselInteraction : WaterInteractionBase
{
    [Header("Water Source Settings")]
    [Tooltip("水を出す機能を有効化")]
    [SerializeField] private bool enableWaterSource = true;

    [Header("Water Receiver Settings")]
    [Tooltip("水を受ける機能を有効化")]
    [SerializeField] private bool enableWaterReceiver = true;

    [Header("Water Transfer Settings")]
    [FormerlySerializedAs("obiWaterEffect")]
    [SerializeField] private GameObject obiWaterObject;      // 水を出す時のObiエフェクト
    [SerializeField] private float transferAmount = 5f;       // 1回で移す水量
    [SerializeField] private float transferDuration = 1f;    // 移す時間（秒）

    private WaterVessel myVessel;
    private bool isTransferring = false;

    protected override void Awake()
    {
        base.Awake();

        myVessel = GetComponent<WaterVessel>();
        if (myVessel == null)
        {
            Debug.LogError($"{gameObject.name}: WaterVessel component is required!");
            enabled = false;
            return;
        }

        // デフォルト設定
        if (enableWaterSource)
        {
            requiresFull = true; // 水を出すには満タンが必要
            conditionType = ConditionType.TiltDetection; // 傾きで水を出す
        }

        if (enableWaterReceiver)
        {
            // 水を受ける場合は空が必要（別途設定）
            // requiresFull = false; // これはOnTriggerEnterで動的に判定
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        // 自分のWaterVesselは除外
        WaterVessel otherVessel = other.GetComponent<WaterVessel>();
        if (otherVessel == null || otherVessel == myVessel) return;

        // 水を受ける側として機能する場合
        if (enableWaterReceiver && !myVessel.IsFull && otherVessel.IsFull)
        {
            // 空の自分が満タンの他の器具から水を受ける
            currentContainer = otherVessel;

            // CollisionDetectionの場合は即座に実行
            if (conditionType == ConditionType.CollisionDetection)
            {
                TransferWaterIn();
            }
            else
            {
                OnContainerEntered(otherVessel);
            }
        }
        // 水を出す側として機能する場合
        else if (enableWaterSource && myVessel.IsFull && !otherVessel.IsFull)
        {
            // 満タンの自分が空の他の器具に水を出す
            currentContainer = otherVessel;

            // CollisionDetectionの場合は即座に実行
            if (conditionType == ConditionType.CollisionDetection)
            {
                TransferWaterOut();
            }
            else
            {
                OnContainerEntered(otherVessel);
            }
        }
    }

    protected override void OnContainerEntered(WaterVessel container)
    {
        // 既にOnTriggerEnterで処理済み
    }

    protected override void ExecuteTask()
    {
        if (isTransferring) return;
        if (currentContainer == null) return;

        // 水を出す側として実行
        if (enableWaterSource && myVessel.IsFull && !currentContainer.IsFull)
        {
            TransferWaterOut();
        }
        // 水を受ける側として実行（CollisionDetection以外の場合）
        else if (enableWaterReceiver && !myVessel.IsFull && currentContainer.IsFull)
        {
            TransferWaterIn();
        }
    }

    /// <summary>
    /// 水を出す（他の器具に移す）
    /// </summary>
    private void TransferWaterOut()
    {
        if (currentContainer == null || currentContainer.IsFull) return;
        if (!myVessel.IsFull) return;

        isTransferring = true;
        isExecuting = true;

        // Obiエフェクト表示
        if (obiWaterObject != null)
        {
            obiWaterObject.SetActive(true);
        }

        // 水を移す
        float actualAmount = Mathf.Min(transferAmount, myVessel.MaxCapacity);
        float quality = myVessel.WaterQuality;
        currentContainer.FillWater(quality);
        myVessel.EmptyWater();

        // ログ出力
        Debug.Log($"[{gameObject.name}] {currentContainer.gameObject.name}に水を移しました。水量: {actualAmount:F0}L、水質: {quality:F0}");

        // 一定時間後にエフェクト停止
        Invoke(nameof(StopTransfer), transferDuration);
    }

    /// <summary>
    /// 水を受ける（他の器具から移される）
    /// </summary>
    private void TransferWaterIn()
    {
        if (currentContainer == null || !currentContainer.IsFull) return;
        if (myVessel.IsFull) return;

        isTransferring = true;
        isExecuting = true;

        // 水を受ける
        float quality = currentContainer.WaterQuality;
        myVessel.FillWater(quality);
        currentContainer.EmptyWater();

        // ログ出力
        Debug.Log($"[{gameObject.name}] {currentContainer.gameObject.name}から水を受け取りました。水量: {myVessel.MaxCapacity:F0}L、水質: {quality:F0}");

        // 処理完了
        isTransferring = false;
        isExecuting = false;
    }

    private void StopTransfer()
    {
        // Obiエフェクト停止
        if (obiWaterObject != null)
        {
            obiWaterObject.SetActive(false);
        }

        isTransferring = false;
        isExecuting = false;
    }

    protected override bool CheckCondition()
    {
        // CollisionDetectionの場合はOnTriggerEnterで既に実行済み
        if (conditionType == ConditionType.CollisionDetection) return false;

        // 水を出す側の場合
        if (enableWaterSource && myVessel.IsFull && currentContainer != null && !currentContainer.IsFull)
        {
            return base.CheckCondition();
        }
        // 水を受ける側の場合（CollisionDetection以外は通常通り）
        else if (enableWaterReceiver && !myVessel.IsFull && currentContainer != null && currentContainer.IsFull)
        {
            return base.CheckCondition();
        }

        return false;
    }
}

